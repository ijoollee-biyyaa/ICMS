import { inject } from '@angular/core';
import {
  patchState,
  signalStore,
  withMethods,
  withState,
} from '@ngrx/signals';
import { rxMethod } from '@ngrx/signals/rxjs-interop';
import { EMPTY, pipe, tap } from 'rxjs';
import { catchError, concatMap } from 'rxjs/operators';

import { AuthService } from '../services/auth.service';

export const MemberStore = signalStore(
  { providedIn: 'root' },

  withState({
    loaded: false,
    isLoading: false,
    error: '',
  }),

  withMethods((store, auth = inject(AuthService)) => ({
    load: rxMethod<void>(
      pipe(
        concatMap(() => {
          if (store['loaded']()) {
            return EMPTY;
          }
          patchState(store, { isLoading: true, error: '' });
          if (!auth.isAuthenticated()) {
            patchState(store, { isLoading: false });
            return EMPTY;
          }
          patchState(store, { loaded: true, isLoading: false });
          return EMPTY;
        }),
        catchError(() => {
          patchState(store, {
            error: 'Could not load member data.',
            isLoading: false,
          });
          return EMPTY;
        }),
      ),
    ),

    clearError: () => patchState(store, { error: '' }),
  })),
);
