import { ChangeDetectionStrategy, Component, DestroyRef, effect, inject, input, signal } from '@angular/core';
import { CurrencyPipe, DatePipe } from '@angular/common';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { InputNumberModule } from 'primeng/inputnumber';
import { SelectModule } from 'primeng/select';
import { TextareaModule } from 'primeng/textarea';
import { MessageModule } from 'primeng/message';
import { TenantContextService } from '../../core/tenancy/tenant-context.service';

interface Movement {
  id: string; kind: string; amount: number; fees: number; segregatedTax: number | null;
  occurredAt: string; settlementId: string | null; reason: string | null;
}
interface Payment {
  id: string; amount: number; method: string; receivedAt: string; settled: number;
  reversed: number; balance: number;
  allocations: { documentId: string; amount: number; requiresReview: boolean }[];
  movements: Movement[];
}
interface PaymentDocument { id: string; kind: string; total: number; available: number }
interface OrderPayments {
  total: number; registered: number; settled: number; reversed: number; balance: number;
  payments: Payment[]; documents: PaymentDocument[];
}

@Component({
  selector: 'app-work-order-payments',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CurrencyPipe, DatePipe, ReactiveFormsModule, ButtonModule, DialogModule, InputNumberModule,
    SelectModule, TextareaModule, MessageModule],
  template: `
    <section class="payments" aria-label="Recebimentos da OS">
      <h3>Recebimentos e conciliação</h3>
      <p>Registros manuais. Informe a liquidação após confirmá-la no banco ou provedor.</p>
      @if (error()) { <p-message severity="error">{{ error() }}</p-message> }
      @if (data(); as summary) {
        <div class="summary">
          <span>Total: <strong>{{ summary.total | currency: 'BRL' }}</strong></span>
          <span>Liquidado: <strong>{{ summary.settled | currency: 'BRL' }}</strong></span>
          <span>Estornado: <strong>{{ summary.reversed | currency: 'BRL' }}</strong></span>
          <span>Saldo da OS: <strong>{{ summary.balance | currency: 'BRL' }}</strong></span>
        </div>
        <div class="actions">
          <p-button label="Registrar recebimento" icon="pi pi-plus" (onClick)="open('Register')" [disabled]="busy()" />
          <p-button label="Atualizar recebimentos" icon="pi pi-refresh" [outlined]="true" (onClick)="load()" [disabled]="busy()" />
        </div>
        @for (payment of summary.payments; track payment.id) {
          <article>
            <strong>{{ methodLabel(payment.method) }} · {{ payment.amount | currency: 'BRL' }}</strong>
            <small>{{ payment.receivedAt | date: 'dd/MM/yyyy HH:mm' }}</small>
            <p>A liquidar: {{ payment.balance | currency: 'BRL' }} · Estornado: {{ payment.reversed | currency: 'BRL' }}</p>
            @if (!payment.allocations.length) {
              <p-message severity="info">Recebimento sem vínculo fiscal. Requer conciliação.</p-message>
            }
            @for (allocation of payment.allocations; track allocation.documentId) {
              <p class="document">Documento {{ allocation.documentId }}: {{ allocation.amount | currency: 'BRL' }}</p>
              @if (allocation.requiresReview) {
                <p-message severity="warn">Vínculo requer revisão por alteração fiscal ou estorno.</p-message>
              }
            }
            @for (movement of payment.movements; track movement.id) {
              <div class="movement">
                <span>{{ movement.kind === 'Settlement' ? 'Liquidação' : 'Estorno' }}: {{ movement.amount | currency: 'BRL' }} · {{ movement.occurredAt | date: 'dd/MM/yyyy HH:mm' }}</span>
                @if (movement.kind === 'Settlement') {
                  <small>Taxas: {{ movement.fees | currency: 'BRL' }} · Segregação tributária:
                    {{ movement.segregatedTax === null ? 'Não informada' : (movement.segregatedTax | currency: 'BRL') }}</small>
                  @if (movement.segregatedTax !== null) {
                    <small>Líquido informado: {{ movement.amount - movement.fees - movement.segregatedTax | currency: 'BRL' }}</small>
                  }
                  @if (tenant.admin() && refundable(payment, movement) > 0) {
                    <p-button label="Registrar estorno" severity="danger" [outlined]="true" (onClick)="open('Reversal', payment, movement)" [disabled]="busy()" />
                  }
                } @else { <small>{{ movement.reason }}</small> }
              </div>
            }
            @if (payment.balance > 0) {
              <p-button label="Registrar liquidação" [outlined]="true" (onClick)="open('Settlement', payment)" [disabled]="busy()" />
            }
          </article>
        } @empty { <p>Nenhum recebimento registrado.</p> }
      } @else {
        <p-button label="Consultar recebimentos" [outlined]="true" (onClick)="load()" />
      }
    </section>
    <p-dialog [header]="mode() === 'Register' ? 'Registrar recebimento' : mode() === 'Settlement' ? 'Registrar liquidação' : 'Registrar estorno'"
      [visible]="mode() !== null" (visibleChange)="close($event)" [modal]="true" [style]="{ width: 'min(36rem, 95vw)' }">
      <form [formGroup]="form" (ngSubmit)="save()" class="payment-form">
        <label for="payment-amount">Valor bruto</label>
        <p-inputnumber inputId="payment-amount" formControlName="amount" mode="currency" currency="BRL" locale="pt-BR" [min]="0.01" />
        @if (mode() === 'Register') {
          <label for="payment-method">Forma de recebimento</label>
          <p-select inputId="payment-method" formControlName="method" [options]="methods" optionLabel="label" optionValue="value" />
          <p>Vínculos opcionais com notas oficiais de produção. Se informados, devem somar o recebimento.</p>
          @for (document of data()?.documents ?? []; track document.id) {
            <label [for]="'allocation-' + document.id">{{ document.kind === 'Nfe' ? 'NF-e' : 'NFS-e' }} · disponível {{ document.available | currency: 'BRL' }}</label>
            <p-inputnumber [inputId]="'allocation-' + document.id" [formControl]="allocationControls[document.id]" mode="currency" currency="BRL" locale="pt-BR" [min]="0" [max]="document.available" />
          }
        }
        @if (mode() === 'Settlement') {
          <label for="payment-fees">Taxas</label>
          <p-inputnumber inputId="payment-fees" formControlName="fees" mode="currency" currency="BRL" locale="pt-BR" [min]="0" />
          <label for="payment-tax">Tributos segregados (deixe vazio se não informado)</label>
          <p-inputnumber inputId="payment-tax" formControlName="tax" mode="currency" currency="BRL" locale="pt-BR" [min]="0" />
          <p>A informação é manual e não confirma execução de Split Payment.</p>
        }
        @if (mode() === 'Reversal') {
          <label for="payment-reason">Justificativa (15 a 255 caracteres)</label>
          <textarea id="payment-reason" pTextarea formControlName="reason" rows="3"></textarea>
          <p>Registre somente o valor bruto efetivamente devolvido. Taxas e tributos da liquidação original permanecem no histórico.</p>
        }
        @if (error()) { <p-message severity="error">{{ error() }}</p-message> }
        <div class="actions">
          <p-button label="Voltar" [outlined]="true" (onClick)="close(false)" [disabled]="busy()" />
          <p-button label="Salvar registro" type="submit" [loading]="busy()" [disabled]="form.invalid" />
        </div>
      </form>
    </p-dialog>
  `,
  styles: `
    .payments { padding: 1.25rem; border: 1px solid var(--border-subtle); border-radius: var(--radius-md); margin-top: 1rem; }
    h3 { margin-top: 0; } p, small { color: var(--text-secondary); }
    .summary, .actions { display: flex; flex-wrap: wrap; gap: .75rem; margin: .75rem 0; }
    article { border-top: 1px solid var(--border-subtle); padding: 1rem 0; display: grid; gap: .5rem; }
    .movement { display: grid; gap: .5rem; padding: .75rem; background: var(--surface-secondary); }
    .document { overflow-wrap: anywhere; }
    .payment-form { display: grid; gap: .75rem; padding: 1rem; }
    textarea { width: 100%; min-height: 44px; }
    :host ::ng-deep .p-inputnumber, :host ::ng-deep .p-select { width: 100%; min-width: 0; }
    :host ::ng-deep .p-inputnumber-input { width: 100%; min-width: 0; min-height: 44px; }
    :host ::ng-deep .p-button { min-height: 44px; }
    @media(max-width: 640px) { .actions { flex-direction: column; } :host ::ng-deep .actions .p-button { width: 100%; } }
  `,
})
export class WorkOrderPaymentsComponent {
  readonly orderId = input.required<string>();
  readonly tenant = inject(TenantContextService);
  private readonly http = inject(HttpClient);
  private readonly destroy = inject(DestroyRef);
  readonly data = signal<OrderPayments | null>(null);
  readonly busy = signal(false);
  readonly error = signal('');
  readonly mode = signal<'Register' | 'Settlement' | 'Reversal' | null>(null);
  readonly methods = [{ label: 'Dinheiro', value: 'Cash' }, { label: 'PIX', value: 'Pix' },
    { label: 'Transferência', value: 'Transfer' }, { label: 'Cartão', value: 'Card' }, { label: 'Cheque', value: 'Cheque' }];
  readonly form = new FormGroup({
    amount: new FormControl<number | null>(null, [Validators.required, Validators.min(.01)]),
    method: new FormControl('Pix', { nonNullable: true }),
    fees: new FormControl(0, { nonNullable: true }),
    tax: new FormControl<number | null>(null),
    reason: new FormControl('', { nonNullable: true }),
  });
  allocationControls: Record<string, FormControl<number | null>> = {};
  private target: Payment | null = null;
  private settlement: Movement | null = null;
  private generation = 0;
  private pending: { signature: string; id: string; at: string } | null = null;
  constructor() {
    effect(() => {
      const context = `${this.orderId()}:${this.tenant.tenant()?.id}`;
      if (!context) return;
      this.generation++; this.data.set(null); this.mode.set(null); this.pending = null; this.busy.set(false);
      void this.load();
    });
  }
  async load() {
    const generation = this.generation;
    try {
      const data = await firstValueFrom(this.http.get<OrderPayments>(`/api/work-orders/${this.orderId()}/payments`));
      if (generation === this.generation && !this.destroy.destroyed) { this.data.set(data); this.error.set(''); }
    } catch (e) { if (generation === this.generation) this.error.set(this.message(e)); }
  }
  methodLabel(method: string) { return this.methods.find(x => x.value === method)?.label ?? method; }
  refundable(payment: Payment, movement: Movement) {
    return movement.amount - payment.movements.filter(x => x.settlementId === movement.id).reduce((sum, x) => sum + x.amount, 0);
  }
  open(mode: 'Register' | 'Settlement' | 'Reversal', payment: Payment | null = null, movement: Movement | null = null) {
    this.target = payment; this.settlement = movement; this.pending = null; this.error.set('');
    this.form.reset({ amount: mode === 'Settlement' ? payment!.balance : mode === 'Reversal' ? this.refundable(payment!, movement!) : null,
      method: 'Pix', fees: 0, tax: null, reason: '' });
    this.form.controls.reason.setValidators(mode === 'Reversal' ? [Validators.required, Validators.minLength(15), Validators.maxLength(255)] : []);
    this.form.controls.reason.updateValueAndValidity();
    this.allocationControls = Object.fromEntries((this.data()?.documents ?? []).map(x => [x.id, new FormControl<number | null>(null)]));
    this.mode.set(mode);
  }
  close(visible: boolean) { if (!visible && !this.busy()) this.mode.set(null); }
  async save() {
    if (this.busy() || this.form.invalid) return;
    const generation = this.generation;
    const value = this.form.getRawValue();
    const mode = this.mode();
    const allocations = Object.entries(this.allocationControls).filter(([, control]) => (control.value ?? 0) > 0)
      .map(([documentId, control]) => ({ documentId, amount: control.value }));
    const base = mode === 'Register' ? { amount: value.amount, method: value.method, allocations }
      : mode === 'Settlement' ? { amount: value.amount, fees: value.fees, segregatedTax: value.tax }
      : { amount: value.amount, settlementId: this.settlement!.id, reason: value.reason.trim() };
    const route = mode === 'Register' ? `/api/work-orders/${this.orderId()}/payments`
      : `/api/payments/${this.target!.id}/${mode === 'Settlement' ? 'settlements' : 'reversals'}`;
    const signature = JSON.stringify({ route, base });
    if (this.pending?.signature !== signature) this.pending = { signature, id: crypto.randomUUID(), at: new Date().toISOString() };
    const dateKey = mode === 'Register' ? 'receivedAt' : mode === 'Settlement' ? 'settledAt' : 'reversedAt';
    this.busy.set(true);
    try {
      await firstValueFrom(this.http.post(route, { ...base, requestId: this.pending.id, [dateKey]: this.pending.at }));
      if (generation !== this.generation || this.destroy.destroyed) return;
      this.pending = null; this.mode.set(null); await this.load();
    } catch (e) { if (generation === this.generation) this.error.set(this.message(e)); }
    finally { if (generation === this.generation) this.busy.set(false); }
  }
  private message(error: unknown): string {
    return error instanceof HttpErrorResponse && typeof error.error?.detail === 'string'
      ? error.error.detail : 'Não foi possível concluir. Atualize os registros antes de tentar novamente.';
  }
}
