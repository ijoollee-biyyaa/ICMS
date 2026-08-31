import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  signal,
} from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MatIconButton } from '@angular/material/button';
import {
  MatError,
  MatFormField,
  MatLabel,
} from '@angular/material/form-field';
import { MatInput } from '@angular/material/input';
import { MatOption, MatSelect } from '@angular/material/select';
import { MatIcon } from '@angular/material/icon';
import { HttpErrorResponse } from '@angular/common/http';

import { AuthService } from '../../../../services/auth.service';
import { ChurchService } from '../../../../services/church.service';
import {
  TeamDetail as TeamDetailDto,
  TeamMember,
  TeamMemberRole,
  TeamAttendanceStatus,
  TeamAttendanceReport,
  TeamAttendanceSummary,
  TeamPayment,
  TeamPaymentSummary,
  SaveAttendanceResult,
} from '../../../../models/team';
import { Member } from '../../../../models/member';
import {
  problemDetail,
  clearFieldErrors,
  dateOrNull,
  firstFieldError,
} from '../../../../common/http-errors';

type Tab = 'members' | 'attendance' | 'payments';

@Component({
  selector: 'app-team-detail',
  imports: [
    ReactiveFormsModule,
    MatFormField,
    MatLabel,
    MatError,
    MatInput,
    MatSelect,
    MatOption,
    MatIconButton,
    MatIcon,
    RouterLink,
  ],
  templateUrl: './team-detail.html',
  styleUrl: './team-detail.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TeamDetailPage {
  private auth = inject(AuthService);
  private fb = inject(FormBuilder);
  private route = inject(ActivatedRoute);
  private churchService = inject(ChurchService);

  readonly churchId = computed(() => this.auth.currentUser()?.churchId ?? null);
  readonly teamId = Number(this.route.snapshot.paramMap.get('id'));

  readonly active = signal<Tab>('members');

  readonly Tabs: { id: Tab; label: string; icon: string }[] = [
    { id: 'members', label: 'Members', icon: 'group' },
    { id: 'attendance', label: 'Attendance', icon: 'fact_check' },
    { id: 'payments', label: 'Payments', icon: 'payments' },
  ];

  readonly team = signal<TeamDetailDto | null>(null);
  readonly members = signal<TeamMember[]>([]);
  readonly roster = signal<Member[]>([]);
  readonly report = signal<TeamAttendanceReport | null>(null);
  readonly payments = signal<TeamPayment[]>([]);
  readonly paymentSummary = signal<TeamPaymentSummary[]>([]);

  readonly loading = signal(false);
  readonly busy = signal(false);
  readonly error = signal('');
  readonly notice = signal('');

  readonly attendanceDate = signal('');
  readonly markings = signal<Record<number, TeamAttendanceStatus>>({});

  readonly editingPayment = signal<number | null>(null);
  readonly editingAmount = signal('');

  addMemberForm = this.fb.nonNullable.group({
    memberId: ['' as number | ''],
  });

  paymentForm = this.fb.nonNullable.group({
    memberId: ['' as number | '', Validators.required],
    month: ['', Validators.required],
    amount: [null as number | null, [Validators.required, Validators.min(0.01)]],
  });

  readonly isCategory = computed(
    () => (this.team()?.subTeamCount ?? 0) > 0,
  );

  readonly availableRoster = computed(() => {
    const inTeam = new Set(this.members().map((m) => m.memberId));
    return this.roster().filter((m) => !inTeam.has(m.id));
  });

  readonly attendable = computed(() => {
    if (this.isCategory()) return [];
    return this.members();
  });

  get today(): string {
    const d = new Date();
    const y = d.getFullYear();
    const m = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');
    return `${y}-${m}-${day}`;
  }

  readonly StatusOptions: { id: TeamAttendanceStatus; code: string }[] = [
    { id: 'Present', code: 'P' },
    { id: 'Late', code: 'L' },
    { id: 'Absent', code: 'A' },
  ];

  readonly paymentTotal = computed(() =>
    this.paymentSummary().reduce((sum, p) => sum + p.totalAmount, 0),
  );

  constructor() {
    this.load();
  }

  load() {
    const churchId = this.churchId();
    if (!churchId || !this.teamId) return;
    this.loading.set(true);
    this.error.set('');

    this.churchService.getTeam(churchId, this.teamId).subscribe({
      next: (team) => {
        this.team.set(team);
        this.loadDetails();
      },
      error: (err) => {
        this.loading.set(false);
        this.error.set(problemDetail(err, 'Could not load the team.'));
      },
    });
  }

  private loadDetails() {
    const churchId = this.churchId();
    if (!churchId) return;

    this.churchService.getTeamMembers(churchId, this.teamId, 1, 100).subscribe({
      next: (page) => {
        this.members.set(page.items);
        this.resetMarkings(page.items);
      },
      error: (err) => {
        this.error.set(problemDetail(err, 'Could not load team members.'));
      },
    });

    this.churchService.getMembers(churchId, 1, 100).subscribe({
      next: (page) => this.roster.set(page.items),
    });

    this.churchService
      .getAttendanceSummary(churchId, this.teamId, null, null)
      .subscribe({
        next: (report) =>
          this.report.set(this.isCategory() ? null : report),
        error: () => this.report.set(null),
      });

    this.churchService.getPaymentSummary(churchId, this.teamId).subscribe({
      next: (summary) => this.paymentSummary.set(summary),
    });

    this.churchService
      .getPayments(churchId, this.teamId, null, null, 1, 100)
      .subscribe({
        next: (page) => this.payments.set(page.items),
      });

    this.loading.set(false);
  }

  private resetMarkings(members: TeamMember[]) {
    const map: Record<number, TeamAttendanceStatus> = {};
    for (const m of members) map[m.memberId] = 'Present';
    this.markings.set(map);
  }

  setActive(tab: Tab) {
    this.active.set(tab);
  }

  onDate(event: Event) {
    this.attendanceDate.set((event.target as HTMLInputElement).value);
  }

  onEditAmount(event: Event) {
    this.editingAmount.set((event.target as HTMLInputElement).value);
  }

  ruleLabel(rule: string): string {
    return rule === 'Multiple'
      ? 'Multiple sub-teams'
      : 'Single sub-team';
  }

  initials(name: string): string {
    const parts = name.trim().split(/\s+/).slice(0, 2);
    if (parts.length === 0) return '?';
    return parts
      .map((part) => part[0])
      .join('')
      .toUpperCase();
  }

  mark(memberId: number, status: TeamAttendanceStatus) {
    this.markings.update((map) => ({ ...map, [memberId]: status }));
  }

  statusOf(memberId: number): TeamAttendanceStatus {
    return this.markings()[memberId] ?? 'Present';
  }

  // ---- members -----------------------------------------------------------

  addMember() {
    const churchId = this.churchId();
    const memberId = this.addMemberForm.controls.memberId.value;
    if (!churchId || !memberId || this.busy()) return;
    this.busy.set(true);
    this.error.set('');
    this.notice.set('');
    this.churchService.joinTeam(churchId, this.teamId, memberId).subscribe({
      next: (membership) => {
        this.busy.set(false);
        this.members.update((list) => [...list, membership]);
        this.markings.update((map) => ({ ...map, [membership.memberId]: 'Present' }));
        this.addMemberForm.reset({ memberId: '' });
        this.notice.set(`${membership.memberName} joined the team.`);
      },
      error: (err) => {
        this.busy.set(false);
        this.error.set(problemDetail(err, 'Could not add the member.'));
      },
    });
  }

  toggleRole(member: TeamMember) {
    const churchId = this.churchId();
    if (!churchId || this.busy()) return;
    const role: TeamMemberRole =
      member.role === 'Leader' ? 'Member' : 'Leader';
    this.busy.set(true);
    this.error.set('');
    this.churchService
      .setTeamMemberRole(churchId, this.teamId, member.memberId, role)
      .subscribe({
        next: (updated) => {
          this.busy.set(false);
          this.members.set(
            this.members().map((m) =>
              m.memberId === updated.memberId ? updated : m,
            ),
          );
        },
        error: (err) => {
          this.busy.set(false);
          this.error.set(problemDetail(err, 'Could not change the role.'));
        },
      });
  }

  removeMember(member: TeamMember) {
    const churchId = this.churchId();
    if (!churchId || this.busy()) return;
    this.busy.set(true);
    this.error.set('');
    this.churchService
      .removeTeamMember(churchId, this.teamId, member.memberId)
      .subscribe({
        next: () => {
          this.busy.set(false);
          this.members.set(
            this.members().filter((m) => m.memberId !== member.memberId),
          );
          this.markings.update((map) => {
            const next = { ...map };
            delete next[member.memberId];
            return next;
          });
          this.notice.set(`${member.memberName} was removed from the team.`);
        },
        error: (err) => {
          this.busy.set(false);
          this.error.set(problemDetail(err, 'Could not remove the member.'));
        },
      });
  }

  // ---- attendance --------------------------------------------------------

  saveAttendance() {
    const churchId = this.churchId();
    const date = dateOrNull(this.attendanceDate());
    if (!churchId || !date || this.busy()) return;

    this.busy.set(true);
    this.error.set('');
    this.notice.set('');
    const entries = this.attendable().map((m) => ({
      memberId: m.memberId,
      status: this.statusOf(m.memberId),
      reason: null,
    }));
    this.churchService
      .saveAttendance(churchId, this.teamId, { attendanceDate: date, entries })
      .subscribe({
        next: (saved: SaveAttendanceResult) => {
          this.busy.set(false);
          this.notice.set(
            `Marked ${saved.totalMarked} on ${saved.attendanceDate}: ` +
              `${saved.presentCount} present, ${saved.lateCount} late, ${saved.absentCount} absent.`,
          );
          this.attendanceDate.set('');
          this.churchService
            .getAttendanceSummary(churchId, this.teamId, null, null)
            .subscribe({
              next: (report) => this.report.set(report),
            });
        },
        error: (err: HttpErrorResponse) => {
          this.busy.set(false);
          const message = firstFieldError(err);
          this.error.set(
            message ?? problemDetail(err, 'Could not save attendance.'),
          );
        },
      });
  }

  // ---- payments ----------------------------------------------------------

  recordPayment() {
    const churchId = this.churchId();
    if (!churchId || this.paymentForm.invalid || this.busy()) return;
    clearFieldErrors(this.paymentForm);

    const v = this.paymentForm.getRawValue();
    const month = v.month?.trim() ? `${v.month}-01` : null;
    this.busy.set(true);
    this.error.set('');
    this.notice.set('');
    this.churchService
      .recordPayment(churchId, this.teamId, {
        memberId: typeof v.memberId === 'number' ? v.memberId : null,
        month,
        amount: v.amount,
      })
      .subscribe({
        next: (payment) => {
          this.busy.set(false);
          this.payments.update((list) => [payment, ...list]);
          this.paymentForm.reset({ memberId: '', month: '', amount: null });
          this.notice.set(
            `Registered ${this.amountText(payment.amount)} for ${payment.memberName}.`,
          );
          this.refreshPayments();
        },
        error: (err: HttpErrorResponse) => {
          this.busy.set(false);
          const message = firstFieldError(err);
          this.error.set(
            message ?? problemDetail(err, 'Could not record the payment.'),
          );
        },
      });
  }

  startEdit(payment: TeamPayment) {
    this.editingPayment.set(payment.id);
    this.editingAmount.set(String(payment.amount));
  }

  cancelEdit() {
    this.editingPayment.set(null);
    this.editingAmount.set('');
  }

  saveEdit(payment: TeamPayment) {
    const churchId = this.churchId();
    const amount = Number(this.editingAmount());
    if (!churchId || !amount || this.busy()) return;
    this.busy.set(true);
    this.error.set('');
    this.churchService
      .updatePayment(churchId, this.teamId, payment.id, { amount })
      .subscribe({
        next: (updated) => {
          this.busy.set(false);
          this.payments.set(
            this.payments().map((p) => (p.id === updated.id ? updated : p)),
          );
          this.editingPayment.set(null);
          this.refreshPayments();
        },
        error: (err) => {
          this.busy.set(false);
          this.error.set(problemDetail(err, 'Could not update the payment.'));
        },
      });
  }

  private refreshPayments() {
    const churchId = this.churchId();
    if (!churchId) return;
    this.churchService.getPaymentSummary(churchId, this.teamId).subscribe({
      next: (summary) => this.paymentSummary.set(summary),
    });
  }

  // ---- helpers -----------------------------------------------------------

  amountText(amount: number): string {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'ETB',
    }).format(amount);
  }

  monthLabel(month: string): string {
    const [y, m] = month.split('-').map(Number);
    return new Date(y, (m ?? 1) - 1, 1).toLocaleDateString(undefined, {
      month: 'long',
      year: 'numeric',
    });
  }

  rateText(rate: number): string {
    return `${rate.toFixed(1)}%`;
  }

  todayText(): string {
    return this.today;
  }
}