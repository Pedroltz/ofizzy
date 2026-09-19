import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  computed,
  inject,
  signal,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ConfirmationService, MessageService } from 'primeng/api';
import { AutoCompleteCompleteEvent, AutoCompleteModule } from 'primeng/autocomplete';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { InputNumberModule } from 'primeng/inputnumber';
import { InputTextModule } from 'primeng/inputtext';
import { PaginatorModule, PaginatorState } from 'primeng/paginator';
import { SkeletonModule } from 'primeng/skeleton';
import { TextareaModule } from 'primeng/textarea';
import { debounceTime, distinctUntilChanged } from 'rxjs';
import { CatalogApiService } from '../../../../core/api/catalog-api.service';
import { Customer, Vehicle } from '../../../../core/api/catalog.models';
import { ViewPreferenceService } from '../../../../core/preferences/view-preference.service';
import {
  DataTableWrapperComponent,
  DataToolbarComponent,
  EmptyStateComponent,
  LoadingStateComponent,
  PageHeaderComponent,
  SearchFieldComponent,
} from '../../../../shared/components';
import { ResponsiveLayoutService } from '../../../../shared/layout/responsive-layout.service';

@Component({
  selector: 'app-vehicles-page',
  imports: [
    ReactiveFormsModule,
    AutoCompleteModule,
    ButtonModule,
    DialogModule,
    InputNumberModule,
    InputTextModule,
    PaginatorModule,
    SkeletonModule,
    TextareaModule,
    PageHeaderComponent,
    DataToolbarComponent,
    SearchFieldComponent,
    DataTableWrapperComponent,
    EmptyStateComponent,
    LoadingStateComponent,
  ],
  templateUrl: './vehicles.page.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class VehiclesPage {
  private readonly api = inject(CatalogApiService);
  private readonly viewPreferences = inject(ViewPreferenceService);
  private readonly fb = inject(FormBuilder);
  private readonly messages = inject(MessageService);
  private readonly confirmation = inject(ConfirmationService);
  private readonly responsive = inject(ResponsiveLayoutService);
  readonly items = signal<Vehicle[]>([]);
  readonly total = signal(0);
  readonly loading = signal(true);
  readonly saving = signal(false);
  readonly dialog = signal(false);
  readonly editing = signal<Vehicle | null>(null);
  readonly customerSuggestions = signal<Customer[]>([]);
  readonly search = this.fb.nonNullable.control('');
  readonly page = signal(1);
  readonly pageSize = 18;
  readonly viewMode = this.viewPreferences.getSignal('vehicles', 'table');
  readonly effectiveViewMode = computed(() =>
    this.responsive.isMobile() ? 'cards' : this.viewMode(),
  );
  readonly form = this.fb.group({
    customer: this.fb.control<Customer | null>(null, Validators.required),
    plate: this.fb.nonNullable.control('', [Validators.required, Validators.minLength(7)]),
    brand: this.fb.nonNullable.control(''),
    model: this.fb.nonNullable.control('', Validators.required),
    year: this.fb.control<number | null>(null),
    color: this.fb.nonNullable.control(''),
    mileage: this.fb.control<number | null>(null),
    chassis: this.fb.nonNullable.control(''),
    notes: this.fb.nonNullable.control(''),
  });
  private readonly destroyRef = inject(DestroyRef);
  private loadVersion = 0;
  private hasLoaded = false;
  constructor() {
    this.search.valueChanges
      .pipe(debounceTime(300), distinctUntilChanged(), takeUntilDestroyed(this.destroyRef))
      .subscribe(() => {
        this.page.set(1);
        void this.load();
      });
    void this.load();
  }
  async load(): Promise<void> {
    const current = ++this.loadVersion;
    const cached = this.api.peekVehicles(this.search.value, this.page(), this.pageSize);
    if (cached) {
      this.items.set(cached.items);
      this.total.set(cached.total);
    }
    this.loading.set(!cached && !this.hasLoaded);
    try {
      const result = await this.api.vehicles(this.search.value, this.page(), this.pageSize);
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
  async searchCustomers(event: AutoCompleteCompleteEvent): Promise<void> {
    const q = event?.query ?? '';
    const res = await this.api.customers(q, 1, 20);
    this.customerSuggestions.set(res.items);
  }
  open(item?: Vehicle): void {
    this.editing.set(item ?? null);
    const customer: Customer | null = item
      ? {
          id: item.customerId,
          name: item.customerName,
          document: null,
          phone: null,
          whatsApp: null,
          email: null,
          address: null,
          notes: null,
          isActive: true,
          createdAt: '',
        }
      : null;
    this.form.reset({
      customer,
      plate: item?.plate ?? '',
      brand: item?.brand ?? '',
      model: item?.model ?? '',
      year: item?.year ?? null,
      color: item?.color ?? '',
      mileage: item?.mileage ?? null,
      chassis: item?.chassis ?? '',
      notes: item?.notes ?? '',
    });
    this.dialog.set(true);
  }
  async save(): Promise<void> {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const value = this.form.getRawValue();
    const customer = value.customer;
    if (!customer || typeof customer !== 'object' || !customer.id) {
      this.messages.add({
        severity: 'warn',
        summary: 'Selecione um cliente',
        detail: 'Selecione um cliente válido da lista de sugestões.',
      });
      this.form.controls.customer.setErrors({ required: true });
      return;
    }
    this.saving.set(true);
    try {
      await this.api.saveVehicle(
        {
          customerId: customer.id,
          plate: value.plate.toUpperCase().trim(),
          brand: value.brand || null,
          model: value.model.trim(),
          year: value.year,
          color: value.color || null,
          mileage: value.mileage,
          chassis: value.chassis || null,
          notes: value.notes || null,
        },
        this.editing()?.id,
      );
      this.messages.add({
        severity: 'success',
        summary: this.editing() ? 'Veículo atualizado' : 'Veículo criado',
      });
      this.dialog.set(false);
      await this.load();
    } finally {
      this.saving.set(false);
    }
  }
  archive(item: Vehicle): void {
    this.confirmation.confirm({
      header: 'Arquivar veículo',
      message: `Arquivar ${item.model} — ${item.plate}? O histórico será preservado.`,
      icon: 'pi pi-folder-open',
      acceptLabel: 'Arquivar',
      rejectLabel: 'Voltar',
      acceptButtonProps: { severity: 'danger' },
      accept: async () => {
        await this.api.archiveVehicle(item.id);
        this.messages.add({ severity: 'success', summary: 'Veículo arquivado' });
        await this.load();
      },
    });
  }
  changePage(event: PaginatorState): void {
    this.page.set((event.page ?? 0) + 1);
    void this.load();
  }
  formatDate(dateStr?: string | null): string {
    if (!dateStr) return '';
    const d = new Date(dateStr);
    return isNaN(d.getTime())
      ? ''
      : new Intl.DateTimeFormat('pt-BR', { dateStyle: 'short' }).format(d);
  }
}
