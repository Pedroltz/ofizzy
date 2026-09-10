import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router, RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { ButtonModule } from 'primeng/button';
import { AuthService } from '../../core/auth/auth.service';
import { TenantContextService } from '../../core/tenancy/tenant-context.service';
import { SettingsPage } from '../settings/settings.page';
@Component({ selector: 'app-onboarding', imports: [SettingsPage, ButtonModule, RouterLink], changeDetection: ChangeDetectionStrategy.OnPush,
 template: `<main class="tenant-page"><a class="tenant-link" routerLink="/organizacoes">Trocar organização</a><h1>Configure sua empresa</h1>
 @if (context.admin()) { <p>Salve os dados da empresa e conclua a configuração para iniciar as operações.</p><app-settings-page />
 <p-button label="Concluir onboarding" [disabled]="busy()" (onClick)="complete()" /> }
 @else { <p>O proprietário ou administrador precisa concluir a configuração da empresa.</p> }
 @if (error()) { <p role="alert">{{ error() }}</p> }</main>` })
export class OnboardingPage {
 readonly context = inject(TenantContextService); private readonly http = inject(HttpClient); private readonly auth = inject(AuthService); private readonly router = inject(Router);
 readonly busy = signal(false); readonly error = signal('');
 async complete(): Promise<void> { this.busy.set(true); try { await firstValueFrom(this.http.post('/api/tenant/onboarding/complete', {})); await this.auth.reload(); await this.router.navigateByUrl(this.context.has('WorkOrders') ? '/' : '/configuracoes'); } catch { this.error.set('Salve os dados da empresa antes de concluir.'); } finally { this.busy.set(false); } }
}
