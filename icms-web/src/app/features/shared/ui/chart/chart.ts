import {
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  effect,
  input,
  OnDestroy,
  viewChild,
} from '@angular/core';
import ApexCharts, { ApexOptions } from 'apexcharts';
import { ThemeService } from '../../../../services/theme.service';

export type ChartType = 'bar' | 'line' | 'area' | 'donut';

export interface ChartSeriesData {
  name: string;
  data: number[];
}

/**
 * Imperative ApexCharts wrapper: renders a chart into a sized container,
 * updates it when inputs or the app theme change, and reacts to container
 * resizes so the chart never ends up invisible.
 */
@Component({
  selector: 'app-chart',
  imports: [],
  template: `<div #chartEl class="w-full"></div>`,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ChartComponent implements OnDestroy {
  readonly chartEl = viewChild<ElementRef<HTMLDivElement>>('chartEl');

  readonly categories = input<string[]>([]);
  readonly series = input<ChartSeriesData[]>([]);
  readonly type = input<ChartType>('bar');
  readonly labels = input<string[]>([]);
  readonly height = input(320);
  readonly colors = input<string[]>(['#f048a8', '#1878c0', '#10b981', '#f59e0b']);

  private chart: ApexCharts | null = null;
  private resizeObserver: ResizeObserver | null = null;

  constructor(private theme: ThemeService) {
    effect(() => {
      const el = this.chartEl()?.nativeElement;
      if (!el) return;
      // Read theme (creates a dependency so this effect reruns on theme change).
      const dark = this.theme.theme() === 'dark';
      this.render(el, dark);
    });
  }

  ngOnDestroy(): void {
    this.resizeObserver?.disconnect();
    if (this.chart) {
      this.chart.destroy();
      this.chart = null;
    }
  }

  private render(el: HTMLDivElement, dark: boolean): void {
    el.style.height = `${this.height()}px`;

    const hasData = this.series().some((s) => (s.data?.length ?? 0) > 0);
    if (!hasData) {
      this.resizeObserver?.disconnect();
      this.resizeObserver = null;
      if (this.chart) {
        this.chart.destroy();
        this.chart = null;
        el.innerHTML = '';
      }
      return;
    }

    const options = this.buildOptions(dark);

    if (!this.chart) {
      this.chart = new ApexCharts(el, options);
      void this.chart.render();
      this.resizeObserver?.disconnect();
      this.resizeObserver = new ResizeObserver(() => {
        (this.chart as unknown as { resize?: () => void } | null)?.resize?.();
      });
      this.resizeObserver.observe(el);
    } else {
      void this.chart.updateOptions(options, false, true);
    }
  }

  private buildOptions(dark: boolean): ApexOptions {
    const type = this.type();
    const axis = dark ? '#94a3b8' : '#94a3b8';
    const axisStrong = dark ? '#cbd5e1' : '#64748b';
    const grid = dark ? '#1e293b' : '#e2e8f0';

    const common: ApexOptions = {
      chart: {
        type,
        toolbar: { show: false },
        fontFamily: 'Roboto, sans-serif',
        foreColor: axis,
        animations: { enabled: true },
        background: 'transparent',
      },
      dataLabels: { enabled: false },
      grid: { borderColor: grid, strokeDashArray: 4 },
      colors: this.colors() ?? ['#f048a8', '#1878c0', '#10b981', '#f59e0b'],
      legend: {
        position: 'bottom',
        horizontalAlign: 'center',
        labels: { colors: axisStrong },
      },
      tooltip: { theme: dark ? 'dark' : 'light' },
      stroke: { curve: 'smooth', width: 3 },
      fill: type === 'area' ? { type: 'gradient' } : { type: 'solid' },
    };

    if (type === 'donut') {
      return {
        ...common,
        chart: { ...common.chart, type: 'donut' },
        series: this.series()[0]?.data ?? [],
        labels: this.labels(),
        plotOptions: {
          pie: {
            donut: { size: '72%', labels: { show: true, total: { show: true } } },
          },
        },
        dataLabels: { enabled: true, formatter: (v) => `${v}` },
      };
    }

    return {
      ...common,
      series: this.series().map((s) => ({ name: s.name, data: s.data ?? [] })),
      xaxis: {
        categories: this.categories(),
        axisBorder: { show: false },
        axisTicks: { show: false },
        labels: { style: { colors: axis } },
      },
      yaxis: { labels: { style: { colors: axis } } },
    };
  }
}