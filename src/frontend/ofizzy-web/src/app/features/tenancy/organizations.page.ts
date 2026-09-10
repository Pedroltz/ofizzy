import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router, RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { ButtonModule } from 'primeng/button';
import { AuthService } from '../../core/auth/auth.service';
import { TenantContext } from '../../core/tenancy/tenant-context.service';
@Component({ selector: 'app-organizations', imports: [ButtonModule, RouterLink], changeDetection: ChangeDetectionStrategy.OnPush,
  template: `<main class="tenant-page"><h1>Suas organizações</h1><p>Selecione a empresa em que deseja trabalhar.</p>
  @if (auth.user()?.isPlatformAdmin) { <a routerLink="/plataforma" class="tenant-link">Administrar plataforma</a> }
  @if (error()) { <p role="alert">{{ error() }}</p> }
  <div class="tenant-grid">@for (tenant of tenants(); track tenant.id) {
    <section class="surface-card"><h2>{{ tenant.name }}</h2><p>{{ tenant.role }} · {{ tenant.status }}</p>
    <p-button label="Acessar organização" [disabled]="busy()" (onClick)="select(tenant)" /></section>
  } @empty { <p>Nenhuma organização disponível. Solicite acesso ao administrador da plataforma.</p> }</div>
  <p-button label="Sair" severity="secondary" (onClick)="logout()" /></main>`, styles: [] })
export class OrganizationsPage {
  readonly auth = inject(AuthService); private readonly http = inject(HttpClient); private readonly router = inject(Router);
  readonly tenants = signal<TenantContext[]>([]); readonly busy = signal(false); readonly error = signal('');
  constructor() { void firstValueFrom(this.http.get<TenantContext[]>('/api/auth/tenants')).then(x => this.tenants.set(x)).catch(() => this.error.set('Não foi possível carregar as organizações.')); }
  async select(tenant: TenantContext): Promise<void> {
    this.busy.set(true);
    try { await this.auth.selectTenant(tenant.id); await this.router.navigateByUrl(tenant.onboardingCompleted ? (tenant.modules.includes('WorkOrders') ? '/' : '/configuracoes') : '/onboarding'); }
    catch { this.error.set('Organização indisponível. Atualize a página e tente novamente.'); }
    finally { this.busy.set(false); }
  }
  async logout(): Promise<void> { await this.auth.logout(); await this.router.navigateByUrl('/login'); }
}
