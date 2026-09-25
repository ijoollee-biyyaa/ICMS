import { ChangeDetectionStrategy, Component, inject, OnInit } from '@angular/core';
import { AuthService } from '../../../services/auth.service';
import { ChurchService } from '../../../services/church.service';
import { SidebarService } from '../../shared/services/sidebar.service';
import { AppLayoutComponent } from '../../shared/layout/app-layout/app-layout.component';

@Component({
  selector: 'app-church-shell',
  imports: [AppLayoutComponent],
  template: `<app-layout></app-layout>`,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ChurchShell implements OnInit {
  private auth = inject(AuthService);
  private sidebarService = inject(SidebarService);
  // private churchService = inject(ChurchService);

  ngOnInit() {
    const items = [
      { name: 'Dashboard', materialIcon: 'dashboard', path: '/church/dashboard' },
      { name: 'Church Profile', materialIcon: 'church', path: '/church/profile' },
      { name: 'Members', materialIcon: 'group', path: '/church/members' },
      { name: 'Teams', materialIcon: 'groups', path: '/church/teams' },
      { name: 'Employees', materialIcon: 'badge', path: '/church/employees' },
      { name: 'Departments', materialIcon: 'account_tree', path: '/church/departments' },
      { name: 'Accounts', materialIcon: 'manage_accounts', path: '/church/accounts' },
      { name: 'Settings', materialIcon: 'settings', path: '/church/settings' },
    ];
    
    if (this.auth.homes().length > 1) {
      items.push({ name: 'Switch Area', materialIcon: 'swap_horiz', path: '/areas' });
    }
    
    this.sidebarService.setNavItems(items);
  }
}
