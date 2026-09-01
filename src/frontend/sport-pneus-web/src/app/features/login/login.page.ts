import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { AuthService } from '../../core/auth/auth.service';

@Component({ selector: 'app-login-page', imports: [ReactiveFormsModule, ButtonModule, CardModule, InputTextModule, PasswordModule], templateUrl: './login.page.html', changeDetection: ChangeDetectionStrategy.OnPush })
export class LoginPage {
  private readonly fb = inject(FormBuilder); private readonly auth = inject(AuthService); private readonly router = inject(Router);
  readonly saving = signal(false); readonly invalidCredentials = signal(false);
  readonly form = this.fb.nonNullable.group({ email: ['', [Validators.required, Validators.email]], password: ['', Validators.required] });
  async submit(): Promise<void> {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.saving.set(true); this.invalidCredentials.set(false);
    try { const value = this.form.getRawValue(); await this.auth.login(value.email, value.password); await this.router.navigateByUrl('/'); }
    catch { this.invalidCredentials.set(true); }
    finally { this.saving.set(false); }
  }
}
