import { Component, computed, inject, signal } from '@angular/core';
import { MatIcon } from '@angular/material/icon';

import { AuthService } from '../../../services/auth.service';
import { ChurchService } from '../../../services/church.service';
import { toSignal } from '@angular/core/rxjs-interop';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-church-profile',
  imports: [MatIcon],
  templateUrl: './church-profile.html',
  styleUrl: './church-profile.scss',
})
export class ChurchProfile {
  private auth = inject(AuthService);
  private churchService = inject(ChurchService);

  readonly churchId = computed(() => this.auth.currentUser()?.churchId ?? null);

  readonly church =
    this.churchId() !== null
      ? toSignal(
          this.churchService.getChurch(environment.districtId, this.churchId()!),
          { initialValue: null },
        )
      : signal(null);

  location(city: string | null, subcity: string | null) {
    return [subcity, city].filter(Boolean).join(', ') || '—';
  }
}
