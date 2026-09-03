import { ChangeDetectionStrategy, Component, HostListener, computed, inject, signal } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from '../core/auth/auth.service';
import { ThemeToggleComponent } from '../shared/components/theme-toggle.component';

@Component({
  selector: 'app-shell',
  imports: [RouterOutlet, RouterLink, RouterLinkActive, ThemeToggleComponent],
  templateUrl: './app-shell.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AppShellComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  readonly user = this.auth.user;
  readonly mobileMenu = signal(false);

  readonly todayFormatted = computed(() => {
    const d = new Date();
    const dayName = d.toLocaleDateString('pt-BR', { weekday: 'long' });
    const dayAndMonth = d.toLocaleDateString('pt-BR', { day: '2-digit', month: 'long' });
    const capitalized = dayName.charAt(0).toUpperCase() + dayName.slice(1);
    return `${capitalized}, ${dayAndMonth}`;
  });

  readonly navigation = [
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

  async logout(): Promise<void> {
    await this.auth.logout();
    await this.router.navigateByUrl('/login');
  }

  @HostListener('document:keydown.escape')
  closeMobileMenu(): void {
    this.mobileMenu.set(false);
  }
}
