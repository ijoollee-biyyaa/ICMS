import {
  Component,
  ChangeDetectionStrategy,
  computed,
  effect,
  inject,
  signal,
} from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { Router } from '@angular/router';
import { MatIconButton } from '@angular/material/button';
import { MatIcon } from '@angular/material/icon';

import { AuthService } from '../../../services/auth.service';
import { StatCard } from '../../shared/ui/stat-card/stat-card';
import { ChurchService } from '../../../services/church.service';
import {
  Member,
  MemberStats,
  JobStatus,
  Gender,
  JoinChannel,
} from '../../../models/member';
import { fullName, initials } from '../../../common/member-names';
import { problemDetail } from '../../../common/http-errors';
import { MemberEditModal } from './member-edit-modal/member-edit-modal';

@Component({
  selector: 'app-church-members',
  imports: [
    MatIconButton,
    MatIcon,
    StatCard,
    DecimalPipe,
    MemberEditModal,
  ],
  templateUrl: './church-members.html',
  styleUrl: './church-members.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ChurchMembers {
  private auth = inject(AuthService);
  private churchService = inject(ChurchService);
  private router = inject(Router);

  readonly churchId = computed(() => this.auth.currentUser()?.churchId ?? null);

  readonly members = signal<Member[]>([]);
  readonly stats = signal<MemberStats | null>(null);
  readonly loading = signal(false);
  readonly deleting = signal(false);
  readonly error = signal('');
  readonly showEditModal = signal(false);
  readonly editing = signal<Member | null>(null);
  readonly toDelete = signal<Member | null>(null);

  readonly page = signal(1);
  readonly pageSize = signal(10);
  readonly totalCount = signal(0);
  readonly query = signal('');

  private searchTimer: ReturnType<typeof setTimeout> | null = null;

  readonly JOBS: JobStatus[] = [
    'Student',
    'Employed',
    'SelfEmployed',
    'Unemployed',
    'Retired',
    'Other',
  ];
  readonly CHANNELS: JoinChannel[] = [
    'Baptism',
    'Salvation',
    'Transfer',
    'Return',
  ];
  readonly PAGE_SIZES = [10, 25, 50, 100];

  readonly totalPages = computed(() =>
    Math.max(1, Math.ceil(this.totalCount() / this.pageSize())),
  );
  readonly rowStart = computed(() =>
    this.totalCount() === 0 ? 0 : (this.page() - 1) * this.pageSize() + 1,
  );
  readonly rowEnd = computed(() =>
    Math.min(this.totalCount(), this.page() * this.pageSize()),
  );
  readonly pageNumbers = computed(() => {
    const total = this.totalPages();
    const current = this.page();
    const start = Math.max(1, Math.min(current - 2, total - 4));
    const end = Math.min(total, start + 4);
    const pages: number[] = [];
    for (let p = start; p <= end; p++) {
      pages.push(p);
    }
    return pages;
  });

  readonly activeCount = computed(() => this.stats()?.active ?? 0);
  readonly transferringCount = computed(() => this.stats()?.transferring ?? 0);
  readonly inactiveCount = computed(() => this.stats()?.inactive ?? 0);

  constructor() {
    effect(() => {
      const id = this.churchId();
      if (id) {
        this.loadStats();
        this.load();
      }
    });
  }

  load() {
    const id = this.churchId();
    if (!id) return;
    this.loading.set(true);
    this.churchService
      .getMembers(id, this.page(), this.pageSize(), this.query().trim() || null)
      .subscribe({
        next: (page) => {
          this.members.set(page.items);
          this.totalCount.set(page.totalCount);
          this.loading.set(false);
        },
        error: () => {
          this.error.set('Could not load members.');
          this.loading.set(false);
        },
      });
  }

  loadStats() {
    const id = this.churchId();
    if (!id) return;
    this.churchService.getMemberStats(id).subscribe({
      next: (stats) => this.stats.set(stats),
      error: () => this.stats.set(null),
    });
  }

  onSearch(event: Event) {
    const value = (event.target as HTMLInputElement).value;
    this.query.set(value);
    if (this.searchTimer) clearTimeout(this.searchTimer);
    this.searchTimer = setTimeout(() => {
      this.page.set(1);
      this.load();
    }, 300);
  }

  /** Downloads the currently loaded members as a CSV file. */
  exportCsv() {
    const rows = this.members().map((m) => ({
      'EFGBC ID': m.efgbcId,
      'Full name': this.nameOf(m),
      Gender: m.gender,
      Status: m.status,
      Occupation: m.jobStatus,
      Phone: m.phone ?? '',
      Email: m.email ?? '',
      Joined: this.joinedLabel(m),
    }));
    this.downloadCsv(rows.map(Object.keys), rows.map((r) => Object.values(r)));
  }

  private downloadCsv(
    headers: string[][],
    body: (string | number)[][],
  ) {
    const esc = (v: string | number) => {
      const s = String(v ?? '');
      return /[",\n]/.test(s) ? `"${s.replace(/"/g, '""')}"` : s;
    };
    const lines =
      headers[0].map((h, i) => esc(h)).join(',') +
      '\n' +
      body.map((r) => r.map(esc).join(',')).join('\n');
    const blob = new Blob([`\uFEFF${lines}`], {
      type: 'text/csv;charset=utf-8;',
    });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `church-members-${new Date().toISOString().slice(0, 10)}.csv`;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
  }

  goToPage(p: number) {
    if (p < 1 || p > this.totalPages() || p === this.page()) return;
    this.page.set(p);
    this.load();
  }

  changePageSize(event: Event) {
    const size = Number((event.target as HTMLSelectElement).value);
    if (!size) return;
    this.pageSize.set(size);
    this.page.set(1);
    if (this.searchTimer) clearTimeout(this.searchTimer);
    this.load();
  }

  openDetail(member: Member) {
    this.router.navigate(['/church/members', member.id]);
  }

  openCreate() {
    this.router.navigate(['/church/members/register']);
  }

  openClearance() {
    const id = this.churchId();
    if (!id) return;
    this.router.navigate(['/church/clearance']);
  }

  openEdit(member: Member) {
    this.editing.set(member);
    this.showEditModal.set(true);
  }

  closeEdit() {
    this.showEditModal.set(false);
    this.editing.set(null);
  }

  onMemberSaved(updated: Member) {
    this.members.update((list) =>
      list.map((m) => (m.id === updated.id ? updated : m)),
    );
    this.loadStats();
  }

  askDelete(member: Member) {
    this.error.set('');
    this.toDelete.set(member);
  }

  cancelDelete() {
    this.toDelete.set(null);
  }

  confirmDelete() {
    const target = this.toDelete();
    if (!target || this.deleting()) return;
    this.deleting.set(true);
    this.error.set('');

    const shot = this.members();
    const index = shot.findIndex((m) => m.id === target.id);
    this.members.set(shot.filter((m) => m.id !== target.id));

    this.churchService.deleteMember(target.id).subscribe({
      next: () => {
        this.deleting.set(false);
        this.toDelete.set(null);
        this.totalCount.set(Math.max(0, this.totalCount() - 1));
        if (this.members().length === 0 && this.page() > 1) {
          this.page.set(this.page() - 1);
          this.load();
        }
        this.loadStats();
      },
      error: (err) => {
        this.deleting.set(false);
        this.members.set(
          index >= 0
            ? [...this.members().slice(0, index), target, ...this.members().slice(index)]
            : this.members(),
        );
        this.error.set(problemDetail(err, 'Could not remove the member.'));
      },
    });
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

  joinedLabel(member: Member): string {
    if (!member.joinedAt) return '—';
    const date = new Date(`${member.joinedAt}T00:00:00`);
    return isNaN(date.getTime())
      ? member.joinedAt
      : date.toLocaleDateString();
  }
}