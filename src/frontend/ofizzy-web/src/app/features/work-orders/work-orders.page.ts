import { WorkOrderFiscalComponent } from '../fiscal/work-order-fiscal.component';
import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  computed,
  inject,
  signal,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { ConfirmationService, MessageService } from 'primeng/api';
import { AutoCompleteCompleteEvent, AutoCompleteModule } from 'primeng/autocomplete';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { DrawerModule } from 'primeng/drawer';
import { InputNumberModule } from 'primeng/inputnumber';
import { InputTextModule } from 'primeng/inputtext';
import { PaginatorModule, PaginatorState } from 'primeng/paginator';
import { TextareaModule } from 'primeng/textarea';
import { debounceTime, distinctUntilChanged } from 'rxjs';
import { CatalogApiService } from '../../core/api/catalog-api.service';
import { Customer, Part, ServiceItem, Vehicle } from '../../core/api/catalog.models';
import { CompanyApiService, CompanyResponse } from '../../core/api/company-api.service';
import { WorkOrderApiService } from '../../core/api/work-order-api.service';
import { TenantContextService } from '../../core/tenancy/tenant-context.service';
import { ViewPreferenceService } from '../../core/preferences/view-preference.service';
import {
  WorkOrder,
  WorkOrderLineRequest,
  WorkOrderStatus,
  WorkOrderSummary,
} from '../../core/api/work-order.models';
import {
  DataTableWrapperComponent,
  DataToolbarComponent,
  EmptyStateComponent,
  LoadingStateComponent,
  PageHeaderComponent,
  SearchFieldComponent,
  StatusBadgeComponent,
} from '../../shared/components';
import { ResponsiveLayoutService } from '../../shared/layout/responsive-layout.service';
import {
  WorkOrderLineChange,
  WorkOrderLinesEditorComponent,
} from './components/work-order-lines-editor.component';

