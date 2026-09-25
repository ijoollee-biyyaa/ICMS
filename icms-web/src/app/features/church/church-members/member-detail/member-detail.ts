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
import { MemberEditModal } from '../member-edit-modal/member-edit-modal';

@Component({
  selector: 'app-member-detail',
  imports: [MatIconButton, MatIcon, RouterLink, MemberEditModal],
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
  readonly showEditModal = signal(false);

  readonly photoUrl = computed(() => {
    const url = this.member()?.photoUrl ?? null;
    if (!url) return null;
    // Resolve relative /uploads paths to the API origin (avoids proxy restart)
    if (url.startsWith('/uploads')) return `${environment.apiBase}${url}`;
    return url;
  });

  openEdit() {
    this.showEditModal.set(true);
  }

  onMemberSaved(updated: Member) {
    this.member.set(updated);
  }

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
        return 'bg-emerald-100 text-emerald-700 dark:bg-emerald-500/15 dark:text-emerald-300';
      case 'Transferring':
        return 'bg-amber-100 text-amber-700 dark:bg-amber-500/15 dark:text-amber-300';
      default:
        return 'bg-gray-100 text-gray-600 dark:bg-gray-500/15 dark:text-gray-300';
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

  healthBadgeClass(health: string | null | undefined): string {
    switch (health) {
      case 'Healthy':
        return 'bg-emerald-100 text-emerald-700 dark:bg-emerald-500/15 dark:text-emerald-300';
      case 'ChronicIllness':
      case 'UnderMedicalCare':
        return 'bg-amber-100 text-amber-700 dark:bg-amber-500/15 dark:text-amber-300';
      case 'Disability':
        return 'bg-indigo-100 text-indigo-700 dark:bg-indigo-500/15 dark:text-indigo-300';
      default:
        return 'bg-gray-100 text-gray-700 dark:bg-gray-800 dark:text-gray-300';
    }
  }

  formatLabel(val: string | null | undefined): string {
    if (!val) return '—';
    return val.replace(/([A-Z])/g, ' $1').trim();
  }

  // ---- Photo upload (via internal API) ------------------------------------

  onPhotoSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    input.value = '';
    if (!file) return;

    if (!file.type.startsWith('image/')) {
      this.error.set('Please choose an image file.');
      return;
    }
    if (file.size > 10 * 1024 * 1024) {
      this.error.set('Photo must be 10 MB or smaller.');
      return;
    }

    const m = this.member();
    if (!m || this.uploadingPhoto()) return;

    this.error.set('');
    this.uploadingPhoto.set(true);

    this.churchService.uploadMemberPhoto(m.id, file).subscribe({
      next: (updated) => {
        this.member.set(updated);
        this.uploadingPhoto.set(false);
      },
      error: (err) => {
        this.uploadingPhoto.set(false);
        this.error.set(problemDetail(err, 'Could not upload photo.'));
      },
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