import { ChangeDetectionStrategy, Component, inject, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../../services/auth.service';
import { SidebarService } from '../../shared/services/sidebar.service';
import { AppLayoutComponent } from '../../shared/layout/app-layout/app-layout.component';

@Component({
  selector: 'app-member-shell',
  imports: [AppLayoutComponent],
  template: `<app-layout></app-layout>`,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MemberShell implements OnInit {
  private auth = inject(AuthService);
  private router = inject(Router);
  private sidebarService = inject(SidebarService);

  ngOnInit() {
    const items = [
      { name: 'Dashboard', materialIcon: 'dashboard', path: '/member/dashboard' },
      { name: 'My Profile', materialIcon: 'badge', path: '/member/profile' },
      { name: 'Attendance', materialIcon: 'event_available', path: '/member/attendance' },
      { name: 'Payments', materialIcon: 'payments', path: '/member/payments' },
      { name: 'Teams', materialIcon: 'groups', path: '/member/teams' },
      { name: 'Manage Account', materialIcon: 'manage_accounts', path: '/member/account' },
    ];
    
    if (
      this.auth.hasRole('Admin') ||
      this.auth.hasRole('DistrictSubAdmin') ||
      this.auth.hasRole('ChurchAdmin')
    ) {
      items.push({ name: 'Switch Area', materialIcon: 'swap_horiz', path: '/areas' });
    }
    
    this.sidebarService.setNavItems(items);
  }
}