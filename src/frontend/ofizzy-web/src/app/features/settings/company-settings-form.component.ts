import { ChangeDetectionStrategy, Component, inject, input, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { TextareaModule } from 'primeng/textarea';
import { MessageModule } from 'primeng/message';
import { MessageService } from 'primeng/api';
import { CompanyApiService } from '../../core/api/company-api.service';
import { AuthService } from '../../core/auth/auth.service';
import { TenantContextService } from '../../core/tenancy/tenant-context.service';
import { SectionCardComponent, LoadingStateComponent, EmptyStateComponent } from '../../shared/components';
@Component({ selector: 'app-company-settings-form', imports: [ReactiveFormsModule, ButtonModule, InputTextModule, TextareaModule, MessageModule, SectionCardComponent, LoadingStateComponent, EmptyStateComponent], templateUrl: './company-settings-form.component.html', changeDetection: ChangeDetectionStrategy.OnPush })
export class CompanySettingsFormComponent {
 readonly completeOnSave = input(false);
 readonly context = inject(TenantContextService);
 private readonly api = inject(CompanyApiService); private readonly http = inject(HttpClient);
 private readonly auth = inject(AuthService); private readonly router = inject(Router);
 private readonly messages = inject(MessageService); private readonly fb = inject(FormBuilder);
 readonly loading = signal(true); readonly loadError = signal(false); readonly saving = signal(false);
 readonly form = this.fb.nonNullable.group({
  name: ['', [Validators.required, Validators.maxLength(160)]], legalName: [''], cnpj: [''], phone: [''], whatsApp: [''], email: ['', Validators.email], address: [''], city: [''], state: [''], postalCode: [''], warrantyTerms: [''], receiptNotes: [''],
 });
 constructor() { if (!this.context.admin()) this.form.disable(); void this.load(); }
 async load(): Promise<void> {
  this.loading.set(true); this.loadError.set(false);
  try {
   const company = await this.api.get();
   this.form.patchValue({ name: company.name, legalName: company.legalName ?? '', cnpj: company.cnpj ?? '', phone: company.phone ?? '', whatsApp: company.whatsApp ?? '', email: company.email ?? '', address: company.address ?? '', city: company.city ?? '', state: company.state ?? '', postalCode: company.postalCode ?? '', warrantyTerms: company.warrantyTerms ?? '', receiptNotes: company.receiptNotes ?? '' });
  } catch { this.loadError.set(true); } finally { this.loading.set(false); }
 }
 async save(): Promise<void> {
  if (this.saving() || this.loading() || !this.context.admin()) return;
  if (this.form.invalid) { this.form.markAllAsTouched(); return; }
  this.saving.set(true);
  try {
   await this.api.update(this.form.getRawValue());
   this.form.markAsPristine();
   if (this.completeOnSave()) {
    await firstValueFrom(this.http.post('/api/tenant/onboarding/complete', {}));
    await this.auth.reload(); await this.router.navigateByUrl(this.auth.destination());
   }
   this.messages.add({ severity: 'success', summary: this.completeOnSave() ? 'Configuração concluída' : 'Dados da empresa salvos' });
  } catch { /* Erro apresentado pelo interceptor. Dados preenchidos permanecem para nova tentativa. */ }
  finally { this.saving.set(false); }
 }
}
