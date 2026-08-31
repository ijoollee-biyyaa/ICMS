import { Component, computed, inject } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { MatIcon } from '@angular/material/icon';

import { AuthService } from '../../../services/auth.service';

interface AreaCard {
  label: string;
  description: string;
  icon: string;
  path: string;
}

@Component({
  selector: 'app-area-select',
  imports: [RouterLink, MatIcon],
  templateUrl: './area-select.html',
  styleUrl: './area-select.scss',
})
export class AreaSelect {
  private auth = inject(AuthService);
  private router = inject(Router);

  readonly displayName = computed(() => this.auth.currentUser()?.displayName ?? '');

  readonly areas = computed<AreaCard[]>(() => {
    const roles = this.auth.roles();
    const cards: AreaCard[] = [];
    if (roles.includes('Admin') || roles.includes('DistrictSubAdmin')) {
      cards.push({
        label: 'District Office',
        description: 'Manage the district, its churches, employees and reports.',
        icon: 'account_balance',
        path: '/district',
      });
    }
    if (roles.includes('ChurchAdmin')) {
      cards.push({
        label: 'Church Administration',
        description: 'Manage your church, members, ministers and office.',
        icon: 'church',
        path: '/church',
      });
    }
    if (roles.includes('Member')) {
      cards.push({
        label: 'Member Area',
        description: 'Your membership, teams and attendance.',
        icon: 'person',
        path: '/member',
      });
    }
    return cards;
  });

  go(path: string) {
    void this.router.navigate([path]);
  }
}
