import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  input,
  output,
} from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { MatIcon } from '@angular/material/icon';
import { MatIconButton } from '@angular/material/button';
import { MatMenu, MatMenuItem, MatMenuTrigger } from '@angular/material/menu';

import { AuthService } from '../../../../services/auth.service';
import { ThemeService } from '../../../../services/theme.service';

@Component({
  selector: 'app-shell-header',
  imports: [
    MatIcon,
    MatIconButton,
    MatMenu,
    MatMenuItem,
    MatMenuTrigger,
    RouterLink,
  ],
  templateUrl: './shell-header.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ShellHeader {
  private auth = inject(AuthService);
  private theme = inject(ThemeService);
  private router = inject(Router);

  readonly title = input('Dashboard');
  /** Optional chip shown on the right (e.g. church code, role). */
  readonly chip = input('');
  readonly chipClass = input('bg-brand-50 text-brand-700');
  /** Nav targets for the account menu. */
  readonly profilePath = input('/member/profile');
  readonly accountPath = input('/member/account');

  /** Emitted when the hamburger is pressed; the parent shell toggles its sidebar. */
  readonly navToggle = output();

  readonly currentUser = this.auth.currentUser;
  readonly isDark = computed(() => this.theme.theme() === 'dark');

  readonly initials = computed(() => {
    const name = this.currentUser()?.displayName?.trim() ?? 'U';
    const parts = name.split(/\s+/);
    return (
      (parts[0]?.[0] ?? '') + (parts[1]?.[0] ?? '')
    ).toUpperCase();
  });

  toggleTheme() {
    this.theme.toggle();
  }

  signOut() {
    void this.auth.logout().then(() => this.router.navigate(['/']));
  }
}