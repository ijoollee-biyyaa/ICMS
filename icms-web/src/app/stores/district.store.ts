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
import {
  addEntity,
  removeEntity,
  setAllEntities,
  withEntities,
} from '@ngrx/signals/entities';
import { EMPTY, forkJoin, pipe } from 'rxjs';
import { catchError, concatMap, exhaustMap, map, tap } from 'rxjs/operators';

import { environment } from '../../environments/environment';
import { DistrictService } from '../services/district.service';
import {
  CreateDistrictRequest,
  DistrictDetail,
  EmployeeRow,
  MinisterRow,
  OfficeExecutives,
  UpdateDistrictRequest,
} from '../models/district';
import { Department } from '../models/department';
import { RoleChangeRequest, UserAccount } from '../models/account';

function problemDetail(err: HttpErrorResponse, fallback: string): string {
  return err.error?.detail ?? fallback;
}

/**
 * Single source of truth for the district office.
 * One singleton store that every district component (shell, dashboard, profile,
 * accounts, departments…) reads from. A load() populates the district record,
 * employees, ministers, executives, departments and accounts together; mutations
 * (`create`, `update`, `delete`) update local state optimistically and rely on
 * this store — not on duplicated component state — so the UI never drifts.
 */
export const DistrictStore = signalStore(
  { providedIn: 'root' },

  withState({
    districtId: environment.districtId as number,
    district: null as DistrictDetail | null,
    ministers: [] as MinisterRow[],
    executives: null as OfficeExecutives | null,
    departments: [] as Department[],
    accounts: [] as UserAccount[],
    loaded: false,
    isLoading: false,
    isSaving: false,
    error: null as string | null,
  }),

  withEntities<EmployeeRow>(),

  withComputed((store) => ({
    officeTitle: computed(() => store.district()?.name ?? 'District Office'),

    churchCount: computed(() => store.district()?.churchCount ?? 0),
    memberCount: computed(() => store.district()?.memberCount ?? 0),
    employeeCount: computed(() => store.entities().length),
    ministerCount: computed(() => store.ministers().length),
    departmentCount: computed(() => store.departments().length),
    accountCount: computed(() => store.accounts().length),

    president: computed(() => store.executives()?.president ?? null),
    vicePresident: computed(() => store.executives()?.vicePresident ?? null),

    /** People whose salary is paid by the district (dual church+district hires). */
    paidFromDistrict: computed(
      () => store.entities().filter((e) => e.salaryPaidBy === 'District').length,
    ),
    activeEmployees: computed(
      () => store.entities().filter((e) => e.status === 'Active').length,
    ),
    onLeaveEmployees: computed(
      () => store.entities().filter((e) => e.status === 'OnLeave').length,
    ),
    fulltimeMinisters: computed(
      () => store.entities().filter((e) => e.employmentType === 'FulltimeMinister').length,
    ),
    employeesByStatus: computed(() =>
      store.entities().reduce(
        (acc, e) => {
          acc[e.status] = (acc[e.status] ?? 0) + 1;
          return acc;
        },
        {} as Record<string, number>,
      ),
    ),
  })),

  withMethods((store, api = inject(DistrictService)) => {
    const load = rxMethod<void>(
      pipe(
        exhaustMap(() => {
          if (store.loaded()) return EMPTY;
          patchState(store, { isLoading: true, error: null });
          const id = store.districtId();

          return forkJoin({
            district: api.getDistrict(id),
            employees: api.getEmployees(id, 1, 100),
            ministers: api.getMinisters(id, 1, 100),
            executives: api.getExecutives(id),
            departments: api.getDepartments(id, 1, 100),
            accounts: api.getAccounts(id),
          }).pipe(
            map((data) => ({
              district: data.district,
              employees: data.employees.items,
              ministers: data.ministers.items,
              executives: data.executives,
              departments: data.departments.items,
              accounts: data.accounts,
            })),
            tap((data) =>
              patchState(store, {
                district: data.district,
                ministers: data.ministers,
                executives: data.executives,
                departments: data.departments,
                accounts: data.accounts,
                ...setAllEntities(data.employees),
                loaded: true,
                isLoading: false,
                error: null,
              }),
            ),
            catchError((err: HttpErrorResponse) => {
              patchState(store, {
                error: problemDetail(err, 'Could not load district data.'),
                isLoading: false,
              });
              return EMPTY;
            }),
          );
        }),
      ),
    );

    return {
      load,

      /** Force a full reload of every district slice. */
      refresh() {
        patchState(store, { loaded: false });
        load();
      },

      loadDepartments: rxMethod<void>(
        pipe(
          concatMap(() => api.getDepartments(store.districtId(), 1, 100)),
          tap((page) =>
            patchState(store, { departments: page.items }),
          ),
          catchError(() => EMPTY),
        ),
      ),

      createDistrict: rxMethod<CreateDistrictRequest>(
        pipe(
          tap(() => patchState(store, { isSaving: true, error: null })),
          exhaustMap((body) =>
            api.createDistrict(body).pipe(
              tap((district) =>
                patchState(store, {
                  district,
                  districtId: district.id,
                  isSaving: false,
                  loaded: false,
                }),
              ),
              catchError((err: HttpErrorResponse) => {
                patchState(store, {
                  isSaving: false,
                  error: problemDetail(err, 'Could not create the district.'),
                });
                return EMPTY;
              }),
            ),
          ),
        ),
      ),

      updateDistrict: rxMethod<UpdateDistrictRequest>(
        pipe(
          tap(() => patchState(store, { isSaving: true, error: null })),
          exhaustMap((body) => {
            const previous = store.district();
            if (previous) {
              patchState(store, {
                district: { ...previous, ...body, address: body.address ?? null },
              });
            }
            return api.updateDistrict(store.districtId(), body).pipe(
              tap((district) =>
                patchState(store, { district, isSaving: false, error: null }),
              ),
              catchError((err: HttpErrorResponse) => {
                patchState(store, {
                  district: previous,
                  isSaving: false,
                  error: problemDetail(err, 'Could not update the district.'),
                });
                return EMPTY;
              }),
            );
          }),
        ),
      ),

      /**
       * Optimistic delete of an office employee. Removes the row immediately,
       * restores it if the server rejects the call.
       */
      deleteEmployee: rxMethod<number>(
        pipe(
          concatMap((employeeId) =>
            api.deleteEmployee(store.districtId(), employeeId).pipe(
              tap(() => patchState(store, removeEntity(employeeId), { error: null })),
              catchError((err: HttpErrorResponse) => {
                const match = store
                  .entities()
                  .find((e) => e.id === employeeId);
                if (match) {
                  patchState(store, addEntity(match));
                }
                patchState(store, {
                  error: problemDetail(err, 'Could not delete employee.'),
                });
                return EMPTY;
              }),
            ),
          ),
        ),
      ),

      /** Optimistic role flip on an account, synced to the backend, rolled back on error. */
      changeAccountRole: rxMethod<{ userId: string; body: RoleChangeRequest }>(
        pipe(
          exhaustMap(({ userId, body }) => {
            const previous = store.accounts();
            const flip = (grant: boolean) =>
              previous.map((a) => {
                if (a.userId !== userId) return a;
                const roles = grant
                  ? a.roles.includes(body.role)
                    ? a.roles
                    : [...a.roles, body.role]
                  : a.roles.filter((r) => r !== body.role);
                return { ...a, roles };
              });
            patchState(store, { accounts: flip(body.grant) });
            return api.changeAccountRole(store.districtId(), userId, body).pipe(
              tap(() => patchState(store, { error: null })),
              catchError((err: HttpErrorResponse) => {
                patchState(store, {
                  accounts: previous,
                  error: problemDetail(err, 'Could not update the account role.'),
                });
                return EMPTY;
              }),
            );
          }),
        ),
      ),

      clearError: () => patchState(store, { error: null }),
    };
  }),
);
