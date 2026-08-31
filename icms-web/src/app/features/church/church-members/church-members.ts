import {
  Component,
  ChangeDetectionStrategy,
  computed,
  inject,
  signal,
} from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { MatIconButton } from '@angular/material/button';
import {
  MatError,
  MatFormField,
  MatHint,
  MatLabel,
} from '@angular/material/form-field';
import { MatInput } from '@angular/material/input';
import { MatOption, MatSelect } from '@angular/material/select';
import { MatIcon } from '@angular/material/icon';

import { AuthService } from '../../../services/auth.service';
import { ChurchService } from '../../../services/church.service';
import {
  Member,
  MemberStats,
  CreateMemberRequest,
  JobStatus,
  Gender,
  JoinChannel,
} from '../../../models/member';
import { fullName, initials } from '../../../common/member-names';
import {
  problemDetail,
  applyFieldErrors,
  clearFieldErrors,
  dateOrNull,
} from '../../../common/http-errors';

@Component({
  selector: 'app-church-members',
  imports: [
    ReactiveFormsModule,
    MatFormField,
    MatLabel,
    MatHint,
    MatError,
    MatInput,
    MatSelect,
    MatOption,
    MatIconButton,
    MatIcon,
  ],
  templateUrl: './church-members.html',
  styleUrl: './church-members.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ChurchMembers {
  private auth = inject(AuthService);
  private fb = inject(FormBuilder);
  private churchService = inject(ChurchService);
  private router = inject(Router);

  readonly churchId = computed(() => this.auth.currentUser()?.churchId ?? null);

  readonly members = signal<Member[]>([]);
  readonly stats = signal<MemberStats | null>(null);
  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly deleting = signal(false);
  readonly error = signal('');
  readonly showForm = signal(false);
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

  form = this.fb.nonNullable.group({
    firstName: ['', Validators.required],
    fatherName: ['', Validators.required],
    grandfatherName: ['', Validators.required],
    gender: ['Male' as Gender, Validators.required],
    jobStatus: ['Employed' as JobStatus, Validators.required],
    joinedVia: ['Baptism' as JoinChannel, Validators.required],
    dateOfBirth: [''],
    phone: [''],
    email: [''],
  });

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
    this.loadStats();
    this.load();
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
    this.error.set('');
    this.editing.set(null);
    this.showForm.set(true);
    this.form.reset({
      firstName: '',
      fatherName: '',
      grandfatherName: '',
      gender: 'Male',
      jobStatus: 'Employed',
      joinedVia: 'Baptism',
      dateOfBirth: '',
      phone: '',
      email: '',
    });
    clearFieldErrors(this.form);
  }

  openEdit(member: Member) {
    this.error.set('');
    this.editing.set(member);
    this.showForm.set(true);
    this.form.reset({
      firstName: member.firstName,
      fatherName: member.fatherName,
      grandfatherName: member.grandfatherName,
      gender: member.gender,
      jobStatus: member.jobStatus,
      joinedVia: member.joinedVia,
      dateOfBirth: member.dateOfBirth ?? '',
      phone: member.phone ?? '',
      email: member.email ?? '',
    });
    clearFieldErrors(this.form);
  }

  closeForm() {
    if (this.saving()) return;
    this.showForm.set(false);
    this.editing.set(null);
  }

  submit() {
    const churchId = this.churchId();
    if (!churchId || this.form.invalid || this.saving()) return;

    clearFieldErrors(this.form);
    this.saving.set(true);
    this.error.set('');

    const v = this.form.getRawValue();
    const editing = this.editing();
    const common = {
      firstName: v.firstName,
      fatherName: v.fatherName,
      grandfatherName: v.grandfatherName,
      gender: v.gender,
      jobStatus: v.jobStatus,
      dateOfBirth: dateOrNull(v.dateOfBirth),
      phone: v.phone || null,
      email: v.email || null,
      photoUrl: editing?.photoUrl ?? null,
    };

    if (editing) {
      this.churchService.updateMember(editing.id, common).subscribe({
        next: (updated) => this.handleSaved(updated, true),
        error: (err) => {
          this.saving.set(false);
          if (!this.applyServerErrors(err)) {
            this.error.set(
              problemDetail(err, 'Could not update the member.'),
            );
          }
        },
      });
      return;
    }

    const body: CreateMemberRequest = {
      ...common,
      churchId,
      joinedVia: v.joinedVia,
      joinedAt: null,
    };
    this.churchService.createMember(body).subscribe({
      next: (created) => this.handleSaved(created, false),
      error: (err) => {
        this.saving.set(false);
        if (!this.applyServerErrors(err)) {
          this.error.set(problemDetail(err, 'Could not register the member.'));
        }
      },
    });
  }

  private handleSaved(member: Member, editing: boolean) {
    this.saving.set(false);
    this.showForm.set(false);
    this.editing.set(null);
    if (editing) {
      this.members.set(
        this.members().map((m) => (m.id === member.id ? member : m)),
      );
    } else if (
      this.members().length < this.pageSize() ||
      this.page() === this.totalPages()
    ) {
      this.members.set([...this.members(), member]);
      this.totalCount.set(this.totalCount() + 1);
    } else {
      this.load();
    }
    this.loadStats();
  }

  private applyServerErrors(err: unknown): boolean {
    const response = err as {
      status?: number;
      error?: { errors?: Record<string, string[]> };
    };
    if (
      response?.status !== 400 ||
      !response.error?.errors
    ) {
      return false;
    }
    return applyFieldErrors(
      this.form,
      err as Parameters<typeof applyFieldErrors>[1],
    );
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