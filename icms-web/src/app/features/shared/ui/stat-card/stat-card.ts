import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { MatIcon } from '@angular/material/icon';

@Component({
  selector: 'app-stat-card',
  imports: [MatIcon],
  templateUrl: './stat-card.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StatCard {
  readonly icon = input.required<string>();
  readonly label = input.required<string>();
  readonly value = input<string | null>(null);
  /** Tailwind color utility classes for the icon chip background. */
  readonly accentClass = input('bg-brand-50 text-brand-600');
  readonly hint = input<string | null>(null);
  readonly trend = input<string | null>(null);
  readonly trendClass = input('text-emerald-600');
}