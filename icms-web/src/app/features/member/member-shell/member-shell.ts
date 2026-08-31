import { Component, computed, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { MatIcon } from '@angular/material/icon';
import { MatIconButton } from '@angular/material/button';

import { AuthService } from '../../../services/auth.service';

interface NavItem {
  label: string;
  icon: string;
  path: string;
}

@Component({
  selector: 'app-member-shell',
  imports: [RouterLink, RouterLinkActive, RouterOutlet, MatIcon, MatIconButton],
  templateUrl: './member-shell.html',
  styleUrl: './member-shell.scss',
})
export class MemberShell {
  private auth = inject(AuthService);
  private router = inject(Router);

  readonly currentUser = this.auth.currentUser;
  readonly isAdmin = computed(() => this.auth.hasRole('Admin'));

  readonly nav = computed<NavItem[]>(() => {
    const items: NavItem[] = [
      { label: 'Dashboard', icon: 'dashboard', path: '/member/dashboard' },
    ];
    if (
      this.auth.hasRole('Admin') ||
      this.auth.hasRole('DistrictSubAdmin') ||
      this.auth.hasRole('ChurchAdmin')
    ) {
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
