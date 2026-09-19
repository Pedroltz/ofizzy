import { ChangeDetectionStrategy, Component, input } from '@angular/core';

@Component({
  selector: 'app-section-card',
  template: `
    <article
      class="surface-card section-card"
      [class.p-none]="padding() === 'none'"
      [class.p-sm]="padding() === 'sm'"
      [class.p-md]="padding() === 'md'"
      [class.p-lg]="padding() === 'lg'"
    >
      @if (title() || subtitle()) {
        <header class="section-card__header">
          <div class="section-card__title-group">
            @if (icon()) {
              <div class="section-card__icon" aria-hidden="true">
                <i [class]="icon()"></i>
              </div>
            }
            <div>
              @if (title()) {
                <h3 class="section-card__title">{{ title() }}</h3>
              }
              @if (subtitle()) {
                <p class="section-card__subtitle">{{ subtitle() }}</p>
              }
            </div>
          </div>
          <div class="section-card__actions">
            <ng-content select="[card-actions]" />
          </div>
        </header>
      }

      <div class="section-card__body">
        <ng-content />
      </div>

      <ng-content select="[card-footer]" />
    </article>
  `,
  styles: [
    `
      .section-card {
        display: flex;
        flex-direction: column;
        position: relative;
        background: var(--surface-primary);
        border: 1px solid var(--border-subtle);
        border-radius: var(--radius-md);
        box-shadow: var(--shadow-card);
        overflow: hidden;
        transition: border-color var(--transition-fast);
      }

      .section-card.p-none .section-card__body {
        padding: 0;
      }
      .section-card.p-sm .section-card__body {
        padding: var(--space-3);
      }
      .section-card.p-md .section-card__body {
        padding: var(--space-4) var(--space-5);
      }
      .section-card.p-lg .section-card__body {
        padding: var(--space-6);
      }

      .section-card__header {
        display: flex;
        align-items: center;
        justify-content: space-between;
        gap: var(--space-3);
        padding: var(--space-4) var(--space-5);
        border-bottom: 1px solid var(--border-subtle);
        background: var(--surface-secondary);
      }

      .section-card__title-group {
        display: flex;
        align-items: center;
        gap: var(--space-3);
      }

      .section-card__icon {
        width: 2rem;
        height: 2rem;
        display: grid;
        place-items: center;
        border-radius: var(--radius-sm);
        background: var(--primary-soft);
        color: var(--primary);
        font-size: var(--text-sm);
      }

      .section-card__title {
        margin: 0;
        font-size: var(--text-base);
        font-weight: var(--font-semibold);
        color: var(--text-primary);
        line-height: var(--leading-tight);
      }

      .section-card__subtitle {
        margin: 0.15rem 0 0;
        font-size: var(--text-xs);
        color: var(--text-muted);
      }

      .section-card__actions {
        display: flex;
        align-items: center;
        gap: var(--space-2);
      }

      .section-card__body {
        flex: 1 1 auto;
      }
    `,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SectionCardComponent {
  readonly title = input<string>('');
  readonly subtitle = input<string>('');
  readonly icon = input<string>('');
  readonly padding = input<'none' | 'sm' | 'md' | 'lg'>('md');
}
