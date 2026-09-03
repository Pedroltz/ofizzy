import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';

@Component({
  selector: 'app-empty-state',
  template: `
    <div class="empty-state surface-card">
      <div class="empty-state__icon" aria-hidden="true">
        <i [class]="icon()"></i>
      </div>
      <h3 class="empty-state__title">{{ title() }}</h3>
      <p class="empty-state__description">{{ description() }}</p>

      <div class="empty-state__actions">
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
    </div>
  `,
  styles: [`
    .empty-state {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      text-align: center;
      padding: var(--space-10) var(--space-6);
      background: var(--surface-primary);
      border: 1px dashed var(--border-strong);
      border-radius: var(--radius-lg);
    }

    .empty-state__icon {
      width: 3.5rem;
      height: 3.5rem;
      display: grid;
      place-items: center;
      margin-bottom: var(--space-4);
      border-radius: var(--radius-full);
      background: var(--surface-secondary);
      border: 1px solid var(--border-subtle);
      color: var(--text-muted);
      font-size: 1.5rem;
    }

    .empty-state__title {
      margin: 0 0 var(--space-2);
      font-size: var(--text-lg);
      font-weight: var(--font-semibold);
      color: var(--text-primary);
    }

    .empty-state__description {
      max-width: 440px;
      margin: 0 0 var(--space-5);
      font-size: var(--text-base);
      color: var(--text-secondary);
      line-height: var(--leading-relaxed);
    }

    .empty-state__actions {
      display: flex;
      align-items: center;
      gap: var(--space-3);
      flex-wrap: wrap;
      justify-content: center;
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class EmptyStateComponent {
  readonly icon = input<string>('pi pi-inbox');
  readonly title = input.required<string>();
  readonly description = input.required<string>();
  readonly actionLabel = input<string>('');
  readonly actionIcon = input<string>('pi pi-plus');
  readonly action = output<void>();
}
