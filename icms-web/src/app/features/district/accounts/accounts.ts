import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  signal,
} from '@angular/core';
import { MatIcon } from '@angular/material/icon';
import { MatIconButton } from '@angular/material/button';

import { AuthService } from '../../../services/auth.service';
import { DistrictStore } from '../../../stores/district.store';
import { UserAccount } from '../../../models/account';
import { StatCard } from '../../shared/ui/stat-card/stat-card';
import { AppTable } from '../../shared/ui/data-table/data-table';
import { TableColumn } from '../../shared/ui/data-table/table-column';

/** Role labels for accounts. Admin is protected (cannot be toggled here). */
const ROLE_META: Record<string, { label: string; chip: string }> = {
  Admin: {
    label: 'Super Admin',
    chip: 'bg-fuchsia-100 text-fuchsia-700 dark:bg-fuchsia-900/30 dark:text-fuchsia-300 dark:border dark:border-fuchsia-800/40',
  },
  DistrictSubAdmin: {
    label: 'District Sub-Admin',
    chip: 'bg-brand/10 text-brand dark:bg-brand/20 dark:text-brand-300 dark:border dark:border-brand/30',
  },
  ChurchAdmin: {
    label: 'Church Admin',
    chip: 'bg-blue-100 text-brand-blue dark:bg-blue-900/30 dark:text-blue-300 dark:border dark:border-blue-800/40',
  },
  Member: {
    label: 'Member',
    chip: 'bg-neutral-100 text-neutral-600 dark:bg-neutral-800 dark:text-neutral-300 dark:border dark:border-neutral-700',
  },
};

@Component({
  selector: 'app-district-accounts',
  imports: [MatIcon, MatIconButton, StatCard, AppTable, TableColumn],
  templateUrl: './accounts.html',
  styleUrl: './accounts.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DistrictAccounts {
  readonly store = inject(DistrictStore);
  readonly auth = inject(AuthService);

  readonly notice = signal('');

  readonly total = computed(() => this.store.accountCount());
  readonly subAdmins = computed(() =>
    this.store
      .accounts()
      .filter((a) => a.roles.includes('DistrictSubAdmin')).length,
  );
  readonly admins = computed(() =>
    this.store.accounts().filter((a) => a.roles.includes('Admin')).length,
  );

  readonly ROLE_META = ROLE_META;

  readonly accountName = (a: UserAccount) => this.fullName(a);
  readonly servesLabel = (a: UserAccount) =>
    a.districtId != null ? 'District Office' : 'A church';
  readonly rolesLabel = (a: UserAccount) =>
    a.roles.length ? a.roles.join(', ') : 'No roles';
  readonly accessLabel = (a: UserAccount) =>
    a.roles.includes('DistrictSubAdmin') ? 'Sub-admin' : 'Standard';

  readonly searchAccount = (a: UserAccount, q: string): boolean => {
    const name = this.fullName(a).toLowerCase();
    const email = (a.email ?? '').toLowerCase();
    const position = (a.position ?? '').toLowerCase();
    const roles = a.roles.join(' ').toLowerCase();
    const serves = a.districtId != null ? 'district office' : 'church';
    return (
      name.includes(q) ||
      email.includes(q) ||
      position.includes(q) ||
      roles.includes(q) ||
      serves.includes(q)
    );
  };

  fullName(account: UserAccount): string {
    return [account.firstName, account.fatherName, account.grandfatherName]
      .filter(Boolean)
      .join(' ');
  }

  initials(account: UserAccount) {
    const full = this.fullName(account).trim();
    if (!full) return '?';
    return full
      .split(' ')
      .slice(0, 2)
      .map((part) => part[0])
      .join('')
      .toUpperCase();
  }

  isCurrentUser(account: UserAccount): boolean {
    return account.userId === this.auth.currentUser()?.userId;
  }

  isProtected(account: UserAccount): boolean {
    return account.roles.includes('Admin');
  }

  roleLabel(role: string): string {
    return ROLE_META[role]?.label ?? role;
  }

  roleChip(role: string): string {
    return (
      ROLE_META[role]?.chip ??
      'bg-neutral-100 text-neutral-600 dark:bg-neutral-800 dark:text-neutral-300'
    );
  }

  toggleSubAdmin(account: UserAccount) {
    if (this.isProtected(account) || this.isCurrentUser(account)) return;
    const grant = !account.roles.includes('DistrictSubAdmin');
    this.notice.set('');
    this.store.changeAccountRole({
      userId: account.userId,
      body: { role: 'DistrictSubAdmin', grant },
    });
  }

  printAccountDetails() {
    const printWindow = window.open('', '_blank');
    if (!printWindow) return;

    const accounts = this.store.accounts();
    const html = `
      <html>
        <head>
          <style>
            body { font-family: Arial, sans-serif; }
            .header { text-align: center; margin-bottom: 20px; }
            .person { margin-bottom: 20px; border-bottom: 1px solid #ddd; padding-bottom: 10px; }
            .roles { color: #555; }
            @media print { body { font-size: 12pt; } }
          </style>
        </head>
        <body>
          <div class="header">
            <h1>EFGBC District - Account Details</h1>
            <p>Total Accounts: ${accounts.length}</p>
            <p>Generated: ${new Date().toLocaleDateString()}</p>
          </div>
          ${accounts
        .map(
          (a) => `
            <div class="person">
              <h3>${this.fullName(a)}</h3>
              <p>Email: ${a.email ?? '—'}</p>
              <p>Position: ${a.position ?? '—'}</p>
              <p class="roles">Roles: ${a.roles.length ? a.roles.join(', ') : '—'}</p>
            </div>
          `,
        )
        .join('')}
        </body>
      </html>
    `;

    printWindow.document.write(html);
    printWindow.document.close();
    printWindow.print();
  }
}
