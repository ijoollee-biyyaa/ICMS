import {
  Component,
  computed,
  effect,
  inject,
  signal,
  OnInit,
} from '@angular/core';
import { CurrencyPipe, DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatIcon } from '@angular/material/icon';
import { MatIconButton } from '@angular/material/button';

import { ChurchStore } from '../../../stores/church.store';
import { ChurchService } from '../../../services/church.service';
import { Church } from '../../../models/church';
import { EmployeeRow } from '../../../models/district';
import { Member } from '../../../models/member';
import { AppTable } from '../../shared/ui/data-table/data-table';
import { TableColumn } from '../../shared/ui/data-table/table-column';
import { StatCard } from '../../shared/ui/stat-card/stat-card';

@Component({
  selector: 'app-district-ministers',
  imports: [
    MatIcon,
    MatIconButton,
    CurrencyPipe,
    DatePipe,
    AppTable,
    TableColumn,
    StatCard,
  ],
  templateUrl: './ministers.html',
  styleUrl: './ministers.scss',
})
export class DistrictMinisters implements OnInit {
  readonly store = inject(ChurchStore);
  private readonly churchService = inject(ChurchService);

  readonly selectedChurch = signal<Church | null>(null);
  readonly ministers = signal<EmployeeRow[]>([]);
  readonly loadingMinisters = signal<boolean>(false);
  readonly activeCardIndex = signal<number>(0);

  readonly selectedMinisterForDossier = signal<EmployeeRow | null>(null);
  readonly dossierMember = signal<Member | null>(null);
  readonly loadingDossier = signal<boolean>(false);

  readonly filterOnlyWithMinisters = signal<boolean>(false);

  readonly activeMinister = computed(() => {
    const list = this.ministers();
    const idx = this.activeCardIndex();
    return list[idx] ?? null;
  });

  readonly totalMinisters = computed(() => this.ministers().length);

  readonly totalDistrictMinisters = computed(() =>
    this.store.churches().reduce((acc, c) => acc + (c.ministerCount ?? 0), 0),
  );

  readonly churchesWithMinistersCount = computed(
    () => this.store.churches().filter((c) => (c.ministerCount ?? 0) > 0).length,
  );

  readonly displayedChurches = computed(() => {
    const list = this.store.churches();
    if (this.filterOnlyWithMinisters()) {
      return list.filter((c) => (c.ministerCount ?? 0) > 0);
    }
    return list;
  });

  constructor() {
    this.store.load();

    effect(
      () => {
        const churches = this.store.churches();
        if (churches.length > 0 && !this.selectedChurch()) {
          const withMinisters = churches.find((c) => (c.ministerCount ?? 0) > 0);
          this.selectChurch(withMinisters ?? churches[0]);
        }
      },
      { allowSignalWrites: true },
    );
  }

  ngOnInit() {}

  selectChurch(church: Church) {
    this.selectedChurch.set(church);
    this.activeCardIndex.set(0);
    this.loadingMinisters.set(true);

    this.churchService.getEmployees(church.id, 1, 100).subscribe({
      next: (page) => {
        const ministerList = page.items.filter(
          (e) => e.employmentType === 'FulltimeMinister',
        );
        this.ministers.set(ministerList);
        this.loadingMinisters.set(false);
      },
      error: () => {
        this.ministers.set([]);
        this.loadingMinisters.set(false);
      },
    });
  }

  nextCard() {
    const total = this.totalMinisters();
    if (total > 0) {
      this.activeCardIndex.update((i) => (i + 1) % total);
    }
  }

  prevCard() {
    const total = this.totalMinisters();
    if (total > 0) {
      this.activeCardIndex.update((i) => (i - 1 + total) % total);
    }
  }

  setCardIndex(index: number) {
    if (index >= 0 && index < this.totalMinisters()) {
      this.activeCardIndex.set(index);
    }
  }

  openDossier(minister: EmployeeRow) {
    this.selectedMinisterForDossier.set(minister);
    this.dossierMember.set(null);

    if (minister.memberId) {
      this.loadingDossier.set(true);
      this.churchService.getMember(minister.memberId).subscribe({
        next: (m) => {
          this.dossierMember.set(m);
          this.loadingDossier.set(false);
        },
        error: () => {
          this.loadingDossier.set(false);
        },
      });
    }
  }

  closeDossier() {
    this.selectedMinisterForDossier.set(null);
    this.dossierMember.set(null);
  }

  toggleFilterMinistersOnly() {
    this.filterOnlyWithMinisters.update((v) => !v);
  }

  // ---- Table helpers ----

  searchChurch = (church: Church, query: string): boolean => {
    const q = query.toLowerCase().trim();
    if (!q) return true;
    return (
      church.name.toLowerCase().includes(q) ||
      church.code.toLowerCase().includes(q) ||
      (church.city?.toLowerCase().includes(q) ?? false) ||
      (church.subcity?.toLowerCase().includes(q) ?? false)
    );
  };

  churchName = (c: Church) => c.name;
  churchCode = (c: Church) => c.code;
  churchType = (c: Church) => c.type;
  churchLocation = (c: Church) => [c.subcity, c.city].filter(Boolean).join(', ');
  churchMinisters = (c: Church) => c.ministerCount ?? 0;
  churchMembers = (c: Church) => c.memberCount ?? 0;

  ministerName(m: EmployeeRow | null): string {
    if (!m) return '—';
    return m.memberName || 'Unnamed Minister';
  }

  memberFullName(m: Member | null): string {
    if (!m) return '—';
    return [m.firstName, m.fatherName, m.grandfatherName].filter(Boolean).join(' ');
  }

  memberLocation(m: Member | null): string {
    if (!m) return '—';
    return [m.localAddress, m.subcity, m.city].filter(Boolean).join(', ') || '—';
  }

  initials(name: string): string {
    const parts = name.trim().split(/\s+/);
    if (parts.length >= 2) {
      return (parts[0][0] + parts[1][0]).toUpperCase();
    }
    return (name[0] || 'M').toUpperCase();
  }
}
