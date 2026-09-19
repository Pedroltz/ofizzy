import { ChangeDetectionStrategy, Component, computed, inject, input, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { Popover } from 'primeng/popover';
import { MessageService } from 'primeng/api';
import { TenantContextService, TenantContext } from '../core/tenancy/tenant-context.service';
import { AuthService } from '../core/auth/auth.service';

@Component({
  selector: 'app-organization-switcher',
  standalone: true,
  imports: [RouterLink, Popover],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @if (!hasMultiple()) {
      <div
        class="header-org-badge"
        [class.compact]="compact()"
        [attr.aria-label]="'Organização: ' + subtitle()"
      >
        <i class="pi pi-building header-org-icon" aria-hidden="true"></i>
        <span class="header-org-name" [title]="subtitle()">{{ subtitle() }}</span>
      </div>
    } @else {
      <button
        type="button"
        class="header-org-trigger org-switcher-trigger"
        [class.compact]="compact()"
        (click)="op.toggle($event)"
        [attr.aria-expanded]="op.overlayVisible"
        aria-haspopup="dialog"
        [attr.aria-label]="'Trocar organização: ' + subtitle()"
      >
        <i class="pi pi-building header-org-icon" aria-hidden="true"></i>
        <span class="header-org-name" [title]="subtitle()">{{ subtitle() }}</span>
        <span class="org-switcher-chevron" [class.open]="op.overlayVisible" aria-hidden="true">
          <i class="pi pi-chevron-down"></i>
        </span>
      </button>

      <p-popover #op styleClass="org-switcher-popover" [dismissable]="true">
        <div class="org-popover-content">
          <div class="org-menu-label">Suas organizações</div>

          <div class="org-menu-list" role="listbox" aria-label="Lista de organizações">
            @for (tenant of tenants(); track tenant.id) {
              <button
                type="button"
                class="org-menu-item"
                [class.active]="tenant.id === currentTenant()?.id"
                [disabled]="switching() !== null"
                (click)="switchTenant(tenant, op)"
                role="option"
                [attr.aria-selected]="tenant.id === currentTenant()?.id"
              >
                <div class="org-item-main">
                  <span class="org-item-name">{{ tenant.name }}</span>
                  <span class="org-item-badge">{{ roles[tenant.role] || tenant.role }}</span>
                </div>
                @if (switching() === tenant.id) {
                  <i class="pi pi-spin pi-spinner org-item-status" aria-hidden="true"></i>
                } @else if (tenant.id === currentTenant()?.id) {
                  <i class="pi pi-check org-item-status active" aria-hidden="true"></i>
                }
              </button>
            }
          </div>

          <div class="org-menu-divider"></div>

          <div class="org-menu-footer">
            <a routerLink="/organizacoes" class="org-menu-action" (click)="op.hide()">
              <i class="pi pi-th-large"></i>
              <span>Minhas organizações</span>
            </a>
            @if (user()?.isPlatformAdmin) {
              <a routerLink="/plataforma" class="org-menu-action" (click)="op.hide()">
                <i class="pi pi-shield"></i>
                <span>Plataforma</span>
              </a>
            }
          </div>
        </div>
      </p-popover>
    }
  `,
})
export class OrganizationSwitcherComponent {
  private readonly router = inject(Router);
  private readonly messageService = inject(MessageService, { optional: true });
  readonly tenantContext = inject(TenantContextService);
  readonly auth = inject(AuthService);

  readonly home = input<string>('/');
  readonly platformArea = input<boolean>(false);
  readonly compact = input<boolean>(false);

  readonly user = this.auth.user;
  readonly currentTenant = this.tenantContext.tenant;
  readonly tenants = this.tenantContext.userTenants;
  readonly hasMultiple = this.tenantContext.hasMultipleTenants;
  readonly switching = signal<string | null>(null);

  readonly roles: Record<string, string> = {
    Owner: 'Proprietário',
    Admin: 'Administrador',
    Member: 'Colaborador',
  };

  readonly subtitle = computed(() => {
    if (this.platformArea()) return 'Organizações e acessos';
    return this.currentTenant()?.name || 'Oficina';
  });

  readonly brandAriaLabel = computed(() => {
    return `Ofizzy — ${this.subtitle()}`;
  });

  async switchTenant(tenant: TenantContext, popover: Popover): Promise<void> {
    if (tenant.id === this.currentTenant()?.id) {
      popover.hide();
      return;
    }
    if (this.switching()) return;

    this.switching.set(tenant.id);
    try {
      await this.auth.selectTenant(tenant.id);
      popover.hide();
      this.messageService?.add({
        severity: 'success',
        summary: 'Organização alterada',
        detail: `Alternado para ${tenant.name}`,
        life: 3000,
      });
      await this.router.navigateByUrl(this.auth.destination());
    } catch {
      // Erro tratado pelo interceptor global
    } finally {
      this.switching.set(null);
    }
  }
}
