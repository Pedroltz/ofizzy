import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { ThemeToggleComponent } from '../../shared/components/theme-toggle.component';

@Component({
  selector: 'app-setup-page',
  imports: [ReactiveFormsModule, ThemeToggleComponent],
  templateUrl: './setup.page.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SetupPage {
  private readonly fb = inject(FormBuilder); private readonly auth = inject(AuthService); private readonly router = inject(Router);
  readonly saving = signal(false);
  readonly form = this.fb.nonNullable.group({
    companyName: ['Sport Pneus', [Validators.required, Validators.maxLength(160)]], cnpj: [''], phone: [''],
    adminName: ['', [Validators.required, Validators.maxLength(120)]], email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(10), Validators.pattern(/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$/)]],
  });
  async submit(): Promise<void> {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.saving.set(true);
    try {
      const value = this.form.getRawValue();
      await this.auth.setup({ ...value, cnpj: value.cnpj.replace(/\D/g, '') || null, phone: value.phone.trim() || null });
      await this.router.navigateByUrl('/');
    } finally { this.saving.set(false); }
  }
}
