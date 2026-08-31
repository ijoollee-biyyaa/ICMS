import {
  ChangeDetectionStrategy,
  Component,
  effect,
  inject,
  signal,
} from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { MatIcon } from '@angular/material/icon';

import { DistrictStore } from '../../../stores/district.store';

@Component({
  selector: 'app-create-district',
  imports: [ReactiveFormsModule, MatIcon, RouterLink],
  templateUrl: './create-district.html',
  styleUrl: './create-district.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CreateDistrictComponent {
  private readonly store = inject(DistrictStore);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);

  readonly submitted = signal(false);

  // Expose store signals through the template.
  readonly isSaving = this.store.isSaving;
  readonly error = this.store.error;

  readonly form = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(100)]],
    code: ['', [Validators.required, Validators.maxLength(20)]],
    address: [''],
  });

  constructor() {
    this.form.controls.code.valueChanges.subscribe((value) => {
      const upper = value.toUpperCase();
      if (upper !== value) {
        this.form.controls.code.setValue(upper, { emitEvent: false });
      }
    });

    effect(() => {
      const district = this.store.district();
      if (this.submitted() && district && !this.store.isSaving() && !this.store.error()) {
        void this.router.navigate(['/district/dashboard']);
      }
    });
  }

  submit() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const payload = this.form.getRawValue();
    this.submitted.set(true);
    this.store.createDistrict({
      name: payload.name,
      code: payload.code,
      address: payload.address || null,
    });
  }
}
