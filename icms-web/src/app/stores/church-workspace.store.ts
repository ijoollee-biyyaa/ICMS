import { computed, inject } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import {
  patchState,
  signalStore,
  withComputed,
  withMethods,
  withProps,
  withState,
} from '@ngrx/signals';
import { rxMethod } from '@ngrx/signals/rxjs-interop';
import { EMPTY, forkJoin, pipe } from 'rxjs';
import {
  catchError,
  concatMap,
  distinctUntilChanged,
  filter,
  tap,
} from 'rxjs/operators';

import { environment } from '../../environments/environment';
import { AuthService } from '../services/auth.service';
import { ChurchService } from '../services/church.service';
import { ChurchDetail } from '../models/church';
import { Department } from '../models/department';
import { EmployeeRow } from '../models/district';
import { Team } from '../models/team';
import { RoleChangeRequest, UserAccount, AccountLockReason } from '../models/account';

function problemDetail(err: HttpErrorResponse, fallback: string): string {
  return err.error?.detail ?? err.error?.title ?? fallback;
}

/**
 * Aggregate state for the workspace of the signed-in church admin: the church
 * record plus live counts of every sub-module, shared across the dashboard,
 * shell and module pages so the UI never drifts.
 */
export const ChurchWorkspaceStore = signalStore(
  { providedIn: 'root' },

  withProps(() => ({
    auth: inject(AuthService),
    service: inject(ChurchService),
  })),

  withState({
    church: null as ChurchDetail | null,
    teams: [] as Team[],
    employees: [] as EmployeeRow[],
    departments: [] as Department[],
    accounts: [] as UserAccount[],
    loaded: false,
    loadedChurchId: null as number | null,
    isLoading: false,
    error: '',
  }),

  withComputed((store) => ({
    churchId: computed(() => store.auth.currentUser()?.churchId ?? null),

    teamCount: computed(() => store.teams().length),
    memberCount: computed(() => store.church()?.memberCount ?? 0),
    daughterCount: computed(() => store.church()?.daughterCount ?? 0),
    employeeCount: computed(() => store.employees().length),
    departmentCount: computed(() => store.departments().length),
    accountCount: computed(() => store.accounts().length),

    activeEmployees: computed(
      () => store.employees().filter((e) => e.status === 'Active').length,
    ),
    paidFromDistrict: computed(
      () =>
        store.employees().filter((e) => e.salaryPaidBy === 'District').length,
    ),

    mainTeams: computed(() =>
      store.teams().filter((t) => t.parentTeamId === null),
    ),
    churchAdmins: computed(() =>
      store.accounts().filter((a) => a.roles.includes('ChurchAdmin')).length,
    ),
  })),

  withMethods((store) => {
    const load = rxMethod<void>(
      pipe(
        concatMap(() => {
          const churchId = store.churchId();
          if (churchId === null) return EMPTY;

          // A church admin can switch accounts/sessions; only skip when the
          // workspace already holds THIS church's data.
          if (store.loaded() && store.loadedChurchId() === churchId) {
            return EMPTY;
          }

          patchState(store, { isLoading: true, error: '' });
          const service = store.service;
          return forkJoin({
            church: service.getChurch(environment.districtId, churchId),
            teams: service.getTeams(churchId, 1, 100),
            employees: service.getEmployees(churchId, 1, 100),
            departments: service.getDepartments(churchId, 1, 100),
            accounts: service.getAccounts(
              environment.districtId,
              churchId,
            ),
          }).pipe(
            tap(({ church, teams, employees, departments, accounts }) =>
              patchState(store, {
                church,
                teams: teams.items,
                employees: employees.items,
                departments: departments.items,
                accounts,
                loaded: true,
                loadedChurchId: churchId,
                isLoading: false,
                error: '',
              }),
            ),
            catchError((err: HttpErrorResponse) => {
              patchState(store, {
                error: problemDetail(err, 'Could not load the church workspace.'),
                isLoading: false,
              });
              return EMPTY;
            }),
          );
        }),
      ),
    );

    const autoLoad = rxMethod<number | null>(
      pipe(
        distinctUntilChanged(),
        filter((id): id is number => id !== null),
        tap(() => load()),
      ),
    );
    autoLoad(store.churchId);

    return {
      load,

      /** Loads the workspace by itself once the signed-in user's church is known. */
      autoLoad,

      reload() {
        patchState(store, { loaded: false, loadedChurchId: null });
        load();
      },

      reset() {
        patchState(store, {
          church: null,
          teams: [],
          employees: [],
          departments: [],
          accounts: [],
          loaded: false,
          loadedChurchId: null,
          isLoading: false,
          error: '',
        });
      },

      clearError: () => patchState(store, { error: '' }),

      changeAccountRole: rxMethod<{
        userId: string;
        body: RoleChangeRequest;
      }>(
        pipe(
          concatMap(({ userId, body }) => {
            const districtId = environment.districtId;
            const churchId = store.churchId();
            if (churchId === null) return EMPTY;
            const previous = store.accounts();
            patchState(store, {
              accounts: body.grant
                ? previous.map((a) =>
                    a.userId === userId && !a.roles.includes(body.role)
                      ? { ...a, roles: [...a.roles, body.role] }
                      : a,
                  )
                : previous.map((a) =>
                    a.userId === userId
                      ? { ...a, roles: a.roles.filter((r) => r !== body.role) }
                      : a,
                  ),
            });
            return store.service.changeAccountRole(districtId, churchId, userId, body).pipe(
              catchError(() => {
                patchState(store, { accounts: previous });
                return EMPTY;
              }),
            );
          }),
        ),
      ),

      lockAccount: rxMethod<{ userId: string; reason: AccountLockReason }>(
        pipe(
          concatMap(({ userId, reason }) => {
            const districtId = environment.districtId;
            const churchId = store.churchId();
            if (churchId === null) return EMPTY;
            const previous = store.accounts();
            patchState(store, {
              accounts: previous.map((a) =>
                a.userId === userId
                  ? { ...a, isAccountLocked: true, lockReason: reason }
                  : a,
              ),
            });
            return store.service
              .lockAccount(districtId, churchId, userId, { reason })
              .pipe(
                catchError(() => {
                  patchState(store, { accounts: previous });
                  return EMPTY;
                }),
              );
          }),
        ),
      ),

      unlockAccount: rxMethod<{ userId: string }>(
        pipe(
          concatMap(({ userId }) => {
            const districtId = environment.districtId;
            const churchId = store.churchId();
            if (churchId === null) return EMPTY;
            const previous = store.accounts();
            patchState(store, {
              accounts: previous.map((a) =>
                a.userId === userId
                  ? { ...a, isAccountLocked: false, lockReason: 'None' }
                  : a,
              ),
            });
            return store.service.unlockAccount(districtId, churchId, userId).pipe(
              catchError(() => {
                patchState(store, { accounts: previous });
                return EMPTY;
              }),
            );
          }),
        ),
      ),
    };
  }),
);