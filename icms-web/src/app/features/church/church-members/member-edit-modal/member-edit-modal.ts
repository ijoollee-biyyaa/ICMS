import {
  Component,
  ChangeDetectionStrategy,
  Input,
  Output,
  EventEmitter,
  inject,
  signal,
  OnChanges,
  SimpleChanges,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatIcon } from '@angular/material/icon';

import { ChurchService } from '../../../../services/church.service';
import {
  Member,
  UpdateMemberRequest,
  Gender,
  MaritalStatus,
  JobStatus,
  HealthStatus,
} from '../../../../models/member';
import { problemDetail } from '../../../../common/http-errors';

export type EditTab = 'personal' | 'contact' | 'work_health' | 'spiritual';

@Component({
  selector: 'app-member-edit-modal',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MatIcon],
  templateUrl: './member-edit-modal.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MemberEditModal implements OnChanges {
  private readonly fb = inject(FormBuilder);
  private readonly churchService = inject(ChurchService);

  @Input() member: Member | null = null;
  @Input() open = false;

  @Output() closed = new EventEmitter<void>();
  @Output() saved = new EventEmitter<Member>();

  readonly activeTab = signal<EditTab>('personal');
  readonly saving = signal(false);
  readonly error = signal('');

  readonly MARITAL_OPTIONS: { value: MaritalStatus; label: string }[] = [
    { value: 'Single', label: 'Single' },
    { value: 'Married', label: 'Married' },
    { value: 'Divorced', label: 'Divorced' },
    { value: 'Widowed', label: 'Widowed' },
  ];

  readonly JOB_OPTIONS: { value: JobStatus; label: string }[] = [
    { value: 'Employed', label: 'Employed' },
    { value: 'SelfEmployed', label: 'Self-Employed' },
    { value: 'Student', label: 'Student' },
    { value: 'Unemployed', label: 'Unemployed' },
    { value: 'Retired', label: 'Retired' },
    { value: 'Other', label: 'Other' },
  ];

  readonly HEALTH_OPTIONS: { value: HealthStatus; label: string }[] = [
    { value: 'Healthy', label: 'Healthy' },
    { value: 'ChronicIllness', label: 'Chronic Illness' },
    { value: 'Disability', label: 'Disability' },
    { value: 'UnderMedicalCare', label: 'Under Medical Care' },
    { value: 'Other', label: 'Other / Special Care' },
  ];

  readonly SPIRITUAL_GIFTS: string[] = [
    'Teaching',
    'Evangelism',
    'Worship / Music',
    'Prayer & Intercession',
    'Leadership',
    'Service / Helping',
    'Administration',
    'Hospitality',
    'Giving',
    'Pastoral Care',
  ];

  readonly form = this.fb.nonNullable.group({
    firstName: ['', [Validators.required, Validators.maxLength(100)]],
    fatherName: ['', [Validators.required, Validators.maxLength(100)]],
    grandfatherName: ['', [Validators.required, Validators.maxLength(100)]],
    gender: ['Male' as Gender, Validators.required],
    dateOfBirth: ['', Validators.required],
    maritalStatus: ['Single' as MaritalStatus, Validators.required],

    phone: ['', [Validators.maxLength(20)]],
    email: ['', [Validators.email, Validators.maxLength(150)]],
    city: ['', [Validators.maxLength(100)]],
    subcity: ['', [Validators.maxLength(100)]],
    localAddress: ['', [Validators.maxLength(200)]],

    jobStatus: ['Employed' as JobStatus, Validators.required],
    healthStatus: ['Healthy' as HealthStatus, Validators.required],

    conversionDate: [''],
    baptismPlace: ['', [Validators.maxLength(200)]],
    baptismDate: [''],
    spiritualGift: ['', [Validators.maxLength(150)]],
  });

  ngOnChanges(changes: SimpleChanges) {
    if (changes['open'] && this.open && this.member) {
      this.populateForm(this.member);
      this.error.set('');
      this.activeTab.set('personal');
    } else if (changes['member'] && this.member && this.open) {
      this.populateForm(this.member);
    }
  }

  private populateForm(m: Member) {
    this.form.reset({
      firstName: m.firstName ?? '',
      fatherName: m.fatherName ?? '',
      grandfatherName: m.grandfatherName ?? '',
      gender: m.gender ?? 'Male',
      dateOfBirth: m.dateOfBirth ? m.dateOfBirth.slice(0, 10) : '',
      maritalStatus: m.maritalStatus ?? 'Single',

      phone: m.phone ?? '',
      email: m.email ?? '',
      city: m.city ?? '',
      subcity: m.subcity ?? '',
      localAddress: m.localAddress ?? '',

      jobStatus: m.jobStatus ?? 'Employed',
      healthStatus: m.healthStatus ?? 'Healthy',

      conversionDate: m.conversionDate ? m.conversionDate.slice(0, 10) : '',
      baptismPlace: m.baptismPlace ?? '',
      baptismDate: m.baptismDate ? m.baptismDate.slice(0, 10) : '',
      spiritualGift: m.spiritualGift ?? '',
    });
  }

  setSpiritualGift(gift: string) {
    const current = this.form.controls.spiritualGift.value;
    if (!current) {
      this.form.patchValue({ spiritualGift: gift });
    } else if (!current.includes(gift)) {
      this.form.patchValue({ spiritualGift: `${current}, ${gift}` });
    }
  }

  close() {
    if (this.saving()) return;
    this.closed.emit();
  }

  submit() {
    if (!this.member || this.saving()) return;

    if (this.form.invalid) {
      this.error.set('Please check all fields for validation errors.');
      this.form.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    this.error.set('');

    const v = this.form.getRawValue();

    const body: UpdateMemberRequest = {
      firstName: v.firstName.trim(),
      fatherName: v.fatherName.trim(),
      grandfatherName: v.grandfatherName.trim(),
      gender: v.gender,
      dateOfBirth: v.dateOfBirth || null,
      maritalStatus: v.maritalStatus,
      jobStatus: v.jobStatus,
      healthStatus: v.healthStatus,
      phone: v.phone?.trim() || null,
      email: v.email?.trim() || null,
      city: v.city?.trim() || null,
      subcity: v.subcity?.trim() || null,
      localAddress: v.localAddress?.trim() || null,
      photoUrl: this.member.photoUrl ?? null,
      conversionDate: v.conversionDate || null,
      baptismPlace: v.baptismPlace?.trim() || null,
      baptismDate: v.baptismDate || null,
      spiritualGift: v.spiritualGift?.trim() || null,
    };

    this.churchService.updateMember(this.member.id, body).subscribe({
      next: (updated) => {
        this.saving.set(false);
        this.saved.emit(updated);
        this.close();
      },
      error: (err) => {
        this.saving.set(false);
        this.error.set(problemDetail(err, 'Could not update member. Please review entries.'));
      },
    });
  }

  getMemberFullName(): string {
    if (!this.member) return 'Member';
    return `${this.member.firstName} ${this.member.fatherName} ${this.member.grandfatherName}`.trim();
  }
}