@Component({
  selector: 'app-work-orders-page',
  imports: [
    FormsModule,
    ReactiveFormsModule,
    AutoCompleteModule,
    ButtonModule,
    DialogModule,
    DrawerModule,
    InputNumberModule,
    InputTextModule,
    PaginatorModule,
    TextareaModule,
    PageHeaderComponent,
    DataToolbarComponent,
    SearchFieldComponent,
    StatusBadgeComponent,
    EmptyStateComponent,
    LoadingStateComponent,
    DataTableWrapperComponent,
    WorkOrderLinesEditorComponent,
    WorkOrderFiscalComponent,
  ],
  templateUrl: './work-orders.page.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class WorkOrdersPage {
  private readonly api = inject(WorkOrderApiService);
  private readonly viewPreferences = inject(ViewPreferenceService);
  private readonly responsive = inject(ResponsiveLayoutService);
  private readonly catalogs = inject(CatalogApiService);
  private readonly companyApi = inject(CompanyApiService);
  private readonly fb = inject(FormBuilder);
  private readonly messages = inject(MessageService);
  private readonly confirmation = inject(ConfirmationService);
  private readonly tenantContext = inject(TenantContextService);

  readonly items = signal<WorkOrderSummary[]>([]);
  readonly customers = signal<Customer[]>([]);
  readonly vehicles = signal<Vehicle[]>([]);
  readonly servicesCatalog = signal<ServiceItem[]>([]);
  readonly partsCatalog = signal<Part[]>([]);
  readonly customerSuggestions = signal<Customer[]>([]);
  readonly vehicleSuggestions = signal<Vehicle[]>([]);
  readonly services = signal<WorkOrderLineRequest[]>([]);
  readonly parts = signal<WorkOrderLineRequest[]>([]);
  readonly company = signal<CompanyResponse | null>(null);

  readonly loading = signal(true);
  readonly saving = signal(false);
  readonly dialog = signal(false);
  readonly detailDialog = signal(false);
  readonly viewing = signal<WorkOrder | null>(null);
  readonly editing = signal<WorkOrder | null>(null);
  readonly total = signal(0);
  readonly page = signal(1);
  readonly pageSize = 18;
  readonly search = this.fb.nonNullable.control('');
  readonly viewMode = this.viewPreferences.getSignal('work-orders', 'table');
  readonly effectiveViewMode = computed(() =>
    this.responsive.isTabletOrSmaller() ? 'cards' : this.viewMode(),
  );
  readonly automotiveEnabled = computed(() => this.tenantContext.has('Automotive'));

  readonly selectedStatus = signal<WorkOrderStatus | 'All'>('All');
  readonly statusFilterOptions: { label: string; value: WorkOrderStatus | 'All' }[] = [
    { label: 'Todas', value: 'All' },
    { label: 'Abertas', value: 'Open' },
    { label: 'Em andamento', value: 'InProgress' },
    { label: 'Concluídas', value: 'Completed' },
    { label: 'Canceladas', value: 'Cancelled' },
  ];

  updateLineFromEditor(type: 'services' | 'parts', event: WorkOrderLineChange): void {
    this.updateLine(type, event.index, event.field, event.value);
  }

  readonly form = this.fb.group({
    customer: this.fb.control<Customer | null>(null, Validators.required),
    vehicle: this.fb.control<Vehicle | null>(null),
    mileage: [null as number | null, Validators.min(0)],
    complaint: [''],
    diagnosis: [''],
    notes: [''],
  });

  readonly servicesSubtotal = computed(() =>
    this.services().reduce((sum, x) => sum + (x.quantity || 0) * (x.unitPrice || 0), 0),
  );

  readonly partsSubtotal = computed(() =>
    this.parts().reduce((sum, x) => sum + (x.quantity || 0) * (x.unitPrice || 0), 0),
  );

  readonly orderTotal = computed(() => this.servicesSubtotal() + this.partsSubtotal());

  readonly statusLabels: Record<WorkOrderStatus, string> = {
    Open: 'Aberta',
    InProgress: 'Em andamento',
    Completed: 'Finalizada',
    Cancelled: 'Cancelada',
  };

  private loadVersion = 0;
  private hasLoaded = false;
  constructor() {
    const destroyRef = inject(DestroyRef);
    this.search.valueChanges
      .pipe(debounceTime(300), distinctUntilChanged(), takeUntilDestroyed(destroyRef))
      .subscribe(() => {
        this.page.set(1);
        void this.load();
      });

    this.form.controls.customer.valueChanges
      .pipe(takeUntilDestroyed(destroyRef))
      .subscribe(async (customer) => {
        if (!this.dialog() || !this.automotiveEnabled()) return;
        if (!this.editing() || customer?.id !== this.editing()?.customerId) {
          this.form.controls.vehicle.setValue(null);
        }
        if (customer) {
          const res = await this.catalogs.vehicles('', 1, 100, customer.id);
          this.vehicles.set(res.items);
          this.vehicleSuggestions.set(res.items);
        } else {
          this.vehicles.set([]);
          this.vehicleSuggestions.set([]);
        }
      });

    const cachedCompany = this.companyApi.peek();
    if (cachedCompany) this.company.set(cachedCompany);
    void this.companyApi
      .get()
      .then((c) => this.company.set(c))
      .catch(() => {});
    void this.load();
  }

  setStatusFilter(status: WorkOrderStatus | 'All'): void {
    this.selectedStatus.set(status);
    this.page.set(1);
    void this.load();
  }

  async load(): Promise<void> {
    const current = ++this.loadVersion;
    const selected = this.selectedStatus();
    const statusParam: WorkOrderStatus | null = selected === 'All' ? null : selected;
    const cached = this.api.peekList(this.search.value, this.page(), this.pageSize, statusParam);
    if (cached) {
      this.items.set(cached.items);
      this.total.set(cached.total);
    }
    this.loading.set(!cached && !this.hasLoaded);
    try {
      const result = await this.api.list(
        this.search.value,
        this.page(),
        this.pageSize,
        statusParam,
      );
      if (current === this.loadVersion) {
        this.items.set(result.items);
        this.total.set(result.total);
        this.hasLoaded = true;
      }
    } finally {
      if (current === this.loadVersion) {
        this.loading.set(false);
      }
    }
  }

  async open(item?: WorkOrderSummary | WorkOrder): Promise<void> {
    const [customers, services, parts] = await Promise.all([
      this.catalogs.customers('', 1, 100),
      this.catalogs.services('', 1, 100),
      this.catalogs.parts('', 1, 100),
    ]);
    this.customers.set(customers.items);
    this.customerSuggestions.set(customers.items);
    this.servicesCatalog.set(services.items);
    this.partsCatalog.set(parts.items);

    if (item) {
      const order = 'servicesTotal' in item ? item : await this.api.get(item.id);
      if (order.status === 'Completed' || order.status === 'Cancelled') {
        this.messages.add({
          severity: 'warn',
          summary: 'Ordem imutável',
          detail: 'Ordens finalizadas ou canceladas não podem ser alteradas.',
        });
        return;
      }
      this.editing.set(order);
      const vehiclesRes = this.automotiveEnabled()
        ? await this.catalogs.vehicles('', 1, 100, order.customerId)
        : { items: [] as Vehicle[] };
      this.vehicles.set(vehiclesRes.items);
      this.vehicleSuggestions.set(vehiclesRes.items);
      this.form.reset({
        customer: customers.items.find((customer) => customer.id === order.customerId) ?? null,
        vehicle: vehiclesRes.items.find((vehicle) => vehicle.id === order.vehicleId) ?? null,
        mileage: order.mileage,
        complaint: order.complaint ?? '',
        diagnosis: order.diagnosis ?? '',
        notes: order.notes ?? '',
      });
      this.services.set(
        order.services.map((s) => ({
          catalogId: s.catalogId,
          description: s.description,
          quantity: s.quantity,
          unitPrice: s.unitPrice,
        })),
      );
      this.parts.set(
        order.parts.map((p) => ({
          catalogId: p.catalogId,
          description: p.description,
          code: p.code,
          quantity: p.quantity,
          unitPrice: p.unitPrice,
        })),
      );
    } else {
      this.editing.set(null);
      this.vehicles.set([]);
      this.vehicleSuggestions.set([]);
      this.services.set([]);
      this.parts.set([]);
      this.form.reset({
        customer: null,
        vehicle: null,
        mileage: null,
        complaint: '',
        diagnosis: '',
        notes: '',
      });
    }
    this.dialog.set(true);
  }

  async viewDetail(item: WorkOrderSummary): Promise<void> {
    try {
      const order = await this.api.get(item.id);
      this.viewing.set(order);
      this.detailDialog.set(true);
    } catch {
      // error handled by interceptor
    }
  }

  addService(id: string): void {
    const item = this.servicesCatalog().find((x) => x.id === id);
    if (item) {
      this.services.update((lines) => [
        ...lines,
        {
          catalogId: item.id,
          description: item.name,
          quantity: 1,
          unitPrice: item.defaultPrice,
        },
      ]);
    }
  }

  addManualService(): void {
    this.services.update((lines) => [
      ...lines,
      { catalogId: null, description: '', quantity: 1, unitPrice: 0 },
    ]);
  }

  addPart(id: string): void {
    const item = this.partsCatalog().find((x) => x.id === id);
    if (item) {
      this.parts.update((lines) => [
        ...lines,
        {
          catalogId: item.id,
          description: item.name,
          code: item.code,
          quantity: 1,
          unitPrice: item.salePrice,
        },
      ]);
    }
  }

  addManualPart(): void {
    this.parts.update((lines) => [
      ...lines,
      { catalogId: null, description: '', code: '', quantity: 1, unitPrice: 0 },
    ]);
  }

  updateLine(kind: 'services' | 'parts', index: number, field: string, value: unknown): void {
    const target = kind === 'services' ? this.services : this.parts;
    target.update((lines) =>
      lines.map((line, i) => (i === index ? { ...line, [field]: value } : line)),
    );
  }

  removeLine(kind: 'services' | 'parts', index: number): void {
    const target = kind === 'services' ? this.services : this.parts;
    target.update((lines) => lines.filter((_, i) => i !== index));
  }

  async save(): Promise<void> {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      const controls = this.form.controls;
      if (!controls.customer.value) {
        this.messages.add({
          severity: 'warn',
          summary: 'Cliente não selecionado',
          detail: 'Selecione o cliente proprietário antes de emitir a OS.',
        });
      } else if (this.automotiveEnabled() && !controls.vehicle.value) {
        this.messages.add({
          severity: 'warn',
          summary: 'Veículo não selecionado',
          detail: 'Selecione o veículo atendido antes de emitir a OS.',
        });
      } else if (controls.mileage.invalid) {
        this.messages.add({
          severity: 'warn',
          summary: 'Quilometragem inválida',
          detail: 'A quilometragem não pode ser negativa.',
        });
      } else {
        this.messages.add({
          severity: 'warn',
          summary: 'Campos incompletos',
          detail: 'Preencha os campos obrigatórios em destaque.',
        });
      }
      return;
    }

    // Validação de linhas
    const emptyServices = this.services().filter((s) => !s.description || !s.description.trim());
    if (emptyServices.length > 0) {
      this.messages.add({
        severity: 'warn',
        summary: 'Serviço sem descrição',
        detail: 'Preencha a descrição de todos os serviços incluídos.',
      });
      return;
    }

    const emptyParts = this.parts().filter((p) => !p.description || !p.description.trim());
    if (emptyParts.length > 0) {
      this.messages.add({
        severity: 'warn',
        summary: 'Peça sem descrição',
        detail: 'Preencha a descrição de todas as peças incluídas.',
      });
      return;
    }

    this.saving.set(true);
    try {
      const value = this.form.getRawValue();
      const id = this.editing()?.id;
      await this.api.save(
        {
          customerId: value.customer!.id,
          vehicleId: value.vehicle?.id ?? null,
          mileage: value.mileage,
          complaint: value.complaint || null,
          diagnosis: value.diagnosis || null,
          notes: value.notes || null,
          services: this.services(),
          parts: this.parts(),
        },
        id,
      );
      this.dialog.set(false);
      this.messages.add({
        severity: 'success',
        summary: id ? 'Ordem de serviço atualizada' : 'Ordem de serviço aberta',
      });
      if (id && this.detailDialog() && this.viewing()?.id === id) {
        this.viewing.set(await this.api.get(id));
      }
      await this.load();
    } catch {
      // O interceptor HTTP já apresenta o erro da API ao usuário.
    } finally {
      this.saving.set(false);
    }
  }

  status(item: WorkOrderSummary | WorkOrder, status: WorkOrderStatus): void {
    if (status === 'Completed') {
      this.confirmation.confirm({
        header: 'Finalizar Ordem de Serviço',
        message: `Deseja finalizar a OS #${item.number}? Após finalizada, a ordem de serviço se torna imutável.`,
        icon: 'pi pi-check-circle',
        acceptLabel: 'Finalizar OS',
        rejectLabel: 'Voltar',
        acceptButtonProps: { severity: 'success' },
        accept: async () => {
          await this.executeStatusChange(item.id, item.number, status);
        },
      });
    } else if (status === 'Cancelled') {
      this.confirmation.confirm({
        header: 'Cancelar Ordem de Serviço',
        message: `Deseja realmente cancelar a OS #${item.number}? Após cancelada, a ordem não poderá mais ser reaberta.`,
        icon: 'pi pi-exclamation-triangle',
        acceptLabel: 'Cancelar OS',
        rejectLabel: 'Voltar',
        acceptButtonProps: { severity: 'danger' },
        accept: async () => {
          await this.executeStatusChange(item.id, item.number, status);
        },
      });
    } else {
      void this.executeStatusChange(item.id, item.number, status);
    }
  }

  private async executeStatusChange(
    id: string,
    number: number,
    status: WorkOrderStatus,
  ): Promise<void> {
    try {
      await this.api.changeStatus(id, status);
      this.messages.add({
        severity: 'success',
        summary: `OS #${number} atualizada para ${this.statusLabels[status]}`,
      });
      if (this.detailDialog() && this.viewing()?.id === id) {
        this.viewing.set(await this.api.get(id));
      }
      await this.load();
    } catch {
      // Handled by api error interceptor
    }
  }

  changePage(event: PaginatorState): void {
    this.page.set((event.page ?? 0) + 1);
    void this.load();
  }

  filterCustomers(event: AutoCompleteCompleteEvent): void {
    const query = this.normalized(event.query);
    this.customerSuggestions.set(
      this.customers()
        .filter((customer) =>
          this.normalized(
            `${customer.name} ${customer.document ?? ''} ${customer.phone ?? ''}`,
          ).includes(query),
        )
        .slice(0, 50),
    );
  }

  filterVehicles(event: AutoCompleteCompleteEvent): void {
    const query = this.normalized(event.query);
    this.vehicleSuggestions.set(
      this.vehicles()
        .filter((vehicle) =>
          this.normalized(`${vehicle.plate} ${vehicle.brand ?? ''} ${vehicle.model}`).includes(
            query,
          ),
        )
        .slice(0, 50),
    );
  }

  vehicleLabel(vehicle: Vehicle): string {
    return `${vehicle.plate} · ${[vehicle.brand, vehicle.model].filter(Boolean).join(' ')}`;
  }

  private normalized(value: string): string {
    return value
      .normalize('NFD')
      .replace(/\p{Diacritic}/gu, '')
      .toLocaleLowerCase('pt-BR')
      .trim();
  }

  money(value: number): string {
    return value.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });
  }

  formatDate(iso?: string | null): string {
    if (!iso) return '-';
    return new Date(iso).toLocaleDateString('pt-BR', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  }

  formatDateOnly(iso?: string | null): string {
    if (!iso) return '-';
    return new Date(iso).toLocaleDateString('pt-BR', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
    });
  }

  formatTimeOnly(iso?: string | null): string {
    if (!iso) return '';
    return new Date(iso).toLocaleTimeString('pt-BR', {
      hour: '2-digit',
      minute: '2-digit',
    });
  }

  printOrder(): void {
    window.print();
  }

  async downloadPdf(order: WorkOrderSummary | WorkOrder): Promise<void> {
    try {
      const blob = await this.api.downloadPdf(order.id);
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `OS-${order.number.toString().padStart(4, '0')}.pdf`;
      a.click();
      window.URL.revokeObjectURL(url);
    } catch {
      this.messages.add({
        severity: 'error',
        summary: 'Erro no download',
        detail: 'Não foi possível baixar o PDF da OS.',
      });
    }
  }
}
