import {
  ChangeDetectionStrategy,
  Component,
  OnInit,
  computed,
  inject,
  signal,
} from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { WorkOrderApiService } from '../../core/api/work-order-api.service';
import { WorkOrderSummary, WorkOrderStatus } from '../../core/api/work-order.models';
import {
  DataTableWrapperComponent,
  EmptyStateComponent,
  LoadingStateComponent,
  PageHeaderComponent,
  SectionCardComponent,
  StatCardComponent,
  StatusBadgeComponent,
} from '../../shared/components';

@Component({
  selector: 'app-dashboard-page',
  imports: [
    RouterLink,
    ButtonModule,
    PageHeaderComponent,
    StatCardComponent,
    SectionCardComponent,
    StatusBadgeComponent,
    EmptyStateComponent,
    LoadingStateComponent,
    DataTableWrapperComponent,
  ],
  templateUrl: './dashboard.page.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DashboardPage implements OnInit {
  private readonly workOrdersApi = inject(WorkOrderApiService);
  private readonly router = inject(Router);

  readonly loading = signal(true);
  readonly activeOrders = signal<WorkOrderSummary[]>([]);
  readonly totalCustomers = signal(0);
  readonly totalVehicles = signal(0);
  readonly totalActiveOrders = signal(0);
  readonly totalCompletedOrders = signal(0);

  readonly openOrders = computed(() =>
    this.activeOrders().filter((o) => o.status === 'Open')
  );

  readonly inProgressOrders = computed(() =>
    this.activeOrders().filter((o) => o.status === 'InProgress')
  );

  readonly totalActiveValue = computed(() =>
    this.activeOrders().reduce((acc, order) => acc + (order.total || 0), 0)
  );

  readonly totalAllOrders = computed(() =>
    this.totalActiveOrders() + this.totalCompletedOrders()
  );

  readonly openPercentage = computed(() => {
    const total = this.totalAllOrders();
    if (!total) return 0;
    return Math.round((this.openOrders().length / total) * 100);
  });

  readonly inProgressPercentage = computed(() => {
    const total = this.totalAllOrders();
    if (!total) return 0;
    return Math.round((this.inProgressOrders().length / total) * 100);
  });

  readonly completedPercentage = computed(() => {
    const total = this.totalAllOrders();
    if (!total) return 0;
    return Math.round((this.totalCompletedOrders() / total) * 100);
  });

  readonly statusLabels: Record<WorkOrderStatus, string> = {
    Open: 'Aberta',
    InProgress: 'Em andamento',
    Completed: 'Finalizada',
    Cancelled: 'Cancelada',
  };

  async ngOnInit(): Promise<void> {
    try {
      const summary = await this.workOrdersApi.dashboardSummary();
      this.activeOrders.set(summary.activeOrders);
      this.totalActiveOrders.set(summary.totalActiveOrders);
      this.totalCompletedOrders.set(summary.totalCompletedOrders);
      this.totalCustomers.set(summary.totalCustomers);
      this.totalVehicles.set(summary.totalVehicles);
    } catch (e) {
      console.error('Erro ao carregar dados do dashboard:', e);
    } finally {
      this.loading.set(false);
    }
  }

  navigateToNewOrder(): void {
    void this.router.navigateByUrl('/ordens');
  }

  money(value: number): string {
    return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(
      value ?? 0
    );
  }

  formatDate(iso?: string | null): string {
    if (!iso) return '-';
    return new Date(iso).toLocaleDateString('pt-BR', {
      day: '2-digit',
      month: '2-digit',
      hour: '2-digit',
      minute: '2-digit',
    });
  }

  formatDateOnly(iso?: string | null): string {
    if (!iso) return '-';
    return new Date(iso).toLocaleDateString('pt-BR', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
    });
  }

  formatTimeOnly(iso?: string | null): string {
    if (!iso) return '';
    return new Date(iso).toLocaleTimeString('pt-BR', {
      hour: '2-digit',
      minute: '2-digit',
    });
  }

  formatTime(iso?: string | null): string {
    if (!iso) return '-';
    return new Date(iso).toLocaleTimeString('pt-BR', {
      hour: '2-digit',
      minute: '2-digit',
    });
  }

  padOrderNumber(num: number): string {
    return num.toString().padStart(4, '0');
  }
}
