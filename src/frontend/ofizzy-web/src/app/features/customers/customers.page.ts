import { ChangeDetectionStrategy, Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { ConfirmationService, MessageService } from 'primeng/api';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { PaginatorModule, PaginatorState } from 'primeng/paginator';
import { SkeletonModule } from 'primeng/skeleton';
import { TextareaModule } from 'primeng/textarea';
import { debounceTime, distinctUntilChanged } from 'rxjs';
import { CatalogApiService } from '../../core/api/catalog-api.service';
import { Customer } from '../../core/api/catalog.models';
import { ViewPreferenceService } from '../../core/preferences/view-preference.service';
import {
  DataTableWrapperComponent,
  DataToolbarComponent,
  EmptyStateComponent,
  LoadingStateComponent,
  PageHeaderComponent,
  SearchFieldComponent,
} from '../../shared/components';
import { ResponsiveLayoutService } from '../../shared/layout/responsive-layout.service';

@Component({
  selector: 'app-customers-page',
  imports: [
    ReactiveFormsModule,
    ButtonModule,
    DialogModule,
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
  templateUrl: './customers.page.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CustomersPage {
  private readonly api = inject(CatalogApiService);
  private readonly viewPreferences = inject(ViewPreferenceService);
  private readonly fb = inject(FormBuilder);
  private readonly messages = inject(MessageService);
  private readonly confirmation = inject(ConfirmationService);
  private readonly responsive = inject(ResponsiveLayoutService);
  readonly items = signal<Customer[]>([]); readonly total = signal(0); readonly loading = signal(true); readonly saving = signal(false); readonly dialog = signal(false); readonly editing = signal<Customer | null>(null);
  readonly search = this.fb.nonNullable.control(''); readonly page = signal(1); readonly pageSize = 12;
  readonly viewMode = this.viewPreferences.getSignal('customers', 'table');
  readonly effectiveViewMode = computed(() => this.responsive.isMobile() ? 'cards' : this.viewMode());
  readonly form = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(160)]],
    document: ['', [(c) => {
      const v = (c.value || '').replace(/\D/g, '');
      return (!v || v.length === 11 || v.length === 14) ? null : { invalidDocument: true };
    }]],
    phone: [''],
    whatsApp: [''],
    email: ['', Validators.email],
    address: [''],
    notes: ['']
  });

  private loadVersion = 0;
  private hasLoaded = false;
  constructor() { this.search.valueChanges.pipe(debounceTime(300), distinctUntilChanged(), takeUntilDestroyed(inject(DestroyRef))).subscribe(() => { this.page.set(1); void this.load(); }); void this.load(); }
  async load(): Promise<void> {
    const current = ++this.loadVersion;
    const cached = this.api.peekCustomers(this.search.value, this.page(), this.pageSize);
    if (cached) {
      this.items.set(cached.items);
      this.total.set(cached.total);
    }
    this.loading.set(!cached && !this.hasLoaded);
    try {
      const result = await this.api.customers(this.search.value, this.page(), this.pageSize);
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
  open(item?: Customer): void { this.editing.set(item ?? null); this.form.reset(item ? { name:item.name, document:item.document ?? '', phone:item.phone ?? '', whatsApp:item.whatsApp ?? '', email:item.email ?? '', address:item.address ?? '', notes:item.notes ?? '' } : { name:'', document:'', phone:'', whatsApp:'', email:'', address:'', notes:'' }); this.dialog.set(true); }
  async save(): Promise<void> { if (this.form.invalid) { this.form.markAllAsTouched(); return; } this.saving.set(true); try { const value = this.form.getRawValue(); await this.api.saveCustomer({ name:value.name, document:value.document || null, phone:value.phone || null, whatsApp:value.whatsApp || null, email:value.email || null, address:value.address || null, notes:value.notes || null }, this.editing()?.id); this.messages.add({ severity:'success', summary:this.editing() ? 'Cliente atualizado' : 'Cliente criado' }); this.dialog.set(false); await this.load(); } finally { this.saving.set(false); } }
  archive(item: Customer): void { this.confirmation.confirm({ header:'Arquivar cliente', message:`Arquivar ${item.name} e seus veículos? O histórico será preservado.`, icon:'pi pi-folder-open', acceptLabel:'Arquivar', rejectLabel:'Voltar', acceptButtonProps:{ severity:'danger' }, accept:async()=>{ await this.api.archiveCustomer(item.id); this.messages.add({ severity:'success', summary:'Cliente arquivado' }); await this.load(); } }); }
  changePage(event: PaginatorState): void { this.page.set((event.page ?? 0) + 1); void this.load(); }
  initials(name: string): string { const parts = (name || '').trim().split(/\s+/).filter(Boolean); return parts.slice(0, 2).map(x => x[0]).join('').toUpperCase() || 'C'; }
  formatDocument(doc?: string | null): string {
    if (!doc) return '';
    const d = doc.replace(/\D/g, '');
    if (d.length === 11) return `${d.slice(0, 3)}.${d.slice(3, 6)}.${d.slice(6, 9)}-${d.slice(9, 11)}`;
    if (d.length === 14) return `${d.slice(0, 2)}.${d.slice(2, 5)}.${d.slice(5, 8)}/${d.slice(8, 12)}-${d.slice(12, 14)}`;
    return doc;
  }
  formatDate(dateStr?: string | null): string {
    if (!dateStr) return '';
    const d = new Date(dateStr);
    return isNaN(d.getTime()) ? '' : new Intl.DateTimeFormat('pt-BR', { dateStyle: 'short' }).format(d);
  }
  cleanPhone(phone?: string | null): string {
    return (phone || '').replace(/\D/g, '');
  }
}
