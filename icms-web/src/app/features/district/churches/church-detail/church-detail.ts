import {
  Component,
  computed,
  inject,
  signal,
  OnInit,
} from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatIcon } from '@angular/material/icon';
import { MatButton, MatIconButton } from '@angular/material/button';
import { CurrencyPipe, DatePipe } from '@angular/common';

import { AuthService } from '../../../../services/auth.service';
import { ChurchService } from '../../../../services/church.service';
import { ChurchDetail } from '../../../../models/church';
import { EmployeeRow } from '../../../../models/district';
import { MemberStats } from '../../../../models/member';
import { environment } from '../../../../../environments/environment';
import { AppTable } from '../../../shared/ui/data-table/data-table';
import { TableColumn } from '../../../shared/ui/data-table/table-column';
import { StatCard } from '../../../shared/ui/stat-card/stat-card';

@Component({
  selector: 'app-district-church-detail',
  imports: [
    RouterLink,
    MatIcon,
    CurrencyPipe,
    DatePipe,
    AppTable,
    TableColumn,
    StatCard,
  ],
  templateUrl: './church-detail.html',
  styleUrl: './church-detail.scss',
})
export class DistrictChurchDetail implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly auth = inject(AuthService);
  private readonly churchService = inject(ChurchService);

  readonly churchId = signal<number>(0);
  readonly church = signal<ChurchDetail | null>(null);
  readonly employees = signal<EmployeeRow[]>([]);
  readonly memberStats = signal<MemberStats | null>(null);
  readonly loading = signal(true);
  readonly error = signal('');

  readonly activeTab = signal<'personnel' | 'overview' | 'members'>('personnel');

  readonly districtId = environment.districtId;

  formatLocation(subcity: string | null, city: string | null): string {
    return [subcity, city].filter(Boolean).join(', ') || 'Location not specified';
  }

  readonly ministers = computed(() =>
    this.employees().filter((e) => e.employmentType === 'FulltimeMinister'),
  );

  readonly staff = computed(() =>
    this.employees().filter((e) => e.employmentType !== 'FulltimeMinister'),
  );

  ngOnInit() {
    this.route.paramMap.subscribe((params) => {
      const id = Number(params.get('id'));
      if (id) {
        this.churchId.set(id);
        this.load();
      } else {
        this.router.navigate(['/district/churches']);
      }
    });
  }

  load() {
    const id = this.churchId();
    const dId = this.districtId;
    if (!id) return;

    this.loading.set(true);
    this.error.set('');

    this.churchService.getChurch(dId, id).subscribe({
      next: (detail) => {
        this.church.set(detail);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Could not load church details.');
        this.loading.set(false);
      },
    });

    this.churchService.getEmployees(id, 1, 100).subscribe({
      next: (page) => this.employees.set(page.items),
      error: () => {},
    });

    this.churchService.getMemberStats(id).subscribe({
      next: (stats) => this.memberStats.set(stats),
      error: () => {},
    });
  }

  // ---- Table helpers for Personnel ----

  searchEmployee = (emp: EmployeeRow, query: string): boolean => {
    const q = query.toLowerCase().trim();
    if (!q) return true;
    const name = this.employeeName(emp).toLowerCase();
    const pos = (emp.position ?? '').toLowerCase();
    const efgbc = (emp.memberEfgbcId ?? '').toLowerCase();
    return name.includes(q) || pos.includes(q) || efgbc.includes(q);
  };

  employeeName(emp: EmployeeRow): string {
    return emp.memberName || 'Unnamed Staff';
  }

  employeePosition(emp: EmployeeRow): string {
    return emp.position;
  }

  employeeType(emp: EmployeeRow): string {
    return emp.employmentType === 'FulltimeMinister' ? 'Minister' : 'Staff';
  }

  statusChip(status: string): string {
    switch (status) {
      case 'Active':
        return 'bg-emerald-50 text-emerald-700 dark:bg-emerald-950/30 dark:text-emerald-300';
      case 'OnLeave':
        return 'bg-amber-50 text-amber-700 dark:bg-amber-950/30 dark:text-amber-300';
      case 'Terminated':
      case 'Suspended':
        return 'bg-red-50 text-red-700 dark:bg-red-950/30 dark:text-red-300';
      default:
        return 'bg-gray-100 text-gray-700 dark:bg-gray-800 dark:text-gray-300';
    }
  }

  employmentLabel(type: string): string {
    switch (type) {
      case 'FulltimeMinister':
        return 'Full-time Minister';
      case 'DistrictStaff':
        return 'District Staff';
      case 'ChurchStaff':
        return 'Church Staff';
      case 'Volunteer':
        return 'Volunteer';
      default:
        return type;
    }
  }
}
