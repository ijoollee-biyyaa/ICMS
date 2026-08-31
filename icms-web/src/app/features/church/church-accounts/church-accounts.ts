import {
  AfterViewInit,
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
  inject,
  signal,
  ViewChild,
} from '@angular/core';
import { MatIcon } from '@angular/material/icon';
import { MatIconButton } from '@angular/material/button';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatSortModule, MatSort } from '@angular/material/sort';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';

import { AuthService } from '../../../services/auth.service';
import { ChurchWorkspaceStore } from '../../../stores/church-workspace.store';
import { AccountLockReason, UserAccount } from '../../../models/account';

/** Role labels for accounts. Admin is protected (cannot be toggled here). */
const ROLE_META: Record<string, { label: string; chip: string }> = {
  Admin: {
    label: 'Super Admin',
    chip: 'bg-fuchsia-100 text-fuchsia-700',
  },
  DistrictSubAdmin: {
    label: 'District Sub-Admin',
    chip: 'bg-brand/10 text-brand',
  },
  ChurchAdmin: {
    label: 'Church Admin',
    chip: 'bg-blue-100 text-brand-blue',
  },
  ChurchSubAdmin: {
    label: 'Church Sub-Admin',
    chip: 'bg-emerald-100 text-emerald-700',
  },
  Member: {
    label: 'Member',
    chip: 'bg-neutral-100 text-neutral-600',
  },
};

const LOCK_REASON_META: Record<AccountLockReason, string> = {
  None: '',
  ClearanceOut: 'Clearance out',
  Death: 'Death',
  BySystem: 'By the system',
};

interface LockOption {
  value: AccountLockReason;
  label: string;
  hint: string;
}

const LOCK_OPTIONS: LockOption[] = [
  {
    value: 'ClearanceOut',
    label: 'Clearance out',
    hint: 'They are leaving the church and no longer need access.',
  },
  {
    value: 'Death',
    label: 'Death',
    hint: 'The account holder has passed away.',
  },
  {
    value: 'BySystem',
    label: 'By the system',
    hint: 'Blocked automatically as a security measure.',
  },
];

@Component({
  selector: 'app-church-accounts',
  imports: [
    MatTableModule,
    MatSortModule,
    MatPaginatorModule,
    MatIcon,
    MatIconButton,
  ],
  templateUrl: './church-accounts.html',
  styleUrl: './church-accounts.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ChurchAccounts implements AfterViewInit {
  readonly store = inject(ChurchWorkspaceStore);
  readonly auth = inject(AuthService);

  readonly dataSource = new MatTableDataSource<UserAccount>();
  readonly displayedColumns = ['name', 'email', 'position', 'status', 'actions'];

  @ViewChild(MatSort) sort!: MatSort;
  @ViewChild(MatPaginator) paginator!: MatPaginator;

  readonly query = signal('');
  readonly notice = signal('');
  readonly error = signal('');

  readonly lockTarget = signal<UserAccount | null>(null);
  readonly lockReason = signal<AccountLockReason>('None');
  readonly pendingLock = signal(false);

  readonly total = this.store.accountCount;
  readonly churchAdmins = this.store.churchAdmins;
  readonly employees = this.store.employeeCount;
  readonly subAdmins = computed(() =>
    this.store.accounts().filter((a) => a.roles.includes('ChurchSubAdmin')).length,
  );

  readonly ROLE_META = ROLE_META;
  readonly LOCK_REASON_META = LOCK_REASON_META;
  readonly lockOptions = LOCK_OPTIONS;

  constructor() {
    effect(() => {
      this.dataSource.data = this.store.accounts();
      this.dataSource.filter = this.query().trim().toLowerCase();
    });
  }

  ngAfterViewInit(): void {
    this.dataSource.sort = this.sort;
    this.dataSource.paginator = this.paginator;
    this.dataSource.sortingDataAccessor = (account, id) =>
      id === 'name'
        ? this.fullName(account)
        : String((account as unknown as Record<string, unknown>)[id] ?? '');
    this.dataSource.filterPredicate = (account, filter) =>
      [
        this.fullName(account),
        account.email ?? '',
        this.positionLabel(account),
        account.roles.join(' '),
      ]
        .join(' ')
        .toLowerCase()
        .includes(filter);
  }

  fullName(account: UserAccount): string {
    return [account.firstName, account.fatherName, account.grandfatherName]
      .filter(Boolean)
      .join(' ');
  }

  isMember(account: UserAccount): boolean {
    return account.kind === 'Member';
  }

  positionLabel(account: UserAccount): string {
    return this.isMember(account) ? 'Member' : (account.position ?? '—');
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

  isLocked(account: UserAccount): boolean {
    return account.isAccountLocked;
  }

  roleLabel(role: string): string {
    return ROLE_META[role]?.label ?? role;
  }

  roleChip(role: string): string {
    return ROLE_META[role]?.chip ?? 'bg-neutral-100 text-neutral-600';
  }

  lockReasonLabel(reason: AccountLockReason): string {
    return LOCK_REASON_META[reason] ?? reason;
  }

  toggleChurchAdmin(account: UserAccount) {
    if (this.isProtected(account) || this.isCurrentUser(account) || this.isMember(account)) return;
    const grant = !account.roles.includes('ChurchAdmin');
    this.notice.set('');
    this.error.set('');
    this.store.changeAccountRole({
      userId: account.userId,
      body: { role: 'ChurchAdmin', grant },
    });
  }

  toggleChurchSubAdmin(account: UserAccount) {
    if (this.isProtected(account) || this.isCurrentUser(account) || this.isMember(account)) return;
    const grant = !account.roles.includes('ChurchSubAdmin');
    this.notice.set('');
    this.error.set('');
    this.store.changeAccountRole({
      userId: account.userId,
      body: { role: 'ChurchSubAdmin', grant },
    });
  }

  openLockDialog(account: UserAccount) {
    if (this.isProtected(account) || this.isCurrentUser(account)) return;
    this.error.set('');
    this.lockReason.set('None');
    this.pendingLock.set(false);
    this.lockTarget.set(account);
  }

  cancelLock() {
    if (this.pendingLock()) return;
    this.lockTarget.set(null);
  }

  confirmLock() {
    const account = this.lockTarget();
    if (!account || this.lockReason() === 'None') return;
    this.pendingLock.set(true);
    this.store.lockAccount({ userId: account.userId, reason: this.lockReason() });
    const name = this.fullName(account);
    this.lockTarget.set(null);
    this.pendingLock.set(false);
    this.notice.set(`${name}'s account is locked (${LOCK_REASON_META[this.lockReason()]}).`);
  }

  unlock(account: UserAccount) {
    if (!account.isAccountLocked) return;
    this.error.set('');
    this.store.unlockAccount({ userId: account.userId });
    this.notice.set(`${this.fullName(account)}'s account is unlocked.`);
  }

  onSearch(event: Event) {
    this.query.set((event.target as HTMLInputElement).value);
  }
}