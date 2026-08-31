import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { MatIcon } from '@angular/material/icon';
import { MatIconButton } from '@angular/material/button';

import { DistrictStore } from '../../../stores/district.store';
import { AuthService } from '../../../services/auth.service';

interface NavSection {
  title: string;
  items: NavItem[];
}

interface NavItem {
  label: string;
  icon: string;
  path: string;
  adminOnly?: boolean;
}

@Component({
  selector: 'app-district-shell',
  imports: [RouterLink, RouterLinkActive, RouterOutlet, MatIcon, MatIconButton],
  templateUrl: './district-shell.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DistrictShell {
  readonly store = inject(DistrictStore);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  readonly currentUser = this.auth.currentUser;

  constructor() {
    this.store.load();
  }

  signOut() {
    void this.auth.logout().then(() => this.router.navigate(['/']));
  }

  readonly isAdmin = computed(() => this.auth.hasRole('Admin'));

  private readonly allSections: NavSection[] = [
    {
      title: 'Overview',
      items: [
        { label: 'Dashboard', icon: 'dashboard', path: '/district/dashboard' },
        { label: 'District Profile', icon: 'badge', path: '/district/profile' },
      ],
    },
    {
      title: 'People',
      items: [
        { label: 'Members', icon: 'group', path: '/district/members' },
        { label: 'Churches', icon: 'church', path: '/district/churches' },
        { label: 'Ministers', icon: 'local_church', path: '/district/ministers' },
      ],
    },
    {
      title: 'Office',
      items: [
        { label: 'Office Employees', icon: 'work', path: '/district/employees' },
        { label: 'Departments', icon: 'account_balance', path: '/district/departments' },
        { label: 'Payments', icon: 'payments', path: '/district/payments' },
      ],
    },
    {
      title: 'Management',
      items: [
        { label: 'Accounts', icon: 'manage_accounts', path: '/district/accounts', adminOnly: true },
        { label: 'Settings', icon: 'settings', path: '/district/settings', adminOnly: true },
        { label: 'Reports', icon: 'bar_chart', path: '/district/reports' },
        { label: 'Create District', icon: 'add_business', path: '/district/create', adminOnly: true },
      ],
    },
  ];

  readonly nav = computed<NavSection[]>(() => {
    const sections = this.allSections.map((section) => ({
      ...section,
      items: section.items.filter((item) => !item.adminOnly || this.isAdmin()),
    }));
    if (this.auth.homes().length > 1) {
      sections.push({
        title: 'Areas',
        items: [
          { label: 'Switch Area', icon: 'swap_horiz', path: '/areas' },
        ],
      });
    }
    return sections;
  });
}
