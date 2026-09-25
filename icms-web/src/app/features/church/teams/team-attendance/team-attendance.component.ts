import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { ChurchService } from '../../../../services/church.service';
import { AuthService } from '../../../../services/auth.service';
import { environment } from '../../../../../environments/environment';

export interface RosterMember {
  id: number;
  name: string;
  efgbc: string;
  role: string;
}

export interface AttendanceDraft {
  status: 'Present' | 'Late' | 'Absent';
  reason: string;
}

export interface AttendanceRecord {
  date: string;
  member: RosterMember;
  e: { memberId: number; status: string; reason: string };
}

@Component({
  selector: 'app-team-attendance',
  imports: [CommonModule, FormsModule, MatIconModule],
  templateUrl: './team-attendance.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TeamAttendanceComponent implements OnInit {
  private churchService = inject(ChurchService);
  private auth = inject(AuthService);
  private route = inject(ActivatedRoute);

  churchId = this.auth.currentUser()?.churchId;
  districtId = environment.districtId;
  teamId = signal<number>(0);

  // Mocks for now based on UI mockup
  teams = signal<any[]>([
    { id: 1, name: 'Choir Team', card: true, rule: 'Single' },
    { id: 2, name: 'Choir A', parent: 1, rule: 'Single' },
    { id: 3, name: 'Choir B', parent: 1, rule: 'Single' },
    { id: 9, name: 'Friday Prayer', parent: 8, rule: 'Multiple' },
    { id: 10, name: 'Monday Prayer', parent: 8, rule: 'Multiple' },
    { id: 12, name: 'Youth Team', rule: 'Single' },
    { id: 13, name: 'Media Team', rule: 'Single' }
  ]);

  currentTeam = signal<any>(null);
  currentDate = signal<string>(new Date().toISOString().split('T')[0]);
  
  members = signal<RosterMember[]>([]);
  draft = signal<Record<number, AttendanceDraft>>({});
  
  existingRecords = signal<AttendanceRecord[]>([]);
  summaryStats = signal<any[]>([]);

  activeTab = signal<'mark' | 'grid' | 'report'>('mark');
  
  toastMessage = signal<string | null>(null);
  errorMessage = signal<string | null>(null);

  ngOnInit() {
    // Optionally pre-select team from query param or just load list
  }

  loadRoster() {
    this.errorMessage.set(null);
    const selected = this.teams().find(t => t.id === this.teamId());
    if (!selected) return;
    
    this.currentTeam.set(selected);

    if (this.currentTeam()?.card) {
      this.errorMessage.set(`409 team_is_category: ${this.currentTeam().name} is a category (card). Attendance belongs to its sub-teams.`);
      return;
    }

    // Mocking API call
    this.members.set([
      { id: 3, name: 'Abebe Kebede', efgbc: 'EFGBC-NWAA-000001', role: 'Leader' },
      { id: 6, name: 'Helen Bekele', efgbc: 'EFGBC-NWAA-000004', role: 'Member' }
    ]);
    
    const draftObj: Record<number, AttendanceDraft> = {};
    this.members().forEach(m => {
      draftObj[m.id] = { status: 'Absent', reason: '' };
    });
    this.draft.set(draftObj);
  }

  updateDraftStatus(mid: number, status: any) {
    const d = { ...this.draft() };
    d[mid].status = status;
    if (status !== 'Absent') {
      d[mid].reason = '';
    }
    this.draft.set(d);
  }

  updateDraftReason(mid: number, reason: string) {
    const d = { ...this.draft() };
    d[mid].reason = reason;
    this.draft.set(d);
  }

  async saveAttendance() {
    if (this.currentTeam()?.card) return;

    const today = new Date().toISOString().split('T')[0];
    if (this.currentDate() > today) {
      this.errorMessage.set('Attendance cannot be marked for a future date.');
      return;
    }

    const entries = Object.keys(this.draft()).map(mid => ({
      memberId: Number(mid),
      status: this.draft()[Number(mid)].status,
      reason: this.draft()[Number(mid)].status === 'Absent' ? this.draft()[Number(mid)].reason : ''
    }));

    try {
      // Mock API call
      // await this.churchService.saveTeamAttendance(this.districtId, this.churchId, this.teamId(), { attendanceDate: this.currentDate(), entries });
      this.showToast(`Saved ${entries.length} record(s) for ${this.currentDate()}.`);
      
      // Update local grid view data (mocking)
      const date = this.currentDate();
      const newRecs = entries.map(e => ({
        date,
        member: this.members().find(m => m.id === e.memberId)!,
        e
      }));
      this.existingRecords.set([...this.existingRecords(), ...newRecs]);
      
    } catch (err: any) {
      this.errorMessage.set('Error saving attendance.');
    }
  }

  setTab(tab: 'mark' | 'grid' | 'report') {
    this.activeTab.set(tab);
    if (tab === 'grid') {
      // load grid
    } else if (tab === 'report') {
      // load report
      this.summaryStats.set(this.members().map(m => ({
        m,
        attended: 1,
        total: 2,
        rate: 50
      })));
    }
  }

  showToast(msg: string) {
    this.toastMessage.set(msg);
    setTimeout(() => this.toastMessage.set(null), 3000);
  }
}
