import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
} from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatIcon } from '@angular/material/icon';

import { MemberStore } from '../../../stores/member.store';

@Component({
  selector: 'app-member-dashboard',
  imports: [MatIcon, DecimalPipe, RouterLink],
  templateUrl: './dashboard.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MemberDashboard {
  readonly store = inject(MemberStore);

  readonly attendanceRateText = computed(() => {
    const rate = this.store.attendanceRate();
    return rate !== null ? rate.toFixed(1) + '%' : '—';
  });

  readonly leaderCount = computed(
    () => this.store.dashboard()?.teams.filter((t) => t.role === 'Leader').length ?? 0,
  );

  constructor() {
    this.store.load();
  }
}