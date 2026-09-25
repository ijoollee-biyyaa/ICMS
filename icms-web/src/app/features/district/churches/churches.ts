import {
  Component,
  computed,
  effect,
  inject,
  signal,
} from '@angular/core';
import {
  ReactiveFormsModule,
  Validators,
  FormBuilder,
  AbstractControl,
} from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { MatButton, MatIconButton } from '@angular/material/button';
import { MatIcon } from '@angular/material/icon';
import { MatMenu, MatMenuItem, MatMenuTrigger } from '@angular/material/menu';

import { ChurchStore } from '../../../stores/church.store';
import {
  Church,
  ChurchType,
  CreateChurchRequest,
  UpdateChurchRequest,
} from '../../../models/church';
import { downloadChurchCredentialsPdf } from '../../../utils/church-credentials-pdf';
import { AppTable } from '../../shared/ui/data-table/data-table';
import { TableColumn } from '../../shared/ui/data-table/table-column';
import { StatCard } from '../../shared/ui/stat-card/stat-card';

const CODE_PATTERN = '^[A-Z0-9-]{2,10}$';
const ADMIN_NAME_KEYS = [
  'adminFirstName',
  'adminFatherName',
  'adminGrandfatherName',
] as const;

/** Mirrors Icms.Application/Common/ChurchConstants.cs (backend max lengths). */
const MAX = {
  name: 200,
  code: 10,
  city: 100,
  subcity: 100,
  email: 100,
  phone: 20,
  tel: 20,
  websiteUrl: 200,
} as const;

type Copied = '' | 'email' | 'password';

@Component({
  selector: 'app-district-churches',
  imports: [
    ReactiveFormsModule,
    RouterLink,
    MatIconButton,
    MatIcon,
    MatMenu,
    MatMenuItem,
    MatMenuTrigger,
    AppTable,
    TableColumn,
    StatCard,
  ],
  templateUrl: './churches.html',
  styleUrl: './churches.scss',
})
export class DistrictChurches {
  readonly store = inject(ChurchStore);
  private readonly fb = inject(FormBuilder);
  private readonly router = inject(Router);

  readonly showForm = signal(false);
  readonly editing = signal<Church | null>(null);
  readonly showCredentials = signal(false);
  readonly copied = signal<Copied>('');

