import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  signal,
} from '@angular/core';
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
import { EmployeeRow } from '../../../models/district';
import {
  Department,
  DepartmentType,
  DepartmentEmployeeRole,
  DepartmentEmployee,
} from '../../../models/department';
import {
  problemDetail,
  applyFieldErrors,
  clearFieldErrors,
} from '../../../common/http-errors';

@Component({
  selector: 'app-church-departments',
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
  templateUrl: './church-departments.html',
  styleUrl: './church-departments.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ChurchDepartments {
  private auth = inject(AuthService);
  private fb = inject(FormBuilder);
  private churchService = inject(ChurchService);

  readonly churchId = computed(() => this.auth.currentUser()?.churchId ?? null);

  readonly departments = signal<Department[]>([]);
  readonly employees = signal<EmployeeRow[]>([]);
  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly error = signal('');
  readonly query = signal('');
  readonly showForm = signal(false);
  readonly editing = signal<Department | null>(null);
  readonly toDelete = signal<Department | null>(null);

  readonly managing = signal<Department | null>(null);
  readonly members = signal<DepartmentEmployee[]>([]);
  readonly membersLoading = signal(false);
  readonly assigning = signal(false);
  readonly removingId = signal<number | null>(null);

  readonly TYPES: DepartmentType[] = [
    'Spiritual',
    'Charity',
    'Development',
    'Administrative',
  ];
  readonly ROLES: DepartmentEmployeeRole[] = ['Head', 'Secretary', 'Staff'];

  form = this.fb.nonNullable.group({
    name: ['', Validators.required],
    type: ['Spiritual' as DepartmentType, Validators.required],
    headEmployeeId: ['' as number | ''],
  });

  assignForm = this.fb.nonNullable.group({
    employeeId: ['' as number | '', Validators.required],
    role: ['Staff' as DepartmentEmployeeRole, Validators.required],
  });

  readonly visible = computed(() => {
    const q = this.query().trim().toLowerCase();
    const list = this.departments();
    if (!q) return list;
    return list.filter(
      (d) =>
        d.name.toLowerCase().includes(q) ||
        d.type.toLowerCase().includes(q) ||
        (d.headEmployeeName ?? '').toLowerCase().includes(q),
    );
  });

  readonly counts = computed(() => {
    const list = this.departments();
    return {
      total: list.length,
      spiritual: list.filter((d) => d.type === 'Spiritual').length,
      administrative: list.filter((d) => d.type === 'Administrative').length,
      staff: list.reduce((sum, d) => sum + d.employeeCount, 0),
    };
  });

  readonly unassigned = computed(() => {
    const active = this.managing();
    const assigned = new Set(this.members().map((m) => m.employeeId));
    if (!active) return this.employees();
    return this.employees()
      .filter((e) => e.status === 'Active')
      .filter((e) => !assigned.has(e.id));
  });

  constructor() {
    this.load();
  }

  load() {
    const churchId = this.churchId();
    if (!churchId) return;
    this.loading.set(true);
    this.error.set('');
    this.churchService.getDepartments(churchId, 1, 100).subscribe({
      next: (page) => {
        this.departments.set(page.items);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Could not load departments.');
        this.loading.set(false);
      },
    });
    this.churchService.getEmployees(churchId, 1, 100).subscribe({
      next: (page) => this.employees.set(page.items),
    });
  }

  onSearch(event: Event) {
    this.query.set((event.target as HTMLInputElement).value);
  }

  typeChip(type: DepartmentType): string {
    switch (type) {
      case 'Spiritual':
        return 'bg-indigo-100 text-indigo-700';
      case 'Charity':
        return 'bg-rose-100 text-rose-700';
      case 'Development':
        return 'bg-amber-100 text-amber-700';
      default:
        return 'bg-slate-100 text-slate-600';
    }
  }

  roleChip(role: DepartmentEmployeeRole): string {
    switch (role) {
      case 'Head':
        return 'bg-brand/10 text-brand';
      case 'Secretary':
        return 'bg-neutral-100 text-neutral-600';
      default:
        return 'bg-slate-100 text-slate-600';
    }
  }

  openCreate() {
    this.error.set('');
    this.editing.set(null);
    this.showForm.set(true);
    this.form.reset({ name: '', type: 'Spiritual', headEmployeeId: '' });
    clearFieldErrors(this.form);
  }

  openEdit(department: Department) {
    this.error.set('');
    this.editing.set(department);
    this.showForm.set(true);
    this.form.reset({
      name: department.name,
      type: department.type,
      headEmployeeId: department.headEmployeeId ?? '',
    });
    clearFieldErrors(this.form);
  }

  closeForm() {
    if (this.saving()) return;
    this.showForm.set(false);
    this.editing.set(null);
  }

  submit() {
    const churchId = this.churchId();
    if (!churchId || this.form.invalid || this.saving()) return;
    clearFieldErrors(this.form);
    this.saving.set(true);
    this.error.set('');

    const v = this.form.getRawValue();
    const headEmployeeId =
      typeof v.headEmployeeId === 'number' ? v.headEmployeeId : null;

    const editing = this.editing();
    if (editing) {
      this.churchService
        .updateDepartment(churchId, editing.id, {
          name: v.name,
          type: v.type,
          headEmployeeId,
        })
        .subscribe({
          next: (department) => {
            this.saving.set(false);
            this.departments.set(
              this.departments().map((d) =>
                d.id === editing.id ? department : d,
              ),
            );
            this.showForm.set(false);
            this.editing.set(null);
          },
          error: (err) => {
            this.saving.set(false);
            if (!this.applyServerErrors(err, this.form)) {
              this.error.set(
                problemDetail(err, 'Could not update the department.'),
              );
            }
          },
        });
      return;
    }

    this.churchService
      .createDepartment(churchId, { name: v.name, type: v.type, headEmployeeId })
      .subscribe({
        next: (department) => {
          this.saving.set(false);
          this.departments.set([...this.departments(), department]);
          this.showForm.set(false);
        },
        error: (err) => {
          this.saving.set(false);
          if (!this.applyServerErrors(err, this.form)) {
            this.error.set(
              problemDetail(err, 'Could not create the department.'),
            );
          }
        },
      });
  }

  askDelete(department: Department) {
    this.error.set('');
    this.toDelete.set(department);
  }

  cancelDelete() {
    this.toDelete.set(null);
  }

  confirmDelete() {
    const churchId = this.churchId();
    const target = this.toDelete();
    if (!churchId || !target || this.saving()) return;
    this.saving.set(true);
    this.error.set('');

    const index = this.departments().findIndex((d) => d.id === target.id);
    const shot = this.departments().filter((d) => d.id !== target.id);
    this.departments.set(shot);

    this.churchService.deleteDepartment(churchId, target.id).subscribe({
      next: () => {
        this.saving.set(false);
        this.toDelete.set(null);
      },
      error: (err) => {
        this.saving.set(false);
        this.departments.set(
          index >= 0
            ? [
                ...this.departments().slice(0, index),
                target,
                ...this.departments().slice(index),
              ]
            : this.departments(),
        );
        this.error.set(problemDetail(err, 'Could not delete the department.'));
      },
    });
  }

  openManage(department: Department) {
    this.managing.set(department);
    this.members.set([]);
    this.assignForm.reset({ employeeId: '', role: 'Staff' });
    clearFieldErrors(this.assignForm);
    this.loadMembers();
  }

  closeManage(restoreMembers = false) {
    if (this.assigning()) return;
    const active = this.managing();
    this.managing.set(null);
    if (active && restoreMembers && this.members().length > 0) {
      this.departments.set(
        this.departments().map((d) =>
          d.id === active.id ? { ...d, employeeCount: this.members().length } : d,
        ),
      );
    }
  }

  private loadMembers() {
    const churchId = this.churchId();
    const active = this.managing();
    if (!churchId || !active) return;
    this.membersLoading.set(true);
    this.error.set('');
    this.churchService
      .getDepartmentEmployees(churchId, active.id)
      .subscribe({
        next: (members) => {
          this.members.set(members);
          this.membersLoading.set(false);
        },
        error: () => {
          this.error.set('Could not load department employees.');
          this.membersLoading.set(false);
        },
      });
  }

  submitAssign() {
    const churchId = this.churchId();
    const active = this.managing();
    if (!churchId || !active || this.assignForm.invalid || this.assigning()) {
      return;
    }
    clearFieldErrors(this.assignForm);
    this.assigning.set(true);
    this.error.set('');

    const v = this.assignForm.getRawValue();
    this.churchService
      .assignDepartmentEmployee(churchId, active.id, {
        employeeId: v.employeeId as number,
        role: v.role,
      })
      .subscribe({
        next: (member) => {
          this.assigning.set(false);
          this.members.set([...this.members(), member]);
          this.assignForm.reset({ employeeId: '', role: 'Staff' });
        },
        error: (err) => {
          this.assigning.set(false);
          if (!this.applyServerErrors(err, this.assignForm)) {
            this.error.set(
              problemDetail(err, 'Could not assign the employee.'),
            );
          }
        },
      });
  }

  removeMember(member: DepartmentEmployee) {
    const churchId = this.churchId();
    const active = this.managing();
    if (!churchId || !active || this.removingId()) return;
    this.removingId.set(member.id);
    this.error.set('');

    const shot = this.members();
    const index = shot.findIndex((m) => m.id === member.id);
    this.members.set(shot.filter((m) => m.id !== member.id));

    this.churchService
      .removeDepartmentEmployee(churchId, active.id, member.id)
      .subscribe({
        next: () => this.removingId.set(null),
        error: (err) => {
          this.removingId.set(null);
          this.members.set(
            index >= 0
              ? [
                  ...this.members().slice(0, index),
                  member,
                  ...this.members().slice(index),
                ]
              : this.members(),
          );
          this.error.set(
            problemDetail(err, 'Could not remove the employee.'),
          );
        },
      });
  }

  private applyServerErrors(
    err: unknown,
    form: typeof this.form | typeof this.assignForm,
  ): boolean {
    const response = err as {
      status?: number;
      error?: { errors?: Record<string, string[]> };
    };
    if (response?.status !== 400 || !response.error?.errors) return false;
    return applyFieldErrors(form, err as Parameters<typeof applyFieldErrors>[1]);
  }
}