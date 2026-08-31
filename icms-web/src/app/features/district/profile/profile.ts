import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { DatePipe } from '@angular/common';
import { MatIcon } from '@angular/material/icon';
import { RouterLink } from '@angular/router';

import { DistrictStore } from '../../../stores/district.store';

interface QuickAction {
  icon: string;
  label: string;
  path: string;
}

@Component({
  selector: 'app-district-profile',
  imports: [MatIcon, RouterLink, DatePipe],
  templateUrl: './profile.html',
  styleUrl: './profile.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DistrictProfile {
  readonly store = inject(DistrictStore);

  quickActions: QuickAction[] = [
    { icon: 'settings', label: 'Settings', path: '/district/settings' },
    { icon: 'manage_accounts', label: 'Accounts', path: '/district/accounts' },
    { icon: 'bar_chart', label: 'Reports', path: '/district/reports' },
  ];
}