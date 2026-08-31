import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  signal,
} from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { MatIconButton } from '@angular/material/button';
import {
  MatError,
  MatFormField,
  MatLabel,
  MatHint,
} from '@angular/material/form-field';
import { MatInput } from '@angular/material/input';
import { MatOption, MatSelect } from '@angular/material/select';
import { MatIcon } from '@angular/material/icon';
import { forkJoin } from 'rxjs';

import { AuthService } from '../../../services/auth.service';
import { ChurchService } from '../../../services/church.service';
import { Team, TeamDetail, MembershipRule } from '../../../models/team';
import {
  problemDetail,
  applyFieldErrors,
  clearFieldErrors,
} from '../../../common/http-errors';

@Component({
  selector: 'app-church-teams',
  imports: [
    ReactiveFormsModule,
    MatFormField,
    MatLabel,
    MatHint,
    MatError,
    MatInput,
    MatSelect,
    MatOption,
    MatIconButton,
    MatIcon,
    RouterLink,
  ],
  templateUrl: './church-teams.html',
  styleUrl: './church-teams.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ChurchTeams {
  private auth = inject(AuthService);
  private fb = inject(FormBuilder);
  private churchService = inject(ChurchService);

  readonly churchId = computed(() => this.auth.currentUser()?.churchId ?? null);

  readonly teams = signal<Team[]>([]);
  readonly details = signal<Record<number, TeamDetail>>({});
  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly deleting = signal(false);
  readonly error = signal('');
  readonly showForm = signal(false);
  readonly editing = signal<Team | null>(null);
  readonly toDelete = signal<Team | null>(null);

  form = this.fb.nonNullable.group({
    name: ['', Validators.required],
    membershipRule: ['Single' as MembershipRule],
    parentTeamId: ['' as number | ''],
  });

  readonly mainTeams = computed(() => {
    const detail = this.details();
    return this.teams()
      .filter((t) => t.parentTeamId === null)
      .map((t) => ({
        team: t,
        detail: detail[t.id],
        subTeams: this.teams().filter((s) => s.parentTeamId === t.id),
      }));
  });

  readonly standalone = computed(() =>
    this.teams().filter((t) => t.parentTeamId !== null),
  );

  readonly memberTotal = computed(() => {
    const detail = this.details();
    return this.teams().reduce((sum, t) => sum + (detail[t.id]?.memberCount ?? 0), 0);
  });

  constructor() {
    this.load();
  }

  load() {
    const id = this.churchId();
    if (!id) return;
    this.loading.set(true);
    this.error.set('');
    this.churchService.getTeams(id, 1, 100).subscribe({
      next: (page) => {
        const teams = page.items;
        this.teams.set(teams);
        if (teams.length === 0) {
          this.loading.set(false);
          return;
        }
        forkJoin(
          teams.map((t) => this.churchService.getTeam(id, t.id)),
        ).subscribe({
          next: (details) => {
            const map: Record<number, TeamDetail> = {};
            for (const d of details) map[d.id] = d;
            this.details.set(map);
            this.loading.set(false);
          },
          error: () => {
            this.loading.set(false);
            this.error.set('Could not load team details.');
          },
        });
      },
      error: () => {
        this.loading.set(false);
        this.error.set('Could not load teams.');
      },
    });
  }

  openCreate(parentTeam: Team | null = null) {
    this.error.set('');
    this.editing.set(null);
    this.showForm.set(true);
    this.form.reset({
      name: '',
      membershipRule: 'Single',
      parentTeamId: parentTeam ? parentTeam.id : '',
    });
    clearFieldErrors(this.form);
  }

  openEdit(team: Team) {
    this.error.set('');
    this.editing.set(team);
    this.showForm.set(true);
    this.form.reset({
      name: team.name,
      membershipRule: team.membershipRule,
      parentTeamId: '',
    });
    clearFieldErrors(this.form);
  }

  closeForm() {
    if (this.saving()) return;
    this.showForm.set(false);
    this.editing.set(null);
  }

  submit() {
    const churchId = this.churchId();
    if (!churchId || this.form.invalid || this.saving()) return;

    clearFieldErrors(this.form);
    this.saving.set(true);
    this.error.set('');

    const v = this.form.getRawValue();
    const editing = this.editing();
    if (editing) {
      this.churchService
        .updateTeam(churchId, editing.id, { name: v.name })
        .subscribe({
          next: (updated) => {
            this.saving.set(false);
            this.teams.set(
              this.teams().map((t) => (t.id === editing.id ? updated : t)),
            );
            this.showForm.set(false);
            this.editing.set(null);
          },
          error: (err) => {
            this.saving.set(false);
            if (!this.applyServerErrors(err)) {
              this.error.set(problemDetail(err, 'Could not rename the team.'));
            }
          },
        });
      return;
    }

    const isSub = typeof v.parentTeamId === 'number' && v.parentTeamId > 0;
    this.churchService
      .createTeam(churchId, {
        name: v.name,
        membershipRule: isSub ? null : v.membershipRule,
        parentTeamId:
          typeof v.parentTeamId === 'number' ? v.parentTeamId : null,
      })
      .subscribe({
        next: (created) => {
          this.saving.set(false);
          this.teams.set([...this.teams(), created]);
          this.showForm.set(false);
        },
        error: (err) => {
          this.saving.set(false);
          if (!this.applyServerErrors(err)) {
            this.error.set(problemDetail(err, 'Could not create the team.'));
          }
        },
      });
  }

  private applyServerErrors(err: unknown): boolean {
    const response = err as {
      status?: number;
      error?: { errors?: Record<string, string[]> };
    };
    if (response?.status !== 400 || !response.error?.errors) return false;
    return applyFieldErrors(
      this.form,
      err as Parameters<typeof applyFieldErrors>[1],
    );
  }

  askDelete(team: Team) {
    this.error.set('');
    this.toDelete.set(team);
  }

  cancelDelete() {
    this.toDelete.set(null);
  }

  confirmDelete() {
    const churchId = this.churchId();
    const target = this.toDelete();
    if (!churchId || !target || this.deleting()) return;
    this.deleting.set(true);
    this.error.set('');
    this.churchService.deleteTeam(churchId, target.id).subscribe({
      next: () => {
        this.deleting.set(false);
        this.toDelete.set(null);
        this.teams.set(this.teams().filter((t) => t.id !== target.id));
        this.details.set(
          Object.fromEntries(
            Object.entries(this.details()).filter(([id]) => id !== String(target.id)),
          ),
        );
      },
      error: (err) => {
        this.deleting.set(false);
        this.toDelete.set(null);
        this.error.set(problemDetail(err, 'Could not delete the team.'));
      },
    });
  }

  ruleLabel(rule: MembershipRule): string {
    return rule === 'Multiple'
      ? 'Multiple sub-teams'
      : 'Single sub-team';
  }

  isCategory(team: Team): boolean {
    const detail = this.details()[team.id];
    return detail
      ? detail.subTeamCount > 0
      : this.teams().some((t) => t.parentTeamId === team.id);
  }
}