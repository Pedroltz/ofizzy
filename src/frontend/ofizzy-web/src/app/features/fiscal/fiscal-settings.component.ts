import { ChangeDetectionStrategy, Component, DestroyRef, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { MessageModule } from 'primeng/message';
import { FieldsetModule } from 'primeng/fieldset';
import { SelectModule } from 'primeng/select';
import { InputTextModule } from 'primeng/inputtext';
import { MessageService } from 'primeng/api';
import { TenantContextService } from '../../core/tenancy/tenant-context.service';
import { CatalogApiService } from '../../core/api/catalog-api.service';
import { FiscalApiService, FiscalSettings, FiscalSettingsResponse, FiscalAddress, FiscalValue, saveFiscalBlob } from './fiscal-api.service';
import { FiscalInutilizationsComponent } from './fiscal-inutilizations.component';
import { FiscalFieldsComponent, FiscalField, fiscalForm, addressFields, productFields, serviceFields } from './fiscal-fields.component';
@Component({
  selector: 'app-fiscal-settings', standalone: true, changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [DatePipe, ReactiveFormsModule, ButtonModule, MessageModule, FieldsetModule, SelectModule, InputTextModule, FiscalFieldsComponent, FiscalInutilizationsComponent],
  template: `@if (tenant.admin()) {
    <div class="fiscal-settings">
      <p-message severity="info">Configure os dados com a contabilidade. Serviços e produtos geram documentos separados. A operação fiscal não registra recebimentos no financeiro.</p-message>
      @if (actionError()) { <p-message severity="error">{{ actionError() }}</p-message> }
      @if (loadError()) { <p-message severity="error">Não foi possível carregar a configuração.</p-message><p-button label="Tentar novamente" (onClick)="load()" /> }
      @if (data(); as value) {
        <p-fieldset legend="Dados fiscais da empresa"><app-fiscal-fields [fields]="settingsFields" [form]="settingsForm" prefix="issuer-" /><app-fiscal-fields [fields]="addressFields" [form]="addressForm" prefix="issuer-address-" /></p-fieldset>
        <p-button label="Salvar configuração fiscal" [loading]="busy()" (onClick)="save()" />
        <p-fieldset legend="Certificado digital A1">
          @if (value.certificate; as certificate) { <p>{{ certificate.subject }} · Validade: {{ certificate.expiresAt | date:'dd/MM/yyyy' }}</p> }
          @else { <p>Nenhum certificado cadastrado.</p> }
          @if (!value.encryptionConfigured) { <p-message severity="warn">O administrador do servidor precisa configurar a proteção fiscal antes do envio do certificado.</p-message> }
          <label for="fiscal-file">Arquivo A1 (.pfx ou .p12)<input id="fiscal-file" type="file" accept=".pfx,.p12" (change)="chooseFile($event)" /></label>
          <label for="fiscal-password">Senha do certificado<input pInputText id="fiscal-password" type="password" [formControl]="password" autocomplete="new-password" /></label>
          <div style="display:flex;gap:.5rem;flex-wrap:wrap;align-items:center">
            <p-button label="Cadastrar ou substituir certificado" [loading]="busy()" (onClick)="upload()" />
            @if (value.devToolsAvailable) { <p-button label="Baixar certificado A1 de teste (Dev)" ariaLabel="Baixar certificado A1 de teste (Dev)" severity="secondary" [outlined]="true" icon="pi pi-download" (onClick)="downloadTestCert()" /> }
          </div>
        </p-fieldset>
        <p-fieldset legend="Classificação fiscal do catálogo">
          <p>Selecione um item. Perfis suportados de produtos: revenda interna 5102/102 ou 5405/500. Dados de ST são valores por unidade, conforme documentação de entrada.</p>
          <label for="fiscal-search">Buscar no catálogo<input pInputText id="fiscal-search" [formControl]="search" /></label><p-button label="Buscar itens" (onClick)="searchCatalog()" />
          <p-select inputId="fiscal-catalog" ariaLabel="Item do catálogo" [options]="catalog()" optionLabel="label" optionValue="value" [formControl]="selected" (onChange)="loadProfile()" appendTo="body" placeholder="Selecione serviço ou produto" />
          @if (profileForm(); as form) {
            <app-fiscal-fields [fields]="profileKind() === 'parts' ? productFields : serviceFields" [form]="form" prefix="profile-" />
            <p-button label="Salvar classificação" [loading]="busy()" (onClick)="saveProfile()" />
          }
        </p-fieldset>
        @if (tenant.has('WorkOrders')) { <p-fieldset legend="Histórico e inutilização NF-e" [toggleable]="true" [collapsed]="true"><app-fiscal-inutilizations /></p-fieldset> }
      }
    </div>
  } @else { <p-message severity="info">A configuração fiscal é administrada pelo responsável da organização.</p-message> }`,
  styles: `.fiscal-settings{display:flex;flex-direction:column;gap:1.25rem;min-width:0}label{display:flex;flex-direction:column;gap:.5rem;margin:1rem 0}input{min-height:44px;max-width:100%}p-select{width:100%;min-height:44px}app-fiscal-fields{margin-bottom:1rem}p{overflow-wrap:anywhere}:host ::ng-deep .p-button{min-height:44px}@media(max-width:640px){input{font-size:16px}}`
})
export class FiscalSettingsComponent {
  readonly tenant = inject(TenantContextService); private readonly api = inject(FiscalApiService); private readonly catalogs = inject(CatalogApiService); private readonly messages = inject(MessageService);
  private readonly destroy = inject(DestroyRef); private readonly tenantId = this.tenant.tenant()?.id;
  readonly actionError = signal('');
  readonly data = signal<FiscalSettingsResponse | null>(null); readonly busy = signal(false); readonly loadError = signal(false);
  readonly addressFields = addressFields; readonly productFields = productFields; readonly serviceFields = serviceFields;
  readonly settingsFields: FiscalField[] = [
    { key: 'cnpj', label: 'CNPJ (somente números)' }, { key: 'legalName', label: 'Razão social' }, { key: 'stateRegistration', label: 'Inscrição estadual' }, { key: 'municipalRegistration', label: 'Inscrição municipal' },
    { key: 'regime', label: 'Regime', options: [{ label: 'MEI', value: 'MEI' }, { label: 'Simples Nacional', value: 'SimplesNacional' }] },
    { key: 'environment', label: 'Ambiente', options: [{ label: 'Homologação (sem valor fiscal)', value: 'Homologation' }, { label: 'Produção (exige liberação)', value: 'Production' }] },
    { key: 'nfeEnabled', label: 'NF-e de produtos', options: [{ label: 'Desabilitada', value: false }, { label: 'Habilitada', value: true }] },
    { key: 'nfseEnabled', label: 'NFS-e de serviços', options: [{ label: 'Desabilitada', value: false }, { label: 'Habilitada', value: true }] },
    { key: 'nfeSeries', label: 'Série da NF-e', type: 'number' }, { key: 'dpsSeries', label: 'Série da DPS', type: 'number' }
  ];
  readonly settingsForm = fiscalForm(this.settingsFields); readonly addressForm = fiscalForm(addressFields);
  readonly password = new FormControl('', { nonNullable: true }); private file: File | null = null;
  readonly search = new FormControl('', { nonNullable: true });
  readonly catalog = signal<{ label: string; value: string }[]>([]); readonly selected = new FormControl(''); readonly profileForm = signal<ReturnType<typeof fiscalForm> | null>(null); readonly profileKind = signal<'parts' | 'services'>('parts');
  private valid() { return !this.destroy.destroyed && this.tenant.tenant()?.id === this.tenantId; }
  constructor() { if (this.tenant.admin()) void this.load(); this.destroy.onDestroy(() => { this.password.reset(); this.file = null; }); }
  async load() {
    this.loadError.set(false);
    try {
      const data = await this.api.settings(); if (!this.valid()) return;
      this.data.set(data); this.settingsForm.patchValue(data.settings as unknown as Record<string, FiscalValue>); this.addressForm.patchValue({ ...data.settings.address });
      if (this.tenant.has('Catalog')) { const [parts, services] = await Promise.all([this.catalogs.parts(this.search.value), this.catalogs.services(this.search.value)]); if (!this.valid()) return; this.catalog.set([...parts.items.map(x => ({ label: 'Produto · ' + x.name, value: 'parts/' + x.id })), ...services.items.map(x => ({ label: 'Serviço · ' + x.name, value: 'services/' + x.id }))]); }
    } catch { if (this.valid()) this.loadError.set(true); }
  }
  async searchCatalog() {
    const query = this.search.value;
    try {
      const [parts, services] = await Promise.all([this.catalogs.parts(query), this.catalogs.services(query)]);
      if (!this.valid() || query !== this.search.value) return;
      this.catalog.set([...parts.items.map(x => ({ label: 'Produto · ' + x.name, value: 'parts/' + x.id })), ...services.items.map(x => ({ label: 'Serviço · ' + x.name, value: 'services/' + x.id }))]);
    } catch { if (this.valid()) this.actionError.set('Não foi possível buscar os itens do catálogo.'); }
  }
  async save() { if (this.busy()) return; this.busy.set(true); this.actionError.set(''); try { await this.api.saveSettings({ ...this.settingsForm.getRawValue(), address: this.addressForm.getRawValue() as unknown as FiscalAddress } as unknown as FiscalSettings); if (this.valid()) { this.messages.add({ severity: 'success', summary: 'Configuração fiscal salva' }); await this.load(); } } catch { if (this.valid()) this.actionError.set('Não foi possível salvar. Revise os dados e a mensagem retornada pelo servidor.'); } finally { if (this.valid()) this.busy.set(false); } }
  chooseFile(event: Event) { this.file = (event.target as HTMLInputElement).files?.[0] ?? null; }
  async upload() { if (this.busy()) return; if (!this.file) { this.messages.add({ severity: 'warn', summary: 'Selecione o certificado A1' }); return; } this.busy.set(true); this.actionError.set(''); try { await this.api.certificate(this.file, this.password.value); if (this.valid()) { this.password.reset(); this.file = null; await this.load(); this.messages.add({ severity: 'success', summary: 'Certificado protegido e cadastrado' }); } } catch { if (this.valid()) this.actionError.set('Não foi possível salvar. Revise os dados e a mensagem retornada pelo servidor.'); } finally { this.password.reset(); if (this.valid()) this.busy.set(false); } }
  async loadProfile() { const selected = this.selected.value; if (!selected) return; const [kind, id] = selected.split('/') as ['parts' | 'services', string]; this.profileForm.set(null); try { const data = await this.api.profile(kind, id); if (!this.valid() || this.selected.value !== selected) return; this.profileKind.set(kind); this.profileForm.set(fiscalForm(kind === 'parts' ? productFields : serviceFields, data)); } catch { if (this.valid()) this.actionError.set('Não foi possível carregar a classificação.'); } }
  async saveProfile() { const form = this.profileForm(); if (!form || !this.selected.value || this.busy()) return; const [kind, id] = this.selected.value.split('/') as ['parts' | 'services', string]; this.busy.set(true); this.actionError.set(''); try { await this.api.saveProfile(kind, id, form.getRawValue()); if (this.valid()) this.messages.add({ severity: 'success', summary: 'Classificação fiscal salva' }); } catch { if (this.valid()) this.actionError.set('Não foi possível salvar. Revise os dados e a mensagem retornada pelo servidor.'); } finally { if (this.valid()) this.busy.set(false); } }
  async downloadTestCert() {
    if (!this.data()?.devToolsAvailable || this.busy()) return;
    this.busy.set(true); this.actionError.set('');
    try {
      const blob = await this.api.downloadDevCertificate();
      if (!this.valid()) return;
      const cnpj = this.settingsForm.get('cnpj')?.value || 'teste';
      saveFiscalBlob(blob, `ofizzy-dev-${cnpj}.pfx`);
      this.messages.add({ severity: 'info', summary: 'Certificado de teste baixado', detail: 'Senha padrão: teste123' });
    } catch {
      if (this.valid()) this.actionError.set('Não foi possível baixar o certificado de teste. Disponível apenas em desenvolvimento com simulação e homologação habilitadas.');
    } finally { if (this.valid()) this.busy.set(false); }
  }
}
