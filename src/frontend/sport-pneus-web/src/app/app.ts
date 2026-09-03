import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ThemeService } from './core/theme/theme.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, ToastModule, ConfirmDialogModule],
  template: '<p-toast position="top-right" /><p-confirmdialog /><router-outlet />',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class App {
  // Ensure ThemeService is initialized eagerly on app bootstrap
  protected readonly themeService = inject(ThemeService);
}

