import {
  ChangeDetectionStrategy,
  Component,
  inject,
  OnInit,
  signal,
} from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { Router } from '@angular/router';
import { MatIcon } from '@angular/material/icon';
import {
  MatFormField,
  MatLabel,
  MatError,
  MatSuffix,
} from '@angular/material/form-field';
import { MatInput } from '@angular/material/input';
import { MatIconButton } from '@angular/material/button';

import { AuthService } from '../../../services/auth.service';
import { PageHeader } from '../../shared/ui/page-header/page-header';
import { SectionCard } from '../../shared/ui/section-card/section-card';
import {
  applyFieldErrors,
  clearFieldErrors,
  problemDetail,
} from '../../../common/http-errors';

function mustMatch(fieldA: string, fieldB: string) {
  return (form: { get: (name: string) => { value: string } | null }) => {
    const a = form.get(fieldA);
    const b = form.get(fieldB);
    return a && b && a.value === b.value ? null : { mismatch: true };
  };
}

@Component({
  selector: 'app-member-account',
  imports: [
    ReactiveFormsModule,
    MatIcon,
    MatFormField,
    MatLabel,
    MatError,
    MatSuffix,
    MatInput,
    MatIconButton,
    PageHeader,
    SectionCard,
  ],
  templateUrl: './member-account.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MemberAccountPage implements OnInit {
  private fb = inject(FormBuilder);
  private auth = inject(AuthService);
  private router = inject(Router);

  readonly currentUser = this.auth.currentUser;

  hideCurrent = signal(true);
  hideNew = signal(true);
  hideConfirm = signal(true);

  readonly busy = signal(false);
  readonly error = signal('');
  readonly notice = signal('');
  readonly changed = signal(false);

  readonly form = this.fb.nonNullable.group(
    {
      currentPassword: ['', [Validators.required]],
      newPassword: ['', [Validators.required, Validators.minLength(8)]],
      confirmPassword: ['', [Validators.required]],
    },
    { validators: mustMatch('newPassword', 'confirmPassword') },
  );

  ngOnInit(): void {
    this.form.valueChanges.subscribe(() => this.error.set(''));
  }

  async changePassword(): Promise<void> {
    this.error.set('');
    this.notice.set('');
    clearFieldErrors(this.form);
    if (this.form.invalid) return;

    this.busy.set(true);
    try {
      const { currentPassword, newPassword } = this.form.getRawValue();
      await this.auth.changePassword(currentPassword, newPassword);
      this.changed.set(true);
      this.busy.set(false);
    } catch (err) {
      const http = err as HttpErrorResponse;
      if (!applyFieldErrors(this.form, http)) {
        this.error.set(
          problemDetail(http, 'Could not change your password.'),
        );
      }
      this.busy.set(false);
    }
  }

  goHome() {
    this.router.navigate(['/member']);
  }
}