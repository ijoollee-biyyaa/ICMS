import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { MatIcon } from '@angular/material/icon';

import { MemberStore } from '../../../stores/member.store';
import { MemberPaymentHistory } from '../../../models/member';
import { formatDateValue } from '../../../common/dates';
import { PageHeader } from '../../shared/ui/page-header/page-header';
import { StatCard } from '../../shared/ui/stat-card/stat-card';
import { AppTable } from '../../shared/ui/data-table/data-table';
import { TableColumn } from '../../shared/ui/data-table/table-column';

@Component({
  selector: 'app-my-payments',
  imports: [MatIcon, DecimalPipe, PageHeader, StatCard, AppTable, TableColumn],
  templateUrl: './my-payments.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MyPayments {
  readonly store = inject(MemberStore);

  readonly payments = computed(() => this.store.history()?.payments ?? []);
  readonly totalCount = computed(() => this.payments().length);
  readonly totalAmount = computed(() =>
    this.payments().reduce((sum, p) => sum + p.amount, 0),
  );

  constructor() {
    this.store.loadHistory();
  }

  formatMonth(value: string): string {
    return formatDateValue(value, { year: 'numeric', month: 'long' });
  }

  formatPaidAt(value: string): string {
    return new Date(value).toLocaleDateString(undefined, {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
    });
  }

  trackPayment(_: number, p: MemberPaymentHistory): number {
    return p.id;
  }

  rowKey = (_: unknown, index: number) => index;
}