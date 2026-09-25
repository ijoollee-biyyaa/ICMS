import {
  ChangeDetectionStrategy,
  Component,
  contentChildren,
  computed,
  input,
  signal,
} from '@angular/core';
import { NgClass, NgTemplateOutlet } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatIcon } from '@angular/material/icon';
import { TableColumn } from './table-column';

type SortDir = 'asc' | 'desc';

/**
 * Polished, pure Tailwind CSS datatable with client-side search, sort and pagination.
 * Declare columns with `<app-table-column>` (each holds an `<ng-template>`
 * cell renderer). All styling is dark-mode aware.
 */
@Component({
  selector: 'app-table',
  imports: [MatIcon, FormsModule, NgClass, NgTemplateOutlet],
  templateUrl: './data-table.html',
  styleUrls: ['./data-table.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AppTable {
  readonly columns = contentChildren(TableColumn);

  /** Rows to display. */
  readonly data = input.required<any[]>();

  /** Data keys used by the global search box. */
  readonly searchKeys = input<string[]>([]);

  /** Optional custom search predicate: (row, query) => boolean. */
  readonly searchFn = input<((row: any, query: string) => boolean) | null>(null);

  /** Optional stable identifier for @for tracking. Defaults to index. */
  readonly rowKey = input<(row: any, index: number) => any>((_, i) => i);

  readonly title = input('');
  readonly subtitle = input('');
  readonly showToolbar = input(true);
  readonly showSearch = input(true);
  readonly searchPlaceholder = input('Search…');

  readonly pageSize = signal(10);
  readonly pageSizeOptions = input<number[]>([5, 10, 25, 50]);

  readonly query = signal('');
  readonly sortSignal = signal<{ key: string; dir: SortDir } | null>(null);
  readonly currentPage = signal(0);

  readonly displayedColumns = computed(() =>
    this.columns()
      .map((c) => c.key())
      .filter(Boolean),
  );

  trackRow(index: number, row: any): any {
    return this.rowKey()(row, index);
  }

  readonly filtered = computed(() => {
    let rows = this.data() ?? [];
    const q = this.query().trim().toLowerCase();

    if (q) {
      const customSearch = this.searchFn();
      if (customSearch) {
        rows = rows.filter((row) => customSearch(row, q));
      } else {
        const keys = this.searchKeys();
        if (keys.length > 0) {
          rows = rows.filter((row) =>
            keys.some((k) =>
              String((row as Record<string, unknown>)[k] ?? '')
                .toLowerCase()
                .includes(q),
            ),
          );
        } else {
          rows = rows.filter((row) =>
            Object.values(row as Record<string, unknown>).some((v) =>
              String(v ?? '')
                .toLowerCase()
                .includes(q),
            ),
          );
        }
      }
    }

    const sort = this.sortSignal();
    if (sort && sort.key) {
      const dir = sort.dir === 'asc' ? 1 : -1;
      const colDef = this.columns().find((c) => c.key() === sort.key);
      const sortValGetter = colDef?.sortValue();

      rows = [...rows].sort((a, b) => {
        let av: any;
        let bv: any;
        if (sortValGetter) {
          av = sortValGetter(a);
          bv = sortValGetter(b);
        } else {
          av = (a as Record<string, unknown>)[sort.key];
          bv = (b as Record<string, unknown>)[sort.key];
        }

        const cmp = compare(av, bv);
        if (cmp !== 0) return cmp * dir;

        // Deterministic secondary tie-breaker
        const keyA = (a.id ?? a.userId ?? JSON.stringify(a)).toString();
        const keyB = (b.id ?? b.userId ?? JSON.stringify(b)).toString();
        return keyA.localeCompare(keyB) * dir;
      });
    }

    return rows;
  });

  readonly pagedRows = computed(() => {
    const start = this.currentPage() * this.pageSize();
    return this.filtered().slice(start, start + this.pageSize());
  });

  readonly totalFiltered = computed(() => this.filtered().length);

  readonly pageCount = computed(() =>
    Math.max(1, Math.ceil(this.totalFiltered() / this.pageSize())),
  );

  readonly startItem = computed(() =>
    this.totalFiltered() === 0 ? 0 : this.currentPage() * this.pageSize() + 1,
  );

  readonly endItem = computed(() =>
    Math.min(
      (this.currentPage() + 1) * this.pageSize(),
      this.totalFiltered(),
    ),
  );

  readonly rangeText = computed(() => {
    const total = this.totalFiltered();
    if (total === 0) return '0 of 0';
    return `${this.startItem()}–${this.endItem()} of ${total}`;
  });

  readonly visiblePages = computed(() => {
    const total = this.pageCount();
    const cur = this.currentPage();
    const delta = 2;
    const range: number[] = [];
    for (
      let i = Math.max(0, cur - delta);
      i <= Math.min(total - 1, cur + delta);
      i++
    ) {
      range.push(i);
    }
    return range;
  });

  toggleSort(key: string) {
    if (!key) return;
    const current = this.sortSignal();
    if (!current || current.key !== key) {
      this.sortSignal.set({ key, dir: 'asc' });
    } else {
      this.sortSignal.set({ key, dir: current.dir === 'asc' ? 'desc' : 'asc' });
    }
    this.currentPage.set(0);
  }

  sortClass(key: string): string {
    const s = this.sortSignal();
    if (!s || s.key !== key) return '';
    return s.dir === 'asc' ? 'asc' : 'desc';
  }

  onQuery() {
    this.currentPage.set(0);
  }

  clearSearch() {
    this.query.set('');
    this.currentPage.set(0);
  }

  setPage(page: number) {
    if (page >= 0 && page < this.pageCount()) {
      this.currentPage.set(page);
    }
  }

  firstPage() {
    this.currentPage.set(0);
  }

  lastPage() {
    this.currentPage.set(this.pageCount() - 1);
  }

  prev() {
    if (this.currentPage() > 0) {
      this.currentPage.update((p) => p - 1);
    }
  }

  next() {
    if (this.currentPage() < this.pageCount() - 1) {
      this.currentPage.update((p) => p + 1);
    }
  }

  setPageSize(size: number | string) {
    this.pageSize.set(Number(size));
    this.currentPage.set(0);
  }
}

function compare(a: unknown, b: unknown): number {
  if (a === b) return 0;
  if (a === null || a === undefined) return 1;
  if (b === null || b === undefined) return -1;
  if (typeof a === 'number' && typeof b === 'number') return a - b;
  if (typeof a === 'boolean' && typeof b === 'boolean')
    return a === b ? 0 : a ? -1 : 1;
  return String(a).localeCompare(String(b), undefined, {
    numeric: true,
    sensitivity: 'base',
  });
}