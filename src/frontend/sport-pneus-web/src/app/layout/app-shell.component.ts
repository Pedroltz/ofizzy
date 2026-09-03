import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from '../core/auth/auth.service';

@Component({ selector: 'app-shell', imports: [RouterOutlet, RouterLink, RouterLinkActive], templateUrl: './app-shell.component.html', changeDetection: ChangeDetectionStrategy.OnPush })
export class AppShellComponent {
  private readonly auth = inject(AuthService); private readonly router = inject(Router);
  readonly user = this.auth.user; readonly mobileMenu = signal(false);
  async logout(): Promise<void> { await this.auth.logout(); await this.router.navigateByUrl('/login'); }
}
