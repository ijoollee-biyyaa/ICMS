import { ChangeDetectionStrategy, Component, inject, OnInit } from '@angular/core';
import { DistrictStore } from '../../../stores/district.store';
import { AuthService } from '../../../services/auth.service';
import { SidebarService } from '../../shared/services/sidebar.service';
import { AppLayoutComponent } from '../../shared/layout/app-layout/app-layout.component';

@Component({
  selector: 'app-district-shell',
  imports: [AppLayoutComponent],
  template: `<app-layout></app-layout>`,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DistrictShell implements OnInit {
  readonly store = inject(DistrictStore);
  private readonly auth = inject(AuthService);
  private sidebarService = inject(SidebarService);

  constructor() {
    this.store.load();
  }

  ngOnInit() {
    const isAdmin = this.auth.hasRole('Admin');

    const navItems = [
      { name: 'Dashboard', materialIcon: 'dashboard', path: '/district/dashboard' },
      { name: 'District Profile', materialIcon: 'badge', path: '/district/profile' },
      { name: 'Churches', materialIcon: 'church', path: '/district/churches' },
      { name: 'Ministers', materialIcon: 'local_church', path: '/district/ministers' },
    ];

    const othersItems = [
      { name: 'Office Employees', materialIcon: 'work', path: '/district/employees' },
      { name: 'Departments', materialIcon: 'account_balance', path: '/district/departments' },
      { name: 'Payments', materialIcon: 'payments', path: '/district/payments' },
      { name: 'Reports', materialIcon: 'bar_chart', path: '/district/reports' },
    ];

    if (isAdmin) {
      othersItems.push(
        { name: 'Accounts', materialIcon: 'manage_accounts', path: '/district/accounts' },
        { name: 'Settings', materialIcon: 'settings', path: '/district/settings' },
        { name: 'Create District', materialIcon: 'add_business', path: '/district/create' }
      );
    }

    if (this.auth.homes().length > 1) {
      othersItems.push({ name: 'Switch Area', materialIcon: 'swap_horiz', path: '/areas' });
    }

    this.sidebarService.setNavItems(navItems, othersItems);
  }
}
