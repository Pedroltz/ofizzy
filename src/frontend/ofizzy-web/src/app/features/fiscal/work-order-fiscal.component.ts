import { ChangeDetectionStrategy, Component, DestroyRef, effect, inject, input, signal } from '@angular/core';
import { CurrencyPipe } from '@angular/common';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { MessageModule } from 'primeng/message';
import { DialogModule } from 'primeng/dialog';
import { FieldsetModule } from 'primeng/fieldset';
import { TextareaModule } from 'primeng/textarea';
import { MessageService } from 'primeng/api';
import { TenantContextService } from '../../core/tenancy/tenant-context.service';
import { WorkOrder } from '../../core/api/work-order.models';
import { FiscalApiService, FiscalOrder, FiscalDocument, FiscalPreparation, FiscalAddress, FiscalValue, saveFiscalBlob } from './fiscal-api.service';
import { FiscalFieldsComponent, FiscalField, fiscalForm, addressFields, productFields, serviceFields } from './fiscal-fields.component';
@Component({
  selector: 'app-work-order-fiscal', standalone: true, changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CurrencyPipe, ReactiveFormsModule, ButtonModule, MessageModule, DialogModule, FieldsetModule, TextareaModule, FiscalFieldsComponent],
  template: `<section class="fiscal-panel" aria-label="Documentos fiscais">
    <h3>Documentos fiscais</h3>
    @if (error()) { <p-message severity="error">Não foi possível consultar os documentos fiscais.</p-message><p-button label="Tentar novamente" (onClick)="load()" /> }
    @if (data(); as fiscal) {
      <div class="fiscal-values">@if (order().services.length) { <p>Serviços · NFS-e <strong>{{ fiscal.servicesTotal | currency:'BRL' }}</strong></p> } @if (order().parts.length) { <p>Produtos · NF-e <strong>{{ fiscal.productsTotal | currency:'BRL' }}</strong></p> }</div>
      <p role="status" aria-live="polite">{{ stateLabel(fiscal.status) }}</p>
      @if (fiscal.status === 'Partial') { <p-message severity="warn">Uma parte já foi autorizada. A próxima tentativa processará somente a pendência.</p-message> }
      @if (fiscal.issues.length && fiscal.status !== 'Completed') {
        <p-message severity="warn">Existem {{ fiscal.issues.length }} pendências antes da emissão.</p-message>
        <ul>@for (issue of fiscal.issues; track $index) { <li>{{ issue.message }}</li> }</ul>
      }
      <div class="fiscal-actions">
        @if (fiscal.status !== 'Completed' && fiscal.status !== 'Processing') { <p-button label="Emitir documentos fiscais" icon="pi pi-send" [loading]="busy()" (onClick)="issue()" /> }
        <p-button label="Preparação fiscal" ariaLabel="Preparação fiscal" icon="pi pi-pencil" [outlined]="true" [disabled]="busy() || fiscal.status === 'Processing'" (onClick)="edit()" />
        <p-button label="Atualizar situação" icon="pi pi-refresh" [outlined]="true" [loading]="busy()" (onClick)="refresh()" />
        @if (hasDownloads()) { <p-button label="Baixar XMLs e PDFs" icon="pi pi-download" [outlined]="true" (onClick)="bundle(true)" /> }
      </div>
      <div class="fiscal-documents">@for (document of fiscal.documents; track document.id) {
        <article><strong>{{ document.kind === 'Nfe' ? 'NF-e · Produtos' : 'NFS-e · Serviços' }}</strong><p>{{ stateLabel(document.state) }} · {{ document.environment === 'Homologation' ? 'Homologação' : 'Produção' }}</p><p>{{ document.message }}</p>
          @if (document.accessKey) { <small>Chave: {{ document.accessKey }}</small> }
          <div class="fiscal-actions">@if (document.canDownload) { <p-button label="XML" icon="pi pi-file-code" [outlined]="true" (onClick)="download(document, 'xml')" /><p-button label="PDF da nota" icon="pi pi-file-pdf" [outlined]="true" (onClick)="download(document, 'pdf')" /> }
          @if (tenant.admin() && document.state === 'Authorized') { <p-button label="Cancelar nota" severity="danger" [text]="true" (onClick)="cancelTarget.set(document)" /> }</div>
        </article>
      }</div>
    } @else if (!error()) { <p role="status">Consultando documentos fiscais…</p> }
  </section>
  <p-dialog header="Preparação fiscal da OS" [modal]="true" [visible]="editing()" (visibleChange)="editing.set($event)" [style]="{ width: 'min(60rem, 94vw)' }" styleClass="fiscal-dialog">
    @if (data()) {
      <p>Os valores vêm da OS. Estes complementos serão preservados no documento emitido.</p>
      <app-fiscal-fields [fields]="recipientFields" [form]="recipientForm" prefix="recipient-" />
      <app-fiscal-fields [fields]="addressFields" [form]="addressForm" prefix="recipient-address-" />
      @for (line of lineForms(); track line.id) { <p-fieldset [legend]="line.label" [toggleable]="true" [collapsed]="true"><app-fiscal-fields [fields]="line.kind === 'parts' ? productFields : serviceFields" [form]="line.form" [prefix]="line.id + '-'" /></p-fieldset> }
      @if (saveErrors().length) { <p-message severity="error"><ul>@for (message of saveErrors(); track $index) { <li>{{ message }}</li> }</ul></p-message> }
      <div class="fiscal-actions"><p-button label="Salvar preparação" [loading]="busy()" (onClick)="save()" /><p-button label="Fechar" [text]="true" (onClick)="editing.set(false)" /></div>
    }
  </p-dialog>
  <p-dialog header="Cancelar documento fiscal" [modal]="true" [visible]="!!cancelTarget()" (visibleChange)="!$event && cancelTarget.set(null)" [style]="{ width: 'min(32rem, 94vw)' }">
    <p>A solicitação será enviada ao emissor fiscal. A OS e o histórico serão preservados.</p>
    <label for="fiscal-cancel-reason">Justificativa (15 a 255 caracteres)</label><textarea pTextarea id="fiscal-cancel-reason" [formControl]="reason" rows="4" maxlength="255"></textarea>
    <div class="fiscal-actions"><p-button label="Confirmar cancelamento" severity="danger" [loading]="busy()" (onClick)="cancel()" /><p-button label="Voltar" [text]="true" (onClick)="cancelTarget.set(null)" /></div>
  </p-dialog>`,
  styles: `.fiscal-panel{border-top:1px solid var(--border-subtle);padding-top:1rem;margin-top:1rem;min-width:0}.fiscal-values,.fiscal-documents{display:grid;grid-template-columns:repeat(auto-fit,minmax(min(100%,250px),1fr));gap:1rem}.fiscal-values p{display:flex;justify-content:space-between;gap:.5rem}.fiscal-actions{display:flex;flex-wrap:wrap;gap:.5rem;margin:1rem 0}article{padding:1rem;border:1px solid var(--border-subtle);border-radius:.75rem;background:var(--surface-secondary);min-width:0}small,p,li{overflow-wrap:anywhere}textarea{display:block;width:100%;min-height:44px}app-fiscal-fields{margin:1rem 0}p-fieldset{display:block;margin:1rem 0}:host ::ng-deep .p-button{min-height:44px}@media(max-width:640px){.fiscal-actions{flex-direction:column}.fiscal-actions p-button{width:100%}:host ::ng-deep .fiscal-actions .p-button{width:100%}textarea{font-size:16px}}`
})
export class WorkOrderFiscalComponent {
  readonly order = input.required<WorkOrder>(); readonly tenant = inject(TenantContextService); private readonly api = inject(FiscalApiService); private readonly destroy = inject(DestroyRef); private readonly messages = inject(MessageService);
  readonly data = signal<FiscalOrder | null>(null); readonly busy = signal(false); readonly error = signal(false); readonly editing = signal(false); readonly saveErrors = signal<string[]>([]); readonly cancelTarget = signal<FiscalDocument | null>(null); readonly reason = new FormControl('', { nonNullable: true });
  readonly addressFields = addressFields; readonly productFields = productFields; readonly serviceFields = serviceFields;
  readonly recipientFields: FiscalField[] = [
    { key: 'name', label: 'Nome / razão social do cliente' }, { key: 'document', label: 'CPF/CNPJ (somente números)' }, { key: 'competence', label: 'Competência do serviço', type: 'date' },
    { key: 'recipientIeIndicator', label: 'Inscrição estadual do cliente', options: [{ label: 'Não contribuinte', value: '9' }, { label: 'Contribuinte', value: '1' }, { label: 'Isento', value: '2' }] },
    { key: 'stateRegistration', label: 'Número da inscrição estadual' }, { key: 'paymentCode', label: 'Pagamento declarado para a NF-e', options: [{ label: 'Dinheiro', value: '01' }, { label: 'Cheque', value: '02' }, { label: 'PIX', value: '17' }, { label: 'Transferência', value: '18' }, { label: 'Sem pagamento', value: '90' }] },
    { key: 'paymentAmount', label: 'Valor do pagamento da NF-e', type: 'number' }
  ];
  readonly recipientForm = fiscalForm(this.recipientFields); readonly addressForm = fiscalForm(addressFields);
  readonly lineForms = signal<{ id: string; label: string; kind: 'parts' | 'services'; form: ReturnType<typeof fiscalForm> }[]>([]);
  private generation = 0; private timer?: ReturnType<typeof setTimeout>; private watched = 0;
  constructor() { effect(() => { const context = `${this.order().id}:${this.tenant.tenant()?.id}`; if (!context) return; this.generation++; this.data.set(null); clearTimeout(this.timer); void this.load(); }); this.destroy.onDestroy(() => { this.generation++; clearTimeout(this.timer); }); }
  private current(generation: number) { return !this.destroy.destroyed && generation === this.generation; }
  async load() { const generation = this.generation; try { const data = await this.api.order(this.order().id); if (!this.current(generation)) return; this.data.set(data); this.error.set(false); } catch { if (this.current(generation)) this.error.set(true); } }
  hasDownloads() { return this.data()?.documents.some(x => x.state === 'Authorized') ?? false; }
  stateLabel(state: string) { return ({ Pending: 'Ainda não emitido', Prepared: 'Preparado', Processing: 'Processando', AwaitingConfirmation: 'Aguardando confirmação', Authorized: 'Autorizado', Completed: 'Documentos emitidos', Partial: 'Emissão parcial', Rejected: 'Rejeitado', CancellationPending: 'Cancelamento pendente', Cancelled: 'Cancelado', Inutilized: 'Inutilizado' } as Record<string, string>)[state] ?? state; }
  async edit() {
    const preparation = this.data()?.preparation; if (!preparation) return; const generation = this.generation;
    this.recipientForm.patchValue(preparation as unknown as Record<string, FiscalValue>); this.addressForm.patchValue({ ...preparation.address }); this.saveErrors.set([]);
    const forms = await Promise.all(([...this.order().parts.map(x => ({ line: x, kind: 'parts' as const })), ...this.order().services.map(x => ({ line: x, kind: 'services' as const }))]).map(async ({ line, kind }) => {
      const data = (kind === 'parts' ? preparation.products : preparation.services)?.[line.id] ?? (line.catalogId ? await this.api.profile(kind, line.catalogId) : {});
      return { id: line.id, label: line.description, kind, form: fiscalForm(kind === 'parts' ? productFields : serviceFields, data) };
    }));
    if (!this.current(generation)) return; this.lineForms.set(forms); this.editing.set(true);
  }
  async save() {
    if (this.busy()) return; const generation = this.generation; this.busy.set(true);
    try {
      const products: Record<string, Record<string, FiscalValue>> = {}; const services: Record<string, Record<string, FiscalValue>> = {};
      for (const line of this.lineForms()) (line.kind === 'parts' ? products : services)[line.id] = line.form.getRawValue();
      await this.api.saveOrder(this.order().id, { ...this.recipientForm.getRawValue(), address: this.addressForm.getRawValue() as unknown as FiscalAddress, products, services } as unknown as FiscalPreparation);
      if (!this.current(generation)) return; await this.load(); this.saveErrors.set(this.data()?.issues.map(x => x.message) ?? []); if (!this.saveErrors().length) this.editing.set(false);
    } catch { if (this.current(generation)) this.messages.add({ severity: 'error', summary: 'Operação fiscal não concluída', detail: 'Revise a mensagem do servidor e atualize a situação antes de tentar novamente.' }); } finally { if (this.current(generation)) this.busy.set(false); }
  }
  async issue() {
    if (this.busy()) return; if (this.data()?.issues.length) { await this.edit(); return; }
    const generation = this.generation; this.busy.set(true);
    try { await this.api.issue(this.order().id); if (!this.current(generation)) return; await this.load(); if (this.data()?.status === 'Completed') await this.autoDownload(); else { this.watched = 0; this.watch(); } }
    catch { if (this.current(generation)) this.messages.add({ severity: 'error', summary: 'Emissão não concluída. Atualize a situação.' }); } finally { if (this.current(generation)) this.busy.set(false); }
  }
  private watch() { clearTimeout(this.timer); if (this.watched++ >= 12 || this.destroy.destroyed || !this.data()?.documents.some(x => ['Processing', 'AwaitingConfirmation', 'CancellationPending'].includes(x.state))) return; this.timer = setTimeout(() => { void this.refresh().then(() => { if (this.data()?.status === 'Completed') void this.autoDownload(); else this.watch(); }); }, 5000); }
  async refresh() { if (this.busy()) return; const generation = this.generation; this.busy.set(true); try { for (const d of this.data()?.documents ?? []) { if (!this.current(generation)) return; if (['Processing', 'AwaitingConfirmation', 'CancellationPending'].includes(d.state)) await this.api.sync(d.id); } if (this.current(generation)) await this.load(); } catch { if (this.current(generation)) this.messages.add({ severity: 'error', summary: 'Operação fiscal não concluída', detail: 'Revise a mensagem do servidor e atualize a situação antes de tentar novamente.' }); } finally { if (this.current(generation)) this.busy.set(false); } }
  private async autoDownload() { const docs = this.data()?.documents.filter(x => x.state === 'Authorized') ?? []; if (docs.length === 1) await this.download(docs[0], 'xml'); else if (docs.length > 1) await this.bundle(); }
  async download(document: FiscalDocument, type: 'xml' | 'pdf') { const generation = this.generation; const blob = await this.api.download(document.id, type); if (this.current(generation)) saveFiscalBlob(blob, `${document.kind}-${document.environment}-${document.id}.${type}`); }
  async bundle(pdf = false) { const generation = this.generation; const blob = await this.api.bundle(this.order().id, pdf); if (this.current(generation)) saveFiscalBlob(blob, `OS-${this.order().number}-fiscal${this.data()?.status === 'Partial' ? '-parcial' : ''}.zip`); }
  async cancel() { const target = this.cancelTarget(); if (!target || this.busy()) return; if (this.reason.value.trim().length < 15) { this.messages.add({ severity: 'warn', summary: 'Informe uma justificativa com pelo menos 15 caracteres' }); return; } const generation = this.generation; this.busy.set(true); try { await this.api.cancel(target.id, this.reason.value.trim()); if (!this.current(generation)) return; this.cancelTarget.set(null); this.reason.reset(); await this.load(); } catch { if (this.current(generation)) this.messages.add({ severity: 'error', summary: 'Operação fiscal não concluída', detail: 'Revise a mensagem do servidor e atualize a situação antes de tentar novamente.' }); } finally { if (this.current(generation)) this.busy.set(false); } }
}
