import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { FormsModule, NgForm } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { ConfirmationService, MessageService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { CheckboxModule } from 'primeng/checkbox';
import { FieldsetModule } from 'primeng/fieldset';
import { MessageModule } from 'primeng/message';
import { DialogModule } from 'primeng/dialog';
import { TableModule } from 'primeng/table';
import { ProductModule } from '../../core/tenancy/tenant-context.service';
import { ViewPreferenceService } from '../../core/preferences/view-preference.service';
import { ResponsiveLayoutService } from '../../shared/layout/responsive-layout.service';
import { PageHeaderComponent, DataToolbarComponent, SearchFieldComponent, EmptyStateComponent, LoadingStateComponent, StatusBadgeComponent, DataTableWrapperComponent, SectionCardComponent } from '../../shared/components';
interface PlatformTenant { id: string; name: string; slug: string; status: string; vertical: string; onboardingCompletedAt: string | null; modules: ProductModule[]; }
@Component({ selector: 'app-platform', imports: [FormsModule, ButtonModule, InputTextModule, SelectModule, CheckboxModule, FieldsetModule, MessageModule, DialogModule, TableModule, PageHeaderComponent, DataToolbarComponent, SearchFieldComponent, EmptyStateComponent, LoadingStateComponent, StatusBadgeComponent, DataTableWrapperComponent, SectionCardComponent], changeDetection: ChangeDetectionStrategy.OnPush, templateUrl: './platform.page.html' })
export class PlatformPage {
 private readonly http = inject(HttpClient);
 private readonly confirmation = inject(ConfirmationService);
 private readonly messages = inject(MessageService);
 private readonly viewPreferences = inject(ViewPreferenceService);
 readonly responsive = inject(ResponsiveLayoutService);
 readonly tenants = signal<PlatformTenant[]>([]);
 readonly busy = signal(false); readonly loading = signal(true); readonly loadError = signal(false);
 readonly dialog = signal(false); readonly editing = signal<PlatformTenant | null>(null);
 readonly validation = signal(''); readonly search = signal(''); readonly statusFilter = signal('');
 readonly viewMode = this.viewPreferences.getSignal('tenants', 'table');
 readonly availableModules: ProductModule[] = ['Customers', 'WorkOrders', 'Catalog', 'Automotive'];
 readonly statuses = [{ label: 'Pendente', value: 'Pending' }, { label: 'Ativa', value: 'Active' }, { label: 'Suspensa', value: 'Suspended' }, { label: 'Arquivada', value: 'Archived' }];
 readonly filterStatuses = [{ label: 'Todos os estados', value: '' }, ...this.statuses];
 readonly editStatuses = computed(() => this.statuses.map(s => ({ ...s, disabled: s.value === 'Active' && !this.editing()?.onboardingCompletedAt })));
 readonly moduleLabels: Record<ProductModule, string> = { Customers: 'Clientes', WorkOrders: 'Ordens de serviço', Catalog: 'Catálogo', Automotive: 'Automotive · Veículos' };
 readonly filtered = computed(() => this.tenants().filter(t => (!this.statusFilter() || t.status === this.statusFilter()) && (t.name + ' ' + t.slug).toLocaleLowerCase('pt-BR').includes(this.search().trim().toLocaleLowerCase('pt-BR'))));
 draft = this.empty(); editStatus = 'Pending';
 constructor() { void this.load(); }
 private empty() { return { name: '', slug: '', vertical: 'Automotive', adminName: '', email: '', password: '', modules: [...this.availableModules] }; }
 statusLabel(status: string): string { return this.statuses.find(x => x.value === status)?.label ?? status; }
 statusVariant(status: string): string { return ({ Active: 'success', Pending: 'warning', Suspended: 'danger', Archived: 'neutral' } as Record<string, string>)[status] ?? 'neutral'; }
 open(tenant?: PlatformTenant): void {
  this.editing.set(tenant ? { ...tenant, modules: [...tenant.modules] } : null);
  this.draft = this.empty(); this.validation.set('');
  if (tenant) { this.draft.modules = [...tenant.modules]; this.editStatus = tenant.status; }
  this.dialog.set(true);
 }
 async load(): Promise<void> {
  this.loading.set(true); this.loadError.set(false);
  try { this.tenants.set(await firstValueFrom(this.http.get<PlatformTenant[]>('/api/platform/tenants'))); }
  catch { this.loadError.set(true); } finally { this.loading.set(false); }
 }
 submit(form: NgForm): void {
  if (this.busy()) return;
  this.validation.set('');
  if (form.invalid || (!this.editing() && (!this.draft.name.trim() || !this.draft.adminName.trim()))) { form.control.markAllAsTouched(); this.validation.set('Revise os campos obrigatórios antes de continuar.'); return; }
  const modules = this.draft.modules;
  if ((modules.includes('Automotive') && !modules.includes('Customers')) || (modules.includes('WorkOrders') && !this.availableModules.every(m => modules.includes(m)))) { this.validation.set('Automotive exige Clientes. Ordens de serviço exige Clientes, Catálogo e Automotive.'); return; }
  const tenant = this.editing();
  if (tenant && this.editStatus === 'Active' && !tenant.onboardingCompletedAt) { this.validation.set('A configuração inicial precisa ser concluída antes da ativação.'); return; }
  if (tenant && tenant.status !== this.editStatus && ['Suspended', 'Archived'].includes(this.editStatus)) {
   this.confirmation.confirm({ header: this.editStatus === 'Suspended' ? 'Suspender empresa' : 'Arquivar empresa', message: `Os usuários de ${tenant.name} perderão acesso às operações. Os dados serão preservados. Deseja continuar?`, acceptLabel: 'Confirmar', rejectLabel: 'Voltar', acceptButtonProps: { severity: 'danger' }, accept: () => { void this.persist(); } });
  } else void this.persist();
 }
 private async persist(): Promise<void> {
  if (this.busy()) return;
  this.busy.set(true);
  const tenant = this.editing();
  try {
   if (tenant) await firstValueFrom(this.http.put('/api/platform/tenants/' + tenant.id, { status: this.editStatus, modules: this.draft.modules }));
   else await firstValueFrom(this.http.post('/api/platform/tenants', { ...this.draft, password: this.draft.password || null }));
   this.dialog.set(false); this.draft = this.empty();
   this.messages.add({ severity: 'success', summary: tenant ? 'Empresa atualizada' : 'Empresa criada', detail: tenant ? undefined : 'O administrador informado pode entrar para concluir a configuração inicial.' });
   await this.load();
  } catch { /* O interceptor apresenta o erro da API; mantenha o formulário preenchido. */ }
  finally { this.busy.set(false); }
 }
}
