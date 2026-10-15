import { TenantContextService } from '../core/tenancy/tenant-context.service';
import {
  ChangeDetectionStrategy,
  Component,
  HostListener,
  computed,
  inject,
  signal,
} from '@angular/core';
import {
  ActivatedRoute,
  Router,
  RouterLink,
  RouterLinkActive,
  RouterOutlet,
} from '@angular/router';
import { AuthService } from '../core/auth/auth.service';
import { ThemeToggleComponent } from '../shared/components/theme-toggle.component';
import { OrganizationSwitcherComponent } from './organization-switcher.component';

@Component({
  selector: 'app-shell',
  imports: [
    RouterOutlet,
    RouterLink,
    RouterLinkActive,
    ThemeToggleComponent,
    OrganizationSwitcherComponent,
  ],
  templateUrl: './app-shell.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AppShellComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  readonly platformArea = inject(ActivatedRoute).snapshot.data['platformArea'] === true;
  readonly home = this.platformArea ? '/organizacoes' : '/';
  readonly tenantContext = inject(TenantContextService);
  readonly user = this.auth.user;
  readonly mobileMenu = signal(false);

  constructor() {
    void this.tenantContext.loadUserTenants();
  }

  readonly todayFormatted = computed(() => {
    const d = new Date();
    const dayName = d.toLocaleDateString('pt-BR', { weekday: 'long' });
    const dayAndMonth = d.toLocaleDateString('pt-BR', { day: '2-digit', month: 'long' });
    const capitalized = dayName.charAt(0).toUpperCase() + dayName.slice(1);
    return `${capitalized}, ${dayAndMonth}`;
  });

  private readonly allNavigation = [
    {
      label: 'Operação',
      items: [
        { label: 'Visão Geral', icon: 'pi pi-home', route: '/', exact: true },
        { label: 'Ordens de Serviço', icon: 'pi pi-file-edit', route: '/ordens', exact: false },
      ],
    },
    {
      label: 'Gestão',
      items: [
        { label: 'Clientes', icon: 'pi pi-users', route: '/clientes', exact: false },
        { label: 'Veículos', icon: 'pi pi-car', route: '/veiculos', exact: false },
        {
          label: 'Catálogo e Ajustes',
          icon: 'pi pi-sliders-h',
          route: '/configuracoes',
          exact: false,
        },
      ],
    },
  ] as const;

  readonly navigation = computed(() =>
    this.platformArea
      ? [
          {
            label: 'Sua conta',
            items: [
              ...(this.user()?.isPlatformAdmin
                ? [
                    {
                      label: 'Empresas',
                      icon: 'pi pi-building',
                      route: '/plataforma',
                      exact: false,
                    },
                  ]
                : []),
              {
                label: 'Minhas organizações',
                icon: 'pi pi-th-large',
                route: '/organizacoes',
                exact: false,
              },
            ],
          },
        ]
      : [
          ...this.allNavigation.map((group) => ({
            ...group,
            items: group.items.filter((item) => {
              const modules: Record<
                string,
                ('Customers' | 'WorkOrders' | 'Catalog' | 'Automotive')[]
              > = {
                '/': ['Customers', 'WorkOrders'],
                '/ordens': ['WorkOrders'],
                '/clientes': ['Customers'],
                '/veiculos': ['Automotive'],
              };
              return (modules[item.route] ?? []).every((m) => this.tenantContext.has(m));
            }),
          })),
          ...(this.user()?.isPlatformAdmin
            ? [
                {
                  label: 'Administração',
                  items: [
                    {
                      label: 'Plataforma',
                      icon: 'pi pi-shield',
                      route: '/plataforma',
                      exact: false,
                    },
                  ],
                },
              ]
            : []),
        ],
  );

  async logout(): Promise<void> {
    await this.auth.logout();
    await this.router.navigateByUrl('/login');
  }

  @HostListener('document:keydown.escape')
  closeMobileMenu(): void {
    this.mobileMenu.set(false);
  }

  @HostListener('document:keydown', ['$event'])
  handleGlobalShortcut(event: KeyboardEvent): void {
    if (event.key === '/' && !this.isEditingText(event.target)) {
      const searchInput = document.querySelector<HTMLInputElement>(
        '.search-field__input, .search-bar input',
      );
      if (searchInput) {
        event.preventDefault();
        searchInput.focus();
        searchInput.select();
      }
    }
  }

  private isEditingText(target: EventTarget | null): boolean {
    if (!target || !(target instanceof HTMLElement)) return false;
    const tag = target.tagName.toLowerCase();
    return tag === 'input' || tag === 'textarea' || tag === 'select' || target.isContentEditable;
  }
}
