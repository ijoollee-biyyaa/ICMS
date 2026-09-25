import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatIcon } from '@angular/material/icon';

import { MemberStore } from '../../../stores/member.store';
import { PageHeader } from '../../shared/ui/page-header/page-header';

@Component({
  selector: 'app-member-teams',
  imports: [RouterLink, MatIcon, PageHeader],
  templateUrl: './member-teams.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MemberTeams {
  readonly store = inject(MemberStore);

  readonly teams = computed(() => this.store.dashboard()?.teams ?? []);
  readonly isLoading = computed(
    () => this.store.isLoading() && !this.store.loaded(),
  );

  constructor() {
    this.store.load();
  }

  roleBadge(role: string): string {
    return role === 'Leader'
      ? 'bg-brand/10 text-brand dark:bg-brand-500/15 dark:text-brand-300'
      : 'bg-gray-100 text-gray-500 dark:bg-gray-800 dark:text-gray-300';
  }

  canManage(role: string): boolean {
    return role === 'Leader';
  }

  formatRate(rate: number | null): string {
    return rate === null ? '—' : `${rate.toFixed(1)}%`;
  }
}