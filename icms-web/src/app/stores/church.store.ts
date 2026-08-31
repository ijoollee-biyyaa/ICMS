import { computed, inject } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import {
  patchState,
  signalStore,
  withComputed,
  withMethods,
  withState,
} from '@ngrx/signals';
import { rxMethod } from '@ngrx/signals/rxjs-interop';
import { EMPTY, pipe } from 'rxjs';
import { catchError, concatMap, exhaustMap, tap } from 'rxjs/operators';

import { environment } from '../../environments/environment';
import { ChurchService } from '../services/church.service';
import {
  Church,
  CreateChurchRequest,
  CreateChurchResponse,
  UpdateChurchRequest,
} from '../models/church';

function problemDetail(err: HttpErrorResponse, fallback: string): string {
  return err.error?.detail ?? err.error?.title ?? fallback;
}

/** Extracts FluentValidation field errors (400 ProblemDetails) keyed by control name. */
function validationFields(
  err: HttpErrorResponse,
): Record<string, string> | null {
  if (err.status !== 400) return null;
  const errors = err.error?.errors as Record<string, string[]> | undefined;
  if (!errors) return null;

  const fields: Record<string, string> = {};
  for (const [key, messages] of Object.entries(errors)) {
    const camel = key.charAt(0).toLowerCase() + key.slice(1);
    fields[camel] = messages?.[0] ?? 'Invalid value.';
  }
  return fields;
}

function insertAt<T>(list: T[], item: T, index: number): T[] {
  if (index < 0) return [item, ...list];
  return [...list.slice(0, index), item, ...list.slice(index)];
}

interface UpdateChurchArg {
  churchId: number;
  body: UpdateChurchRequest;
}

/**
 * Central store for the district's churches. Singleton, shares one list across
 * every component. Mutations keep the list fresh here — no component-level state
 * — so the UI never drifts.
 */
export const ChurchStore = signalStore(
  { providedIn: 'root' },

  withState({
    districtId: environment.districtId,
    churches: [] as Church[],
    created: null as CreateChurchResponse | null,
    fieldErrors: null as Record<string, string> | null,
    loaded: false,
    isLoading: false,
    isSaving: false,
    error: '',
  }),

  withComputed((store) => ({
    churchCount: computed(() => store.churches().length),
    localCount: computed(
      () => store.churches().filter((c) => c.type === 'Local').length,
    ),
    daughterCount: computed(
      () => store.churches().filter((c) => c.type === 'Daughter').length,
    ),
  })),

  withMethods((store, service = inject(ChurchService)) => {
    const load = rxMethod<void>(
      pipe(
        exhaustMap(() => {
          if (store.loaded()) return EMPTY;
          patchState(store, { isLoading: true, error: '' });
          return service.getChurches(store.districtId(), 1, 100).pipe(
            tap((churches) =>
              patchState(store, { churches, loaded: true, isLoading: false }),
            ),
            catchError((err: HttpErrorResponse) => {
              patchState(store, {
                error: problemDetail(err, 'Could not load churches.'),
                isLoading: false,
              });
              return EMPTY;
            }),
          );
        }),
      ),
    );

    const createChurch = rxMethod<CreateChurchRequest>(
      pipe(
        tap(() =>
          patchState(store, { isSaving: true, error: '', fieldErrors: null }),
        ),
        exhaustMap((body) =>
          service.createChurch(store.districtId(), body).pipe(
            tap((res) =>
              patchState(store, {
                created: res,
                churches: [...store.churches(), res.church],
                isSaving: false,
                error: '',
              }),
            ),
            catchError((err: HttpErrorResponse) => {
              const fields = validationFields(err);
              patchState(store, {
                isSaving: false,
                error: fields ? '' : problemDetail(err, 'Could not create the church.'),
                fieldErrors: fields,
              });
              return EMPTY;
            }),
          ),
        ),
      ),
    );

    const updateChurch = rxMethod<UpdateChurchArg>(
      pipe(
        tap(() =>
          patchState(store, { isSaving: true, error: '', fieldErrors: null }),
        ),
        exhaustMap((arg) =>
          service
            .updateChurch(store.districtId(), arg.churchId, arg.body)
            .pipe(
              tap((church) =>
                patchState(store, {
                  churches: store
                    .churches()
                    .map((c) => (c.id === arg.churchId ? church : c)),
                  isSaving: false,
                  error: '',
                }),
              ),
              catchError((err: HttpErrorResponse) => {
                const fields = validationFields(err);
                patchState(store, {
                  isSaving: false,
                  error: fields ? '' : problemDetail(err, 'Could not update the church.'),
                  fieldErrors: fields,
                });
                return EMPTY;
              }),
            ),
        ),
      ),
    );

    const deleteChurch = rxMethod<number>(
      pipe(
        exhaustMap((churchId) => {
          const current = store.churches();
          const index = current.findIndex((c) => c.id === churchId);
          const removed = index >= 0 ? current[index] : null;

          patchState(store, {
            churches: current.filter((c) => c.id !== churchId),
            error: '',
          });

          return service.deleteChurch(store.districtId(), churchId).pipe(
            tap(() => patchState(store, { error: '' })),
            catchError((err: HttpErrorResponse) => {
              patchState(store, {
                churches: removed
                  ? insertAt(store.churches(), removed, index)
                  : store.churches(),
                error: problemDetail(err, 'Could not delete the church.'),
              });
              return EMPTY;
            }),
          );
        }),
      ),
    );

    return {
      load,
      createChurch,
      updateChurch,
      deleteChurch,

      reload() {
        patchState(store, { loaded: false });
        load();
      },

      clearCreated: () => patchState(store, { created: null }),
      clearError: () => patchState(store, { error: '' }),
      clearErrors: () => patchState(store, { error: '', fieldErrors: null }),
    };
  }),
);
