import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MatIcon } from '@angular/material/icon';

import { AuthService } from '../../../services/auth.service';
import { MemberStore } from '../../../stores/member.store';
import { TeamMeetingsManager } from '../../shared/team-meetings-manager/team-meetings-manager';
import { TeamPaymentsManager } from '../../shared/team-payments-manager/team-payments-manager';

@Component({
  selector: 'app-member-team-meetings',
  imports: [RouterLink, MatIcon, TeamMeetingsManager, TeamPaymentsManager],
  templateUrl: './member-team-meetings.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MemberTeamMeetings {
  private auth = inject(AuthService);
  private route = inject(ActivatedRoute);
  readonly store = inject(MemberStore);

  readonly teamId = Number(this.route.snapshot.paramMap.get('id'));
  readonly currentTeam = computed(
    () => this.store.dashboard()?.teams.find((t) => t.teamId === this.teamId) ?? null,
  );
  readonly isLeader = computed(() => this.currentTeam()?.role === 'Leader');
  readonly churchId = computed(() => this.currentTeam()?.churchId ?? null);
  readonly teamName = computed(() => this.currentTeam()?.teamName ?? null);

  constructor() {
    this.store.load();
  }
}