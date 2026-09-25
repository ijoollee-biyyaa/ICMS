import { ContentChild, Directive, TemplateRef, input, signal } from '@angular/core';

/**
 * Declares one column of `AppTable`. Put an `<app-table-column key="…">` inside
 * `<app-table>` and give it a cell template:
 *
 *   <app-table-column key="amount" header="Amount" [sortable]="true">
 *     <ng-template let-row>{{ row.amount | number }}</ng-template>
 *   </app-table-column>
 */
@Directive({
  selector: 'app-table-column',
})
export class TableColumn {
  readonly key = input<string>('');
  readonly header = input<string>('');
  readonly sortable = input(false);
  /** Optional function to derive a value for sorting when row[key] isn't a plain value. */
  readonly sortValue = input<((row: any) => any) | null>(null);
  readonly align = input<'left' | 'center' | 'right'>('left');
  readonly headerClass = input<string>('');
  readonly cellClass = input<string>('');

  @ContentChild(TemplateRef)
  readonly cellTpl!: TemplateRef<unknown>;

  readonly ready = signal(false);
}