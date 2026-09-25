import {
  Component,
  ChangeDetectionStrategy,
  computed,
  inject,
  signal,
  OnInit,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { MatIcon } from '@angular/material/icon';
import { MatIconButton } from '@angular/material/button';

import { AuthService } from '../../../../services/auth.service';
import { ChurchService } from '../../../../services/church.service';
import {
  Gender,
  MaritalStatus,
  JobStatus,
  HealthStatus,
  JoinChannel,
  CreateMemberRequest,
} from '../../../../models/member';
import { ClearanceRequest } from '../../../../models/clearance';
import { problemDetail } from '../../../../common/http-errors';

export type RegisterStep = 'personal' | 'contact' | 'work_health' | 'spiritual';

@Component({
  selector: 'app-member-register',
  imports: [CommonModule, ReactiveFormsModule, RouterModule, MatIcon, MatIconButton],
  templateUrl: './member-register.html',
  styleUrl: './member-register.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MemberRegister implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly churchService = inject(ChurchService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  readonly churchId = computed(() => this.auth.currentUser()?.churchId ?? null);

  readonly currentStep = signal<RegisterStep>('personal');
  readonly saving = signal(false);
  readonly error = signal('');
  readonly clearanceLoading = signal(false);

  // Clearance tracking if linked
  readonly clearanceId = signal<number | null>(null);
  readonly clearanceData = signal<ClearanceRequest | null>(null);

  readonly MARITAL_OPTIONS: { value: MaritalStatus; label: string; icon: string }[] = [
    { value: 'Single', label: 'Single', icon: 'person' },
    { value: 'Married', label: 'Married', icon: 'favorite' },
    { value: 'Divorced', label: 'Divorced', icon: 'heart_broken' },
    { value: 'Widowed', label: 'Widowed', icon: 'sentiment_neutral' },
  ];

  readonly JOB_OPTIONS: { value: JobStatus; label: string; icon: string; desc: string }[] = [
    { value: 'Employed', label: 'Employed', icon: 'work', desc: 'Working full-time or part-time' },
    { value: 'SelfEmployed', label: 'Self-Employed', icon: 'storefront', desc: 'Business owner / Contractor' },
    { value: 'Student', label: 'Student', icon: 'school', desc: 'High school or University' },
    { value: 'Unemployed', label: 'Unemployed', icon: 'person_search', desc: 'Seeking job opportunities' },
    { value: 'Retired', label: 'Retired', icon: 'elderly', desc: 'Completed professional career' },
    { value: 'Other', label: 'Other', icon: 'more_horiz', desc: 'Other employment situation' },
  ];

  readonly HEALTH_OPTIONS: { value: HealthStatus; label: string; icon: string; desc: string }[] = [
    { value: 'Healthy', label: 'Healthy', icon: 'verified', desc: 'Normal healthy condition' },
    { value: 'ChronicIllness', label: 'Chronic Illness', icon: 'medical_services', desc: 'Long-term health condition' },
    { value: 'Disability', label: 'Disability', icon: 'accessible', desc: 'Physical or sensory disability' },
    { value: 'UnderMedicalCare', label: 'Under Medical Care', icon: 'local_hospital', desc: 'Active treatment / recovery' },
    { value: 'Other', label: 'Special Care / Other', icon: 'healing', desc: 'Other health status' },
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
    // Step 1: Personal
    firstName: ['', [Validators.required, Validators.maxLength(100)]],
    fatherName: ['', [Validators.required, Validators.maxLength(100)]],
    grandfatherName: ['', [Validators.required, Validators.maxLength(100)]],
    gender: ['Male' as Gender, Validators.required],
    dateOfBirth: ['', Validators.required],
    maritalStatus: ['Single' as MaritalStatus, Validators.required],
    photoUrl: [''],

    // Step 2: Contact & Location
    phone: ['', [Validators.maxLength(20)]],
    email: ['', [Validators.email, Validators.maxLength(150)]],
    city: ['Addis Ababa', [Validators.maxLength(100)]],
    subcity: ['', [Validators.maxLength(100)]],
    localAddress: ['', [Validators.maxLength(200)]],

    // Step 3: Job & Health
    jobStatus: ['Employed' as JobStatus, Validators.required],
    healthStatus: ['Healthy' as HealthStatus, Validators.required],

    // Step 4: Spiritual
    joinedVia: ['Salvation' as JoinChannel, Validators.required],
    joinedAt: [''],
    conversionDate: [''],
    baptismPlace: ['', [Validators.maxLength(200)]],
    baptismDate: [''],
    spiritualGift: ['', [Validators.maxLength(150)]],
  });

  readonly steps: { id: RegisterStep; label: string; icon: string }[] = [
    { id: 'personal', label: 'Personal Info', icon: 'person' },
    { id: 'contact', label: 'Contact & Location', icon: 'location_on' },
    { id: 'work_health', label: 'Job & Health', icon: 'health_and_safety' },
    { id: 'spiritual', label: 'Spiritual Journey', icon: 'church' },
  ];

  readonly currentStepIndex = computed(() => {
    return this.steps.findIndex((s) => s.id === this.currentStep());
  });

  readonly isFirstStep = computed(() => this.currentStepIndex() === 0);
  readonly isLastStep = computed(() => this.currentStepIndex() === this.steps.length - 1);

  ngOnInit() {
    this.route.queryParamMap.subscribe((params) => {
      const cid = params.get('clearanceId') || params.get('newFromClearance');
      if (cid) {
        const idNum = Number(cid);
        if (!isNaN(idNum) && idNum > 0) {
          this.clearanceId.set(idNum);
          this.form.patchValue({ joinedVia: 'Transfer' });
          this.loadClearance(idNum);
        }
      }
    });
  }

  loadClearance(id: number) {
    const churchId = this.churchId();
    if (!churchId) return;

    this.clearanceLoading.set(true);
    this.churchService.getClearance(churchId, id).subscribe({
      next: (req) => {
        this.clearanceData.set(req);
        this.clearanceLoading.set(false);

        // Pre-fill names from clearance if available
        if (req.memberName) {
          // Internal transfer or rejoin — member already exists, split full name
          const parts = req.memberName.trim().split(/\s+/);
          this.form.patchValue({
            firstName: parts[0] ?? '',
            fatherName: parts[1] ?? '',
            grandfatherName: parts.slice(2).join(' ') || '',
            joinedVia: 'Transfer',
          });
        } else if (req.incomingFirstName || req.incomingFatherName || req.incomingGrandfatherName) {
          // External incoming — name stored in separate fields
          this.form.patchValue({
            firstName: req.incomingFirstName ?? '',
            fatherName: req.incomingFatherName ?? '',
            grandfatherName: req.incomingGrandfatherName ?? '',
            joinedVia: 'Transfer',
          });
        }
      },
      error: () => {
        this.clearanceLoading.set(false);
      },
    });
  }

  goToStep(step: RegisterStep) {
    const targetIdx = this.steps.findIndex((s) => s.id === step);
    const currIdx = this.currentStepIndex();

    // If moving forward, validate current step first
    if (targetIdx > currIdx) {
      if (!this.validateCurrentStep()) return;
    }

    this.currentStep.set(step);
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  next() {
    if (!this.validateCurrentStep()) return;

    const idx = this.currentStepIndex();
    if (idx < this.steps.length - 1) {
      this.currentStep.set(this.steps[idx + 1].id);
      window.scrollTo({ top: 0, behavior: 'smooth' });
    }
  }

  prev() {
    const idx = this.currentStepIndex();
    if (idx > 0) {
      this.currentStep.set(this.steps[idx - 1].id);
      window.scrollTo({ top: 0, behavior: 'smooth' });
    }
  }

  validateCurrentStep(): boolean {
    this.error.set('');
    const v = this.form.controls;

    if (this.currentStep() === 'personal') {
      v.firstName.markAsTouched();
      v.fatherName.markAsTouched();
      v.grandfatherName.markAsTouched();
      v.dateOfBirth.markAsTouched();
      v.gender.markAsTouched();
      v.maritalStatus.markAsTouched();

      if (
        v.firstName.invalid ||
        v.fatherName.invalid ||
        v.grandfatherName.invalid ||
        v.dateOfBirth.invalid ||
        v.gender.invalid ||
        v.maritalStatus.invalid
      ) {
        this.error.set('Please fill in all required personal details correctly.');
        return false;
      }
    } else if (this.currentStep() === 'contact') {
      v.email.markAsTouched();
      v.phone.markAsTouched();
      if (v.email.invalid || v.phone.invalid) {
        this.error.set('Please ensure contact details are formatted properly.');
        return false;
      }
    } else if (this.currentStep() === 'work_health') {
      v.jobStatus.markAsTouched();
      v.healthStatus.markAsTouched();
      if (v.jobStatus.invalid || v.healthStatus.invalid) {
        this.error.set('Please select employment and health status.');
        return false;
      }
    }

    return true;
  }

  setSpiritualGift(gift: string) {
    const current = this.form.controls.spiritualGift.value;
    if (!current) {
      this.form.patchValue({ spiritualGift: gift });
    } else if (!current.includes(gift)) {
      this.form.patchValue({ spiritualGift: `${current}, ${gift}` });
    }
  }

  submit() {
    const churchId = this.churchId();
    if (!churchId) {
      this.error.set('No active church selected.');
      return;
    }

    if (!this.form.valid) {
      this.error.set('Please check all fields across steps for errors.');
      return;
    }

    this.saving.set(true);
    this.error.set('');

    const v = this.form.getRawValue();

    const body: CreateMemberRequest = {
      churchId,
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
      photoUrl: v.photoUrl?.trim() || null,
      joinedVia: this.clearanceId() ? 'Transfer' : v.joinedVia,
      joinedAt: v.joinedAt || null,
      conversionDate: v.conversionDate || null,
      baptismPlace: v.baptismPlace?.trim() || null,
      baptismDate: v.baptismDate || null,
      spiritualGift: v.spiritualGift?.trim() || null,
      clearanceId: this.clearanceId() ?? null,
    };

    this.churchService.createMember(body).subscribe({
      next: (created) => {
        this.saving.set(false);
        this.router.navigate(['/church/members', created.id]);
      },
      error: (err) => {
        this.saving.set(false);
        this.error.set(problemDetail(err, 'Could not register member. Please check details.'));
      },
    });
  }

  fullName(): string {
    const v = this.form.getRawValue();
    return [v.firstName, v.fatherName, v.grandfatherName].filter(Boolean).join(' ') || 'New Member';
  }
}
