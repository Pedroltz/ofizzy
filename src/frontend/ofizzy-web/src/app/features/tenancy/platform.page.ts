import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { CheckboxModule } from 'primeng/checkbox';
import { ProductModule } from '../../core/tenancy/tenant-context.service';
interface PlatformTenant { id: string; name: string; slug: string; status: string; vertical: string; onboardingCompletedAt: string | null; modules: ProductModule[]; }
@Component({ selector: 'app-platform', imports: [FormsModule, RouterLink, ButtonModule, InputTextModule, SelectModule, CheckboxModule], changeDetection: ChangeDetectionStrategy.OnPush,
 template: `<main class="tenant-page"><a routerLink="/organizacoes" class="tenant-link">Minhas organizações</a><h1>Administração da plataforma</h1>
 @if (message()) { <p role="status">{{ message() }}</p> }
 <section class="surface-card"><h2>Novo tenant</h2><form #form="ngForm" (ngSubmit)="create()" class="tenant-form">
 <label>Nome<input pInputText name="name" [(ngModel)]="draft.name" required maxlength="160" /></label>
 <label>Identificador<input pInputText name="slug" [(ngModel)]="draft.slug" required pattern="[a-z0-9]+(-[a-z0-9]+)*" maxlength="80" /></label>
 <label>Vertical<p-select name="vertical" [(ngModel)]="draft.vertical" [options]="['Automotive']" /></label>
 <label>Nome do administrador<input pInputText name="adminName" [(ngModel)]="draft.adminName" required maxlength="120" /></label>
 <label>E-mail do administrador<input pInputText name="email" type="email" [(ngModel)]="draft.email" required email /></label>
 <label>Senha inicial<input pInputText name="password" type="password" [(ngModel)]="draft.password" autocomplete="new-password" minlength="10" maxlength="200" /></label>
 <p>Para usuário existente, deixe a senha vazia. Para novo usuário, disponibilize a senha inicial por canal privado.</p>
 <fieldset><legend>Módulos</legend>@for (module of availableModules; track module) { <label class="tenant-check"><p-checkbox name="modules" [value]="module" [(ngModel)]="draft.modules" [inputId]="'new-' + module" /><span>{{ module }}</span></label> }</fieldset>
 <p-button type="submit" label="Criar tenant" [disabled]="busy() || form.invalid === true" /></form></section>
 <div class="tenant-grid">@for (tenant of tenants(); track tenant.id) { <section class="surface-card"><h2>{{ tenant.name }}</h2><p>{{ tenant.slug }} · {{ tenant.vertical }}</p><p>{{ tenant.onboardingCompletedAt ? 'Onboarding concluído' : 'Aguardando onboarding' }}</p>
 <label>Estado<p-select [(ngModel)]="tenant.status" [options]="statuses" [attr.aria-label]="'Estado de ' + tenant.name" /></label>
 <fieldset><legend>Módulos de {{ tenant.name }}</legend>@for (module of availableModules; track module) { <label class="tenant-check"><p-checkbox [value]="module" [(ngModel)]="tenant.modules" [inputId]="tenant.id + module" /><span>{{ module }}</span></label> }</fieldset>
 <p-button label="Salvar alterações" [disabled]="busy()" (onClick)="save(tenant)" /></section> }</div></main>` })
export class PlatformPage {
 private readonly http = inject(HttpClient); readonly tenants = signal<PlatformTenant[]>([]); readonly busy = signal(false); readonly message = signal('');
 readonly availableModules: ProductModule[] = ['Customers', 'WorkOrders', 'Catalog', 'Automotive']; readonly statuses = ['Pending', 'Active', 'Suspended', 'Archived'];
 draft = this.empty();
 private empty() { return { name: '', slug: '', vertical: 'Automotive', adminName: '', email: '', password: '', modules: [...this.availableModules] }; }
 constructor() { void this.load().catch(() => this.message.set('Não foi possível carregar os tenants.')); }
 private async load(): Promise<void> { this.tenants.set(await firstValueFrom(this.http.get<PlatformTenant[]>('/api/platform/tenants'))); }
 async create(): Promise<void> {
  this.busy.set(true); this.message.set('');
  try { await firstValueFrom(this.http.post('/api/platform/tenants', { ...this.draft, password: this.draft.password || null })); this.draft = this.empty(); await this.load(); this.message.set('Tenant criado. O administrador pode entrar e concluir o onboarding.'); }
  catch { this.message.set('Não foi possível criar. Verifique o identificador, usuário, senha e dependências dos módulos.'); } finally { this.busy.set(false); }
 }
 async save(tenant: PlatformTenant): Promise<void> {
  this.busy.set(true);
  try { await firstValueFrom(this.http.put('/api/platform/tenants/' + tenant.id, { status: tenant.status, modules: tenant.modules })); await this.load(); this.message.set('Tenant atualizado.'); }
  catch { this.message.set('Não foi possível salvar. Verifique o onboarding e as dependências dos módulos.'); } finally { this.busy.set(false); }
 }
}
