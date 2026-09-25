import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import {
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators as V,
} from '@angular/forms';
import { DecimalPipe } from '@angular/common';
import { MatIcon } from '@angular/material/icon';
import { MatFormField, MatLabel, MatError } from '@angular/material/form-field';
import { MatInput } from '@angular/material/input';
import { MatSelect, MatOption } from '@angular/material/select';

import { MemberStore } from '../../../stores/member.store';
import { ChurchService } from '../../../services/church.service';
import { AuthService } from '../../../services/auth.service';
import { Gender, JobStatus } from '../../../models/member';
import { PageHeader } from '../../shared/ui/page-header/page-header';
import { SectionCard } from '../../shared/ui/section-card/section-card';

function dateOrNull(v: string): string | null {
  if (!v) return null;
  return new Date(v + 'T00:00:00').toISOString();
}

@Component({
  selector: 'app-member-profile',
  imports: [
    MatIcon,
    DecimalPipe,
    PageHeader,
    SectionCard,
    ReactiveFormsModule,
    MatFormField,
    MatInput,
    MatLabel,
    MatError,
    MatSelect,
    MatOption,
  ],
  templateUrl: './member-profile.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MemberProfilePage {
  readonly store = inject(MemberStore);
  readonly service = inject(ChurchService);
  readonly auth = inject(AuthService);

  readonly editing = signal(false);
  readonly busy = signal(false);
  readonly error = signal('');
  readonly notice = signal('');

  readonly genders: Gender[] = ['Male', 'Female'];
  readonly jobStatuses: JobStatus[] = [
    'Student',
    'Employed',
    'SelfEmployed',
    'Unemployed',
    'Retired',
    'Other',
  ];

  readonly memberId = computed(() => {
    return this.tryMemberId();
  });

  form = new FormGroup({
    firstName: new FormControl('', V.required),
    fatherName: new FormControl('', V.required),
    grandfatherName: new FormControl('', V.required),
    gender: new FormControl<Gender>('Male', V.required),
    jobStatus: new FormControl<JobStatus>('Other', V.required),
    dateOfBirth: new FormControl(''),
    phone: new FormControl(''),
    email: new FormControl('', V.email),
  });

  constructor() {
    this.store.load();
  }

  private tryMemberId(): number | null {
    const authId = this.auth.memberId();
    const dashId = this.store.dashboard()?.memberId ?? null;
    return dashId ?? authId;
  }

  async openEdit() {
    this.error.set('');
    this.notice.set('');
    const id = this.memberId();
    if (id === null) {
      this.error.set('No member profile is linked to this account.');
      this.editing.set(true);
      return;
    }
    this.editing.set(true);
    this.service.getMember(id).subscribe((m) => {
      this.form.patchValue({
        firstName: m.firstName,
        fatherName: m.fatherName,
        grandfatherName: m.grandfatherName,
        gender: m.gender,
        jobStatus: m.jobStatus,
        dateOfBirth: m.dateOfBirth ? m.dateOfBirth.slice(0, 10) : '',
        phone: m.phone ?? '',
        email: m.email ?? '',
      });
    });
  }

  closeEdit() {
    if (this.busy()) return;
    this.editing.set(false);
    this.form.reset({ gender: 'Male', jobStatus: 'Other' });
    this.error.set('');
    this.notice.set('');
  }

  save() {
    const id = this.memberId();
    if (id === null || this.form.invalid || this.busy()) return;
    const v = this.form.getRawValue();
    const body = {
      firstName: v.firstName ?? '',
      fatherName: v.fatherName ?? '',
      grandfatherName: v.grandfatherName ?? '',
      gender: v.gender ?? 'Male',
      jobStatus: v.jobStatus ?? 'Other',
      dateOfBirth: dateOrNull(v.dateOfBirth ?? ''),
      phone: v.phone || null,
      email: v.email || null,
      photoUrl: null,
    };

    this.busy.set(true);
    this.error.set('');
    this.service.updateMember(id, body).subscribe({
      next: () => {
        this.busy.set(false);
        this.editing.set(false);
        this.store.reload();
        this.notice.set('Profile updated.');
      },
      error: (err: { error?: { detail?: string; title?: string } }) => {
        this.busy.set(false);
        this.error.set(err.error?.detail ?? err.error?.title ?? 'Could not save your profile.');
      },
    });
  }
}