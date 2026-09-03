import { ChangeDetectionStrategy, Component, HostListener, inject, signal } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from '../core/auth/auth.service';

@Component({
  selector: 'app-shell',
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './app-shell.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AppShellComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  readonly user = this.auth.user;
  readonly mobileMenu = signal(false);
  readonly navigation = [
    {
      label: 'Operação',
      items: [
        { label: 'Visão geral', icon: 'pi pi-home', route: '/', exact: true },
        { label: 'Ordens de serviço', icon: 'pi pi-file-edit', route: '/ordens', exact: false },
      ],
    },
    {
      label: 'Gestão',
      items: [
        { label: 'Clientes', icon: 'pi pi-users', route: '/clientes', exact: false },
        { label: 'Veículos', icon: 'pi pi-car', route: '/veiculos', exact: false },
        {
          label: 'Catálogo e ajustes',
          icon: 'pi pi-sliders-h',
          route: '/configuracoes',
          exact: false,
        },
      ],
    },
  ] as const;

  async logout(): Promise<void> {
    await this.auth.logout();
    await this.router.navigateByUrl('/login');
  }

  @HostListener('document:keydown.escape')
  closeMobileMenu(): void {
    this.mobileMenu.set(false);
  }
}
