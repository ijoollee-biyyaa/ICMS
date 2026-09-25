import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatIcon } from '@angular/material/icon';

import { DistrictStore } from '../../../stores/district.store';

interface ModuleCard {
  icon: string;
  title: string;
  description: string;
  path: string;
}

@Component({
  selector: 'app-dashboard',
  imports: [MatIcon, DatePipe, RouterLink],
  templateUrl: './dashboard.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DistrictDashboard {
  readonly store = inject(DistrictStore);

  readonly modules = signal<ModuleCard[]>([
    {
      icon: 'church', title: 'Churches', description: 'Churches, departments and church employees.',
      path: '/district/churches',
    },
    {
      icon: 'work', title: 'Office Employees', description: 'The district office team and executives.',
      path: '/district/employees',
    },
    {
      icon: 'account_balance', title: 'Departments', description: 'Office units running the district’s work.',
      path: '/district/departments',
    },
    {
      icon: 'local_church', title: 'Ministers', description: 'Full-time ministers across the district.',
      path: '/district/ministers',
    },
    {
      icon: 'payments', title: 'Payments', description: 'Tithes, offerings and salary payments.',
      path: '/district/payments',
    },
    {
      icon: 'manage_accounts', title: 'Accounts', description: 'Accounts for staff, ministers and members.',
      path: '/district/accounts',
    },
    {
      icon: 'bar_chart', title: 'Reports', description: 'Complex district reports and analytics.',
      path: '/district/reports',
    },
  ]);
}
