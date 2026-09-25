import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { MatIcon } from '@angular/material/icon';

import { MemberStore } from '../../../stores/member.store';
import { AttendanceStatusValue, MemberAttendanceHistory } from '../../../models/member';
import { formatDateValue } from '../../../common/dates';
import { PageHeader } from '../../shared/ui/page-header/page-header';
import { StatCard } from '../../shared/ui/stat-card/stat-card';
import { AppTable } from '../../shared/ui/data-table/data-table';
import { TableColumn } from '../../shared/ui/data-table/table-column';

@Component({
  selector: 'app-my-attendance',
  imports: [MatIcon, DecimalPipe, PageHeader, StatCard, AppTable, TableColumn],
  templateUrl: './my-attendance.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MyAttendance {
  readonly store = inject(MemberStore);

  readonly records = computed(() => this.store.history()?.attendance ?? []);
  readonly total = computed(() => this.records().length);
  readonly present = computed(
    () => this.records().filter((r) => r.status === 'Present').length,
  );
  readonly late = computed(
    () => this.records().filter((r) => r.status === 'Late').length,
  );
  readonly absent = computed(
    () => this.records().filter((r) => r.status === 'Absent').length,
  );

  constructor() {
    this.store.loadHistory();
  }

  badgeClass(status: AttendanceStatusValue): string {
    switch (status) {
      case 'Present':
        return 'bg-emerald-100 text-emerald-700 dark:bg-emerald-500/15 dark:text-emerald-300';
      case 'Late':
        return 'bg-amber-100 text-amber-700 dark:bg-amber-500/15 dark:text-amber-300';
      case 'Absent':
        return 'bg-red-100 text-red-700 dark:bg-red-500/15 dark:text-red-300';
    }
  }

  formatDate(value: string): string {
    return formatDateValue(value);
  }

  rowKey = (_: unknown, index: number) => index;

  trackRecord(_: number, r: MemberAttendanceHistory): string {
    return `${r.teamId}-${r.attendanceDate}`;
  }
}