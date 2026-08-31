import {
  Component,
  ChangeDetectionStrategy,
  computed,
  inject,
  signal,
} from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatIconButton } from '@angular/material/button';
import { MatIcon } from '@angular/material/icon';

import { ChurchService } from '../../../../services/church.service';
import {
  Member,
  MemberDashboard,
  MemberAccountInfo,
  IssueMemberCredentials,
} from '../../../../models/member';
import { environment } from '../../../../../environments/environment';
import { fullName, initials } from '../../../../common/member-names';
import { problemDetail } from '../../../../common/http-errors';

@Component({
  selector: 'app-member-detail',
  imports: [MatIconButton, MatIcon, RouterLink],
  templateUrl: './member-detail.html',
  styleUrl: './member-detail.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MemberDetailPage {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private churchService = inject(ChurchService);

  private readonly memberId = Number(this.route.snapshot.paramMap.get('id') ?? 0);

  readonly member = signal<Member | null>(null);
  readonly dashboard = signal<MemberDashboard | null>(null);
  readonly account = signal<MemberAccountInfo | null>(null);
  readonly loading = signal(true);
  readonly error = signal('');
  readonly uploadingPhoto = signal(false);
  readonly issuing = signal(false);
  readonly credentials = signal<IssueMemberCredentials | null>(null);
  readonly copied = signal(false);

  readonly photoUrl = computed(() => this.member()?.photoUrl ?? null);

  constructor() {
    this.load();
  }

  load() {
    this.loading.set(true);
    this.error.set('');
    this.churchService.getMember(this.memberId).subscribe({
      next: (member) => {
        this.member.set(member);
        this.loading.set(false);
        this.loadDashboard();
        this.loadAccount();
      },
      error: (err) => {
        this.loading.set(false);
        this.error.set(problemDetail(err, 'Could not load this member.'));
      },
    });
  }

  private loadDashboard() {
    this.churchService.getMemberDashboard(this.memberId).subscribe({
      next: (dashboard) => this.dashboard.set(dashboard),
      error: () => this.dashboard.set(null),
    });
  }

  private loadAccount() {
    this.churchService.getMemberAccount(this.memberId).subscribe({
      next: (account) => this.account.set(account),
      error: () => this.account.set(null),
    });
  }

  goBack() {
    this.router.navigate(['/church/members']);
  }

  nameOf(member: Member): string {
    return fullName(
      member.firstName,
      member.fatherName,
      member.grandfatherName,
    );
  }

  initialsOf(member: Member): string {
    return initials(
      member.firstName,
      member.fatherName,
      member.grandfatherName,
    );
  }

  joinedLabel(value: string | null): string {
    if (!value) return '—';
    const date = new Date(`${value}T00:00:00`);
    return isNaN(date.getTime()) ? value : date.toLocaleDateString();
  }

  statusClass(status: string): string {
    switch (status) {
      case 'Active':
        return 'bg-emerald-100 text-emerald-700';
      case 'Transferring':
        return 'bg-amber-100 text-amber-700';
      default:
        return 'bg-neutral-100 text-neutral-600';
    }
  }

  formatMoney(value: number): string {
    return `${value.toLocaleString('en-US', { maximumFractionDigits: 0 })} Birr`;
  }

  percent(value: number | null): string {
    return value === null ? '—' : `${Math.round(value)}%`;
  }

  employmentLabel(employmentType: string | null): string {
    if (!employmentType) return 'Not employed';
    switch (employmentType) {
      case 'FulltimeMinister':
        return 'Full-time minister';
      case 'ChurchStaff':
        return 'Church staff';
      case 'DistrictStaff':
        return 'District staff';
      default:
        return employmentType;
    }
  }

  // ---- Photo upload (direct to Cloudinary, URL saved via member update) ---

  onPhotoSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    input.value = '';
    if (!file) return;

    const { cloudName, uploadPreset } = environment.cloudinary;
    if (!cloudName || !uploadPreset) {
      this.error.set(
        'Profile photo upload is not configured. Set environment.cloudinary in the app settings.',
      );
      return;
    }

    if (!file.type.startsWith('image/')) {
      this.error.set('Please choose an image file.');
      return;
    }
    if (file.size > 5 * 1024 * 1024) {
      this.error.set('Photo must be 5 MB or smaller.');
      return;
    }

    const m = this.member();
    if (!m || this.uploadingPhoto()) return;

    this.error.set('');
    this.uploadingPhoto.set(true);

    const form = new FormData();
    form.append('file', file);
    form.append('upload_preset', uploadPreset);

    fetch(
      `https://api.cloudinary.com/v1_1/${cloudName}/image/upload`,
      { method: 'POST', body: form },
    )
      .then((res) => res.json())
      .then((data) => {
        const url = data?.secure_url as string | undefined;
        if (!url) {
          this.error.set('Cloudinary rejected the image. Check the upload preset.');
          this.uploadingPhoto.set(false);
          return;
        }
        this.churchService
          .updateMember(m.id, {
            firstName: m.firstName,
            fatherName: m.fatherName,
            grandfatherName: m.grandfatherName,
            dateOfBirth: m.dateOfBirth,
            gender: m.gender,
            jobStatus: m.jobStatus,
            phone: m.phone,
            email: m.email,
            photoUrl: url,
          })
          .subscribe({
            next: (updated) => {
              this.member.set(updated);
              this.uploadingPhoto.set(false);
            },
            error: (err) => {
              this.uploadingPhoto.set(false);
              this.error.set(
                problemDetail(err, 'Uploaded but could not save the photo.'),
              );
            },
          });
      })
      .catch(() => {
        this.uploadingPhoto.set(false);
        this.error.set('Could not reach the photo service.');
      });
  }

  // ---- Login credentials -------------------------------------------------

  issueCredentials() {
    if (this.issuing()) return;
    this.issuing.set(true);
    this.error.set('');
    this.churchService.issueMemberCredentials(this.memberId).subscribe({
      next: (credentials) => {
        this.issuing.set(false);
        this.credentials.set(credentials);
        this.account.set({
          memberId: credentials.memberId,
          hasAccount: true,
          email: credentials.email,
        });
      },
      error: (err) => {
        this.issuing.set(false);
        this.error.set(
          problemDetail(err, 'Could not issue login credentials.'),
        );
      },
    });
  }

  closeCredentials() {
    this.credentials.set(null);
    this.copied.set(false);
  }

  copyPassword() {
    const password = this.credentials()?.tempPassword;
    if (!password) return;
    navigator.clipboard?.writeText(password).then(() => {
      this.copied.set(true);
    });
  }
}