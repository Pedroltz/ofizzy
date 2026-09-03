import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';

export type StatusVariant =
  | 'Open'
  | 'InProgress'
  | 'Completed'
  | 'Cancelled'
  | 'info'
  | 'warning'
  | 'success'
  | 'danger'
  | 'neutral';

const STATUS_LABELS: Record<string, string> = {
  Open: 'Aberta',
  InProgress: 'Em andamento',
  Completed: 'Concluída',
  Cancelled: 'Cancelada',
  info: 'Informativo',
  warning: 'Atenção',
  success: 'Sucesso',
  danger: 'Erro',
  neutral: 'Neutro',
};

const STATUS_ICONS: Record<string, string> = {
  Open: 'pi pi-clock',
  InProgress: 'pi pi-spin pi-spinner',
  Completed: 'pi pi-check',
  Cancelled: 'pi pi-times',
};

@Component({
  selector: 'app-status-badge',
  template: `
    <span
      class="status-badge"
      [class]="'variant-' + normalizedVariant()"
      [class.size-sm]="size() === 'sm'"
      [class.size-md]="size() === 'md'"
    >
      @if (showDot()) {
        <span class="status-badge__dot" aria-hidden="true"></span>
      }
      @if (resolvedIcon()) {
        <i [class]="resolvedIcon()" aria-hidden="true"></i>
      }
      <span class="status-badge__label">{{ resolvedLabel() }}</span>
    </span>
  `,
  styles: [`
    .status-badge {
      display: inline-flex;
      align-items: center;
      gap: 0.35rem;
      border-radius: var(--radius-sm);
      font-weight: var(--font-semibold);
      letter-spacing: 0.02em;
      line-height: var(--leading-none);
      white-space: nowrap;
      transition: background-color var(--transition-fast), color var(--transition-fast);
    }

    .size-sm {
      height: 1.5rem;
      padding: 0 0.5rem;
      font-size: var(--text-xs);
    }

    .size-md {
      height: 1.75rem;
      padding: 0 0.65rem;
      font-size: var(--text-xs);
    }

    .status-badge i {
      font-size: 0.75em;
    }

    .status-badge__dot {
      width: 0.45rem;
      height: 0.45rem;
      border-radius: var(--radius-full);
      background-color: currentColor;
    }

    /* Variants */
    .variant-open,
    .variant-warning {
      color: var(--warning-text);
      background: var(--warning-soft);
      border: 1px solid var(--warning);
    }

    .variant-in-progress,
    .variant-info {
      color: var(--info-text);
      background: var(--info-soft);
      border: 1px solid var(--info);
    }

    .variant-completed,
    .variant-success {
      color: var(--success-text);
      background: var(--success-soft);
      border: 1px solid var(--success);
    }

    .variant-cancelled,
    .variant-danger {
      color: var(--danger-text);
      background: var(--danger-soft);
      border: 1px solid var(--danger);
    }

    .variant-neutral {
      color: var(--text-secondary);
      background: var(--surface-hover);
      border: 1px solid var(--border-default);
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StatusBadgeComponent {
  readonly status = input.required<StatusVariant | string>();
  readonly label = input<string>('');
  readonly icon = input<string>('');
  readonly showDot = input<boolean>(false);
  readonly size = input<'sm' | 'md'>('md');

  readonly normalizedVariant = computed(() => {
    const s = this.status();
    switch (s) {
      case 'Open': return 'open';
      case 'InProgress': return 'in-progress';
      case 'Completed': return 'completed';
      case 'Cancelled': return 'cancelled';
      default: return s.toLowerCase();
    }
  });

  readonly resolvedLabel = computed(() => {
    return this.label() || STATUS_LABELS[this.status()] || this.status();
  });

  readonly resolvedIcon = computed(() => {
    if (this.icon()) return this.icon();
    return STATUS_ICONS[this.status()] || '';
  });
}
