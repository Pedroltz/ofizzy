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
      @if (actionLabel()) {
        <button type="button" class="primary-button" (click)="action.emit()">
          @if (actionIcon()) {
            <i [class]="actionIcon()"></i>
          }
          <span>{{ actionLabel() }}</span>
        </button>
      }
    </header>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PageHeaderComponent {
  readonly eyebrow = input<string>('SISTEMA');
  readonly title = input.required<string>();
  readonly description = input.required<string>();
  readonly actionLabel = input<string>('');
  readonly actionIcon = input<string>('pi pi-plus');
  readonly action = output<void>();
}
