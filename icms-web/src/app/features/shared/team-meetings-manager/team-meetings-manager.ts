import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  input,
  OnInit,
  signal,
} from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatIcon } from '@angular/material/icon';
import { MatIconButton } from '@angular/material/button';
import {
  MatError,
  MatFormField,
  MatLabel,
} from '@angular/material/form-field';
import { MatInput } from '@angular/material/input';

import { ChurchService } from '../../../services/church.service';
import {
  TeamMember,
  TeamMeetingDto,
  TeamMeetingDetailDto,
  TeamAttendanceStatus,
} from '../../../models/team';
import {
  problemDetail,
  applyFieldErrors,
  clearFieldErrors,
} from '../../../common/http-errors';
import { toLocalDate } from '../../../common/dates';

@Component({
  selector: 'app-team-meetings-manager',
  imports: [
    ReactiveFormsModule,
    MatFormField,
    MatLabel,
    MatError,
    MatInput,
    MatIcon,
    MatIconButton,
  ],
  templateUrl: './team-meetings-manager.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TeamMeetingsManager implements OnInit {
  readonly churchId = input.required<number>();
  readonly teamId = input.required<number>();

  private service = inject(ChurchService);
  private fb = inject(FormBuilder);

  readonly meetings = signal<TeamMeetingDto[]>([]);
  readonly roster = signal<TeamMember[]>([]);

  readonly loading = signal(false);
  readonly busy = signal(false);
  readonly error = signal('');
  readonly notice = signal('');

  // create form
  readonly showForm = signal(false);
  meetingForm = this.fb.nonNullable.group({
    meetingDate: ['', Validators.required],
    title: ['', Validators.maxLength(200)],
    notes: ['', Validators.maxLength(500)],
  });

  // record/edit attendance for an expanded meeting
  readonly openMeeting = signal<TeamMeetingDetailDto | null>(null);
  readonly markings = signal<Record<number, TeamAttendanceStatus>>({});
  readonly reasons = signal<Record<number, string>>({});

  readonly StatusOptions: { id: TeamAttendanceStatus; code: string }[] = [
    { id: 'Present', code: 'P' },
    { id: 'Late', code: 'L' },
    { id: 'Absent', code: 'A' },
  ];

  readonly hasRoster = computed(() => this.roster().length > 0);

  get today(): string {
    const d = new Date();
    const y = d.getFullYear();
    const m = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');
    return `${y}-${m}-${day}`;
  }

  ngOnInit(): void {
    this.load();
  }

  load() {
    const churchId = this.churchId();
    const teamId = this.teamId();
    if (!churchId || !teamId) return;
    this.loading.set(true);
    this.error.set('');
    this.service.getMeetings(churchId, teamId, 1, 100).subscribe({
      next: (page) => {
        this.meetings.set(page.items);
        this.loading.set(false);
      },
      error: (err) => {
        this.loading.set(false);
        this.error.set(problemDetail(err, 'Could not load meetings.'));
      },
    });
    this.service.getTeamMembers(churchId, teamId, 1, 100).subscribe({
      next: (page) => this.roster.set(page.items),
    });
  }

  toggleForm() {
    if (this.busy()) return;
    this.showForm.update((v) => !v);
    if (this.showForm()) {
      this.resetForm();
    }
    this.notice.set('');
  }

  private resetForm() {
    clearFieldErrors(this.meetingForm);
    this.meetingForm.reset({
      meetingDate: this.today,
      title: '',
      notes: '',
    });
  }

  createMeeting() {
    const churchId = this.churchId();
    const teamId = this.teamId();
    if (!churchId || !teamId || this.meetingForm.invalid || this.busy()) return;

    clearFieldErrors(this.meetingForm);
    this.busy.set(true);
    this.error.set('');
    this.notice.set('');

    const v = this.meetingForm.getRawValue();
    this.service
      .createMeeting(churchId, teamId, {
        meetingDate: v.meetingDate,
        title: v.title?.trim() || null,
        notes: v.notes?.trim() || null,
      })
      .subscribe({
        next: (meeting) => {
          this.busy.set(false);
          this.meetings.update((list) => [meeting, ...list]);
          this.showForm.set(false);
          this.notice.set(`Created meeting for ${this.formatDate(v.meetingDate)}.`);
        },
        error: (err: HttpErrorResponse) => {
          this.busy.set(false);
          if (!applyFieldErrors(this.meetingForm, err)) {
            this.error.set(problemDetail(err, 'Could not create the meeting.'));
          }
        },
      });
  }

  toggleMeeting(meeting: TeamMeetingDto) {
    if (this.openMeeting()?.id === meeting.id) {
      this.openMeeting.set(null);
      return;
    }
    const churchId = this.churchId();
    const teamId = this.teamId();
    if (!churchId || !teamId) return;
    this.openMeeting.set(null);
    this.service.getMeeting(churchId, teamId, meeting.id).subscribe({
      next: (detail) => {
        this.openMeeting.set(detail);
        this.applyMarkings(detail);
      },
      error: (err) => {
        this.error.set(problemDetail(err, 'Could not load the meeting.'));
      },
    });
  }

  mark(memberId: number, status: TeamAttendanceStatus) {
    this.markings.update((map) => ({ ...map, [memberId]: status }));
  }

  statusOf(memberId: number): TeamAttendanceStatus {
    return this.markings()[memberId] ?? 'Present';
  }

  reasonOf(memberId: number): string {
    return this.reasons()[memberId] ?? '';
  }

  onReason(memberId: number, event: Event) {
    const value = (event.target as HTMLInputElement).value;
    this.reasons.update((map) => ({ ...map, [memberId]: value }));
  }

  saveAttendance() {
    const churchId = this.churchId();
    const teamId = this.teamId();
    const meeting = this.openMeeting();
    if (!churchId || !teamId || !meeting || this.busy()) return;
    this.busy.set(true);
    this.error.set('');
    this.notice.set('');
    const entries = this.roster()
      .map((member) => ({
        memberId: member.memberId,
        status: this.statusOf(member.memberId),
        reason: this.reasonOf(member.memberId).trim() || null,
      }))
      .filter((e) => e.status);
    this.service
      .saveMeetingAttendance(churchId, teamId, meeting.id, {
        meetingId: meeting.id,
        entries,
      })
      .subscribe({
        next: (detail) => {
          this.busy.set(false);
          this.openMeeting.set(detail);
          this.applyMarkings(detail);
          this.notice.set('Attendance saved.');
          this.refreshCounts();
        },
        error: (err) => {
          this.busy.set(false);
          this.error.set(problemDetail(err, 'Could not save attendance.'));
        },
      });
  }

  deleteMeeting(meeting: TeamMeetingDto) {
    const churchId = this.churchId();
    const teamId = this.teamId();
    const message =
      'Delete this meeting and ALL its attendance? This cannot be undone.';
    if (!churchId || !teamId || !window.confirm(message)) return;
    this.busy.set(true);
    this.error.set('');
    this.service.deleteMeeting(churchId, teamId, meeting.id).subscribe({
      next: () => {
        this.busy.set(false);
        this.meetings.update((list) =>
          list.filter((m) => m.id !== meeting.id),
        );
        if (this.openMeeting()?.id === meeting.id) this.openMeeting.set(null);
        this.notice.set('Meeting deleted.');
      },
      error: (err) => {
        this.busy.set(false);
        this.error.set(problemDetail(err, 'Could not delete the meeting.'));
      },
    });
  }

  private applyMarkings(detail: TeamMeetingDetailDto) {
    const map: Record<number, TeamAttendanceStatus> = {};
    const reasons: Record<number, string> = {};
    for (const row of detail.attendance) {
      if (row.status) map[row.memberId] = row.status;
      reasons[row.memberId] = row.reason ?? '';
    }
    // default unmarked roster members to Present for easy submission
    for (const member of this.roster()) {
      if (!(member.memberId in map)) map[member.memberId] = 'Present';
    }
    this.markings.set(map);
    this.reasons.set(reasons);
  }

  private refreshCounts() {
    const churchId = this.churchId();
    const teamId = this.teamId();
    if (!churchId || !teamId) return;
    this.service.getMeetings(churchId, teamId, 1, 100).subscribe({
      next: (page) => this.meetings.set(page.items),
    });
  }

  formatDate(value: string): string {
    const date = toLocalDate(value);
    if (!date) return value;
    return date.toLocaleDateString(undefined, {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
    });
  }

  badgeClass(status: TeamAttendanceStatus): string {
    switch (status) {
      case 'Present':
        return 'bg-emerald-100 text-emerald-700 dark:bg-emerald-500/15 dark:text-emerald-300';
      case 'Late':
        return 'bg-amber-100 text-amber-700 dark:bg-amber-500/15 dark:text-amber-300';
      case 'Absent':
        return 'bg-red-100 text-red-700 dark:bg-red-500/15 dark:text-red-300';
    }
  }
}