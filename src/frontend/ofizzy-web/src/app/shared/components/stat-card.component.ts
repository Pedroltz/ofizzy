import { ChangeDetectionStrategy, Component, input } from '@angular/core';

export type StatCardSeverity = 'primary' | 'success' | 'warning' | 'danger' | 'info' | 'neutral';

@Component({
  selector: 'app-stat-card',
  template: `
    <article class="surface-card stat-card" [class]="'severity-' + severity()">
      <div class="stat-card__top">
        <span class="stat-card__label">{{ label() }}</span>
        @if (icon()) {
          <div class="stat-card__icon" aria-hidden="true">
            <i [class]="icon()"></i>
          </div>
        }
      </div>

      <div class="stat-card__main">
        <strong class="stat-card__value">{{ value() }}</strong>
      </div>

      @if (hint() || trend()) {
        <div class="stat-card__footer">
          @if (trend()) {
            <span
              class="stat-card__trend"
              [class.up]="trendDirection() === 'up'"
              [class.down]="trendDirection() === 'down'"
            >
              @if (trendDirection() === 'up') {
                <i class="pi pi-arrow-up"></i>
              } @else if (trendDirection() === 'down') {
                <i class="pi pi-arrow-down"></i>
              }
              <span>{{ trend() }}</span>
            </span>
          }
          @if (hint()) {
            <span class="stat-card__hint">{{ hint() }}</span>
          }
        </div>
      }
    </article>
  `,
  styles: [
    `
      .stat-card {
        display: flex;
        flex-direction: column;
        padding: var(--space-4) var(--space-5);
        background: var(--surface-primary);
        border: 1px solid var(--border-subtle);
        border-radius: var(--radius-md);
        box-shadow: var(--shadow-card);
        gap: var(--space-2);
        transition:
          border-color var(--transition-fast),
          box-shadow var(--transition-fast);
      }

      .stat-card:hover {
        border-color: var(--border-default);
      }

      .stat-card__top {
        display: flex;
        align-items: center;
        justify-content: space-between;
        gap: var(--space-2);
      }

      .stat-card__label {
        font-size: var(--text-xs);
        font-weight: var(--font-medium);
        color: var(--text-secondary);
        text-transform: uppercase;
        letter-spacing: 0.04em;
      }

      .stat-card__icon {
        width: 2rem;
        height: 2rem;
        display: grid;
        place-items: center;
        border-radius: var(--radius-sm);
        font-size: var(--text-sm);
        transition: transform var(--transition-fast);
      }

      .stat-card:hover .stat-card__icon {
        transform: scale(1.05);
      }

      .stat-card__value {
        font-size: var(--text-2xl);
        font-weight: var(--font-bold);
        color: var(--text-primary);
        font-variant-numeric: tabular-nums;
        line-height: var(--leading-tight);
      }

      .stat-card__footer {
        display: flex;
        align-items: center;
        gap: var(--space-2);
        font-size: var(--text-xs);
        color: var(--text-muted);
        margin-top: auto;
      }

      .stat-card__trend {
        display: inline-flex;
        align-items: center;
        gap: 0.2rem;
        font-weight: var(--font-semibold);
        font-size: var(--text-xs);
      }

      .stat-card__trend.up {
        color: var(--success);
      }

      .stat-card__trend.down {
        color: var(--danger);
      }

      /* Severities */
      .severity-primary .stat-card__icon {
        background: var(--primary-soft);
        color: var(--primary);
      }

      .severity-success .stat-card__icon {
        background: var(--success-soft);
        color: var(--success);
      }

      .severity-warning .stat-card__icon {
        background: var(--warning-soft);
        color: var(--warning);
      }

      .severity-danger .stat-card__icon {
        background: var(--danger-soft);
        color: var(--danger);
      }

      .severity-info .stat-card__icon {
        background: var(--info-soft);
        color: var(--info);
      }

      .severity-neutral .stat-card__icon {
        background: var(--surface-hover);
        color: var(--text-secondary);
      }
    `,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StatCardComponent {
  readonly label = input.required<string>();
  readonly value = input.required<string | number>();
  readonly hint = input<string>('');
  readonly icon = input<string>('');
  readonly trend = input<string>('');
  readonly trendDirection = input<'up' | 'down' | 'neutral'>('neutral');
  readonly severity = input<StatCardSeverity>('primary');
}
