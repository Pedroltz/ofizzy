import { TenantContextService } from '../../core/tenancy/tenant-context.service';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ConfirmationService, MessageService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { InputNumberModule } from 'primeng/inputnumber';
import { InputTextModule } from 'primeng/inputtext';
import { TabsModule } from 'primeng/tabs';
import { TextareaModule } from 'primeng/textarea';
import { CatalogApiService } from '../../core/api/catalog-api.service';
import { Part, ServiceItem } from '../../core/api/catalog.models';
import { CompanyApiService, CompanyResponse } from '../../core/api/company-api.service';
import {
  EmptyStateComponent,
  PageHeaderComponent,
  SectionCardComponent,
} from '../../shared/components';

@Component({
  selector: 'app-settings-page',
  imports: [
    ReactiveFormsModule,
    ButtonModule,
    DialogModule,
    InputNumberModule,
    InputTextModule,
    TabsModule,
    TextareaModule,
    PageHeaderComponent,
    EmptyStateComponent,
    SectionCardComponent,
  ],
  templateUrl: './settings.page.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SettingsPage {
  readonly tenantContext = inject(TenantContextService);
  readonly catalogEnabled = () => this.tenantContext.has('Catalog') && this.tenantContext.tenant()?.status === 'Active';
  private readonly api = inject(CatalogApiService);
  private readonly companyApi = inject(CompanyApiService);
  private readonly fb = inject(FormBuilder);
  private readonly messages = inject(MessageService);
  private readonly confirmation = inject(ConfirmationService);

  readonly services = signal<ServiceItem[]>([]);
  readonly parts = signal<Part[]>([]);
  readonly company = signal<CompanyResponse | null>(null);
  readonly loading = signal(true);
  readonly saving = signal(false);
  readonly savingCompany = signal(false);
  readonly serviceDialog = signal(false);
  readonly partDialog = signal(false);
  readonly editingService = signal<ServiceItem | null>(null);
  readonly editingPart = signal<Part | null>(null);

  readonly serviceForm = this.fb.nonNullable.group({
    name: ['', Validators.required],
    description: [''],
    defaultPrice: [0, [Validators.required, Validators.min(0)]]
  });

  readonly partForm = this.fb.nonNullable.group({
    name: ['', Validators.required],
    code: ['', Validators.required],
    costPrice: [0, [Validators.required, Validators.min(0)]],
    salePrice: [0, [Validators.required, Validators.min(0)]]
  });

  readonly companyForm = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(160)]],
    legalName: [''],
    cnpj: [''],
    phone: [''],
    whatsApp: [''],
    email: [''],
    address: [''],
    city: [''],
    state: [''],
    postalCode: [''],
    warrantyTerms: [''],
    receiptNotes: ['']
  });

  constructor() {
    if (!this.tenantContext.admin()) this.companyForm.disable();
    const services = this.api.peekServices();
    const parts = this.api.peekParts();
    const company = this.companyApi.peek();
    if (services) this.services.set(services.items);
    if (parts) this.parts.set(parts.items);
    if (company) {
      this.company.set(company);
      this.patchCompany(company);
    }
    this.loading.set(!services && !parts && !company);
    void this.load();
  }

  async load(): Promise<void> {
    this.loading.set(
      !this.api.peekServices() && !this.api.peekParts() && !this.companyApi.peek()
    );
    try {
      const [services, parts, comp] = await Promise.all([
        this.catalogEnabled() ? this.api.services() : Promise.resolve({ items: [] }),
        this.catalogEnabled() ? this.api.parts() : Promise.resolve({ items: [] }),
        this.companyApi.get().catch(() => null)
      ]);
      this.services.set(services.items);
      this.parts.set(parts.items);
      if (comp) {
        this.company.set(comp);
        this.patchCompany(comp);
      }
    } finally {
      this.loading.set(false);
    }
  }

  private patchCompany(comp: CompanyResponse): void {
    this.companyForm.patchValue({
      name: comp.name || '', legalName: comp.legalName || '', cnpj: comp.cnpj || '',
      phone: comp.phone || '', whatsApp: comp.whatsApp || '', email: comp.email || '',
      address: comp.address || '', city: comp.city || '', state: comp.state || '',
      postalCode: comp.postalCode || '', warrantyTerms: comp.warrantyTerms || '',
      receiptNotes: comp.receiptNotes || ''
    });
  }

  async saveCompany(): Promise<void> {
    if (this.companyForm.invalid) {
      this.companyForm.markAllAsTouched();
      return;
    }
    this.savingCompany.set(true);
    try {
      const updated = await this.companyApi.update(this.companyForm.getRawValue());
      this.company.set(updated);
      this.messages.add({ severity: 'success', summary: 'Dados da oficina salvos com sucesso!' });
    } catch {
      this.messages.add({ severity: 'error', summary: 'Erro ao salvar dados da oficina.' });
    } finally {
      this.savingCompany.set(false);
    }
  }

  openService(item?: ServiceItem): void {
    this.editingService.set(item ?? null);
    this.serviceForm.reset({
      name: item?.name ?? '',
      description: item?.description ?? '',
      defaultPrice: item?.defaultPrice ?? 0
    });
    this.serviceDialog.set(true);
  }

  openPart(item?: Part): void {
    this.editingPart.set(item ?? null);
    this.partForm.reset({
      name: item?.name ?? '',
      code: item?.code ?? '',
      costPrice: item?.costPrice ?? 0,
      salePrice: item?.salePrice ?? 0
    });
    this.partDialog.set(true);
  }

  async saveService(): Promise<void> {
    if (this.serviceForm.invalid) {
      this.serviceForm.markAllAsTouched();
      return;
    }
    this.saving.set(true);
    try {
      const v = this.serviceForm.getRawValue();
      await this.api.saveService({ name: v.name, description: v.description || null, defaultPrice: v.defaultPrice }, this.editingService()?.id);
      this.serviceDialog.set(false);
      this.messages.add({ severity: 'success', summary: this.editingService() ? 'Serviço atualizado' : 'Serviço criado' });
      await this.load();
    } finally {
      this.saving.set(false);
    }
  }

  async savePart(): Promise<void> {
    if (this.partForm.invalid) {
      this.partForm.markAllAsTouched();
      return;
    }
    this.saving.set(true);
    try {
      await this.api.savePart(this.partForm.getRawValue(), this.editingPart()?.id);
      this.partDialog.set(false);
      this.messages.add({ severity: 'success', summary: this.editingPart() ? 'Peça atualizada' : 'Peça criada' });
      await this.load();
    } finally {
      this.saving.set(false);
    }
  }

  archiveService(item: ServiceItem): void {
    this.confirmation.confirm({
      header: 'Arquivar serviço',
      message: `Arquivar ${item.name}?`,
      acceptLabel: 'Arquivar',
      rejectLabel: 'Voltar',
      acceptButtonProps: { severity: 'danger' },
      accept: async () => {
        await this.api.archiveService(item.id);
        await this.load();
      }
    });
  }

  archivePart(item: Part): void {
    this.confirmation.confirm({
      header: 'Arquivar peça',
      message: `Arquivar ${item.name}?`,
      acceptLabel: 'Arquivar',
      rejectLabel: 'Voltar',
      acceptButtonProps: { severity: 'danger' },
      accept: async () => {
        await this.api.archivePart(item.id);
        await this.load();
      }
    });
  }

  money(value: number): string {
    return value.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });
  }

  marginPercent(part: Part): string {
    if (!part.costPrice || part.costPrice <= 0) return '';
    const margin = ((part.salePrice - part.costPrice) / part.costPrice) * 100;
    return `+${margin.toFixed(0)}% margem`;
  }
}
