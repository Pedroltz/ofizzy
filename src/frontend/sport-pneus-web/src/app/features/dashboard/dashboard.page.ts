import { ChangeDetectionStrategy, Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { SkeletonModule } from 'primeng/skeleton';
import { WorkOrderApiService } from '../../core/api/work-order-api.service';
import { WorkOrderSummary, WorkOrderStatus } from '../../core/api/work-order.models';

@Component({
  selector: 'app-dashboard-page',
  imports: [RouterLink, ButtonModule, SkeletonModule],
  templateUrl: './dashboard.page.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DashboardPage implements OnInit {
  private readonly workOrdersApi = inject(WorkOrderApiService);

  readonly loading = signal(true);
  readonly activeOrders = signal<WorkOrderSummary[]>([]);
  readonly totalCustomers = signal(0);
  readonly totalVehicles = signal(0);
  readonly totalActiveOrders = signal(0);
  readonly totalCompletedOrders = signal(0);

  readonly statusLabels: Record<WorkOrderStatus, string> = {
    Open: 'Aberta',
    InProgress: 'Em andamento',
    Completed: 'Finalizada',
    Cancelled: 'Cancelada'
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


  money(value: number): string {
    return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(value ?? 0);
  }

  formatDate(iso?: string | null): string {
    if (!iso) return '-';
    return new Date(iso).toLocaleDateString('pt-BR', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }
}