  readonly form = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(MAX.name)]],
    code: [
      '',
      [
        Validators.required,
        Validators.maxLength(MAX.code),
        Validators.pattern(CODE_PATTERN),
      ],
    ],
    type: ['Local' as ChurchType, [Validators.required]],
    parentChurchId: this.fb.control<number | null>(null),
    city: ['', [Validators.maxLength(MAX.city)]],
    subcity: ['', [Validators.maxLength(MAX.subcity)]],
    email: ['', [Validators.maxLength(MAX.email), Validators.email]],
    phone: ['', [Validators.maxLength(MAX.phone)]],
    tel: ['', [Validators.maxLength(MAX.tel)]],
    mapAddress: [''],
    websiteUrl: ['', [Validators.maxLength(MAX.websiteUrl)]],
    adminFirstName: [''],
    adminFatherName: [''],
    adminGrandfatherName: [''],
  });

  readonly type = signal<ChurchType>('Local');
  readonly isSaving = this.store.isSaving;
  readonly localChurches = computed(() =>
    this.store
      .churches()
      .filter((c) => c.type === 'Local' && c.id !== this.editing()?.id),
  );

  private lastSaving = false;
  private pendingCreate = false;

  constructor() {
    this.store.load();

    this.form.controls.parentChurchId.addValidators(() =>
      this.form.controls.type.value === 'Daughter' && !this.form.controls.parentChurchId.value
        ? { required: true }
        : null,
    );

    this.form.controls.code.valueChanges.subscribe((value) => {
      const upper = value.toUpperCase();
      if (upper !== value) {
        this.form.controls.code.setValue(upper, { emitEvent: false });
      }
    });

    this.form.controls.type.valueChanges.subscribe((value) => {
      this.type.set(value as ChurchType);
      if (value !== 'Daughter') {
        this.form.controls.parentChurchId.setValue(null);
      }
      this.form.controls.parentChurchId.updateValueAndValidity();
    });

    for (const control of Object.values(this.form.controls) as AbstractControl[]) {
      control.valueChanges.subscribe(() => this.clearServerError(control));
    }

    effect(() => {
      const fields = this.store.fieldErrors();
      if (!fields) return;
      for (const [field, message] of Object.entries(fields)) {
        const control = this.form.get(field);
        if (control) {
          control.setErrors({ server: message });
          control.markAsTouched();
        }
      }
    });

    effect(() => {
      const saving = this.store.isSaving();
      const wasSaving = this.lastSaving;
      this.lastSaving = saving;
      if (wasSaving && !saving) {
        this.onSaveCompleted();
      }
    });
  }

  // ---- Table helpers ----

  searchChurch = (church: Church, query: string): boolean => {
    const q = query.toLowerCase().trim();
    if (!q) return true;
    return (
      church.name.toLowerCase().includes(q) ||
      church.code.toLowerCase().includes(q) ||
      (church.city?.toLowerCase().includes(q) ?? false) ||
      (church.subcity?.toLowerCase().includes(q) ?? false) ||
      (church.phone?.toLowerCase().includes(q) ?? false) ||
      (church.email?.toLowerCase().includes(q) ?? false) ||
      church.type.toLowerCase().includes(q)
    );
  };

  churchName = (c: Church) => c.name;
  churchCode = (c: Church) => c.code;
  churchTypeVal = (c: Church) => c.type;
  churchLocationVal = (c: Church) => [c.subcity, c.city].filter(Boolean).join(', ');
  churchMembersVal = (c: Church) => c.memberCount ?? 0;
  churchPersonnelVal = (c: Church) => (c.employeeCount ?? 0) + (c.ministerCount ?? 0);

  viewChurch(church: Church) {
    this.router.navigate(['/district/churches', church.id]);
  }

  // ---- Form helpers ----

  openForm(church: Church | null = null) {
    this.store.clearErrors();
    this.editing.set(church);
    this.setAdminRequired(!church);

    if (church) {
      this.form.reset();
      this.form.patchValue({
        name: church.name,
        code: church.code,
        type: church.type,
        parentChurchId: church.parentChurchId,
        city: church.city || '',
        subcity: church.subcity || '',
        email: church.email || '',
        phone: church.phone || '',
        tel: church.tel || '',
        mapAddress: church.mapAddress || '',
        websiteUrl: church.websiteUrl || '',
      });
    } else {
      this.form.reset({ type: 'Local' });
    }
    this.type.set(this.form.controls.type.value as ChurchType);
    this.showForm.set(true);
  }

  closeForm() {
    this.showForm.set(false);
    this.editing.set(null);
    this.pendingCreate = false;
    this.form.reset({ type: 'Local' });
    this.type.set('Local');
    this.store.clearErrors();
  }

  submit() {
    this.form.updateValueAndValidity();
    if (!this.form.valid) {
      this.form.markAllAsTouched();
      return;
    }

    const editingId = this.editing()?.id;
    this.pendingCreate = editingId === undefined;
    this.store.clearErrors();

    if (this.pendingCreate) {
      this.store.createChurch(this.buildCreateBody());
    } else {
      this.store.updateChurch({
        churchId: editingId as number,
        body: this.buildUpdateBody(),
      });
    }
  }

  deleteChurch(church: Church) {
    const message =
      church.type === 'Local'
        ? `Delete ${church.name}? This cannot be undone.`
        : `Delete daughter church ${church.name}? This cannot be undone.`;
    if (!confirm(message)) {
      return;
    }
    this.pendingCreate = false;
    this.store.deleteChurch(church.id);
  }

  parentName(church: Church): string | null {
    return (
      this.store
        .churches()
        .find((c) => c.id === church.parentChurchId)?.name ?? null
    );
  }

  location(city: string | null, subcity: string | null) {
    return [subcity, city].filter(Boolean).join(', ') || null;
  }

  downloadCredentials() {
    const created = this.store.created();
    if (created) {
      downloadChurchCredentialsPdf(created);
    }
  }

  copyCredentials(which: 'email' | 'password') {
    const created = this.store.created();
    if (!created) return;
    const value =
      which === 'email' ? created.adminEmail : created.adminTempPassword;
    navigator.clipboard?.writeText(value).then(() => {
      this.copied.set(which);
      setTimeout(() => this.copied.set(''), 1500);
    });
  }

  revealCredentials() {
    this.showCredentials.set(true);
    requestAnimationFrame(() =>
      document.getElementById('credentials-card')?.scrollIntoView({
        behavior: 'smooth',
        block: 'center',
      }),
    );
  }

  dismissCredentials() {
    this.showCredentials.set(false);
    this.store.clearCreated();
  }

  toggleCredentialsReveal() {
    this.showCredentials.update((v) => !v);
  }

  url(site: string | null): string | null {
    if (!site) return null;
    return /^https?:\/\//i.test(site) ? site : `https://${site}`;
  }

  private buildUpdateBody(): UpdateChurchRequest {
    const v = this.form.getRawValue();
    return {
      name: v.name,
      code: v.code,
      city: v.city || null,
      subcity: v.subcity || null,
      email: v.email || null,
      phone: v.phone || null,
      tel: v.tel || null,
      mapAddress: v.mapAddress || null,
      websiteUrl: v.websiteUrl || null,
    };
  }

  private buildCreateBody(): CreateChurchRequest {
    const v = this.form.getRawValue();
    const isDaughter = v.type === 'Daughter';
    return {
      name: v.name,
      code: v.code,
      type: v.type,
      parentChurchId:
        isDaughter && v.parentChurchId != null ? v.parentChurchId : null,
      city: v.city || null,
      subcity: v.subcity || null,
      email: v.email || null,
      phone: v.phone || null,
      tel: v.tel || null,
      mapAddress: v.mapAddress || null,
      websiteUrl: v.websiteUrl || null,
      adminFirstName: v.adminFirstName,
      adminFatherName: v.adminFatherName,
      adminGrandfatherName: v.adminGrandfatherName,
    };
  }

  private setAdminRequired(required: boolean) {
    for (const key of ADMIN_NAME_KEYS) {
      const control = this.form.get(key);
      control?.setValidators(required ? [Validators.required] : []);
      control?.updateValueAndValidity();
    }
  }

  private clearServerError(control: AbstractControl) {
    const errors = control.errors;
    if (!errors || !('server' in errors) || (errors as Record<string, unknown>)['server'] === null) {
      return;
    }
    if (!control.dirty) {
      return;
    }
    const { server: _server, ...rest } = errors as Record<string, unknown>;
    control.setErrors(Object.keys(rest).length ? rest : null);
  }

  private onSaveCompleted() {
    if (this.store.error() || this.store.fieldErrors()) {
      this.showCredentials.set(false);
      return;
    }

    const wasCreate = this.pendingCreate;
    const created = this.store.created();
    this.closeForm();

    if (wasCreate && created) {
      this.showCredentials.set(true);
    }
  }
}