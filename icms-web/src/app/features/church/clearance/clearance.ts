import {
  Component,
  ChangeDetectionStrategy,
  computed,
  inject,
  signal,
  OnInit,
} from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { MatIcon } from '@angular/material/icon';

import { AuthService } from '../../../services/auth.service';
import { ChurchService } from '../../../services/church.service';
import { StatCard } from '../../shared/ui/stat-card/stat-card';
import { PageHeader } from '../../shared/ui/page-header/page-header';
import {
  ClearanceRequest,
  ClearanceStats,
  ClearanceDirection,
  ClearanceRequestStatus,
  ClearanceMemberLookup,
  ClearanceChurchLookup,
} from '../../../models/clearance';
import { problemDetail } from '../../../common/http-errors';
import { environment } from '../../../../environments/environment';
import { jsPDF } from 'jspdf';

@Component({
  selector: 'app-clearance-page',
  imports: [DecimalPipe, MatIcon, FormsModule, StatCard, PageHeader],
  templateUrl: './clearance.html',
  styleUrl: './clearance.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ClearancePage implements OnInit {
  private auth = inject(AuthService);
  private churchService = inject(ChurchService);
  private router = inject(Router);

  readonly churchId = computed(() => this.auth.currentUser()?.churchId ?? null);

  readonly clearances = signal<ClearanceRequest[]>([]);
  readonly incomingTransfers = signal<ClearanceRequest[]>([]);
  readonly stats = signal<ClearanceStats | null>(null);
  readonly loading = signal(false);
  readonly error = signal('');
  readonly totalCount = signal(0);
  readonly page = signal(1);
  readonly pageSize = signal(20);
  readonly query = signal('');
  readonly activeTab = signal<'outgoing' | 'incoming' | 'district' | 'rejoin'>('outgoing');
  readonly selected = signal<ClearanceRequest | null>(null);
  readonly showCreateModal = signal(false);
  readonly createType = signal<'outgoing' | 'incoming' | 'rejoin'>('outgoing');
  readonly saving = signal(false);
  readonly uploading = signal(false);
  readonly uploadProgress = signal('');
  readonly acceptingId = signal<number | null>(null);

  private searchTimer: ReturnType<typeof setTimeout> | null = null;
  readonly Math = Math;

  readonly PAGE_SIZES = [10, 20, 50, 100];

  readonly totalPages = computed(() =>
    Math.max(1, Math.ceil(this.totalCount() / this.pageSize())),
  );
  readonly pageNumbers = computed(() => {
    const total = this.totalPages();
    const current = this.page();
    const start = Math.max(1, Math.min(current - 2, total - 4));
    const end = Math.min(total, start + 4);
    const pages: number[] = [];
    for (let p = start; p <= end; p++) pages.push(p);
    return pages;
  });

  // ---- Outgoing form ----
  readonly outgoingMemberSearch = signal('');
  readonly outgoingMembers = signal<ClearanceMemberLookup[]>([]);
  readonly outgoingSelectedMember = signal<ClearanceMemberLookup | null>(null);
  readonly outgoingDestMode = signal<'sameDistrict' | 'external'>('sameDistrict');
  readonly outgoingDistrictChurchSearch = signal('');
  readonly outgoingDistrictChurches = signal<ClearanceChurchLookup[]>([]);
  readonly outgoingSelectedDestChurch = signal<ClearanceChurchLookup | null>(null);
  readonly outgoingExternalChurch = signal('');
  readonly outgoingExternalDistrict = signal('');
  readonly outgoingNotes = signal('');

  // ---- Incoming form ----
  readonly incomingSourceChurch = signal('');
  readonly incomingSourceDistrict = signal('');
  readonly incomingFirstName = signal('');
  readonly incomingFatherName = signal('');
  readonly incomingGrandfatherName = signal('');
  readonly incomingPreviousEfgbcId = signal('');
  readonly incomingNotes = signal('');
  readonly incomingFileUrl = signal<string | null>(null);

  // ---- Rejoin form ----
  readonly rejoinSearch = signal('');
  readonly rejoinCandidates = signal<ClearanceMemberLookup[]>([]);
  readonly rejoinSelectedCandidate = signal<ClearanceMemberLookup | null>(null);
  readonly rejoinNotes = signal('');
  readonly rejoinFileUrl = signal<string | null>(null);

  ngOnInit() {
    this.loadStats();
    this.load();
  }

  load() {
    const cid = this.churchId();
    if (!cid) return;
    this.loading.set(true);

    if (this.activeTab() === 'incoming') {
      this.churchService
        .getClearances(null, cid, 'Incoming', null, this.query().trim() || null, this.page(), this.pageSize())
        .subscribe({
          next: (page) => {
            page.items.forEach(i => i.direction = i.destinationChurchId === cid ? 'Incoming' : 'Outgoing');
            this.clearances.set(page.items);
            this.totalCount.set(page.totalCount);
            this.loading.set(false);
          },
          error: () => {
            this.error.set('Could not load incoming transfers.');
            this.loading.set(false);
          },
        });
    } else if (this.activeTab() === 'district') {
      this.churchService.getChurch(environment.districtId, cid).subscribe({
        next: (church) => {
          const districtId = (church as any).districtId ?? null;
          if (districtId) {
            this.loadDistrictClearances(districtId);
          } else {
            this.loading.set(false);
          }
        },
        error: () => this.loading.set(false),
      });
    } else {
      this.churchService
        .getClearances(cid, null, 'Outgoing', null, this.query().trim() || null, this.page(), this.pageSize())
        .subscribe({
          next: (page) => {
            page.items.forEach(i => i.direction = i.destinationChurchId === cid ? 'Incoming' : 'Outgoing');
            this.clearances.set(page.items);
            this.totalCount.set(page.totalCount);
            this.loading.set(false);
          },
          error: () => {
            this.error.set('Could not load clearance requests.');
            this.loading.set(false);
          },
        });
    }
  }

  private loadDistrictClearances(districtId: number) {
    const cid = this.churchId();
    this.churchService
      .getClearances(null, null, null, null, this.query().trim() || null, this.page(), this.pageSize())
      .subscribe({
        next: (page) => {
          page.items.forEach(i => i.direction = i.destinationChurchId === cid ? 'Incoming' : 'Outgoing');
          this.clearances.set(page.items);
          this.totalCount.set(page.totalCount);
          this.loading.set(false);
        },
        error: () => {
          this.error.set('Could not load district clearances.');
          this.loading.set(false);
        },
      });
  }

  loadStats() {
    const cid = this.churchId();
    if (!cid) return;
    this.churchService.getClearanceStats(cid).subscribe({
      next: (stats) => this.stats.set(stats),
      error: () => this.stats.set(null),
    });
  }

  onSearch(event: Event) {
    const value = (event.target as HTMLInputElement).value;
    this.query.set(value);
    if (this.searchTimer) clearTimeout(this.searchTimer);
    this.searchTimer = setTimeout(() => {
      this.page.set(1);
      this.load();
    }, 300);
  }

  switchTab(tab: string) {
    this.activeTab.set(tab as 'outgoing' | 'incoming' | 'district' | 'rejoin');
    this.page.set(1);
    this.load();
  }

  goToPage(p: number) {
    if (p < 1 || p > this.totalPages() || p === this.page()) return;
    this.page.set(p);
    this.load();
  }

  changePageSize(event: Event) {
    const size = Number((event.target as HTMLSelectElement).value);
    if (!size) return;
    this.pageSize.set(size);
    this.page.set(1);
    this.load();
  }

  openDetail(item: ClearanceRequest) {
    this.selected.set(item);
  }

  closeDetail() {
    this.selected.set(null);
  }

  // ---- Create modals ----

  openCreate(type: 'outgoing' | 'incoming' | 'rejoin') {
    this.createType.set(type);
    this.error.set('');
    this.resetForm();
    this.showCreateModal.set(true);
  }

  closeCreate() {
    if (this.saving()) return;
    this.showCreateModal.set(false);
  }

  private resetForm() {
    this.outgoingMemberSearch.set('');
    this.outgoingMembers.set([]);
    this.outgoingSelectedMember.set(null);
    this.outgoingDestMode.set('sameDistrict');
    this.outgoingDistrictChurchSearch.set('');
    this.outgoingDistrictChurches.set([]);
    this.outgoingSelectedDestChurch.set(null);
    this.outgoingExternalChurch.set('');
    this.outgoingExternalDistrict.set('');
    this.outgoingNotes.set('');
    this.incomingSourceChurch.set('');
    this.incomingSourceDistrict.set('');
    this.incomingFirstName.set('');
    this.incomingFatherName.set('');
    this.incomingGrandfatherName.set('');
    this.incomingPreviousEfgbcId.set('');
    this.incomingNotes.set('');
    this.incomingFileUrl.set(null);
    this.rejoinSearch.set('');
    this.rejoinCandidates.set([]);
    this.rejoinSelectedCandidate.set(null);
    this.rejoinNotes.set('');
    this.rejoinFileUrl.set(null);
  }

  // ---- Rejoin: search ----

  onRejoinSearch(event: Event) {
    const val = (event.target as HTMLInputElement).value;
    this.rejoinSearch.set(val);

    if (val.length < 2) {
      this.rejoinCandidates.set([]);
      this.rejoinSelectedCandidate.set(null);
      return;
    }

    const cid = this.churchId();
    if (!cid) return;

    this.churchService.searchRejoinCandidates(cid, val).subscribe({
      next: (results) => this.rejoinCandidates.set(results),
      error: () => this.rejoinCandidates.set([]),
    });
  }

  selectRejoinCandidate(c: ClearanceMemberLookup) {
    this.rejoinSelectedCandidate.set(c);
    this.rejoinCandidates.set([]);
    this.rejoinSearch.set(c.fullName);
  }

  // ---- Outgoing: member search ----

  onOutgoingMemberSearch(event: Event) {
    const value = (event.target as HTMLInputElement).value;
    this.outgoingMemberSearch.set(value);
    this.outgoingSelectedMember.set(null);
    const cid = this.churchId();
    if (!cid || value.trim().length < 1) {
      this.outgoingMembers.set([]);
      return;
    }
    this.churchService.searchClearanceMembers(cid, value.trim()).subscribe({
      next: (members) => this.outgoingMembers.set(members),
      error: () => this.outgoingMembers.set([]),
    });
  }

  selectOutgoingMember(member: ClearanceMemberLookup) {
    this.outgoingSelectedMember.set(member);
    this.outgoingMemberSearch.set(member.fullName + ' (' + member.efgbcId + ')');
    this.outgoingMembers.set([]);
  }

  // ---- Outgoing: same-district church search ----

  onOutgoingDestSearch(event: Event) {
    const value = (event.target as HTMLInputElement).value;
    this.outgoingDistrictChurchSearch.set(value);
    this.outgoingSelectedDestChurch.set(null);
    const did = environment.districtId;
    if (!did || value.trim().length < 1) {
      this.outgoingDistrictChurches.set([]);
      return;
    }
    this.churchService.searchDistrictChurches(did, value.trim()).subscribe({
      next: (churches) => this.outgoingDistrictChurches.set(churches),
      error: () => this.outgoingDistrictChurches.set([]),
    });
  }

  selectOutgoingDestChurch(church: ClearanceChurchLookup) {
    this.outgoingSelectedDestChurch.set(church);
    this.outgoingDistrictChurchSearch.set(church.name);
    this.outgoingDistrictChurches.set([]);
  }

  // ---- File upload ----

  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    input.value = '';
    if (!file) return;

    const { cloudName, uploadPreset } = environment.cloudinary;
    if (!cloudName || !uploadPreset) { this.error.set('File upload not configured.'); return; }
    if (file.size > 10 * 1024 * 1024) { this.error.set('File must be 10 MB or smaller.'); return; }

    this.uploading.set(true);
    this.uploadProgress.set('Uploading...');

    const form = new FormData();
    form.append('file', file);
    form.append('upload_preset', uploadPreset);

    fetch(`https://api.cloudinary.com/v1_1/${cloudName}/raw/upload`, { method: 'POST', body: form })
      .then((res) => res.json())
      .then((data) => {
        const url = data?.secure_url as string | undefined;
        this.uploading.set(false);
        if (!url) { this.error.set('Cloudinary rejected the file.'); return; }
        
        if (this.createType() === 'rejoin') {
          this.rejoinFileUrl.set(url);
        } else {
          this.incomingFileUrl.set(url);
        }
        
        this.uploadProgress.set('File uploaded');
      })
      .catch(() => {
        this.uploading.set(false);
        this.error.set('Could not reach the upload service.');
      });
  }

  // ---- Submit create ----

  submitCreate() {
    const cid = this.churchId();
    if (!cid || this.saving()) return;

    this.saving.set(true);
    this.error.set('');

    if (this.createType() === 'outgoing') {
      const member = this.outgoingSelectedMember();
      if (!member) {
        this.error.set('Please select a member.');
        this.saving.set(false);
        return;
      }

      const body: any = {
        churchId: cid,
        memberId: member.id,
        notes: this.outgoingNotes() || undefined,
      };

      if (this.outgoingDestMode() === 'sameDistrict') {
        const dest = this.outgoingSelectedDestChurch();
        if (dest) {
          body.destinationChurchId = dest.id;
        }
        body.type = 'Internal';
      } else {
        body.destinationChurchName = this.outgoingExternalChurch() || undefined;
        body.destinationDistrictOrDenomination = this.outgoingExternalDistrict() || undefined;
        body.type = 'CrossDistrict'; // or CrossDenomination, we can just use CrossDistrict for now
      }

      this.churchService.createOutgoingClearance(cid, body).subscribe({
        next: () => this.onCreateSuccess(),
        error: (err) => this.onCreateError(err, 'outgoing'),
      });
    } else if (this.createType() === 'rejoin') {
      const member = this.rejoinSelectedCandidate();
      if (!member) {
        this.error.set('Please select a deactivated member to rejoin.');
        this.saving.set(false);
        return;
      }

      const body = {
        memberId: member.id,
        recommendationNotes: this.rejoinNotes() || undefined,
        clearanceDocumentUrl: this.rejoinFileUrl() ?? undefined,
      };

      this.churchService.createRejoinClearance(cid, body).subscribe({
        next: (created) => {
          this.saving.set(false);
          this.showCreateModal.set(false);
          this.load();
          this.loadStats();
          // We can navigate to their profile if we want, or just reload the list
        },
        error: (err) => this.onCreateError(err, 'rejoin'),
      });
    } else {
      this.churchService
        .createIncomingClearance(cid, {
          destinationChurchId: cid,
          sourceChurchName: this.incomingSourceChurch() || 'External Church',
          sourceDistrictOrDenomination: this.incomingSourceDistrict() || undefined,
          incomingFirstName: this.incomingFirstName() || '',
          incomingFatherName: this.incomingFatherName() || '',
          incomingGrandfatherName: this.incomingGrandfatherName() || '',
          previousEfgbcId: this.incomingPreviousEfgbcId() || undefined,
          clearanceDocumentUrl: this.incomingFileUrl() ?? undefined,
        })
        .subscribe({
          next: (created) => {
            this.saving.set(false);
            this.showCreateModal.set(false);
            if (created.status === 'Initiated' || created.status === 'Completed') {
              this.router.navigate(['/church/members/register'], {
                queryParams: { clearanceId: created.id },
              });
            } else {
              this.load();
              this.loadStats();
            }
          },
          error: (err) => this.onCreateError(err, 'incoming'),
        });
    }
  }

  registerMemberFromClearance(item: ClearanceRequest) {
    this.closeDetail();
    this.router.navigate(['/church/members/register'], {
      queryParams: { clearanceId: item.id },
    });
  }

  private onCreateSuccess() {
    this.saving.set(false);
    this.showCreateModal.set(false);
    this.load();
    this.loadStats();
  }

  private onCreateError(err: any, type: string) {
    this.saving.set(false);
    this.error.set(problemDetail(err, `Could not create ${type} clearance.`));
  }

  // ---- Accept transfer ----

  acceptTransfer(item: ClearanceRequest) {
    if (this.acceptingId()) return;
    this.acceptingId.set(item.id);
    this.error.set('');

    const cid = this.churchId();
    if (!cid) return;

    this.churchService.acceptTransfer(cid, item.id, {}).subscribe({
      next: () => {
        this.acceptingId.set(null);
        this.selected.set(null);
        this.load();
        this.loadStats();
      },
      error: (err) => {
        this.acceptingId.set(null);
        this.error.set(problemDetail(err, 'Could not accept transfer.'));
      },
    });
  }

  // ---- PDF download ----

  downloadPdf(item: ClearanceRequest) {
    if (!item) return;

    try {
      const doc = new jsPDF();

      // Title
      doc.setFontSize(22);
      doc.setTextColor(34, 197, 94); // emerald-500
      doc.text('Clearance Certificate', 105, 20, { align: 'center' });

      // Info box
      doc.setFontSize(12);
      doc.setTextColor(50, 50, 50);
      doc.text(`Clearance Code: ${item.clearanceCode || 'N/A'}`, 20, 40);
      doc.text(`Date Issued: ${this.formatDate(item.initiatedAt)}`, 20, 50);
      doc.text(`Status: ${item.status}`, 20, 60);

      // Member details
      doc.setFontSize(16);
      doc.setTextColor(0, 0, 0);
      doc.text('Member Information', 20, 80);
      
      doc.setFontSize(12);
      doc.setTextColor(50, 50, 50);
      doc.text(`Name: ${item.memberName || 'N/A'}`, 20, 90);
      doc.text(`ID (EFGBC): ${item.memberEfgbcId || 'N/A'}`, 20, 100);

      // Transfer details
      doc.setFontSize(16);
      doc.setTextColor(0, 0, 0);
      doc.text('Transfer Details', 20, 120);
      
      doc.setFontSize(12);
      doc.setTextColor(50, 50, 50);
      doc.text(`From (Source): ${item.sourceChurchName || 'External/Unknown'}`, 20, 130);
      doc.text(`To (Destination): ${item.destinationChurchName || item.destinationDistrictOrDenomination || 'Unknown'}`, 20, 140);
      doc.text(`Type: ${item.type}`, 20, 150);

      // Notes
      if (item.recommendationNotes) {
        doc.setFontSize(14);
        doc.setTextColor(0, 0, 0);
        doc.text('Notes', 20, 170);
        
        doc.setFontSize(11);
        doc.setTextColor(100, 100, 100);
        const splitNotes = doc.splitTextToSize(item.recommendationNotes, 170);
        doc.text(splitNotes, 20, 180);
      }

      // Footer signature
      doc.setFontSize(10);
      doc.setTextColor(150, 150, 150);
      doc.text('_____________________________', 140, 260);
      doc.text('Authorized Signature', 150, 265);

      doc.save(`Clearance_${item.clearanceCode || 'Certificate'}.pdf`);
    } catch (e) {
      console.error(e);
      this.error.set('Failed to generate PDF. Ensure jspdf is installed.');
    }
  }

  // ---- Helpers ----

  directionLabel(d: ClearanceDirection): string {
    switch (d) {
      case 'Incoming': return 'Incoming';
      case 'Outgoing': return 'Outgoing';
      default: return d;
    }
  }

  directionClass(d: ClearanceDirection): string {
    switch (d) {
      case 'Incoming':
        return 'bg-emerald-100 text-emerald-800 dark:bg-emerald-500/15 dark:text-emerald-300';
      case 'Outgoing':
        return 'bg-rose-100 text-rose-800 dark:bg-rose-500/15 dark:text-rose-300';
      default:
        return 'bg-gray-100 text-gray-800 dark:bg-gray-500/15 dark:text-gray-300';
    }
  }

  statusClass(s: ClearanceRequestStatus): string {
    switch (s) {
      case 'Initiated': return 'bg-amber-100 text-amber-800 dark:bg-amber-500/15 dark:text-amber-300';
      case 'Completed': return 'bg-emerald-100 text-emerald-800 dark:bg-emerald-500/15 dark:text-emerald-300';
      case 'Voided': return 'bg-red-100 text-red-800 dark:bg-red-500/15 dark:text-red-300';
      default: return 'bg-gray-100 text-gray-800 dark:bg-gray-500/15 dark:text-gray-300';
    }
  }

  formatDate(dateStr: string | null): string {
    if (!dateStr) return '—';
    const d = new Date(dateStr);
    return isNaN(d.getTime()) ? dateStr : d.toLocaleDateString();
  }

  formatDateTime(dateStr: string | null): string {
    if (!dateStr) return '—';
    const d = new Date(dateStr);
    return isNaN(d.getTime()) ? dateStr : d.toLocaleDateString() + ' ' + d.toLocaleTimeString();
  }
}
