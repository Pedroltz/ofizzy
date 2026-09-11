import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { ButtonModule } from 'primeng/button';
import { AuthService } from '../../core/auth/auth.service';
import { TenantContext } from '../../core/tenancy/tenant-context.service';
import { PageHeaderComponent, EmptyStateComponent, LoadingStateComponent, StatusBadgeComponent } from '../../shared/components';
@Component({ selector: 'app-organizations', imports: [ButtonModule, PageHeaderComponent, EmptyStateComponent, LoadingStateComponent, StatusBadgeComponent], changeDetection: ChangeDetectionStrategy.OnPush,
 template: `<section class="page-container saas-page">
 <app-page-header eyebrow="SUA CONTA" title="Minhas organizações" description="Selecione a empresa em que deseja trabalhar." />
 @if (loading()) { <app-loading-state mode="cards" [count]="3" /> }
 @else if (error()) { <app-empty-state icon="pi pi-exclamation-circle" title="Não foi possível carregar suas empresas" description="Verifique a conexão e tente novamente." actionLabel="Tentar novamente" (action)="load()" /> }
 @else if (!tenants().length) {
  <app-empty-state icon="pi pi-building" title="Nenhuma empresa vinculada à sua conta" [description]="auth.user()?.isPlatformAdmin ? 'Você administra a plataforma, mas o acesso às operações depende de um vínculo com a empresa. Ao cadastrar uma empresa, informe seu e-mail como administrador e deixe a senha vazia para usar esta conta.' : 'Solicite ao administrador da plataforma um vínculo com a empresa. Se recebeu outra conta, saia e entre com o e-mail informado no cadastro.'" [actionLabel]="auth.user()?.isPlatformAdmin ? 'Administrar empresas' : ''" (action)="platform()" />
 } @else {
 <div class="catalog-grid">@for (tenant of tenants(); track tenant.id) {
 <article class="surface-card saas-company-card">
  <div class="saas-card-heading"><i class="pi pi-building" aria-hidden="true"></i><app-status-badge [status]="tenant.onboardingCompleted ? 'success' : 'warning'" [label]="tenant.onboardingCompleted ? 'Configurada' : 'Configuração pendente'" /></div>
  <h2>{{ tenant.name }}</h2><p class="text-muted">{{ roles[tenant.role] }} · {{ tenant.status === 'Pending' ? 'Pendente' : 'Ativa' }}</p>
  <p>{{ tenant.onboardingCompleted ? 'Acesse os dados e as operações desta empresa.' : 'Os dados da empresa precisam ser configurados antes de iniciar as operações.' }}</p>
  <p-button [label]="tenant.onboardingCompleted ? 'Acessar empresa' : 'Concluir configuração'" [fluid]="true" [disabled]="busy()" [loading]="selecting() === tenant.id" (onClick)="select(tenant)" />
 </article>
 }</div>
 }
 </section>` })
export class OrganizationsPage {
 readonly auth = inject(AuthService); private readonly http = inject(HttpClient); private readonly router = inject(Router);
 readonly tenants = signal<TenantContext[]>([]); readonly busy = signal(false); readonly loading = signal(true); readonly error = signal(false); readonly selecting = signal<string | null>(null);
 readonly roles = { Owner: 'Proprietário', Admin: 'Administrador', Member: 'Colaborador' };
 constructor() { void this.load(); }
 async load(): Promise<void> { this.loading.set(true); this.error.set(false); try { this.tenants.set(await firstValueFrom(this.http.get<TenantContext[]>('/api/auth/tenants'))); } catch { this.error.set(true); } finally { this.loading.set(false); } }
 platform(): void { void this.router.navigateByUrl('/plataforma'); }
 async select(tenant: TenantContext): Promise<void> {
  if (this.busy()) return;
  this.busy.set(true); this.selecting.set(tenant.id);
  try { await this.auth.selectTenant(tenant.id); await this.router.navigateByUrl(this.auth.destination()); }
  catch { /* O interceptor exibe o erro da API. */ }
  finally { this.busy.set(false); this.selecting.set(null); }
 }
}
