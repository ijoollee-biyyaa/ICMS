import { ChangeDetectionStrategy, Component, effect, inject, signal } from '@angular/core';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatIcon } from '@angular/material/icon';
import { MatSlideToggle } from '@angular/material/slide-toggle';

import { DistrictStore } from '../../../stores/district.store';

type Section = 'general' | 'notifications' | 'security';

interface SectionItem {
  id: Section;
  label: string;
  icon: string;
}

@Component({
  selector: 'app-district-settings',
  imports: [MatIcon, MatSlideToggle, FormsModule, ReactiveFormsModule],
  templateUrl: './settings.html',
  styleUrl: './settings.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DistrictSettings {
  readonly store = inject(DistrictStore);
  private readonly fb = inject(FormBuilder);

  readonly sections: SectionItem[] = [
    { id: 'general', label: 'General', icon: 'tune' },
    { id: 'notifications', label: 'Notifications', icon: 'notifications' },
    { id: 'security', label: 'Security', icon: 'shield' },
  ];

  readonly active = signal<Section>('general');
  readonly notifyPayroll = signal(true);
  readonly notifyReports = signal(true);
  readonly notifyMemberJoin = signal(false);
  readonly twoFactor = signal(false);
  readonly sessionTimeout = signal(true);

  readonly form = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(100)]],
    code: ['', [Validators.required, Validators.maxLength(20)]],
    address: [''],
  });

  constructor() {
    effect(() => {
      const district = this.store.district();
      if (!district) return;
      this.form.patchValue(
        {
          name: district.name,
          code: district.code,
          address: district.address ?? '',
        },
        { emitEvent: false },
      );
    });
  }

  setSection(section: Section) {
    this.active.set(section);
  }

  save() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const payload = this.form.getRawValue();
    this.store.updateDistrict({
      name: payload.name,
      code: payload.code,
      address: payload.address || null,
    });
  }
}
