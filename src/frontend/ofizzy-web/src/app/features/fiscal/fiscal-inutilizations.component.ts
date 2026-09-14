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
    <p>Use somente para números reservados pelo Ofizzy cuja emissão foi rejeitada. A SEFAZ deve confirmar que não existe NF-e autorizada. A inutilização confirmada é definitiva.</p>
    <app-fiscal-fields [fields]="fields" [form]="form" prefix="inut-" />
    <label for="inut-reason">Justificativa (15 a 255 caracteres)</label>
    <textarea pTextarea id="inut-reason" [formControl]="reason" rows="3" maxlength="255"></textarea>
    @if (error()) { <p-message severity="error">{{ error() }}</p-message> }
    <div class="actions"><p-button label="Revisar inutilização" [disabled]="busy()" (onClick)="review()" /><p-button label="Atualizar histórico" [loading]="busy()" (onClick)="load()" /></div>
    <div role="status" aria-live="polite">{{ result() }}</div>
    <div class="history">@for (item of records(); track item.id) {
      <article><strong>Série {{ item.series }} · {{ item.firstNumber }} a {{ item.lastNumber }}</strong>
        <p>{{ item.environment === 'Production' ? 'Produção' : 'Homologação' }} · {{ item.year }} · {{ item.createdAt | date:'dd/MM/yyyy HH:mm' }}</p>
        <p>{{ label(item.state) }} — {{ item.message }}</p>
        @if (item.protocol) { <p>Protocolo: {{ item.protocol }}</p> }
        @if (item.state === 'Pending') { <p-button label="Recuperar protocolo" [disabled]="busy()" (onClick)="recover(item)" /> }
      </article>
    } @empty { <p>Nenhum pedido registrado.</p> }</div>
  </section>
  <p-dialog header="Confirmar inutilização" [modal]="true" [visible]="confirming()" (visibleChange)="confirming.set($event)" [style]="{ width: 'min(32rem, 94vw)' }">
    <p>Inutilizar números {{ form.controls['firstNumber'].value }} a {{ form.controls['lastNumber'].value }}, série {{ form.controls['series'].value }}, ano {{ form.controls['year'].value }} no ambiente fiscal configurado?</p>
    <p>Depois da confirmação da SEFAZ, esses números não poderão ser usados.</p>
    @if (error()) { <p-message severity="error">{{ error() }}</p-message> }
    <div class="actions"><p-button label="Confirmar inutilização" severity="danger" [loading]="busy()" (onClick)="create()" /><p-button label="Voltar" [disabled]="busy()" (onClick)="confirming.set(false)" /></div>
  </p-dialog>`,
  styles: `:host,section{display:block;min-width:0}textarea{display:block;width:100%;min-height:44px;margin:.5rem 0 1rem}.actions{display:flex;flex-wrap:wrap;gap:.5rem;margin:1rem 0}.history{display:grid;grid-template-columns:repeat(auto-fit,minmax(min(100%,260px),1fr));gap:1rem}article{border:1px solid var(--border-subtle);border-radius:.75rem;padding:1rem;min-width:0}p,strong{overflow-wrap:anywhere}:host ::ng-deep .p-button{min-height:44px}@media(max-width:640px){textarea{font-size:16px}}`
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
