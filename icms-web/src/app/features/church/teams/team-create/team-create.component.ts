import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { ChurchService } from '../../../../services/church.service';
import { AuthService } from '../../../../services/auth.service';
import { environment } from '../../../../../environments/environment';

export interface TeamNode {
  id: number;
  name: string;
  isCard: boolean;
  rule: string;
  subs: TeamNode[];
}

@Component({
  selector: 'app-team-create',
  imports: [CommonModule, FormsModule, MatIconModule],
  templateUrl: './team-create.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TeamCreateComponent implements OnInit {
  private churchService = inject(ChurchService);
  private auth = inject(AuthService);
  private router = inject(Router);

  churchId = this.auth.currentUser()?.churchId;
  districtId = environment.districtId;

  teams = signal<TeamNode[]>([]);
  
  isModalOpen = signal(false);
  mode = signal<'card' | 'sub' | 'standalone'>('card');
  rule = signal<'Single' | 'Multiple'>('Single');
  parentTeamId = signal<number | null>(null);
  teamName = signal('');
  
  errorMessage = signal<string | null>(null);
  toastMessage = signal<string | null>(null);

  ngOnInit() {
    this.loadTeams();
  }

  loadTeams() {
    if (!this.churchId) return;
    // Mocking real loading for now based on mockup structure until we implement tree loading
    this.teams.set([
      { id: 1, name: 'Choir', isCard: true, rule: 'Single', subs: [
        { id: 2, name: 'Choir A', isCard: false, rule: 'Single', subs: [] },
        { id: 3, name: 'Choir B', isCard: false, rule: 'Single', subs: [] }
      ]},
      { id: 8, name: 'Prayer', isCard: true, rule: 'Multiple', subs: [
        { id: 9, name: 'Friday Prayer', isCard: false, rule: 'Multiple', subs: [] },
        { id: 10, name: 'Monday Prayer', isCard: false, rule: 'Multiple', subs: [] }
      ]},
      { id: 12, name: 'Youth Team', isCard: false, rule: 'Single', subs: [] }
    ]);
  }

  openModal() {
    this.teamName.set('');
    this.errorMessage.set(null);
    this.mode.set('card');
    this.rule.set('Single');
    this.parentTeamId.set(null);
    const cardTeams = this.teams().filter(t => t.isCard);
    if (cardTeams.length > 0) {
      this.parentTeamId.set(cardTeams[0].id);
    }
    this.isModalOpen.set(true);
  }

  closeModal() {
    this.isModalOpen.set(false);
  }

  setMode(m: 'card' | 'sub' | 'standalone') {
    this.mode.set(m);
  }

  setRule(r: 'Single' | 'Multiple') {
    this.rule.set(r);
  }

  confirmCreate() {
    const name = this.teamName().trim();
    if (!name) {
      this.errorMessage.set('Team name is required.');
      return;
    }

    if (!this.churchId) return;

    // Setup payload based on UI state
    let parentId: number | null = null;
      let finalRule = this.rule();
      
      if (this.mode() === 'sub') {
        parentId = this.parentTeamId();
        const parent = this.teams().find(t => t.id === parentId);
        if (parent) finalRule = parent.rule as any;
      } else if (this.mode() === 'standalone') {
        finalRule = 'Single';
      }

    const body = {
      name,
      parentTeamId: parentId,
      teamIsCategory: this.mode() === 'card',
      membershipRule: finalRule
    };

    this.churchService.createTeam(this.churchId, body as any).subscribe({
      next: () => {
        this.closeModal();
        this.showToast(`Team "${name}" created successfully.`);
        this.loadTeams(); // Reload from backend
      },
      error: (err: any) => {
        if (err.status === 409) {
          this.errorMessage.set(`A team named '${name}' already exists.`);
        } else {
          this.errorMessage.set('An error occurred while creating the team.');
        }
      }
    });
  }
  
  showToast(msg: string) {
    this.toastMessage.set(msg);
    setTimeout(() => this.toastMessage.set(null), 3000);
  }
}
