import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { MessageModule } from 'primeng/message';
import { TenantContextService } from '../../core/tenancy/tenant-context.service';
import { CompanySettingsFormComponent } from '../settings/company-settings-form.component';
import { PageHeaderComponent, EmptyStateComponent } from '../../shared/components';
@Component({
  selector: 'app-onboarding',
  imports: [
    CompanySettingsFormComponent,
    PageHeaderComponent,
    EmptyStateComponent,
    ButtonModule,
    MessageModule,
    RouterLink,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `<section class="page-container saas-page">
    <app-page-header
      eyebrow="CONFIGURAÇÃO INICIAL"
      [title]="context.tenant()?.name || 'Configure sua empresa'"
      description="Confira os dados da empresa para iniciar suas operações."
    >
      <p-button
        label="Trocar empresa"
        severity="secondary"
        [outlined]="true"
        routerLink="/organizacoes"
      />
    </app-page-header>
    @if (!context.tenant()) {
      <app-empty-state
        icon="pi pi-building"
        title="Selecione uma empresa"
        description="Escolha uma organização antes de configurar seus dados."
      />
    } @else if (context.admin()) {
      <app-company-settings-form [completeOnSave]="true" />
    } @else {
      <p-message severity="info"
        >O proprietário ou administrador desta empresa precisa concluir a configuração inicial. Você
        poderá acessar as operações assim que essa etapa for finalizada.</p-message
      >
    }
  </section>`,
})
export class OnboardingPage {
  readonly context = inject(TenantContextService);
}
