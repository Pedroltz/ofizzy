import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-data-table-wrapper',
  template: `
    <div class="data-table-wrapper surface-card">
      <div class="data-table-wrapper__scroll">
        <ng-content />
      </div>
      <div class="data-table-wrapper__footer">
        <ng-content select="[table-footer]" />
      </div>
    </div>
  `,
  styles: [
    `
      .data-table-wrapper {
        display: flex;
        flex-direction: column;
        background: var(--surface-primary);
        border: 1px solid var(--border-subtle);
        border-radius: var(--radius-md);
        box-shadow: var(--shadow-card);
        overflow: hidden;
        flex: 1 1 auto;
        min-height: 0;
      }

      .data-table-wrapper__scroll {
        width: 100%;
        overflow-x: auto;
        overflow-y: auto;
        flex: 1 1 auto;
        min-height: 0;
        -webkit-overflow-scrolling: touch;
      }

      .data-table-wrapper__footer {
        border-top: 1px solid var(--border-subtle);
        background: var(--surface-secondary);
        flex-shrink: 0;
      }

      .data-table-wrapper__footer:empty {
        display: none;
      }
    `,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DataTableWrapperComponent {}
