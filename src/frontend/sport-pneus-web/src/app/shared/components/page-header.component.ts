import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';

@Component({ selector: 'app-page-header', template: `<header class="page-header"><div><p class="eyebrow">{{ eyebrow() }}</p><h1>{{ title() }}</h1><p>{{ description() }}</p></div><button type="button" class="primary-button" (click)="action.emit()">{{ actionLabel() }}</button></header>`, changeDetection: ChangeDetectionStrategy.OnPush })
export class PageHeaderComponent { readonly eyebrow = input('CADASTROS'); readonly title = input.required<string>(); readonly description = input.required<string>(); readonly actionLabel = input.required<string>(); readonly action = output<void>(); }
