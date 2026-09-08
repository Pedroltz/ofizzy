import {
  ChangeDetectionStrategy,
  Component,
  input,
  model,
} from '@angular/core';

export type ViewMode = 'table' | 'cards';

@Component({
  selector: 'app-data-toolbar',
  template: `
    <div class="data-toolbar">
      <div class="data-toolbar__search">
        <ng-content select="[toolbar-search]" />
      </div>

      <div class="data-toolbar__filters">
        <ng-content select="[toolbar-filters]" />
      </div>

      <div class="data-toolbar__actions">
        @if (itemCount() !== null && itemCount() !== undefined) {
          <span class="data-toolbar__counter">
            {{ itemCount() }} {{ itemCount() === 1 ? itemLabelSingular() : itemLabelPlural() }}
          </span>
        }

        @if (showViewToggle()) {
          <div class="data-toolbar__view-toggle" role="group" aria-label="Modo de visualização">
            <button
              type="button"
              class="data-toolbar__toggle-btn"
              [class.active]="viewMode() === 'table'"
              (click)="viewMode.set('table')"
              title="Visualização em Lista"
              aria-label="Visualização em Lista"
            >
              <i class="pi pi-table" aria-hidden="true"></i>
            </button>
            <button
              type="button"
              class="data-toolbar__toggle-btn"
              [class.active]="viewMode() === 'cards'"
              (click)="viewMode.set('cards')"
              title="Visualização em Blocos"
              aria-label="Visualização em Blocos"
            >
              <i class="pi pi-th-large" aria-hidden="true"></i>
            </button>
          </div>
        }

        <ng-content select="[toolbar-actions]" />
      </div>
    </div>
  `,
  styles: [`
    .data-toolbar {
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: var(--space-4);
      margin-bottom: var(--space-5);
      flex-wrap: wrap;
    }

    .data-toolbar__search {
      flex: 1 1 320px;
      min-width: min(100%, 280px);
    }

    .data-toolbar__filters {
      display: flex;
      align-items: center;
      gap: var(--space-2);
      flex-wrap: wrap;
    }

    .data-toolbar__actions {
      display: flex;
      align-items: center;
      gap: var(--space-3);
      margin-left: auto;
    }

    .data-toolbar__counter {
      font-size: var(--text-xs);
      font-weight: var(--font-medium);
      color: var(--text-muted);
      white-space: nowrap;
    }

    .data-toolbar__view-toggle {
      display: inline-flex;
      padding: 2px;
      background: var(--surface-secondary);
      border: 1px solid var(--border-default);
      border-radius: var(--radius-sm);
      gap: 2px;
    }

    .data-toolbar__toggle-btn {
      display: grid;
      place-items: center;
      width: 2rem;
      height: 2rem;
      border: none;
      border-radius: var(--radius-xs);
      background: transparent;
      color: var(--text-muted);
      cursor: pointer;
      transition: all var(--transition-fast);
    }

    .data-toolbar__toggle-btn:hover {
      color: var(--text-primary);
    }

    .data-toolbar__toggle-btn.active {
      background: var(--surface-primary);
      color: var(--primary);
      box-shadow: var(--shadow-card);
    }

    @media (max-width: 640px) {
      .data-toolbar {
        flex-direction: column;
        align-items: stretch;
        gap: var(--space-3);
      }

      .data-toolbar__actions {
        width: 100%;
        justify-content: space-between;
        margin-left: 0;
      }

      .data-toolbar__view-toggle {
        display: none; /* Mobile always uses card lists */
      }
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DataToolbarComponent {
  readonly itemCount = input<number | null>(null);
  readonly itemLabelSingular = input<string>('item');
  readonly itemLabelPlural = input<string>('itens');
  readonly showViewToggle = input<boolean>(true);
  readonly viewMode = model<ViewMode>('table');
}
