import { FiscalSettingsComponent } from '../fiscal/fiscal-settings.component';
import { TenantContextService } from '../../core/tenancy/tenant-context.service';
import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  computed,
  inject,
  signal,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { ConfirmationService, MessageService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { InputNumberModule } from 'primeng/inputnumber';
import { InputTextModule } from 'primeng/inputtext';
import { PaginatorModule, PaginatorState } from 'primeng/paginator';
import { TabsModule } from 'primeng/tabs';
import { TextareaModule } from 'primeng/textarea';
import { ToggleSwitch } from 'primeng/toggleswitch';
import { debounceTime, distinctUntilChanged } from 'rxjs';
import { CatalogApiService } from '../../core/api/catalog-api.service';
import { Part, ServiceItem } from '../../core/api/catalog.models';
import { ViewPreferenceService } from '../../core/preferences/view-preference.service';
import { ResponsiveLayoutService } from '../../shared/layout/responsive-layout.service';
import { FiscalApiService } from '../fiscal/fiscal-api.service';
import {
  FiscalFieldsComponent,
  fiscalForm,
  productFields,
  serviceFields,
} from '../fiscal/fiscal-fields.component';
import { CompanySettingsFormComponent } from './company-settings-form.component';
import {
  DataTableWrapperComponent,
  DataToolbarComponent,
  EmptyStateComponent,
  LoadingStateComponent,
  PageHeaderComponent,
  SearchFieldComponent,
} from '../../shared/components';

@Component({
  selector: 'app-settings-page',
  imports: [
    ReactiveFormsModule,
    ButtonModule,
    DialogModule,
    InputNumberModule,
    InputTextModule,
    PaginatorModule,
    TabsModule,
    TextareaModule,
    ToggleSwitch,
    PageHeaderComponent,
    CompanySettingsFormComponent,
    FiscalSettingsComponent,
    FiscalFieldsComponent,
    EmptyStateComponent,
    DataTableWrapperComponent,
    DataToolbarComponent,
    SearchFieldComponent,
    LoadingStateComponent,
  ],
  templateUrl: './settings.page.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SettingsPage {
  readonly tenantContext = inject(TenantContextService);
  readonly catalogEnabled = () =>
    this.tenantContext.has('Catalog') && this.tenantContext.tenant()?.status === 'Active';
  private readonly api = inject(CatalogApiService);
  private readonly fiscalApi = inject(FiscalApiService);
  private readonly fb = inject(FormBuilder);
  private readonly messages = inject(MessageService);
  private readonly confirmation = inject(ConfirmationService);
  private readonly viewPreferences = inject(ViewPreferenceService);
  private readonly responsive = inject(ResponsiveLayoutService);
  private readonly destroyRef = inject(DestroyRef);

  readonly productFields = productFields;
  readonly serviceFields = serviceFields;

  readonly services = signal<ServiceItem[]>([]);
  readonly parts = signal<Part[]>([]);
  readonly loadingServices = signal(true);
  readonly loadingParts = signal(true);
  readonly saving = signal(false);
  readonly serviceDialog = signal(false);
  readonly partDialog = signal(false);
  readonly editingService = signal<ServiceItem | null>(null);
  readonly editingPart = signal<Part | null>(null);

  readonly servicesSearch = this.fb.nonNullable.control('');
  readonly partsSearch = this.fb.nonNullable.control('');
  readonly servicesPage = signal(1);
  readonly partsPage = signal(1);
  readonly servicesTotal = signal(0);
  readonly partsTotal = signal(0);
  readonly pageSize = 18;

  readonly servicesViewMode = this.viewPreferences.getSignal('settings-services', 'table');
  readonly partsViewMode = this.viewPreferences.getSignal('settings-parts', 'table');

  readonly effectiveServicesViewMode = computed(() =>
    this.responsive.isTabletOrSmaller() ? 'cards' : this.servicesViewMode(),
  );

  readonly effectivePartsViewMode = computed(() =>
    this.responsive.isTabletOrSmaller() ? 'cards' : this.partsViewMode(),
  );

  readonly loadingPartFiscal = signal(false);
  readonly loadingServiceFiscal = signal(false);

  readonly partFiscalEnabled = new FormControl(false, { nonNullable: true });
  readonly serviceFiscalEnabled = new FormControl(false, { nonNullable: true });

  readonly defaultPartFiscal = {
    ncm: '',
    cest: '',
    origin: '0',
    unit: 'UN',
    gtin: 'SEM GTIN',
    cfop: '5102',
    csosn: '102',
    pisCst: '07',
    cofinsCst: '07',
    retainedStBase: null,
    retainedStAmount: null,
    substituteAmount: null,
    stRate: null,
  };

  readonly defaultServiceFiscal = {
    nationalCode: '',
    municipalCode: '',
    nbs: '',
    approximateTaxRate: null,
  };

  readonly partFiscalForm = fiscalForm(productFields, this.defaultPartFiscal);
  readonly serviceFiscalForm = fiscalForm(serviceFields, this.defaultServiceFiscal);

  readonly serviceForm = this.fb.nonNullable.group({
    name: ['', Validators.required],
    description: [''],
    defaultPrice: [0, [Validators.required, Validators.min(0)]],
  });

  readonly partForm = this.fb.nonNullable.group({
    name: ['', Validators.required],
    code: ['', Validators.required],
    costPrice: [0, [Validators.required, Validators.min(0)]],
    salePrice: [0, [Validators.required, Validators.min(0)]],
  });

  constructor() {
    const cachedServices = this.api.peekServices('', 1, this.pageSize);
    const cachedParts = this.api.peekParts('', 1, this.pageSize);
    if (cachedServices) {
      this.services.set(cachedServices.items);
      this.servicesTotal.set(cachedServices.total);
    }
    if (cachedParts) {
      this.parts.set(cachedParts.items);
      this.partsTotal.set(cachedParts.total);
    }

    this.servicesSearch.valueChanges
      .pipe(debounceTime(250), distinctUntilChanged(), takeUntilDestroyed(this.destroyRef))
      .subscribe(() => {
        this.servicesPage.set(1);
        void this.loadServices();
      });

    this.partsSearch.valueChanges
      .pipe(debounceTime(250), distinctUntilChanged(), takeUntilDestroyed(this.destroyRef))
      .subscribe(() => {
        this.partsPage.set(1);
        void this.loadParts();
      });

    void this.load();
  }

  async load(): Promise<void> {
    await Promise.all([this.loadServices(), this.loadParts()]);
  }

  async loadServices(): Promise<void> {
    if (!this.catalogEnabled()) {
      this.services.set([]);
      this.servicesTotal.set(0);
      this.loadingServices.set(false);
      return;
    }
    this.loadingServices.set(true);
    try {
      const res = await this.api.services(
        this.servicesSearch.value,
        this.servicesPage(),
        this.pageSize,
      );
      this.services.set(res.items);
      this.servicesTotal.set(res.total);
    } finally {
      this.loadingServices.set(false);
    }
  }

  async loadParts(): Promise<void> {
    if (!this.catalogEnabled()) {
      this.parts.set([]);
      this.partsTotal.set(0);
      this.loadingParts.set(false);
      return;
    }
    this.loadingParts.set(true);
    try {
      const res = await this.api.parts(this.partsSearch.value, this.partsPage(), this.pageSize);
      this.parts.set(res.items);
      this.partsTotal.set(res.total);
    } finally {
      this.loadingParts.set(false);
    }
  }

  onServicesPageChange(event: PaginatorState): void {
    const next = (event.page ?? 0) + 1;
    if (next !== this.servicesPage()) {
      this.servicesPage.set(next);
      void this.loadServices();
    }
  }

  onPartsPageChange(event: PaginatorState): void {
    const next = (event.page ?? 0) + 1;
    if (next !== this.partsPage()) {
      this.partsPage.set(next);
      void this.loadParts();
    }
  }

  async openService(item?: ServiceItem): Promise<void> {
    this.editingService.set(item ?? null);
    this.serviceForm.reset({
      name: item?.name ?? '',
      description: item?.description ?? '',
      defaultPrice: item?.defaultPrice ?? 0,
    });
    this.serviceFiscalEnabled.setValue(false);
    this.serviceFiscalForm.reset(this.defaultServiceFiscal);
    this.serviceDialog.set(true);

    if (item && this.tenantContext.admin()) {
      this.loadingServiceFiscal.set(true);
      try {
        const profile = await this.fiscalApi.profile('services', item.id);
        if (this.editingService()?.id === item.id) {
          const hasFiscal = Boolean(profile['nationalCode']);
          if (hasFiscal) {
            this.serviceFiscalEnabled.setValue(true);
            this.serviceFiscalForm.patchValue(profile);
          }
        }
      } catch {
        // Carregamento silencioso se não houver perfil prévio
      } finally {
        if (this.editingService()?.id === item.id) {
          this.loadingServiceFiscal.set(false);
        }
      }
    }
  }

  async openPart(item?: Part): Promise<void> {
    this.editingPart.set(item ?? null);
    this.partForm.reset({
      name: item?.name ?? '',
      code: item?.code ?? '',
      costPrice: item?.costPrice ?? 0,
      salePrice: item?.salePrice ?? 0,
    });
    this.partFiscalEnabled.setValue(false);
    this.partFiscalForm.reset(this.defaultPartFiscal);
    this.partDialog.set(true);

    if (item && this.tenantContext.admin()) {
      this.loadingPartFiscal.set(true);
      try {
        const profile = await this.fiscalApi.profile('parts', item.id);
        if (this.editingPart()?.id === item.id) {
          const hasFiscal = Boolean(profile['ncm'] || profile['cfop'] || profile['csosn']);
          if (hasFiscal) {
            this.partFiscalEnabled.setValue(true);
            this.partFiscalForm.patchValue(profile);
          }
        }
      } catch {
        // Carregamento silencioso se não houver perfil prévio
      } finally {
        if (this.editingPart()?.id === item.id) {
          this.loadingPartFiscal.set(false);
        }
      }
    }
  }

  async saveService(): Promise<void> {
    if (this.serviceForm.invalid) {
      this.serviceForm.markAllAsTouched();
      return;
    }

    if (this.serviceFiscalEnabled.value && this.tenantContext.admin()) {
      const fiscalValues = this.serviceFiscalForm.getRawValue();
      const code = String(fiscalValues['nationalCode'] || '').trim();
      if (!/^\d{6}$/.test(code)) {
        this.serviceFiscalForm
          .get('nationalCode')
          ?.setErrors({ custom: 'Código deve conter 6 dígitos numéricos (ex: 140101).' });
        this.serviceFiscalForm.markAllAsTouched();
        this.messages.add({
          severity: 'error',
          summary: 'Código tributação nacional inválido',
          detail:
            'O código nacional de tributação deve conter exatamente 6 dígitos numéricos (ex: 140101).',
        });
        return;
      }
      const munCode = String(fiscalValues['municipalCode'] || '').trim();
      if (munCode && !/^\d{3}$/.test(munCode)) {
        this.serviceFiscalForm
          .get('municipalCode')
          ?.setErrors({
            custom:
              'Código municipal deve conter 3 dígitos numéricos (ex: 001) ou ficar em branco.',
          });
        this.serviceFiscalForm.markAllAsTouched();
        this.messages.add({
          severity: 'error',
          summary: 'Código municipal inválido',
          detail:
            'O código municipal de tributação deve conter exatamente 3 dígitos numéricos (ex: 001). Se não aplicável, deixe em branco.',
        });
        return;
      }
      if (this.serviceFiscalForm.invalid) {
        this.serviceFiscalForm.markAllAsTouched();
        this.messages.add({
          severity: 'error',
          summary: 'Dados fiscais pendentes',
          detail: 'Preencha os campos fiscais destacados em vermelho antes de salvar.',
        });
        return;
      }
    }

    this.saving.set(true);
    try {
      const v = this.serviceForm.getRawValue();
      const service = await this.api.saveService(
        { name: v.name, description: v.description || null, defaultPrice: v.defaultPrice },
        this.editingService()?.id,
      );
      if (this.serviceFiscalEnabled.value && this.tenantContext.admin()) {
        try {
          const rawFiscal = this.serviceFiscalForm.getRawValue();
          const cleanFiscal = {
            ...rawFiscal,
            nationalCode: String(rawFiscal['nationalCode'] || '').trim(),
            municipalCode: String(rawFiscal['municipalCode'] || '').trim() || null,
            nbs: String(rawFiscal['nbs'] || '').trim() || null,
          };
          await this.fiscalApi.saveProfile('services', service.id, cleanFiscal);
          this.messages.add({
            severity: 'success',
            summary: this.editingService()
              ? 'Serviço e dados fiscais atualizados'
              : 'Serviço e dados fiscais cadastrados',
          });
        } catch (fiscalErr: unknown) {
          const msg = fiscalErr instanceof Error ? fiscalErr.message : 'Revise os dados fiscais.';
          this.messages.add({
            severity: 'warn',
            summary: 'Serviço salvo, mas houve falha fiscal',
            detail: msg,
          });
        }
      } else {
        this.messages.add({
          severity: 'success',
          summary: this.editingService() ? 'Serviço atualizado' : 'Serviço criado',
        });
      }
      this.serviceDialog.set(false);
      await this.load();
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'Verifique os dados informados.';
      this.messages.add({
        severity: 'error',
        summary: 'Não foi possível salvar o serviço',
        detail: msg,
      });
    } finally {
      this.saving.set(false);
    }
  }

  async savePart(): Promise<void> {
    if (this.partForm.invalid) {
      this.partForm.markAllAsTouched();
      return;
    }

    if (this.partFiscalEnabled.value && this.tenantContext.admin()) {
      const fiscalValues = this.partFiscalForm.getRawValue();
      const ncm = String(fiscalValues['ncm'] || '').trim();
      if (!/^\d{8}$/.test(ncm)) {
        this.partFiscalForm
          .get('ncm')
          ?.setErrors({
            custom: 'O NCM deve conter exatamente 8 dígitos numéricos (ex: 40111000).',
          });
        this.partFiscalForm.markAllAsTouched();
        this.messages.add({
          severity: 'error',
          summary: 'NCM inválido',
          detail: 'O NCM deve conter exatamente 8 dígitos numéricos (ex: 40111000).',
        });
        return;
      }
      const cfop = String(fiscalValues['cfop'] || '').trim();
      const csosn = String(fiscalValues['csosn'] || '').trim();
      const validCombo =
        (cfop === '5102' && csosn === '102') || (cfop === '5405' && csosn === '500');
      if (!validCombo) {
        this.partFiscalForm
          .get('cfop')
          ?.setErrors({ custom: 'Combinação deve ser 5102/102 ou 5405/500.' });
        this.partFiscalForm
          .get('csosn')
          ?.setErrors({ custom: 'Combinação deve ser 5102/102 ou 5405/500.' });
        this.partFiscalForm.markAllAsTouched();
        this.messages.add({
          severity: 'error',
          summary: 'Combinação fiscal inválida',
          detail: 'Perfis suportados: Revenda 5102/102 ou Substituição Tributária 5405/500.',
        });
        return;
      }
      if (csosn === '500') {
        const base = fiscalValues['retainedStBase'];
        const amt = fiscalValues['retainedStAmount'];
        const sub = fiscalValues['substituteAmount'];
        const rate = fiscalValues['stRate'];
        let hasStError = false;
        if (base === null || base === '') {
          this.partFiscalForm.get('retainedStBase')?.setErrors({ required: true });
          hasStError = true;
        }
        if (amt === null || amt === '') {
          this.partFiscalForm.get('retainedStAmount')?.setErrors({ required: true });
          hasStError = true;
        }
        if (sub === null || sub === '') {
          this.partFiscalForm.get('substituteAmount')?.setErrors({ required: true });
          hasStError = true;
        }
        if (rate === null || rate === '') {
          this.partFiscalForm.get('stRate')?.setErrors({ required: true });
          hasStError = true;
        }
        if (hasStError) {
          this.partFiscalForm.markAllAsTouched();
          this.messages.add({
            severity: 'error',
            summary: 'Dados de ST incompletos',
            detail: 'Para CSOSN 500, informe a Base ST, ICMS ST, ICMS Substituto e Alíquota ST.',
          });
          return;
        }
      }
      if (this.partFiscalForm.invalid) {
        this.partFiscalForm.markAllAsTouched();
        this.messages.add({
          severity: 'error',
          summary: 'Dados fiscais pendentes',
          detail: 'Preencha os campos fiscais destacados em vermelho antes de salvar.',
        });
        return;
      }
    }

    this.saving.set(true);
    try {
      const part = await this.api.savePart(this.partForm.getRawValue(), this.editingPart()?.id);
      if (this.partFiscalEnabled.value && this.tenantContext.admin()) {
        try {
          await this.fiscalApi.saveProfile('parts', part.id, this.partFiscalForm.getRawValue());
          this.messages.add({
            severity: 'success',
            summary: this.editingPart()
              ? 'Peça e dados fiscais atualizados'
              : 'Peça e classificação fiscal cadastradas',
          });
        } catch (fiscalErr: unknown) {
          const msg =
            fiscalErr instanceof Error ? fiscalErr.message : 'Revise a classificação fiscal.';
          this.messages.add({
            severity: 'warn',
            summary: 'Peça salva, mas houve falha fiscal',
            detail: msg,
          });
        }
      } else {
        this.messages.add({
          severity: 'success',
          summary: this.editingPart() ? 'Peça atualizada' : 'Peça criada',
        });
      }
      this.partDialog.set(false);
      await this.load();
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'Verifique os dados informados.';
      this.messages.add({
        severity: 'error',
        summary: 'Não foi possível salvar a peça',
        detail: msg,
      });
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
      },
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
      },
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
