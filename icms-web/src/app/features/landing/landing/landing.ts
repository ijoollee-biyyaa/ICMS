import { Component, computed, inject } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { MatIcon } from '@angular/material/icon';

import { environment } from '../../../../environments/environment';
import { DistrictService } from '../../../services/district.service';
import { ThemeService } from '../../../services/theme.service';

@Component({
  selector: 'app-landing',
  imports: [RouterLink, MatIcon],
  templateUrl: './landing.html',
  styleUrl: './landing.scss',
})
export class Landing {
  private districtService = inject(DistrictService);
  private themeService = inject(ThemeService);

  readonly isDark = computed(() => this.themeService.theme() === 'dark');

  district = rxResource({
    stream: () => this.districtService.getDistrict(environment.districtId),
  });

  toggleTheme(): void {
    this.themeService.toggle();
  }
}