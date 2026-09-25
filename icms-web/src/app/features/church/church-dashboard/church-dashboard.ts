import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatIcon } from '@angular/material/icon';

import { ChurchWorkspaceStore } from '../../../stores/church-workspace.store';
import { StatCard } from '../../shared/ui/stat-card/stat-card';
import { ChartCard } from '../../shared/ui/chart-card/chart-card';
import { SectionCard } from '../../shared/ui/section-card/section-card';

interface ModuleCard {
  icon: string;
  title: string;
  description: string;
  path: string;
}

@Component({
  selector: 'app-church-dashboard',
  imports: [MatIcon, RouterLink, StatCard, ChartCard, SectionCard, DecimalPipe],
  templateUrl: './church-dashboard.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ChurchDashboard {
  readonly store = inject(ChurchWorkspaceStore);

  readonly modules = signal<ModuleCard[]>([]);

  /** Donut: the church "by the numbers" across main modules. */
  readonly overviewLabels = computed(() => [
    'Teams',
    'Employees',
    'Departments',
    'Accounts',
    'Daughter churches',
  ]);
  readonly overviewSeries = computed(() => [
    { name: 'Count', data: [this.store.teamCount(), this.store.employeeCount(), this.store.departmentCount(), this.store.accountCount(), this.store.daughterCount()] },
  ]);

  /** Bar: main teams vs their sub-team structure. */
  readonly teamCategories = computed(() =>
    this.store.mainTeams().map((t) => t.name),
  );
  readonly teamSeries = computed(() => [
    {
      name: 'Sub-teams',
      data: this.store.mainTeams().map(
        (t) =>
          this.store.teams().filter((child) => child.parentTeamId === t.id)
            .length,
      ),
    },
  ]);

  constructor() {
    this.modules.set([
      {
        icon: 'church', title: 'Church Profile', description: 'Your church’s registered details and contact.',
        path: '/church/profile',
      },
      {
        icon: 'group', title: 'Members', description: 'Everyone registered under your church.',
        path: '/church/members',
      },
      {
        icon: 'diversity_3', title: 'Teams', description: 'Choir, worship and ministry teams with attendance and giving.',
        path: '/church/teams',
      },
      {
        icon: 'work', title: 'Employees', description: 'Church office staff and their payroll badges.',
        path: '/church/employees',
      },
      {
        icon: 'account_balance', title: 'Departments', description: 'Office units that run the church’s work.',
        path: '/church/departments',
      },
      {
        icon: 'manage_accounts', title: 'Accounts', description: 'Account holders and church administrator roles.',
        path: '/church/accounts',
      },
    ]);
    this.store.load();
  }

  location(city: string | null, subcity: string | null): string {
    return [city, subcity].filter(Boolean).join(', ');
  }
}