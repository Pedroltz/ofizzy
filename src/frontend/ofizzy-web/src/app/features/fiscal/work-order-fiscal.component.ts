import { ChangeDetectionStrategy, Component, DestroyRef, effect, inject, input, signal } from '@angular/core';
import { CurrencyPipe } from '@angular/common';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { MessageModule } from 'primeng/message';
import { DialogModule } from 'primeng/dialog';
import { FieldsetModule } from 'primeng/fieldset';
import { TextareaModule } from 'primeng/textarea';
import { TagModule } from 'primeng/tag';
import { MessageService } from 'primeng/api';
import { TenantContextService } from '../../core/tenancy/tenant-context.service';
import { WorkOrder } from '../../core/api/work-order.models';
import { SectionCardComponent } from '../../shared/components/section-card.component';
import { FiscalApiService, FiscalOrder, FiscalDocument, FiscalPreparation, FiscalAddress, FiscalValue, saveFiscalBlob } from './fiscal-api.service';
import { FiscalFieldsComponent, FiscalField, fiscalForm, addressFields, productFields, serviceFields } from './fiscal-fields.component';

@Component({
  selector: 'app-work-order-fiscal', standalone: true, changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CurrencyPipe, ReactiveFormsModule, ButtonModule, MessageModule, DialogModule, FieldsetModule, TextareaModule, TagModule, FiscalFieldsComponent, SectionCardComponent],
  template: `<section class="fiscal-card" aria-label="Documentos fiscais">
    <div class="fiscal-card-header">
      <div class="icon-circle info"><i class="pi pi-file-check"></i></div>
      <div>
        <h3 class="fiscal-title">Documentos fiscais</h3>
        <p class="fiscal-subtitle">Gestão e emissão de NF-e e NFS-e para esta ordem de serviço</p>
      </div>
    </div>
    @if (error()) {
      <div class="flex items-center gap-2">
        <p-message severity="error">Não foi possível consultar os documentos fiscais.</p-message>
        <p-button label="Tentar novamente" icon="pi pi-refresh" [outlined]="true" (onClick)="load()" />
      </div>
    }
    @if (data(); as fiscal) {
      <div class="fiscal-values-bar">
        @if (order().services.length) {
          <div class="fiscal-value-pill">
            <span class="label">Serviços · NFS-e</span>
            <strong>{{ fiscal.servicesTotal | currency:'BRL' }}</strong>
          </div>
        }
        @if (order().parts.length) {
          <div class="fiscal-value-pill">
            <span class="label">Produtos · NF-e</span>
            <strong>{{ fiscal.productsTotal | currency:'BRL' }}</strong>
          </div>
        }
      </div>
      <div class="fiscal-status-row" role="status" aria-live="polite">
        <span class="fiscal-status-label">Situação:</span>
        <p-tag [value]="stateLabel(fiscal.status)" [severity]="statusSeverity(fiscal.status)" />
      </div>
      @if (fiscal.status === 'Partial') { <p-message severity="warn">Uma parte já foi autorizada. A próxima tentativa processará somente a pendência.</p-message> }
      @if (fiscal.issues.length && fiscal.status !== 'Completed') {
        <div class="fiscal-issues-container">
          <p-message severity="warn">Existem {{ fiscal.issues.length }} pendências antes da emissão.</p-message>
          <ul class="fiscal-issues-list">@for (issue of fiscal.issues; track $index) { <li>{{ issue.message }}</li> }</ul>
        </div>
      }
      <div class="fiscal-actions">
        @if (fiscal.status !== 'Completed' && fiscal.status !== 'Processing') { <p-button label="Emitir documentos fiscais" icon="pi pi-send" [loading]="busy()" (onClick)="issue()" /> }
        <p-button label="Preparação fiscal" ariaLabel="Preparação fiscal" icon="pi pi-pencil" [outlined]="true" [disabled]="busy() || fiscal.status === 'Processing'" (onClick)="edit()" />
        <p-button label="Atualizar situação" icon="pi pi-refresh" [outlined]="true" [loading]="busy()" (onClick)="refresh()" />
        @if (hasDownloads()) { <p-button label="Baixar XMLs e PDFs" icon="pi pi-download" [outlined]="true" (onClick)="bundle(true)" /> }
      </div>
      <div class="fiscal-documents">@for (document of fiscal.documents; track document.id) {
        <article class="fiscal-doc-card">
          <div class="fiscal-doc-header">
            <strong>{{ document.kind === 'Nfe' ? 'NF-e · Produtos' : 'NFS-e · Serviços' }}</strong>
            <p-tag [value]="stateLabel(document.state)" [severity]="statusSeverity(document.state)" />
          </div>
          <p class="fiscal-doc-meta">{{ stateLabel(document.state) }} · {{ document.environment === 'Homologation' ? 'Homologação' : 'Produção' }}</p>
          @if (document.message) { <p class="fiscal-doc-msg">{{ document.message }}</p> }
          @if (document.accessKey) { <small class="fiscal-doc-key">Chave: {{ document.accessKey }}</small> }
          <div class="fiscal-doc-actions">@if (document.canDownload) { <p-button label="XML" icon="pi pi-file-code" [outlined]="true" size="small" (onClick)="download(document, 'xml')" /><p-button label="PDF da nota" icon="pi pi-file-pdf" [outlined]="true" size="small" (onClick)="download(document, 'pdf')" /> }
          @if (tenant.admin() && document.state === 'Authorized') { <p-button label="Cancelar nota" severity="danger" [text]="true" size="small" (onClick)="cancelTarget.set(document)" /> }</div>
        </article>
      }</div>
    } @else if (!error()) { <p role="status" class="text-muted text-sm my-2">Consultando documentos fiscais…</p> }
  </section>
  <p-dialog header="Preparação fiscal da OS" [modal]="true" [visible]="editing()" (visibleChange)="editing.set($event)" [style]="{ width: 'min(64rem, 95vw)' }" styleClass="wo-center-dialog fiscal-prep-dialog">
    @if (data()) {
      <div class="fiscal-prep-content">
        <div class="fiscal-prep-banner">
          <div class="icon-circle info flex-shrink-0"><i class="pi pi-info-circle"></i></div>
          <div>
            <h4 class="m-0 text-sm font-semibold text-primary">Complementos fiscais da Ordem de Serviço</h4>
            <p class="m-0 mt-1 text-xs text-secondary leading-relaxed">Os valores base são sincronizados a partir da OS. Os dados fiscais informados abaixo serão validados e vinculados aos documentos fiscais emitidos (NF-e / NFS-e).</p>
          </div>
        </div>
        <app-section-card title="Dados do Tomador / Cliente" subtitle="Identificação jurídica e fiscal do destinatário" icon="pi pi-user" padding="md">
          <app-fiscal-fields [fields]="clientFields" [form]="recipientForm" prefix="recipient-" />
        </app-section-card>
        <app-section-card title="Endereço Fiscal do Tomador" subtitle="Localização para cálculo e emissão de notas" icon="pi pi-map-marker" padding="md">
          <app-fiscal-fields [fields]="addressFields" [form]="addressForm" prefix="recipient-address-" />
        </app-section-card>
        <app-section-card title="Declaração de Pagamento (NF-e)" subtitle="Informações da forma de quitação para autorização na SEFAZ" icon="pi pi-wallet" padding="md">
          <app-fiscal-fields [fields]="paymentFields" [form]="recipientForm" prefix="recipient-" />
        </app-section-card>
        @if (lineForms().length) {
          <app-section-card title="Itens e Tributação" subtitle="Classificação fiscal, NCM/NBS e alíquotas de cada item da OS" icon="pi pi-tags" padding="md">
            <div class="fiscal-lines-list">
              @for (line of lineForms(); track line.id) {
                <p-fieldset [legend]="(line.kind === 'parts' ? 'Peça · ' : 'Serviço · ') + line.label" [toggleable]="true" [collapsed]="true" styleClass="fiscal-line-fieldset">
                  <div class="pt-2">
                    <app-fiscal-fields [fields]="line.kind === 'parts' ? productFields : serviceFields" [form]="line.form" [prefix]="line.id + '-'" />
                  </div>
                </p-fieldset>
              }
            </div>
          </app-section-card>
        }
        @if (saveErrors().length) {
          <p-message severity="error">
            <div class="flex flex-col gap-1 text-sm py-1">
              <span class="font-semibold">Existem pendências a serem corrigidas:</span>
              <ul class="list-disc pl-5 m-0 space-y-1">@for (message of saveErrors(); track $index) { <li>{{ message }}</li> }</ul>
            </div>
          </p-message>
        }
      </div>
    }
    <ng-template #footer>
      <div class="dialog-actions">
        <p-button label="Fechar" [text]="true" severity="secondary" (onClick)="editing.set(false)" />
        <p-button label="Salvar preparação" [loading]="busy()" (onClick)="save()" />
      </div>
    </ng-template>
  </p-dialog>
  <p-dialog header="Cancelar documento fiscal" [modal]="true" [visible]="!!cancelTarget()" (visibleChange)="!$event && cancelTarget.set(null)" [style]="{ width: 'min(36rem, 94vw)' }" styleClass="wo-center-dialog">
    <div class="fiscal-cancel-content">
      <div class="fiscal-cancel-alert">
        <i class="pi pi-exclamation-triangle text-amber-500 text-xl flex-shrink-0"></i>
        <div>
          <h4 class="font-semibold text-primary text-sm m-0 mb-1">Atenção ao cancelamento</h4>
          <p class="text-secondary text-xs m-0 leading-relaxed">A solicitação de cancelamento será transmitida à SEFAZ / Prefeitura. A ordem de serviço e seus registros históricos serão preservados.</p>
        </div>
      </div>
      <div>
        <label for="fiscal-cancel-reason" class="block text-xs font-semibold text-muted uppercase tracking-wider mb-2">Justificativa (15 a 255 caracteres) *</label>
        <textarea pTextarea id="fiscal-cancel-reason" [formControl]="reason" rows="4" maxlength="255" class="w-full" placeholder="Descreva o motivo do cancelamento conforme exigência fiscal..."></textarea>
      </div>
    </div>
    <ng-template #footer>
      <div class="dialog-actions">
        <p-button label="Voltar" [text]="true" severity="secondary" (onClick)="cancelTarget.set(null)" />
        <p-button label="Confirmar cancelamento" severity="danger" [loading]="busy()" (onClick)="cancel()" />
      </div>
    </ng-template>
  </p-dialog>`,
  styles: `.fiscal-card{background:var(--surface-secondary);border:1px solid var(--border-subtle);border-radius:var(--radius-md);padding:1.25rem 1.5rem;display:flex;flex-direction:column;gap:1.25rem;min-width:0}.fiscal-card-header{display:flex;align-items:center;gap:0.75rem}.fiscal-title{margin:0;font-size:1rem;font-weight:600;color:var(--text-primary);line-height:1.25}.fiscal-subtitle{margin:0.2rem 0 0;font-size:0.75rem;color:var(--text-muted)}.fiscal-values-bar{display:flex;flex-wrap:wrap;gap:0.75rem}.fiscal-value-pill{display:flex;align-items:center;gap:0.75rem;padding:0.5rem 0.85rem;background:var(--surface-primary);border:1px solid var(--border-subtle);border-radius:var(--radius-sm);font-size:0.85rem}.fiscal-value-pill .label{color:var(--text-secondary)}.fiscal-value-pill strong{color:var(--text-primary)}.fiscal-status-row{display:flex;align-items:center;gap:0.5rem}.fiscal-status-label{font-size:0.75rem;font-weight:600;color:var(--text-muted);text-transform:uppercase;letter-spacing:0.05em}.fiscal-issues-container{display:flex;flex-direction:column;gap:0.5rem}.fiscal-issues-list{list-style-type:disc;padding-left:1.5rem;margin:0.25rem 0 0;display:flex;flex-direction:column;gap:0.35rem;font-size:0.85rem;color:var(--text-secondary)}.fiscal-actions{display:flex;flex-wrap:wrap;gap:0.75rem;align-items:center}.fiscal-documents{display:grid;grid-template-columns:repeat(auto-fit,minmax(min(100%,280px),1fr));gap:1rem}.fiscal-doc-card{padding:1.25rem;border:1px solid var(--border-subtle);border-radius:var(--radius-md);background:var(--surface-primary);display:flex;flex-direction:column;gap:0.5rem;min-width:0}.fiscal-doc-header{display:flex;align-items:center;justify-content:space-between;gap:0.5rem}.fiscal-doc-meta{font-size:0.8rem;color:var(--text-muted);margin:0}.fiscal-doc-msg{font-size:0.85rem;color:var(--text-secondary);margin:0}.fiscal-doc-key{font-family:monospace;font-size:0.75rem;color:var(--text-muted);word-break:break-all}.fiscal-doc-actions{margin-top:auto;padding-top:0.75rem;border-top:1px solid var(--border-subtle);display:flex;flex-wrap:wrap;gap:0.5rem}.fiscal-prep-content,.fiscal-cancel-content{padding:1.5rem;display:flex;flex-direction:column;gap:1.25rem;box-sizing:border-box}.fiscal-prep-banner{display:flex;align-items:flex-start;gap:1rem;padding:1rem 1.25rem;background:var(--surface-secondary);border:1px solid var(--border-subtle);border-radius:var(--radius-md)}.fiscal-lines-list{display:flex;flex-direction:column;gap:0.75rem}:host ::ng-deep .fiscal-line-fieldset{border:1px solid var(--border-subtle);border-radius:var(--radius-sm);background:var(--surface-primary);overflow:hidden}:host ::ng-deep .fiscal-line-fieldset .p-fieldset-legend{background:var(--surface-secondary);border:1px solid var(--border-subtle);border-radius:var(--radius-sm);padding:0.5rem 0.85rem;font-size:0.85rem;font-weight:500}:host ::ng-deep .fiscal-line-fieldset .p-fieldset-content{padding:0.75rem 1rem}.fiscal-cancel-alert{display:flex;align-items:flex-start;gap:0.85rem;padding:1rem 1.25rem;background:rgba(245,158,11,0.08);border:1px solid rgba(245,158,11,0.25);border-radius:var(--radius-md)}small,p,li{overflow-wrap:anywhere}textarea{display:block;width:100%;min-height:44px}:host ::ng-deep .p-button{min-height:44px}.dialog-actions{display:flex;justify-content:flex-end;gap:0.5rem}@media(max-width:640px){.fiscal-prep-content,.fiscal-cancel-content{padding:1rem;gap:1rem}.fiscal-actions,.dialog-actions{flex-direction:column}.fiscal-actions p-button,.dialog-actions p-button{width:100%}:host ::ng-deep .fiscal-actions .p-button,:host ::ng-deep .dialog-actions .p-button{width:100%}textarea{font-size:16px}}`
})
export class WorkOrderFiscalComponent {
  readonly order = input.required<WorkOrder>(); readonly tenant = inject(TenantContextService); private readonly api = inject(FiscalApiService); private readonly destroy = inject(DestroyRef); private readonly messages = inject(MessageService);
  readonly data = signal<FiscalOrder | null>(null); readonly busy = signal(false); readonly error = signal(false); readonly editing = signal(false); readonly saveErrors = signal<string[]>([]); readonly cancelTarget = signal<FiscalDocument | null>(null); readonly reason = new FormControl('', { nonNullable: true });
  readonly addressFields = addressFields; readonly productFields = productFields; readonly serviceFields = serviceFields;
  readonly clientFields: FiscalField[] = [
    { key: 'name', label: 'Nome / razão social do cliente', colSpan: 2 },
    { key: 'document', label: 'CPF/CNPJ (somente números)' },
    { key: 'competence', label: 'Competência do serviço', type: 'date' },
    { key: 'recipientIeIndicator', label: 'Inscrição estadual do cliente', options: [{ label: 'Não contribuinte', value: '9' }, { label: 'Contribuinte', value: '1' }, { label: 'Isento', value: '2' }] },
    { key: 'stateRegistration', label: 'Número da inscrição estadual' }
  ];
  readonly paymentFields: FiscalField[] = [
    { key: 'paymentCode', label: 'Pagamento declarado para a NF-e', options: [{ label: 'Dinheiro', value: '01' }, { label: 'Cheque', value: '02' }, { label: 'PIX', value: '17' }, { label: 'Transferência', value: '18' }, { label: 'Sem pagamento', value: '90' }] },
    { key: 'paymentAmount', label: 'Valor do pagamento da NF-e', type: 'number' }
  ];
  readonly recipientFields: FiscalField[] = [...this.clientFields, ...this.paymentFields];
  readonly recipientForm = fiscalForm(this.recipientFields); readonly addressForm = fiscalForm(addressFields);
  readonly lineForms = signal<{ id: string; label: string; kind: 'parts' | 'services'; form: ReturnType<typeof fiscalForm> }[]>([]);
  private generation = 0; private timer?: ReturnType<typeof setTimeout>; private watched = 0;
  constructor() { effect(() => { const context = `${this.order().id}:${this.tenant.tenant()?.id}`; if (!context) return; this.generation++; this.data.set(null); clearTimeout(this.timer); void this.load(); }); this.destroy.onDestroy(() => { this.generation++; clearTimeout(this.timer); }); }
  private current(generation: number) { return !this.destroy.destroyed && generation === this.generation; }
  async load() { const generation = this.generation; try { const data = await this.api.order(this.order().id); if (!this.current(generation)) return; this.data.set(data); this.error.set(false); } catch { if (this.current(generation)) this.error.set(true); } }
  hasDownloads() { return this.data()?.documents.some(x => x.state === 'Authorized') ?? false; }
  stateLabel(state: string) { return ({ Pending: 'Ainda não emitido', Prepared: 'Preparado', Processing: 'Processando', AwaitingConfirmation: 'Aguardando confirmação', Authorized: 'Autorizado', Completed: 'Documentos emitidos', Partial: 'Emissão parcial', Rejected: 'Rejeitado', CancellationPending: 'Cancelamento pendente', Cancelled: 'Cancelado', Inutilized: 'Inutilizado' } as Record<string, string>)[state] ?? state; }
  statusSeverity(state: string): 'success' | 'info' | 'warn' | 'danger' | 'secondary' {
    switch (state) {
      case 'Completed':
      case 'Authorized':
        return 'success';
      case 'Processing':
      case 'AwaitingConfirmation':
      case 'Prepared':
        return 'info';
      case 'Partial':
      case 'CancellationPending':
        return 'warn';
      case 'Rejected':
      case 'Cancelled':
        return 'danger';
      default:
        return 'secondary';
    }
  }
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
