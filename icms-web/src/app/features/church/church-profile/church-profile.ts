import { Component, computed, effect, inject, signal } from '@angular/core';
import { MatIcon } from '@angular/material/icon';
import {
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators as V,
} from '@angular/forms';
import { MatFormField, MatLabel, MatError } from '@angular/material/form-field';
import { MatInput } from '@angular/material/input';

import { AuthService } from '../../../services/auth.service';
import { ChurchService } from '../../../services/church.service';
import { environment } from '../../../../environments/environment';
import { Church, UpdateChurchRequest } from '../../../models/church';
import { PageHeader } from '../../shared/ui/page-header/page-header';
import { SectionCard } from '../../shared/ui/section-card/section-card';

@Component({
  selector: 'app-church-profile',
  imports: [
    MatIcon,
    PageHeader,
    SectionCard,
    ReactiveFormsModule,
    MatFormField,
    MatInput,
    MatLabel,
    MatError,
  ],
  templateUrl: './church-profile.html',
  styleUrl: './church-profile.scss',
})
export class ChurchProfile {
  private auth = inject(AuthService);
  private churchService = inject(ChurchService);

  readonly churchId = computed(() => this.auth.currentUser()?.churchId ?? null);

  readonly church = signal<Church | null>(null);
  readonly loading = signal(true);

  readonly editing = signal(false);
  readonly busy = signal(false);
  readonly error = signal('');
  readonly notice = signal('');

  form = new FormGroup({
    name: new FormControl('', V.required),
    code: new FormControl('', V.required),
    city: new FormControl(''),
    subcity: new FormControl(''),
    email: new FormControl('', V.email),
    phone: new FormControl(''),
    tel: new FormControl(''),
    mapAddress: new FormControl(''),
    websiteUrl: new FormControl(''),
  });

  constructor() {
    effect(() => {
      const id = this.churchId();
      if (id !== null) {
        this.loading.set(true);
        this.churchService
          .getChurch(environment.districtId, id)
          .subscribe({
            next: (c) => {
              this.church.set(c);
              this.loading.set(false);
              this.patchFrom(c);
            },
            error: () => {
              this.loading.set(false);
              this.error.set('Could not load the church profile.');
            },
          });
      } else {
        this.loading.set(false);
      }
    });
  }

  private patchFrom(c: Church) {
    this.form.patchValue({
      name: c.name,
      code: c.code,
      city: c.city ?? '',
      subcity: c.subcity ?? '',
      email: c.email ?? '',
      phone: c.phone ?? '',
      tel: c.tel ?? '',
      mapAddress: c.mapAddress ?? '',
      websiteUrl: c.websiteUrl ?? '',
    });
  }

  location(city: string | null, subcity: string | null) {
    return [subcity, city].filter(Boolean).join(', ') || '—';
  }

  openEdit() {
    const c = this.church();
    if (!c) return;
    this.error.set('');
    this.notice.set('');
    this.patchFrom(c);
    this.editing.set(true);
  }

  closeEdit() {
    if (this.busy()) return;
    this.editing.set(false);
  }

  save() {
    const c = this.church();
    if (!c || this.form.invalid || this.busy()) return;

    const v = this.form.getRawValue();
    const body: UpdateChurchRequest = {
      name: v.name ?? '',
      code: v.code ?? '',
      city: v.city || null,
      subcity: v.subcity || null,
      email: v.email || null,
      phone: v.phone || null,
      tel: v.tel || null,
      mapAddress: v.mapAddress || null,
      websiteUrl: v.websiteUrl || null,
    };

    this.busy.set(true);
    this.error.set('');
    this.churchService
      .updateChurch(environment.districtId, c.id, body)
      .subscribe({
        next: (updated) => {
          this.busy.set(false);
          this.editing.set(false);
          this.church.set(updated);
          this.patchFrom(updated);
          this.notice.set('Church profile updated.');
        },
        error: (err: { error?: { detail?: string; title?: string } }) => {
          this.busy.set(false);
          this.error.set(
            err.error?.detail ?? err.error?.title ?? 'Could not save the church profile.',
          );
        },
      });
  }
}