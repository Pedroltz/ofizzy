import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { SkeletonModule } from 'primeng/skeleton';

export type LoadingMode = 'table' | 'cards' | 'lines' | 'stats';

@Component({
  selector: 'app-loading-state',
  imports: [SkeletonModule],
  template: `
    @switch (mode()) {
      @case ('table') {
        <div class="loading-table surface-card" aria-label="Carregando tabela..." aria-busy="true">
          <div class="loading-table__header">
            <p-skeleton width="100%" height="2rem" />
          </div>
          <div class="loading-table__rows">
            @for (_ of items(); track $index) {
              <div class="loading-table__row">
                <p-skeleton width="25%" height="1.4rem" />
                <p-skeleton width="30%" height="1.4rem" />
                <p-skeleton width="20%" height="1.4rem" />
                <p-skeleton width="15%" height="1.4rem" />
              </div>
            }
          </div>
        </div>
      }

      @case ('cards') {
        <div class="loading-cards" aria-label="Carregando cartões..." aria-busy="true">
          @for (_ of items(); track $index) {
            <div class="loading-card surface-card">
              <div class="loading-card__top">
                <p-skeleton shape="circle" size="2.5rem" />
                <p-skeleton width="35%" height="1.5rem" />
              </div>
              <p-skeleton width="60%" height="1.4rem" />
              <p-skeleton width="100%" height="3rem" />
              <div class="loading-card__footer">
                <p-skeleton width="40%" height="1rem" />
                <p-skeleton width="25%" height="1.8rem" />
              </div>
            </div>
          }
        </div>
      }

      @case ('stats') {
        <div class="loading-stats" aria-label="Carregando estatísticas..." aria-busy="true">
          @for (_ of items(); track $index) {
            <div class="loading-stat surface-card">
              <p-skeleton width="50%" height="1rem" />
              <p-skeleton width="70%" height="2rem" />
              <p-skeleton width="40%" height="0.8rem" />
            </div>
          }
        </div>
      }

      @default {
        <div class="loading-lines" aria-label="Carregando..." aria-busy="true">
          @for (_ of items(); track $index) {
            <p-skeleton width="100%" height="2.2rem" />
          }
        </div>
      }
    }
  `,
  styles: [
    `
      .loading-table {
        padding: var(--space-4);
        background: var(--surface-primary);
        border: 1px solid var(--border-subtle);
        border-radius: var(--radius-md);
        display: flex;
        flex-direction: column;
        gap: var(--space-3);
      }

      .loading-table__rows {
        display: flex;
        flex-direction: column;
        gap: var(--space-3);
      }

      .loading-table__row {
        display: flex;
        align-items: center;
        justify-content: space-between;
        gap: var(--space-4);
      }

      .loading-cards {
        display: grid;
        grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
        gap: var(--space-4);
      }

      .loading-card {
        padding: var(--space-4);
        background: var(--surface-primary);
        border: 1px solid var(--border-subtle);
        border-radius: var(--radius-md);
        display: flex;
        flex-direction: column;
        gap: var(--space-3);
      }

      .loading-card__top,
      .loading-card__footer {
        display: flex;
        align-items: center;
        justify-content: space-between;
        gap: var(--space-2);
      }

      .loading-stats {
        display: grid;
        grid-template-columns: repeat(auto-fill, minmax(220px, 1fr));
        gap: var(--space-4);
      }

      .loading-stat {
        padding: var(--space-4);
        background: var(--surface-primary);
        border: 1px solid var(--border-subtle);
        border-radius: var(--radius-md);
        display: flex;
        flex-direction: column;
        gap: var(--space-2);
      }

      .loading-lines {
        display: flex;
        flex-direction: column;
        gap: var(--space-3);
      }

      @media (max-width: 640px) {
        .loading-cards,
        .loading-stats {
          grid-template-columns: 1fr;
        }
      }
    `,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoadingStateComponent {
  readonly mode = input<LoadingMode>('table');
  readonly count = input<number>(4);

  readonly items = computed(() => Array.from({ length: Math.max(1, this.count()) }));
}
