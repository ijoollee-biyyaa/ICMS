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
import { catchError, exhaustMap } from 'rxjs/operators';

import { AuthService } from '../services/auth.service';
import { ChurchService } from '../services/church.service';
import { MemberDashboard, MemberHistory } from '../models/member';

function problemDetail(err: HttpErrorResponse, fallback: string): string {
  return err.error?.detail ?? err.error?.title ?? fallback;
}

/**
 * Central store for the logged-in member's own dashboard. Singleton, shared by
 * every member feature so the dashboard and sub-pages never drift. Only the
 * member's own records are loaded (via their MemberId claim in the token).
 */
export const MemberStore = signalStore(
  { providedIn: 'root' },

  withState({
    dashboard: null as MemberDashboard | null,
    history: null as MemberHistory | null,
    loaded: false,
    historyLoaded: false,
    isLoading: false,
    error: '',
  }),

  withComputed((store) => ({
    attendanceRate: computed(() => store.dashboard()?.service.attendanceRate ?? null),
    totalPayments: computed(() => store.dashboard()?.service.totalPayments ?? 0),
    teamCount: computed(() => store.dashboard()?.teamCount ?? 0),
  })),

  withMethods((store, auth = inject(AuthService), service = inject(ChurchService)) => {
    const load = rxMethod<void>(
      pipe(
        exhaustMap(() => {
          const memberId = auth.memberId();
          if (store.loaded()) return EMPTY;
          if (memberId === null) {
            patchState(store, {
              error: 'No member profile linked to this account.',
              isLoading: false,
            });
            return EMPTY;
          }
          patchState(store, { isLoading: true, error: '' });
          return service.getMemberDashboard(memberId).pipe(
            exhaustMap((dashboard) => {
              patchState(store, { dashboard, loaded: true, isLoading: false });
              return EMPTY;
            }),
            catchError((err: HttpErrorResponse) => {
              patchState(store, {
                error: problemDetail(err, 'Could not load your dashboard.'),
                isLoading: false,
              });
              return EMPTY;
            }),
          );
        }),
      ),
    );

    const loadHistory = rxMethod<void>(
      pipe(
        exhaustMap(() => {
          const memberId = auth.memberId();
          if (store.historyLoaded()) return EMPTY;
          if (memberId === null) {
            patchState(store, {
              error: 'No member profile linked to this account.',
              isLoading: false,
            });
            return EMPTY;
          }
          patchState(store, { isLoading: true, error: '' });
          return service.getMemberHistory(memberId).pipe(
            exhaustMap((history) => {
              patchState(store, { history, historyLoaded: true, isLoading: false });
              return EMPTY;
            }),
            catchError((err: HttpErrorResponse) => {
              patchState(store, {
                error: problemDetail(err, 'Could not load your history.'),
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
      loadHistory,
      reload() {
        patchState(store, { loaded: false });
        load();
      },
      clearError: () => patchState(store, { error: '' }),
    };
  }),
);
