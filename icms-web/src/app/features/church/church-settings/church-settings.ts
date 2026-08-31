import {
  ChangeDetectionStrategy,
  Component,
  inject,
  signal,
} from '@angular/core';
import {
  FormBuilder,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import {
  MatError,
  MatFormField,
  MatHint,
  MatLabel,
} from '@angular/material/form-field';
import { MatInput } from '@angular/material/input';
import { MatIcon } from '@angular/material/icon';
import { Router } from '@angular/router';

import { AuthService } from '../../../services/auth.service';
import { ChurchWorkspaceStore } from '../../../stores/church-workspace.store';
import { HttpErrorResponse } from '@angular/common/http';
import { problemDetail } from '../../../common/http-errors';

type Section = 'general' | 'security';

@Component({
  selector: 'app-church-settings',
  imports: [
    ReactiveFormsModule,
    MatFormField,
    MatLabel,
    MatHint,
    MatError,
    MatInput,
    MatIcon,
  ],
  templateUrl: './church-settings.html',
  styleUrl: './church-settings.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ChurchSettings {
  readonly store = inject(ChurchWorkspaceStore);
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  readonly sections: { id: Section; label: string; icon: string }[] = [
    { id: 'general', label: 'Church', icon: 'church' },
    { id: 'security', label: 'Security', icon: 'shield' },
  ];
  readonly active = signal<Section>('general');

  readonly saving = signal(false);
  readonly success = signal('');
  readonly error = signal('');

  readonly form = this.fb.nonNullable.group(
    {
      currentPassword: ['', Validators.required],
      newPassword: ['', [Validators.required, Validators.minLength(8)]],
      confirmPassword: ['', Validators.required],
    },
    {
      validators: (group) => {
        const current = group.get('currentPassword');
        const next = group.get('newPassword');
        const confirm = group.get('confirmPassword');
        if (next?.value && next.value === current?.value) {
          next.setErrors({ sameAsCurrent: true });
        }
        if (confirm?.value && next?.value && confirm.value !== next.value) {
          confirm.setErrors({ mismatch: true });
        }
        return null;
      },
    },
  );

  setSection(section: Section) {
    this.active.set(section);
  }

  async save() {
    if (this.form.invalid || this.saving()) {
      this.form.markAllAsTouched();
      return;
    }
    this.saving.set(true);
    this.success.set('');
    this.error.set('');
    try {
      const { currentPassword, newPassword } = this.form.getRawValue();
      await this.auth.changePassword(currentPassword, newPassword);
      this.success.set('Password changed. Logging you out to use the new one…');
      setTimeout(() => this.router.navigate(['/']), 1800);
    } catch (err) {
      const http = err as HttpErrorResponse;
      this.error.set(
        problemDetail(
          http,
          'Could not change the password. Check your current password.',
        ),
      );
      this.form.controls.currentPassword.setErrors({ server: this.error() });
    } finally {
      this.saving.set(false);
    }
  }

  churchName(): string {
    return this.store.church()?.name ?? 'My Church';
  }

  churchLocation(): string {
    const church = this.store.church();
    if (!church) return '—';
    return [church.city, church.subcity]
      .filter(Boolean)
      .join(', ');
  }

  adminName(): string {
    const user = this.auth.currentUser();
    return user ? user.displayName || user.email : '—';
  }
}