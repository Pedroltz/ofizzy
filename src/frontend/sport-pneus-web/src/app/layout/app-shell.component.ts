import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { Router, RouterLink, RouterOutlet } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { DrawerModule } from 'primeng/drawer';
import { AuthService } from '../core/auth/auth.service';

@Component({ selector: 'app-shell', imports: [RouterOutlet, RouterLink, ButtonModule, DrawerModule], templateUrl: './app-shell.component.html', changeDetection: ChangeDetectionStrategy.OnPush })
export class AppShellComponent {
  private readonly auth = inject(AuthService); private readonly router = inject(Router);
  readonly user = this.auth.user; readonly mobileMenu = signal(false);
  async logout(): Promise<void> { await this.auth.logout(); await this.router.navigateByUrl('/login'); }
}
