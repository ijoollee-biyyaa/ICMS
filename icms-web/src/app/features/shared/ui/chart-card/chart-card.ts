import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { ChartComponent, ChartSeriesData } from '../chart/chart';

export type ChartType = 'bar' | 'line' | 'area' | 'donut';

/**
 * Tailadmin-style card that wraps `app-chart` with a title + subtitle header.
 * Reusable across dashboards for a consistent "chart in a card" look.
 */
@Component({
  selector: 'app-chart-card',
  imports: [ChartComponent],
  templateUrl: './chart-card.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ChartCard {
  readonly title = input('');
  readonly subtitle = input('');
  readonly type = input<ChartType>('area');
  readonly categories = input<string[]>([]);
  readonly labels = input<string[]>([]);
  readonly series = input<ChartSeriesData[]>([]);
  readonly height = input(320);
  readonly colors = input<string[]>(['#f048a8', '#1878c0', '#10b981', '#f59e0b']);
}