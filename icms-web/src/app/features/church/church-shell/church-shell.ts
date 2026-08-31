import { Component, computed, inject, signal } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { MatIcon } from '@angular/material/icon';
import { MatIconButton } from '@angular/material/button';

import { AuthService } from '../../../services/auth.service';
import { ChurchService } from '../../../services/church.service';
import { toSignal } from '@angular/core/rxjs-interop';
import { environment } from '../../../../environments/environment';

interface NavItem {
  label: string;
  icon: string;
  path: string;
}

@Component({
  selector: 'app-church-shell',
  imports: [RouterLink, RouterLinkActive, RouterOutlet, MatIcon, MatIconButton],
  templateUrl: './church-shell.html',
  styleUrl: './church-shell.scss',
})
export class ChurchShell {
  private auth = inject(AuthService);
  private router = inject(Router);
  private churchService = inject(ChurchService);

  readonly currentUser = this.auth.currentUser;
  readonly churchId = computed(() => this.auth.currentUser()?.churchId ?? null);

  readonly church =
    this.churchId() !== null
      ? toSignal(
          this.churchService.getChurch(environment.districtId, this.churchId()!),
          { initialValue: null },
        )
      : signal(null);

  readonly nav = computed<NavItem[]>(() => {
    const items: NavItem[] = [
      { label: 'Dashboard', icon: 'dashboard', path: '/church/dashboard' },
      { label: 'Church Profile', icon: 'church', path: '/church/profile' },
      { label: 'Members', icon: 'group', path: '/church/members' },
      { label: 'Teams', icon: 'groups', path: '/church/teams' },
      { label: 'Employees', icon: 'badge', path: '/church/employees' },
      { label: 'Departments', icon: 'account_tree', path: '/church/departments' },
      { label: 'Accounts', icon: 'manage_accounts', path: '/church/accounts' },
      { label: 'Settings', icon: 'settings', path: '/church/settings' },
    ];
    if (this.auth.homes().length > 1) {
      items.push({ label: 'Switch Area', icon: 'swap_horiz', path: '/areas' });
    }
    return items;
  });

  signOut() {
    void this.auth.logout().then(() => {
      this.router.navigate(['/']);
    });
  }
}
