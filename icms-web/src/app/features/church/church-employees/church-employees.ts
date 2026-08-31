import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  signal,
} from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatIconButton } from '@angular/material/button';
import {
  MatError,
  MatFormField,
  MatHint,
  MatLabel,
} from '@angular/material/form-field';
import { MatInput } from '@angular/material/input';
import { MatOption, MatSelect } from '@angular/material/select';
import { MatIcon } from '@angular/material/icon';

import { AuthService } from '../../../services/auth.service';
import { ChurchService } from '../../../services/church.service';
import { Member } from '../../../models/member';
import {
  EmployeeRow,
  EmploymentType,
  MinisterTitle,
  EmployeeStatus,
} from '../../../models/district';
import {
  problemDetail,
  applyFieldErrors,
  clearFieldErrors,
  dateOrNull,
} from '../../../common/http-errors';

@Component({
  selector: 'app-church-employees',
  imports: [
    ReactiveFormsModule,
    MatFormField,
    MatLabel,
    MatHint,
    MatError,
    MatInput,
    MatSelect,
    MatOption,
    MatIconButton,
    MatIcon,
  ],
  templateUrl: './church-employees.html',
  styleUrl: './church-employees.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ChurchEmployees {
  private auth = inject(AuthService);
  private fb = inject(FormBuilder);
  private churchService = inject(ChurchService);

  readonly churchId = computed(() => this.auth.currentUser()?.churchId ?? null);

  readonly employees = signal<EmployeeRow[]>([]);
  readonly roster = signal<Member[]>([]);
  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly deleting = signal(false);
  readonly error = signal('');
  readonly query = signal('');
  readonly showForm = signal(false);
  readonly editing = signal<EmployeeRow | null>(null);
  readonly toDelete = signal<EmployeeRow | null>(null);
  readonly created = signal<EmployeeRow | null>(null);

  readonly EMPLOYMENT_TYPES: EmploymentType[] = [
    'FulltimeMinister',
    'ChurchStaff',
    'Volunteer',
  ];
  readonly TITLES: MinisterTitle[] = [
    'Pastor',
    'Evangelist',
    'Prophet',
    'Teacher',
    'Apostle',
  ];
  readonly STATUSES: EmployeeStatus[] = [
    'Active',
    'OnLeave',
    'Resigned',
    'Terminated',
  ];

  form = this.fb.nonNullable.group({
    memberId: ['' as number | ''],
    firstName: [''],
    fatherName: [''],
    grandfatherName: [''],
    position: ['', Validators.required],
    employmentType: ['ChurchStaff' as EmploymentType, Validators.required],
    ministerTitle: ['' as MinisterTitle | ''],
    salary: [null as number | null],
    hireDate: [''],
    status: ['Active' as EmployeeStatus],
  });

  readonly visible = computed(() => {
    const q = this.query().trim().toLowerCase();
    const list = this.employees();
    if (!q) return list;
    return list.filter((e) => {
      const name = (e.memberName ?? '').toLowerCase();
      return (
        name.includes(q) ||
        e.position.toLowerCase().includes(q) ||
        (e.memberEfgbcId ?? '').toLowerCase().includes(q)
      );
    });
  });

  readonly activeCount = computed(
    () => this.employees().filter((e) => e.status === 'Active').length,
  );
  readonly ministerCount = computed(
    () =>
      this.employees().filter((e) => e.employmentType === 'FulltimeMinister')
        .length,
  );
  readonly paidFromDistrict = computed(
    () => this.employees().filter((e) => e.salaryPaidBy === 'District').length,
  );

  private readonly employmentType$ = toSignal(
    this.form.controls.employmentType.valueChanges,
    { initialValue: this.form.controls.employmentType.value },
  );

  readonly isFulltime = computed(
    () => this.employmentType$() === 'FulltimeMinister',
  );

  constructor() {
    this.load();
  }

  onSearch(event: Event) {
    this.query.set((event.target as HTMLInputElement).value);
  }

  load() {
    const churchId = this.churchId();
    if (!churchId) return;
    this.loading.set(true);
    this.error.set('');
    this.churchService.getEmployees(churchId, 1, 100).subscribe({
      next: (page) => {
        this.employees.set(page.items);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Could not load employees.');
        this.loading.set(false);
      },
    });
    this.churchService.getMembers(churchId, 1, 100).subscribe({
      next: (page) => this.roster.set(page.items),
    });
  }

  openCreate() {
    this.error.set('');
    this.editing.set(null);
    this.created.set(null);
    this.showForm.set(true);
    this.form.reset({
      memberId: '',
      firstName: '',
      fatherName: '',
      grandfatherName: '',
      position: '',
      employmentType: 'ChurchStaff',
      ministerTitle: '',
      salary: null,
      hireDate: '',
      status: 'Active',
    });
    clearFieldErrors(this.form);
  }

  openEdit(employee: EmployeeRow) {
    this.error.set('');
    this.editing.set(employee);
    this.created.set(null);
    this.showForm.set(true);
    this.form.reset({
      memberId: '',
      firstName: '',
      fatherName: '',
      grandfatherName: '',
      position: employee.position,
      employmentType: employee.employmentType,
      ministerTitle: employee.ministerTitle ?? '',
      salary: employee.salary,
      hireDate: employee.hireDate ?? '',
      status: employee.status,
    });
    clearFieldErrors(this.form);
  }

  closeForm() {
    if (this.saving()) return;
    this.showForm.set(false);
    this.editing.set(null);
    this.created.set(null);
  }

  submit() {
    const churchId = this.churchId();
    if (!churchId || this.form.invalid || this.saving()) return;
    clearFieldErrors(this.form);
    this.saving.set(true);
    this.error.set('');

    const v = this.form.getRawValue();
    const editing = this.editing();
    if (editing) {
      this.churchService
        .updateEmployee(churchId, editing.id, {
          position: v.position,
          ministerTitle: v.ministerTitle || null,
          salary: v.salary,
          hireDate: dateOrNull(v.hireDate),
          status: v.status,
          isDistrictPresident: false,
          isVicePresident: false,
        })
        .subscribe({
          next: (updated) => {
            this.saving.set(false);
            this.employees.set(
              this.employees().map((e) => (e.id === editing.id ? updated : e)),
            );
            this.showForm.set(false);
            this.editing.set(null);
          },
          error: (err) => {
            this.saving.set(false);
            if (!this.applyServerErrors(err)) {
              this.error.set(problemDetail(err, 'Could not update the employee.'));
            }
          },
        });
      return;
    }

    const usesMember = typeof v.memberId === 'number';
    const memberId = usesMember ? (v.memberId as number) : null;
    this.churchService
      .createEmployee(churchId, {
        position: v.position,
        employmentType: v.employmentType,
        ministerTitle: v.ministerTitle || null,
        memberId,
        salary: v.salary,
        hireDate: dateOrNull(v.hireDate),
        isDistrictPresident: false,
        isVicePresident: false,
        firstName: usesMember ? null : v.firstName || null,
        fatherName: usesMember ? null : v.fatherName || null,
        grandfatherName: usesMember ? null : v.grandfatherName || null,
      })
      .subscribe({
        next: (employee) => {
          this.saving.set(false);
          this.employees.set([...this.employees(), employee]);
          if (employee.accountEmail) {
            this.created.set(employee);
            this.showForm.set(false);
          } else {
            this.showForm.set(false);
          }
        },
        error: (err) => {
          this.saving.set(false);
          if (!this.applyServerErrors(err)) {
            this.error.set(problemDetail(err, 'Could not hire the employee.'));
          }
        },
      });
  }

  private applyServerErrors(err: unknown): boolean {
    const response = err as {
      status?: number;
      error?: { errors?: Record<string, string[]> };
    };
    if (response?.status !== 400 || !response.error?.errors) return false;
    return applyFieldErrors(
      this.form,
      err as Parameters<typeof applyFieldErrors>[1],
    );
  }

  askDelete(employee: EmployeeRow) {
    this.error.set('');
    this.toDelete.set(employee);
  }

  cancelDelete() {
    this.toDelete.set(null);
  }

  confirmDelete() {
    const churchId = this.churchId();
    const target = this.toDelete();
    if (!churchId || !target || this.deleting()) return;
    this.deleting.set(true);
    this.error.set('');

    const shot = this.employees();
    const index = shot.findIndex((e) => e.id === target.id);
    this.employees.set(shot.filter((e) => e.id !== target.id));

    this.churchService.deleteEmployee(churchId, target.id).subscribe({
      next: () => {
        this.deleting.set(false);
        this.toDelete.set(null);
      },
      error: (err) => {
        this.deleting.set(false);
        this.employees.set(
          index >= 0
            ? [
                ...this.employees().slice(0, index),
                target,
                ...this.employees().slice(index),
              ]
            : this.employees(),
        );
        this.error.set(problemDetail(err, 'Could not remove the employee.'));
      },
    });
  }

  dismissCreated() {
    this.created.set(null);
  }

  fullName(member: Member): string {
    return [member.firstName, member.fatherName, member.grandfatherName]
      .filter(Boolean)
      .join(' ');
  }

  employeeName(employee: EmployeeRow): string {
    return employee.memberName ?? '—';
  }

  employmentLabel(type: EmploymentType): string {
    switch (type) {
      case 'FulltimeMinister':
        return 'Full-time minister';
      case 'ChurchStaff':
        return 'Church staff';
      case 'Volunteer':
        return 'Volunteer';
      default:
        return type;
    }
  }

  statusChip(status: EmployeeStatus): string {
    switch (status) {
      case 'Active':
        return 'bg-emerald-100 text-emerald-700';
      case 'OnLeave':
        return 'bg-amber-100 text-amber-700';
      default:
        return 'bg-neutral-100 text-neutral-600';
    }
  }

  salaryText(salary: number | null): string {
    if (salary === null || salary === undefined) return '—';
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'ETB',
    }).format(salary);
  }
}