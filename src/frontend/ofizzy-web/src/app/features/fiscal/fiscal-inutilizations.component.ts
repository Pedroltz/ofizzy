import { ChangeDetectionStrategy, Component, DestroyRef, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { DatePipe } from '@angular/common';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { MessageModule } from 'primeng/message';
import { TextareaModule } from 'primeng/textarea';
import { FiscalApiService, FiscalInutilization } from './fiscal-api.service';
import { FiscalFieldsComponent, FiscalField, fiscalForm } from './fiscal-fields.component';

@Component({
  selector: 'app-fiscal-inutilizations', standalone: true, changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [DatePipe, ReactiveFormsModule, ButtonModule, DialogModule, MessageModule, TextareaModule, FiscalFieldsComponent],
  template: `<section aria-label="Inutilização de numeração NF-e">
    <p class="text-sm text-secondary mb-4">Use somente para números reservados pelo Ofizzy cuja emissão foi rejeitada. A SEFAZ deve confirmar que não existe NF-e autorizada. A inutilização confirmada é definitiva.</p>
    <app-fiscal-fields [fields]="fields" [form]="form" prefix="inut-" />
    <div class="mt-4 mb-3">
      <label for="inut-reason" class="block text-xs font-semibold text-muted uppercase tracking-wider mb-1.5">Justificativa (15 a 255 caracteres)</label>
      <textarea pTextarea id="inut-reason" [formControl]="reason" rows="3" maxlength="255" class="w-full"></textarea>
    </div>
    @if (error()) { <p-message severity="error" class="my-3 block">{{ error() }}</p-message> }
    <div class="flex flex-wrap gap-2.5 items-center my-4">
      <p-button label="Revisar inutilização" [disabled]="busy()" (onClick)="review()" />
      <p-button label="Atualizar histórico" [outlined]="true" [loading]="busy()" (onClick)="load()" />
    </div>
    @if (result()) { <div role="status" aria-live="polite" class="my-3"><p-message severity="info">{{ result() }}</p-message></div> }
    <div class="history">@for (item of records(); track item.id) {
      <article class="history-card">
        <div class="flex items-center justify-between gap-2">
          <strong class="text-primary text-sm">Série {{ item.series }} · {{ item.firstNumber }} a {{ item.lastNumber }}</strong>
          <span class="font-mono text-xs px-2 py-0.5 rounded border border-subtle bg-surface-secondary text-secondary">{{ item.year }}</span>
        </div>
        <p class="text-xs text-muted m-0">{{ item.environment === 'Production' ? 'Produção' : 'Homologação' }} · {{ item.createdAt | date:'dd/MM/yyyy HH:mm' }}</p>
        <p class="text-sm text-secondary m-0">{{ label(item.state) }} — {{ item.message }}</p>
        @if (item.protocol) { <p class="text-xs font-mono text-muted m-0">Protocolo: {{ item.protocol }}</p> }
        @if (item.state === 'Pending') {
          <div class="mt-2 pt-2 border-t border-subtle">
            <p-button label="Recuperar protocolo" [outlined]="true" size="small" [disabled]="busy()" (onClick)="recover(item)" />
          </div>
        }
      </article>
    } @empty { <p class="text-sm text-muted m-0">Nenhum pedido registrado.</p> }</div>
  </section>
  <p-dialog header="Confirmar inutilização" [modal]="true" [visible]="confirming()" (visibleChange)="confirming.set($event)" [style]="{ width: 'min(32rem, 94vw)' }">
    <div class="flex flex-col gap-3 py-1">
      <p class="text-secondary text-sm m-0">Inutilizar números {{ form.controls['firstNumber'].value }} a {{ form.controls['lastNumber'].value }}, série {{ form.controls['series'].value }}, ano {{ form.controls['year'].value }} no ambiente fiscal configurado?</p>
      <p class="text-secondary text-sm m-0">Depois da confirmação da SEFAZ, esses números não poderão ser usados.</p>
      @if (error()) { <p-message severity="error" class="block">{{ error() }}</p-message> }
    </div>
    <ng-template #footer>
      <div class="dialog-actions">
        <p-button label="Voltar" [text]="true" severity="secondary" [disabled]="busy()" (onClick)="confirming.set(false)" />
        <p-button label="Confirmar inutilização" severity="danger" [loading]="busy()" (onClick)="create()" />
      </div>
    </ng-template>
  </p-dialog>`,
  styles: `:host,section{display:block;min-width:0}.history{display:grid;grid-template-columns:repeat(auto-fit,minmax(min(100%,280px),1fr));gap:1rem;margin-top:1rem}.history-card{border:1px solid var(--border-subtle);border-radius:var(--radius-md);padding:1.25rem;background:var(--surface-primary);display:flex;flex-direction:column;gap:0.5rem;min-width:0}textarea{display:block;width:100%;min-height:44px}p,strong{overflow-wrap:anywhere}:host ::ng-deep .p-button{min-height:44px}.dialog-actions{display:flex;justify-content:flex-end;gap:0.5rem;padding-top:0.75rem}@media(max-width:640px){textarea{font-size:16px}}`
})
export class FiscalInutilizationsComponent {
  private readonly api = inject(FiscalApiService); private readonly destroy = inject(DestroyRef);
  readonly records = signal<FiscalInutilization[]>([]); readonly busy = signal(false); readonly error = signal(''); readonly result = signal(''); readonly confirming = signal(false);
  readonly fields: FiscalField[] = [{ key: 'series', label: 'Série NF-e', type: 'number' }, { key: 'year', label: 'Ano da numeração', type: 'number' }, { key: 'firstNumber', label: 'Primeiro número', type: 'number' }, { key: 'lastNumber', label: 'Último número', type: 'number' }];
  readonly form = fiscalForm(this.fields, { series: 1, year: new Date().getFullYear() }); readonly reason = new FormControl('', { nonNullable: true });
  constructor() { void this.load(); }
  private failed(error: unknown) { if (!this.destroy.destroyed) this.error.set(error instanceof HttpErrorResponse && typeof error.error?.detail === 'string' ? error.error.detail : 'Não foi possível concluir a operação. Atualize o histórico antes de tentar novamente.'); }
  async load() { if (this.busy()) return; this.busy.set(true); try { const data = await this.api.inutilizations(); if (!this.destroy.destroyed) { this.records.set(data); this.error.set(''); } } catch (error) { this.failed(error); } finally { if (!this.destroy.destroyed) this.busy.set(false); } }
  review() { this.error.set(''); if (this.reason.value.trim().length < 15 || Object.values(this.form.getRawValue()).some(value => typeof value !== 'number' || !Number.isInteger(value))) { this.error.set('Preencha série, ano, números inteiros e uma justificativa com pelo menos 15 caracteres.'); return; } this.confirming.set(true); }
  async create() {
    if (this.busy()) return; this.busy.set(true); this.error.set('');
    try { await this.api.inutilize({ ...this.form.getRawValue(), reason: this.reason.value.trim() }); if (!this.destroy.destroyed) { this.confirming.set(false); this.result.set('Pedido registrado. Confira a situação no histórico.'); } }
    catch (error) { this.failed(error); }
    finally { if (!this.destroy.destroyed) this.busy.set(false); }
    if (!this.destroy.destroyed) { const error = this.error(); await this.load(); if (error) this.error.set(error); }
  }
  async recover(item: FiscalInutilization) {
    if (this.busy()) return; this.busy.set(true); this.error.set('');
    try { await this.api.recoverInutilization(item.id); if (!this.destroy.destroyed) this.result.set('Situação atualizada. Confira o protocolo no histórico.'); }
    catch (error) { this.failed(error); }
    finally { if (!this.destroy.destroyed) this.busy.set(false); }
    if (!this.destroy.destroyed) { const error = this.error(); await this.load(); if (error) this.error.set(error); }
  }
  label(state: string) { return ({ Pending: 'Aguardando confirmação', Confirmed: 'Inutilização confirmada', Rejected: 'Pedido rejeitado' } as Record<string, string>)[state] ?? state; }
}
