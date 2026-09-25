import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  OnInit,
  signal,
} from '@angular/core';
import { CurrencyPipe, DatePipe } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatIcon } from '@angular/material/icon';
import { MatIconButton } from '@angular/material/button';
import {
  MatError,
  MatFormField,
  MatHint,
  MatLabel,
} from '@angular/material/form-field';
import { MatInput } from '@angular/material/input';
import { MatOption, MatSelect } from '@angular/material/select';

import { environment } from '../../../../environments/environment';
import { DistrictService } from '../../../services/district.service';
import { ChurchService } from '../../../services/church.service';
import { AuthService } from '../../../services/auth.service';
import {
  EmployeeRow,
  EmploymentType,
  MinisterTitle,
  EmployeeStatus,
  OfficeExecutives,
  MinisterRow,
  CreateEmployeeRequest,
  UpdateEmployeeRequest,
} from '../../../models/district';
import { Member } from '../../../models/member';
import { AppTable } from '../../shared/ui/data-table/data-table';
import { TableColumn } from '../../shared/ui/data-table/table-column';
import { StatCard } from '../../shared/ui/stat-card/stat-card';
import { problemDetail } from '../../../common/http-errors';

@Component({
  selector: 'app-district-employees',
  imports: [
    ReactiveFormsModule,
    DatePipe,
    MatIcon,
    MatFormField,
    MatLabel,
    MatInput,
    MatSelect,
    MatOption,
    MatError,
    MatHint,
    AppTable,
    TableColumn,
    StatCard,
  ],
  templateUrl: './employees.html',
  styleUrl: './employees.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DistrictEmployees implements OnInit {
  private readonly districtService = inject(DistrictService);
  private readonly churchService = inject(ChurchService);
  private readonly auth = inject(AuthService);
  private readonly fb = inject(FormBuilder);

  readonly districtId = environment.districtId;

  // State signals
  readonly employees = signal<EmployeeRow[]>([]);
  readonly executives = signal<OfficeExecutives | null>(null);
  readonly candidateMinisters = signal<MinisterRow[]>([]);
  readonly loading = signal<boolean>(false);
  readonly saving = signal<boolean>(false);
  readonly deleting = signal<boolean>(false);

  readonly selectedCandidate = computed(() => {
    const memberId = this.hireForm.get('memberId')?.value;
    if (!memberId) return null;
    return this.candidateMinisters().find((m) => m.memberId === memberId) ?? null;
  });

  // Filter & Search
  readonly searchQuery = signal<string>('');
  readonly selectedTypeFilter = signal<string>('ALL');

  // Modals state
  readonly showHireModal = signal<boolean>(false);
  readonly showEditModal = signal<boolean>(false);
  readonly showDeleteModal = signal<boolean>(false);
  readonly selectedEmployee = signal<EmployeeRow | null>(null);
  readonly newlyCreatedCredentials = signal<{
    name: string;
    email: string;
    tempPass: string;
    position: string;
  } | null>(null);

  readonly errorMessage = signal<string | null>(null);

  // Hire Form
  readonly hireForm = this.fb.nonNullable.group({
    employmentType: ['DistrictStaff' as EmploymentType, Validators.required],
    position: ['', [Validators.required, Validators.maxLength(100)]],
    ministerTitle: ['' as MinisterTitle | ''],
    memberId: [null as number | null],
    salary: [null as number | null],
    hireDate: [new Date().toISOString().slice(0, 10), Validators.required],
    isDistrictPresident: [false],
    isVicePresident: [false],
    firstName: ['', Validators.maxLength(50)],
    fatherName: ['', Validators.maxLength(50)],
    grandfatherName: ['', Validators.maxLength(50)],
  });

  // Edit Form
  readonly editForm = this.fb.nonNullable.group({
    position: ['', [Validators.required, Validators.maxLength(100)]],
    ministerTitle: ['' as MinisterTitle | ''],
    salary: [null as number | null],
    hireDate: [''],
    status: ['Active' as EmployeeStatus, Validators.required],
    isDistrictPresident: [false],
    isVicePresident: [false],
  });

  // Computed metrics
  readonly totalEmployees = computed(() => this.employees().length);
  
  readonly staffCount = computed(
    () => this.employees().filter((e) => e.employmentType === 'DistrictStaff').length
  );
  
  readonly ministerCount = computed(
    () => this.employees().filter((e) => e.employmentType === 'FulltimeMinister').length
  );
  
  readonly districtPayrollCount = computed(
    () => this.employees().filter((e) => e.salaryPaidBy === 'District').length
  );

  readonly president = computed(() => this.executives()?.president ?? null);
  readonly vicePresident = computed(() => this.executives()?.vicePresident ?? null);

  readonly filteredEmployees = computed(() => {
    const list = this.employees();
    const filter = this.selectedTypeFilter();
    if (filter === 'ALL') return list;
    if (filter === 'STAFF') return list.filter((e) => e.employmentType === 'DistrictStaff');
    if (filter === 'MINISTER') return list.filter((e) => e.employmentType === 'FulltimeMinister');
    if (filter === 'EXECUTIVE') return list.filter((e) => e.isDistrictPresident || e.isVicePresident);
    return list;
  });

  ngOnInit(): void {
    this.loadData();
    this.loadCandidates();
  }

  loadData(): void {
    this.loading.set(true);
    this.errorMessage.set(null);

    this.districtService.getEmployees(this.districtId, 1, 100).subscribe({
      next: (res) => {
        this.employees.set(res.items || []);
        this.loading.set(false);
      },
      error: (err) => {
        this.errorMessage.set(problemDetail(err, 'Failed to load office employees.'));
        this.loading.set(false);
      },
    });

    this.districtService.getExecutives(this.districtId).subscribe({
      next: (execs) => {
        this.executives.set(execs);
      },
      error: () => {
        // Executives query failure is non-blocking
      },
    });
  }

  loadCandidates(): void {
    // Only load active full-time ministers across the district's local churches
    this.districtService.getMinisters(this.districtId, 1, 100).subscribe({
      next: (res) => {
        this.candidateMinisters.set(res.items || []);
      },
      error: () => {
        // Non-blocking
      },
    });
  }

  onCandidateSelected(memberId: number): void {
    const candidate = this.candidateMinisters().find((m) => m.memberId === memberId);
    if (candidate && candidate.ministerTitle) {
      this.hireForm.patchValue({ ministerTitle: candidate.ministerTitle });
    }
  }

  onPresidentToggle(checked: boolean): void {
    if (checked) {
      this.hireForm.patchValue({
        isVicePresident: false,
        position: this.hireForm.get('position')?.value || 'District President',
      });
    }
  }

  onVicePresidentToggle(checked: boolean): void {
    if (checked) {
      this.hireForm.patchValue({
        isDistrictPresident: false,
        position: this.hireForm.get('position')?.value || 'District Vice President',
      });
    }
  }

  // ---- Modal openers & handlers ----

  openHireModal(): void {
    this.hireForm.reset({
      employmentType: 'DistrictStaff',
      position: '',
      ministerTitle: '',
      memberId: null,
      salary: null,
      hireDate: new Date().toISOString().slice(0, 10),
      isDistrictPresident: false,
      isVicePresident: false,
      firstName: '',
      fatherName: '',
      grandfatherName: '',
    });
    this.errorMessage.set(null);
    this.showHireModal.set(true);
    this.loadCandidates();
  }

  closeHireModal(): void {
    if (this.saving()) return;
    this.showHireModal.set(false);
  }

  submitHire(): void {
    if (this.hireForm.invalid) {
      this.hireForm.markAllAsTouched();
      return;
    }

    const val = this.hireForm.getRawValue();

    // Client-side validation: if FulltimeMinister, candidate minister is required
    if (val.employmentType === 'FulltimeMinister') {
      if (!val.memberId) {
        this.errorMessage.set('Please select an active full-time minister from the church ministry roster.');
        return;
      }
      const candidate = this.candidateMinisters().find((m) => m.memberId === val.memberId);
      if (!candidate) {
        this.errorMessage.set('Selected candidate must be an active full-time minister in a local church.');
        return;
      }
    }

    // If DistrictStaff, names are required
    if (val.employmentType === 'DistrictStaff' && (!val.firstName?.trim() || !val.fatherName?.trim())) {
      this.errorMessage.set('First name and father name are required for office staff.');
      return;
    }

    // Executive rules check
    if ((val.isDistrictPresident || val.isVicePresident) && val.employmentType !== 'FulltimeMinister') {
      this.errorMessage.set('The District President and Vice President must be full-time ministers.');
      return;
    }

    if (val.isDistrictPresident && val.isVicePresident) {
      this.errorMessage.set('An employee cannot hold both President and Vice President roles simultaneously.');
      return;
    }

    this.saving.set(true);
    this.errorMessage.set(null);

    const body: CreateEmployeeRequest = {
      position: val.position.trim(),
      employmentType: val.employmentType,
      ministerTitle: val.ministerTitle ? (val.ministerTitle as MinisterTitle) : null,
      memberId: val.memberId,
      salary: val.salary,
      hireDate: val.hireDate || null,
      isDistrictPresident: val.isDistrictPresident,
      isVicePresident: val.isVicePresident,
      firstName: val.firstName?.trim() || null,
      fatherName: val.fatherName?.trim() || null,
      grandfatherName: val.grandfatherName?.trim() || null,
    };

    this.districtService.createEmployee(this.districtId, body).subscribe({
      next: (created) => {
        this.saving.set(false);
        this.showHireModal.set(false);
        this.loadData();

        // If credentials were created, show the alert modal
        if (created.accountEmail && created.accountTempPassword) {
          this.newlyCreatedCredentials.set({
            name: created.memberName || `${val.firstName} ${val.fatherName}`,
            email: created.accountEmail,
            tempPass: created.accountTempPassword,
            position: created.position,
          });
        }
      },
      error: (err) => {
        this.saving.set(false);
        this.errorMessage.set(problemDetail(err, 'Failed to hire office employee.'));
      },
    });
  }

  openEditModal(emp: EmployeeRow): void {
    this.selectedEmployee.set(emp);
    this.editForm.reset({
      position: emp.position,
      ministerTitle: emp.ministerTitle ?? '',
      salary: emp.salary,
      hireDate: emp.hireDate ?? '',
      status: emp.status,
      isDistrictPresident: emp.isDistrictPresident,
      isVicePresident: emp.isVicePresident,
    });
    this.errorMessage.set(null);
    this.showEditModal.set(true);
  }

  closeEditModal(): void {
    if (this.saving()) return;
    this.showEditModal.set(false);
    this.selectedEmployee.set(null);
  }

  submitEdit(): void {
    const emp = this.selectedEmployee();
    if (!emp) return;

    if (this.editForm.invalid) {
      this.editForm.markAllAsTouched();
      return;
    }

    const val = this.editForm.getRawValue();

    if ((val.isDistrictPresident || val.isVicePresident) && emp.employmentType !== 'FulltimeMinister') {
      this.errorMessage.set('The District President and Vice President must be full-time ministers.');
      return;
    }

    if (val.isDistrictPresident && val.isVicePresident) {
      this.errorMessage.set('An employee cannot hold both President and Vice President roles simultaneously.');
      return;
    }

    this.saving.set(true);
    this.errorMessage.set(null);

    const body: UpdateEmployeeRequest = {
      position: val.position.trim(),
      ministerTitle: val.ministerTitle ? (val.ministerTitle as MinisterTitle) : null,
      salary: val.salary,
      hireDate: val.hireDate || null,
      status: val.status,
      isDistrictPresident: val.isDistrictPresident,
      isVicePresident: val.isVicePresident,
    };

    this.districtService.updateEmployee(this.districtId, emp.id, body).subscribe({
      next: () => {
        this.saving.set(false);
        this.closeEditModal();
        this.loadData();
      },
      error: (err) => {
        this.saving.set(false);
        this.errorMessage.set(problemDetail(err, 'Failed to update employee.'));
      },
    });
  }

  openDeleteModal(emp: EmployeeRow): void {
    this.selectedEmployee.set(emp);
    this.showDeleteModal.set(true);
  }

  closeDeleteModal(): void {
    if (this.deleting()) return;
    this.showDeleteModal.set(false);
    this.selectedEmployee.set(null);
  }

  confirmDelete(): void {
    const emp = this.selectedEmployee();
    if (!emp) return;

    this.deleting.set(true);
    this.districtService.deleteEmployee(this.districtId, emp.id).subscribe({
      next: () => {
        this.deleting.set(false);
        this.closeDeleteModal();
        this.loadData();
      },
      error: (err) => {
        this.deleting.set(false);
        this.errorMessage.set(problemDetail(err, 'Failed to remove office employee.'));
      },
    });
  }

  closeCredentialsModal(): void {
    this.newlyCreatedCredentials.set(null);
  }

  // ---- Table helpers ----

  searchEmployee = (emp: EmployeeRow, query: string): boolean => {
    const q = query.toLowerCase().trim();
    if (!q) return true;
    return (
      (emp.memberName?.toLowerCase().includes(q) ?? false) ||
      emp.position.toLowerCase().includes(q) ||
      (emp.ministerTitle?.toLowerCase().includes(q) ?? false) ||
      (emp.memberChurchName?.toLowerCase().includes(q) ?? false) ||
      (emp.memberEfgbcId?.toLowerCase().includes(q) ?? false)
    );
  };

  matchesType(emp: EmployeeRow): boolean {
    const filter = this.selectedTypeFilter();
    if (filter === 'ALL') return true;
    if (filter === 'STAFF') return emp.employmentType === 'DistrictStaff';
    if (filter === 'MINISTER') return emp.employmentType === 'FulltimeMinister';
    if (filter === 'EXECUTIVE') return emp.isDistrictPresident || emp.isVicePresident;
    return true;
  }

  employeeName = (e: EmployeeRow) => e.memberName || 'Unnamed Employee';
  employeePosition = (e: EmployeeRow) => e.position;
  employeeType = (e: EmployeeRow) => e.employmentType;
  employeeChurch = (e: EmployeeRow) => e.memberChurchName || 'District HQ';
  employeeStatus = (e: EmployeeRow) => e.status;

  initials(name: string | null): string {
    if (!name) return 'DE';
    const parts = name.trim().split(/\s+/);
    if (parts.length >= 2) {
      return (parts[0][0] + parts[1][0]).toUpperCase();
    }
    return (name[0] || 'D').toUpperCase();
  }

  memberFullName(m: Member): string {
    return [m.firstName, m.fatherName, m.grandfatherName].filter(Boolean).join(' ');
  }
}
