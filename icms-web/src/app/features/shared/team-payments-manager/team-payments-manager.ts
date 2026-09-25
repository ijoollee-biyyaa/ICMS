import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  input,
  OnInit,
  signal,
} from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatIcon } from '@angular/material/icon';
import { MatIconButton } from '@angular/material/button';
import {
  MatError,
  MatFormField,
  MatLabel,
} from '@angular/material/form-field';
import { MatInput } from '@angular/material/input';
import { MatOption, MatSelect } from '@angular/material/select';

import { ChurchService } from '../../../services/church.service';
import {
  TeamMember,
  TeamPayment,
  TeamPaymentSummary,
} from '../../../models/team';
import {
  problemDetail,
  applyFieldErrors,
  clearFieldErrors,
} from '../../../common/http-errors';
import { formatDateValue } from '../../../common/dates';

@Component({
  selector: 'app-team-payments-manager',
  imports: [
    ReactiveFormsModule,
    MatFormField,
    MatLabel,
    MatError,
    MatInput,
    MatSelect,
    MatOption,
    MatIcon,
    MatIconButton,
  ],
  templateUrl: './team-payments-manager.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TeamPaymentsManager implements OnInit {
  readonly churchId = input.required<number>();
  readonly teamId = input.required<number>();

  private service = inject(ChurchService);
  private fb = inject(FormBuilder);

  readonly roster = signal<TeamMember[]>([]);
  readonly payments = signal<TeamPayment[]>([]);
  readonly summary = signal<TeamPaymentSummary[]>([]);

  readonly loading = signal(false);
  readonly busy = signal(false);
  readonly error = signal('');
  readonly notice = signal('');

  readonly editingPayment = signal<number | null>(null);
  readonly editingAmount = signal('');

  paymentForm = this.fb.nonNullable.group({
    memberId: ['' as number | '', Validators.required],
    month: ['', Validators.required],
    amount: [null as number | null, [Validators.required, Validators.min(0.01)]],
  });

  readonly paymentTotal = computed(() =>
    this.summary().reduce((sum, p) => sum + p.totalAmount, 0),
  );

  ngOnInit(): void {
    this.load();
  }

  load() {
    const churchId = this.churchId();
    const teamId = this.teamId();
    if (!churchId || !teamId) return;
    this.loading.set(true);
    this.error.set('');
    this.service
      .getPaymentSummary(churchId, teamId)
      .subscribe({
        next: (summary) => this.summary.set(summary),
        error: (err) => {
          this.loading.set(false);
          this.error.set(problemDetail(err, 'Could not load the giving report.'));
        },
      });
    this.service.getPayments(churchId, teamId, null, null, 1, 100).subscribe({
      next: (page) => {
        this.payments.set(page.items);
        this.loading.set(false);
      },
      error: (err) => {
        this.loading.set(false);
        this.error.set(problemDetail(err, 'Could not load payments.'));
      },
    });
    this.service.getTeamMembers(churchId, teamId, 1, 100).subscribe({
      next: (page) => this.roster.set(page.items),
    });
  }

  recordPayment() {
    const churchId = this.churchId();
    const teamId = this.teamId();
    if (!churchId || !teamId || this.paymentForm.invalid || this.busy()) return;

    clearFieldErrors(this.paymentForm);
    this.busy.set(true);
    this.error.set('');
    this.notice.set('');

    const v = this.paymentForm.getRawValue();
    const month = v.month?.trim() ? `${v.month}-01` : null;
    this.service
      .recordPayment(churchId, teamId, {
        memberId: typeof v.memberId === 'number' ? v.memberId : null,
        month,
        amount: v.amount,
      })
      .subscribe({
        next: (payment) => {
          this.busy.set(false);
          this.payments.update((list) => [payment, ...list]);
          this.paymentForm.reset({ memberId: '', month: '', amount: null });
          this.notice.set(
            `Registered ${this.amountText(payment.amount)} for ${payment.memberName}.`,
          );
          this.refresh();
        },
        error: (err: HttpErrorResponse) => {
          this.busy.set(false);
          if (!applyFieldErrors(this.paymentForm, err)) {
            this.error.set(problemDetail(err, 'Could not record the payment.'));
          }
        },
      });
  }

  startEdit(payment: TeamPayment) {
    this.editingPayment.set(payment.id);
    this.editingAmount.set(String(payment.amount));
  }

  cancelEdit() {
    this.editingPayment.set(null);
    this.editingAmount.set('');
  }

  onEditAmount(event: Event) {
    this.editingAmount.set((event.target as HTMLInputElement).value);
  }

  saveEdit(payment: TeamPayment) {
    const churchId = this.churchId();
    const teamId = this.teamId();
    const amount = Number(this.editingAmount());
    if (!churchId || !teamId || !amount || this.busy()) return;
    this.busy.set(true);
    this.error.set('');
    this.service
      .updatePayment(churchId, teamId, payment.id, { amount })
      .subscribe({
        next: (updated) => {
          this.busy.set(false);
          this.payments.set(
            this.payments().map((p) => (p.id === updated.id ? updated : p)),
          );
          this.editingPayment.set(null);
          this.refresh();
        },
        error: (err) => {
          this.busy.set(false);
          this.error.set(problemDetail(err, 'Could not update the payment.'));
        },
      });
  }

  private refresh() {
    const churchId = this.churchId();
    const teamId = this.teamId();
    if (!churchId || !teamId) return;
    this.service.getPaymentSummary(churchId, teamId).subscribe({
      next: (summary) => this.summary.set(summary),
    });
  }

  amountText(amount: number): string {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'ETB',
    }).format(amount);
  }

  monthLabel(month: string): string {
    return formatDateValue(month, { year: 'numeric', month: 'long' });
  }
}