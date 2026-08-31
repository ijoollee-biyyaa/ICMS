import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatIcon } from '@angular/material/icon';

import { ChurchWorkspaceStore } from '../../../stores/church-workspace.store';

interface ModuleCard {
  icon: string;
  title: string;
  description: string;
  path: string;
}

@Component({
  selector: 'app-church-dashboard',
  imports: [MatIcon, RouterLink],
  templateUrl: './church-dashboard.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ChurchDashboard {
  readonly store = inject(ChurchWorkspaceStore);

  readonly modules = signal<ModuleCard[]>([
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

  constructor() {
    this.store.load();
  }

  location(city: string | null, subcity: string | null): string {
    return [city, subcity].filter(Boolean).join(', ');
  }
}