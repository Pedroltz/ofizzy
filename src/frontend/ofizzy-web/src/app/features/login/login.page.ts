import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { InputTextModule } from 'primeng/inputtext';
import { ButtonModule } from 'primeng/button';
import { MessageModule } from 'primeng/message';
import { ThemeToggleComponent } from '../../shared/components/theme-toggle.component';

@Component({
  selector: 'app-login-page',
  imports: [ReactiveFormsModule, ThemeToggleComponent, InputTextModule, ButtonModule, MessageModule],
  templateUrl: './login.page.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoginPage {
  private readonly fb = inject(FormBuilder); private readonly auth = inject(AuthService); private readonly router = inject(Router);
  readonly saving = signal(false); readonly invalidCredentials = signal(false);
  readonly form = this.fb.nonNullable.group({ email: ['', [Validators.required, Validators.email]], password: ['', Validators.required] });
  async submit(): Promise<void> {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.saving.set(true); this.invalidCredentials.set(false);
    try { const value = this.form.getRawValue(); await this.auth.login(value.email, value.password); await this.router.navigateByUrl(this.auth.destination()); }
    catch (error) { this.invalidCredentials.set(error instanceof HttpErrorResponse && error.status === 401); }
    finally { this.saving.set(false); }
  }
}
