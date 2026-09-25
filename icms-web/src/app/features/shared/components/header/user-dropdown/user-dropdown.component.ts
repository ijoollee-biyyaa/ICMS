import { Component, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { MatIcon } from '@angular/material/icon';

import { AuthService } from '../../../../../services/auth.service';
import { DropdownComponent } from '../../ui/dropdown/dropdown.component';

@Component({
  selector: 'app-user-dropdown',
  templateUrl: './user-dropdown.component.html',
  imports: [CommonModule, RouterModule, DropdownComponent, MatIcon],
})
export class UserDropdownComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  isOpen = false;

  readonly currentUser = this.auth.currentUser;
  readonly displayName = computed(() => this.currentUser()?.displayName?.trim() || 'User');
  readonly email = computed(() => this.currentUser()?.email || '');
  readonly role = computed(() => this.currentUser()?.role || '');

  readonly initials = computed(() => {
    const name = this.displayName();
    const parts = name.split(/\s+/).filter(Boolean);
    if (!parts.length) return 'U';
    return ((parts[0]?.[0] ?? '') + (parts[1]?.[0] ?? '')).toUpperCase();
  });

  readonly profileLink = computed(() => {
    const url = this.router.url;
    if (url.startsWith('/district')) return '/district/profile';
    if (url.startsWith('/church')) return '/church/profile';
    return '/member/profile';
  });

  readonly settingsLink = computed(() => {
    const url = this.router.url;
    if (url.startsWith('/district')) return '/district/settings';
    if (url.startsWith('/church')) return '/church/settings';
    return '/member/account';
  });

  readonly canSwitchArea = computed(() => {
    return (
      this.auth.homes().length > 1 ||
      this.auth.hasRole('Admin') ||
      this.auth.hasRole('ChurchAdmin')
    );
  });

  toggleDropdown() {
    this.isOpen = !this.isOpen;
  }

  closeDropdown() {
    this.isOpen = false;
  }

  async signOut() {
    this.closeDropdown();
    try {
      await this.auth.logout();
    } finally {
      this.router.navigate(['/']);
    }
  }
}