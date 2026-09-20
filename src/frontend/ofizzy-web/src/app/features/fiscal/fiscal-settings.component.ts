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
import {
  FiscalApiService,
  FiscalSettings,
  FiscalSettingsResponse,
  FiscalHomologationReadiness,
  FiscalAddress,
  FiscalValue,
  saveFiscalBlob,
} from './fiscal-api.service';
import { FiscalInutilizationsComponent } from './fiscal-inutilizations.component';
import {
  FiscalFieldsComponent,
  FiscalField,
  fiscalForm,
  addressFields,
  productFields,
  serviceFields,
} from './fiscal-fields.component';
@Component({
  selector: 'app-fiscal-settings',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    DatePipe,
    ReactiveFormsModule,
    ButtonModule,
    MessageModule,
    FieldsetModule,
    SelectModule,
    InputTextModule,
    FiscalFieldsComponent,
    FiscalInutilizationsComponent,
  ],
  template: `@if (tenant.admin()) {
      <div class="fiscal-settings">
        <p-message severity="info"
          >Configure os dados com a contabilidade. Serviços e produtos geram documentos separados. A
          operação fiscal não registra recebimentos no financeiro.</p-message
        >
        @if (actionError()) {
          <p-message severity="error">{{ actionError() }}</p-message>
        }
        @if (loadError()) {
          <div class="flex items-center gap-2">
            <p-message severity="error">Não foi possível carregar a configuração.</p-message>
            <p-button
              label="Tentar novamente"
              icon="pi pi-refresh"
              [outlined]="true"
              (onClick)="load()"
            />
          </div>
        }
        @if (data(); as value) {
          <p-fieldset legend="Dados fiscais da empresa" styleClass="fiscal-fieldset">
            <div class="fiscal-section">
              <h4 class="text-xs font-semibold text-muted uppercase tracking-wider mb-2">
                Dados Gerais e Tributários
              </h4>
              <app-fiscal-fields [fields]="settingsFields" [form]="settingsForm" prefix="issuer-" />
            </div>
            <div class="fiscal-section pt-4 border-t border-subtle mt-4">
              <h4 class="text-xs font-semibold text-muted uppercase tracking-wider mb-2">
                Endereço Fiscal da Empresa
              </h4>
              <app-fiscal-fields
                [fields]="addressFields"
                [form]="addressForm"
                prefix="issuer-address-"
              />
            </div>
            <div class="flex justify-end mt-4 pt-3 border-t border-subtle">
              <p-button
                label="Salvar configuração fiscal"
                icon="pi pi-check"
                [loading]="busy()"
                (onClick)="save()"
              />
            </div>
          </p-fieldset>
          @if (readiness(); as readiness) {
            <p-fieldset legend="Prontidão para homologação" styleClass="fiscal-fieldset">
              <p-message [severity]="readiness.readyForExternalHomologation ? 'success' : 'warn'" class="mb-4 block">
                @if (readiness.readyForExternalHomologation) {
                  Os requisitos técnicos locais foram conferidos. Registre as confirmações externas antes de transmitir.
                } @else {
                  Existem bloqueios técnicos que precisam ser resolvidos antes da homologação oficial.
                }
              </p-message>
              <div class="homologation-checks">
                @for (check of readiness.checks; track check.code) {
                  <div class="homologation-check">
                    <i
                      [class]="check.passed ? 'pi pi-check-circle text-success' : 'pi pi-exclamation-triangle text-warning'"
                      aria-hidden="true"
                    ></i>
                    <div>
                      <strong>{{ check.label }}</strong>
                      <p>{{ check.detail }}</p>
                    </div>
                    <span class="homologation-check__status" [class.homologation-check__status--ok]="check.passed">
                      {{ check.passed ? 'Conferido' : check.requiresExternalConfirmation ? 'Pendente externo' : 'Bloqueado' }}
                    </span>
                  </div>
                }
              </div>
            </p-fieldset>
          }
          <p-fieldset legend="Certificado digital A1" styleClass="fiscal-fieldset">
            @if (value.certificate; as certificate) {
              <div class="fiscal-cert-badge mb-4">
                <div class="flex items-center gap-2.5">
                  <i class="pi pi-shield text-success text-lg" aria-hidden="true"></i>
                  <div>
                    <strong class="text-primary text-sm block">{{ certificate.subject }}</strong>
                    <span class="text-xs text-muted"
                      >Validade: {{ certificate.expiresAt | date: 'dd/MM/yyyy' }}</span
                    >
                  </div>
                </div>
                <span
                  class="font-mono text-xs text-success bg-success-soft px-2 py-0.5 rounded border border-subtle"
                  >Ativo</span
                >
              </div>
            } @else {
              <p class="text-sm text-muted mb-4">Nenhum certificado cadastrado.</p>
            }
            @if (!value.encryptionConfigured) {
              <p-message severity="warn" class="mb-4 block"
                >O administrador do servidor precisa configurar a proteção fiscal antes do envio do
                certificado.</p-message
              >
            }
            <div class="grid grid-cols-1 md:grid-cols-2 gap-4 mb-4">
              <div>
                <label
                  for="fiscal-file"
                  class="block text-xs font-semibold text-muted uppercase tracking-wider mb-1.5"
                  >Arquivo A1 (.pfx ou .p12)</label
                >
                <input
                  id="fiscal-file"
                  type="file"
                  accept=".pfx,.p12"
                  class="w-full"
                  (change)="chooseFile($event)"
                />
              </div>
              <div>
                <label
                  for="fiscal-password"
                  class="block text-xs font-semibold text-muted uppercase tracking-wider mb-1.5"
                  >Senha do certificado</label
                >
                <input
                  pInputText
                  id="fiscal-password"
                  type="password"
                  [formControl]="password"
                  autocomplete="new-password"
                  class="w-full"
                  [invalid]="password.invalid && password.touched"
                />
                @if (password.invalid && password.touched) {
                  <small class="fiscal-error-msg" role="alert">
                    <i class="pi pi-exclamation-circle" aria-hidden="true"></i>
                    <span>Informe a senha do certificado</span>
                  </small>
                }
              </div>
            </div>
            <div class="flex flex-wrap gap-2.5 items-center mt-2">
              <p-button
                label="Cadastrar ou substituir certificado"
                [loading]="busy()"
                (onClick)="upload()"
              />
              @if (value.devToolsAvailable && isHomologation()) {
                <p-button
                  label="Baixar certificado A1 de teste (Dev)"
                  ariaLabel="Baixar certificado A1 de teste (Dev)"
                  severity="secondary"
                  [outlined]="true"
                  icon="pi pi-download"
                  (onClick)="downloadTestCert()"
                />
              }
            </div>
            @if (value.devToolsAvailable && isHomologation()) {
              <div class="dev-cert-subtle-hint">
                <span>Senha padrão de homologação: <span class="font-mono">teste123</span></span>
              </div>
            }
          </p-fieldset>
          <p-fieldset legend="Classificação fiscal do catálogo" styleClass="fiscal-fieldset">
            <p class="text-sm text-muted mb-4">
              Selecione um item. Perfis suportados de produtos: revenda interna 5102/102 ou
              5405/500. Dados de ST são valores por unidade, conforme documentação de entrada.
            </p>
            <div class="grid grid-cols-1 md:grid-cols-[1fr_auto] gap-3 items-end mb-4">
              <div>
                <label
                  for="fiscal-search"
                  class="block text-xs font-semibold text-muted uppercase tracking-wider mb-1.5"
                  >Buscar no catálogo</label
                >
                <input
                  pInputText
                  id="fiscal-search"
                  [formControl]="search"
                  placeholder="Digite para buscar produtos e serviços..."
                  class="w-full"
                />
              </div>
              <p-button label="Buscar itens" [outlined]="true" (onClick)="searchCatalog()" />
            </div>
            <div class="mb-4">
              <label
                for="fiscal-catalog"
                class="block text-xs font-semibold text-muted uppercase tracking-wider mb-1.5"
                >Item do catálogo</label
              >
              <p-select
                inputId="fiscal-catalog"
                ariaLabel="Item do catálogo"
                [options]="catalog()"
                optionLabel="label"
                optionValue="value"
                [formControl]="selected"
                (onChange)="loadProfile()"
                appendTo="body"
                placeholder="Selecione serviço ou produto"
                class="w-full"
              />
            </div>
            @if (profileForm(); as form) {
              <div class="pt-4 border-t border-subtle mt-4">
                <h4 class="text-xs font-semibold text-muted uppercase tracking-wider mb-3">
                  Parâmetros Fiscais do Item
                </h4>
                <app-fiscal-fields
                  [fields]="profileKind() === 'parts' ? productFields : serviceFields"
                  [form]="form"
                  prefix="profile-"
                />
                <div class="flex justify-end mt-4 pt-3 border-t border-subtle">
                  <p-button
                    label="Salvar classificação"
                    icon="pi pi-check"
                    [loading]="busy()"
                    (onClick)="saveProfile()"
                  />
                </div>
              </div>
            }
          </p-fieldset>
          @if (tenant.has('WorkOrders')) {
            <p-fieldset
              legend="Histórico e inutilização NF-e"
              [toggleable]="true"
              [collapsed]="true"
              styleClass="fiscal-fieldset"
            >
              <app-fiscal-inutilizations />
            </p-fieldset>
          }
        }
      </div>
    } @else {
      <p-message severity="info"
        >A configuração fiscal é administrada pelo responsável da organização.</p-message
      >
    }`,
  styles: `
    .fiscal-settings {
      display: flex;
      flex-direction: column;
      gap: 1.75rem;
      min-width: 0;
    }
    .fiscal-cert-badge {
      padding: 0.75rem 1rem;
      border-radius: var(--radius-sm);
      background: var(--surface-secondary);
      border: 1px solid var(--border-subtle);
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: 0.75rem;
      flex-wrap: wrap;
    }
    .homologation-checks {
      display: grid;
      gap: 0.75rem;
    }
    .homologation-check {
      display: grid;
      grid-template-columns: auto minmax(0, 1fr) auto;
      align-items: start;
      gap: 0.65rem;
      padding: 0.75rem;
      border: 1px solid var(--border-subtle);
      border-radius: var(--radius-sm);
      background: var(--surface-secondary);
    }
    .homologation-check > i {
      margin-top: 0.15rem;
    }
    .homologation-check p {
      margin: 0.2rem 0 0;
      font-size: 0.8125rem;
      color: var(--text-muted);
    }
    .homologation-check__status {
      font-size: 0.75rem;
      font-weight: 600;
      color: var(--warning);
      background: var(--warning-soft);
      border-radius: 999px;
      padding: 0.2rem 0.5rem;
      white-space: nowrap;
    }
    .homologation-check__status--ok {
      color: var(--success);
      background: var(--success-soft);
    }
    .dev-cert-subtle-hint {
      margin-top: 0.4rem;
      font-size: 0.75rem;
      color: var(--text-muted);
    }
    input {
      min-height: 44px;
      max-width: 100%;
    }
    p-select {
      width: 100%;
      min-height: 44px;
    }
    p {
      overflow-wrap: anywhere;
    }
    :host ::ng-deep .p-button {
      min-height: 44px;
    }
    @media (max-width: 640px) {
      .homologation-check {
        grid-template-columns: auto minmax(0, 1fr);
      }
      .homologation-check__status {
        grid-column: 2;
        justify-self: start;
      }
      input {
        font-size: 16px;
      }
    }
  `,
})
export class FiscalSettingsComponent {
  readonly tenant = inject(TenantContextService);
  private readonly api = inject(FiscalApiService);
  private readonly catalogs = inject(CatalogApiService);
  private readonly messages = inject(MessageService);
  private readonly destroy = inject(DestroyRef);
  private readonly tenantId = this.tenant.tenant()?.id;
  readonly actionError = signal('');
  readonly data = signal<FiscalSettingsResponse | null>(null);
  readonly readiness = signal<FiscalHomologationReadiness | null>(null);
  readonly busy = signal(false);
  readonly loadError = signal(false);
  readonly addressFields = addressFields;
  readonly productFields = productFields;
  readonly serviceFields = serviceFields;
  readonly settingsFields: FiscalField[] = [
    {
      key: 'cnpj',
      label: 'CNPJ (somente números)',
      required: true,
      pattern: /^\d{14}$/,
      patternMessage: 'CNPJ deve conter 14 dígitos numéricos',
    },
    { key: 'legalName', label: 'Razão social', required: true, maxLength: 60 },
    { key: 'stateRegistration', label: 'Inscrição estadual', maxLength: 14 },
    { key: 'municipalRegistration', label: 'Inscrição municipal', maxLength: 15 },
    {
      key: 'regime',
      label: 'Regime',
      required: true,
      options: [
        { label: 'MEI', value: 'MEI' },
        { label: 'Simples Nacional', value: 'SimplesNacional' },
      ],
    },
    {
      key: 'environment',
      label: 'Ambiente',
      required: true,
      options: [
        { label: 'Homologação (sem valor fiscal)', value: 'Homologation' },
        { label: 'Produção (exige liberação)', value: 'Production' },
      ],
    },
    {
      key: 'nfeEnabled',
      label: 'NF-e de produtos',
      options: [
        { label: 'Desabilitada', value: false },
        { label: 'Habilitada', value: true },
      ],
    },
    {
      key: 'nfseEnabled',
      label: 'NFS-e de serviços',
      options: [
        { label: 'Desabilitada', value: false },
        { label: 'Habilitada', value: true },
      ],
    },
    { key: 'nfeSeries', label: 'Série da NF-e', type: 'number', required: true },
    { key: 'dpsSeries', label: 'Série da DPS', type: 'number', required: true },
  ];
  readonly settingsForm = fiscalForm(this.settingsFields);
  readonly addressForm = fiscalForm(addressFields);
  readonly password = new FormControl('', { nonNullable: true });
  private file: File | null = null;
  readonly search = new FormControl('', { nonNullable: true });
  readonly catalog = signal<{ label: string; value: string }[]>([]);
  readonly selected = new FormControl('');
  readonly profileForm = signal<ReturnType<typeof fiscalForm> | null>(null);
  readonly profileKind = signal<'parts' | 'services'>('parts');
  private valid() {
    return !this.destroy.destroyed && this.tenant.tenant()?.id === this.tenantId;
  }
  constructor() {
    if (this.tenant.admin()) void this.load();
    this.destroy.onDestroy(() => {
      this.password.reset();
      this.file = null;
    });
  }
  async load() {
    this.loadError.set(false);
    try {
      const data = await this.api.settings();
      if (!this.valid()) return;
      this.data.set(data);
      this.settingsForm.patchValue(data.settings as unknown as Record<string, FiscalValue>);
      this.addressForm.patchValue({ ...data.settings.address });
      try {
        const readiness = await this.api.homologationReadiness();
        if (this.valid()) this.readiness.set(readiness);
      } catch {
        if (this.valid()) this.readiness.set(null);
      }
      if (this.tenant.has('Catalog')) {
        const [parts, services] = await Promise.all([
          this.catalogs.parts(this.search.value),
          this.catalogs.services(this.search.value),
        ]);
        if (!this.valid()) return;
        this.catalog.set([
          ...parts.items.map((x) => ({ label: 'Produto · ' + x.name, value: 'parts/' + x.id })),
          ...services.items.map((x) => ({
            label: 'Serviço · ' + x.name,
            value: 'services/' + x.id,
          })),
        ]);
      }
    } catch {
      if (this.valid()) this.loadError.set(true);
    }
  }
  async searchCatalog() {
    const query = this.search.value;
    try {
      const [parts, services] = await Promise.all([
        this.catalogs.parts(query),
        this.catalogs.services(query),
      ]);
      if (!this.valid() || query !== this.search.value) return;
      this.catalog.set([
        ...parts.items.map((x) => ({ label: 'Produto · ' + x.name, value: 'parts/' + x.id })),
        ...services.items.map((x) => ({ label: 'Serviço · ' + x.name, value: 'services/' + x.id })),
      ]);
    } catch {
      if (this.valid()) this.actionError.set('Não foi possível buscar os itens do catálogo.');
    }
  }
  async save() {
    if (this.busy()) return;
    if (
      this.settingsForm.get('nfeEnabled')?.value &&
      !this.settingsForm.get('stateRegistration')?.value
    ) {
      this.settingsForm
        .get('stateRegistration')
        ?.setErrors({
          custom: 'Inscrição estadual é obrigatória quando a NF-e estiver habilitada.',
        });
    }
    if (this.settingsForm.invalid || this.addressForm.invalid) {
      this.settingsForm.markAllAsTouched();
      this.addressForm.markAllAsTouched();
      this.messages.add({
        severity: 'error',
        summary: 'Dados fiscais pendentes',
        detail: 'Preencha os campos destacados em vermelho antes de salvar.',
      });
      return;
    }
    this.busy.set(true);
    this.actionError.set('');
    try {
      await this.api.saveSettings({
        ...this.settingsForm.getRawValue(),
        address: this.addressForm.getRawValue() as unknown as FiscalAddress,
      } as unknown as FiscalSettings);
      if (this.valid()) {
        this.messages.add({ severity: 'success', summary: 'Configuração fiscal salva' });
        await this.load();
      }
    } catch {
      if (this.valid())
        this.actionError.set(
          'Não foi possível salvar. Revise os dados e a mensagem retornada pelo servidor.',
        );
    } finally {
      if (this.valid()) this.busy.set(false);
    }
  }
  chooseFile(event: Event) {
    this.file = (event.target as HTMLInputElement).files?.[0] ?? null;
  }
  async upload() {
    if (this.busy()) return;
    if (!this.file) {
      this.messages.add({ severity: 'warn', summary: 'Selecione o arquivo do certificado A1' });
      return;
    }
    if (!this.password.value) {
      this.password.setErrors({ required: true });
      this.password.markAsTouched();
      this.messages.add({ severity: 'warn', summary: 'Informe a senha do certificado A1' });
      return;
    }
    this.busy.set(true);
    this.actionError.set('');
    try {
      await this.api.certificate(this.file, this.password.value);
      if (this.valid()) {
        this.password.reset();
        this.file = null;
        await this.load();
        this.messages.add({ severity: 'success', summary: 'Certificado protegido e cadastrado' });
      }
    } catch {
      if (this.valid())
        this.actionError.set(
          'Não foi possível salvar. Revise os dados e a mensagem retornada pelo servidor.',
        );
    } finally {
      this.password.reset();
      if (this.valid()) this.busy.set(false);
    }
  }
  async loadProfile() {
    const selected = this.selected.value;
    if (!selected) return;
    const [kind, id] = selected.split('/') as ['parts' | 'services', string];
    this.profileForm.set(null);
    try {
      const data = await this.api.profile(kind, id);
      if (!this.valid() || this.selected.value !== selected) return;
      this.profileKind.set(kind);
      this.profileForm.set(fiscalForm(kind === 'parts' ? productFields : serviceFields, data));
    } catch {
      if (this.valid()) this.actionError.set('Não foi possível carregar a classificação.');
    }
  }
  async saveProfile() {
    const form = this.profileForm();
    if (!form || !this.selected.value || this.busy()) return;
    if (form.invalid) {
      form.markAllAsTouched();
      this.messages.add({
        severity: 'error',
        summary: 'Classificação fiscal pendente',
        detail: 'Preencha os campos destacados em vermelho antes de salvar.',
      });
      return;
    }
    const [kind, id] = this.selected.value.split('/') as ['parts' | 'services', string];
    this.busy.set(true);
    this.actionError.set('');
    try {
      await this.api.saveProfile(kind, id, form.getRawValue());
      if (this.valid())
        this.messages.add({ severity: 'success', summary: 'Classificação fiscal salva' });
    } catch {
      if (this.valid())
        this.actionError.set(
          'Não foi possível salvar. Revise os dados e a mensagem retornada pelo servidor.',
        );
    } finally {
      if (this.valid()) this.busy.set(false);
    }
  }
  isHomologation(): boolean {
    const formEnv = this.settingsForm.get('environment')?.value;
    const currentEnv = formEnv ?? this.data()?.settings?.environment;
    return currentEnv === 'Homologation' || currentEnv === '2' || currentEnv === 2;
  }
  async downloadTestCert() {
    if (!this.data()?.devToolsAvailable || !this.isHomologation() || this.busy()) return;
    this.busy.set(true);
    this.actionError.set('');
    try {
      const blob = await this.api.downloadDevCertificate();
      if (!this.valid()) return;
      const cnpj = this.settingsForm.get('cnpj')?.value || 'teste';
      saveFiscalBlob(blob, `ofizzy-dev-${cnpj}.pfx`);
      this.password.setValue('teste123');
      this.password.markAsDirty();
      this.password.markAsTouched();
      this.messages.add({
        severity: 'info',
        summary: 'Certificado de teste baixado',
        detail: 'Senha padrão: teste123 (preenchida no formulário)',
      });
    } catch {
      if (this.valid())
        this.actionError.set(
          'Não foi possível baixar o certificado de teste. Disponível apenas em desenvolvimento com simulação e homologação habilitadas.',
        );
    } finally {
      if (this.valid()) this.busy.set(false);
    }
  }
}
