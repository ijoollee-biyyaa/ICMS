import { Component, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { MatFormField, MatLabel } from '@angular/material/form-field';
import { MatInput } from '@angular/material/input';
import { MatButton, MatIconButton } from '@angular/material/button';
import { MatIcon } from '@angular/material/icon';
import { MatProgressSpinner } from '@angular/material/progress-spinner';

import { AuthService } from '../../../services/auth.service';
import { ThemeService } from '../../../services/theme.service';

@Component({
  selector: 'app-login',
  imports: [
    ReactiveFormsModule,
    RouterLink,
    MatFormField,
    MatLabel,
    MatInput,
    MatIconButton,
    MatIcon,
    MatProgressSpinner,
  ],
  templateUrl: './login.html',
  styleUrl: './login.scss',
})
export class Login {
  private fb = inject(FormBuilder);
  private auth = inject(AuthService);
  private router = inject(Router);
  private themeService = inject(ThemeService);

  readonly isDark = computed(() => this.themeService.theme() === 'dark');

  form = this.fb.nonNullable.group({
    username: ['', Validators.required],
    password: ['', Validators.required],
  });

  hidePassword = signal(true);
  busy = signal(false);
  error = signal('');

  toggleTheme(): void {
    this.themeService.toggle();
  }

  constructor() {
    // Session may already be restored from the refresh cookie at boot — send
    // signed-in users straight to their area.
    if (this.auth.isAuthenticated()) {
      const homes = this.auth.homes();
      void this.router.navigate(homes.length === 1 ? [homes[0]] : ['/areas']);
    }
  }

  onSubmit() {
    if (this.form.invalid) {
      return;
    }
    this.busy.set(true);
    this.error.set('');
    void this.auth.login(this.form.getRawValue()).then(
      (profile) => {
        const homes = this.auth.homes();
        this.router.navigate(
          homes.length === 1 ? [homes[0]] : ['/areas'],
        );
      },
      (err) => {
        this.busy.set(false);
        this.error.set(
          err.error?.detail ?? err.error?.title ?? 'Sign in failed. Please try again.',
        );
      },
    );
  }
}