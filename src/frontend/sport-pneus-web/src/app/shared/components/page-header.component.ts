import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';

@Component({
  selector: 'app-page-header',
  template: `
    <header class="page-header">
      <div class="page-header-text">
        @if (eyebrow()) {
          <p class="eyebrow">{{ eyebrow() }}</p>
        }
        <h1>{{ title() }}</h1>
        <p class="page-description">{{ description() }}</p>
      </div>
      <div class="page-header-actions">
        <ng-content />
        @if (actionLabel()) {
          <button type="button" class="primary-button" (click)="action.emit()">
            @if (actionIcon()) {
              <i [class]="actionIcon()"></i>
            }
            <span>{{ actionLabel() }}</span>
          </button>
        }
      </div>
    </header>
  `,
  styles: [`
    .page-header-actions {
      display: flex;
      align-items: center;
      gap: var(--space-3);
      flex-wrap: wrap;
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PageHeaderComponent {
  readonly eyebrow = input<string>('');
  readonly title = input.required<string>();
  readonly description = input.required<string>();
  readonly actionLabel = input<string>('');
  readonly actionIcon = input<string>('pi pi-plus');
  readonly action = output<void>();
}
