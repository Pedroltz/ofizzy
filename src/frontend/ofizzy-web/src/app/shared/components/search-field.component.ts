import {
  ChangeDetectionStrategy,
  Component,
  forwardRef,
  input,
  model,
  output,
} from '@angular/core';
import { ControlValueAccessor, FormsModule, NG_VALUE_ACCESSOR } from '@angular/forms';

@Component({
  selector: 'app-search-field',
  imports: [FormsModule],
  template: `
    <div class="search-field" [class.focused]="isFocused">
      <i class="pi pi-search search-field__icon" aria-hidden="true"></i>
      <input
        type="search"
        class="search-field__input"
        [placeholder]="placeholder()"
        [attr.aria-label]="ariaLabel() || placeholder()"
        [value]="value()"
        (input)="onInput($event)"
        (focus)="isFocused = true"
        (blur)="onBlur()"
      />
      @if (!isFocused && !value()) {
        <kbd class="search-field__kbd" aria-hidden="true">/</kbd>
      }
      @if (value()) {
        <button
          type="button"
          class="search-field__clear"
          (click)="clear()"
          aria-label="Limpar busca"
        >
          <i class="pi pi-times"></i>
        </button>
      }
    </div>
  `,
  styles: [
    `
      .search-field {
        position: relative;
        display: flex;
        align-items: center;
        width: 100%;
        min-height: 2.5rem;
        background: var(--surface-primary);
        border: 1px solid var(--border-default);
        border-radius: var(--radius-sm);
        transition:
          border-color var(--transition-fast),
          box-shadow var(--transition-fast);
      }

      .search-field.focused {
        border-color: var(--primary);
        box-shadow: 0 0 0 3px var(--primary-soft);
      }

      .search-field__icon {
        margin-left: var(--space-3);
        color: var(--text-muted);
        font-size: var(--text-sm);
        pointer-events: none;
      }

      .search-field__input {
        flex: 1;
        min-width: 0;
        height: 100%;
        padding: 0.5rem 0.75rem;
        border: none;
        background: transparent;
        color: var(--text-primary);
        font-size: var(--text-base);
        outline: none;
        box-shadow: none;
      }

      .search-field__input::placeholder {
        color: var(--text-muted);
      }

      /* Remove browser default cancel buttons */
      .search-field__input::-webkit-search-cancel-button {
        -webkit-appearance: none;
        appearance: none;
      }

      .search-field__clear {
        display: grid;
        place-items: center;
        width: 2rem;
        height: 2rem;
        margin-right: var(--space-1);
        border: none;
        border-radius: var(--radius-xs);
        background: transparent;
        color: var(--text-muted);
        cursor: pointer;
        transition:
          background-color var(--transition-fast),
          color var(--transition-fast);
      }

      .search-field__clear:hover {
        background: var(--surface-hover);
        color: var(--text-primary);
      }

      .search-field__kbd {
        display: inline-flex;
        align-items: center;
        justify-content: center;
        min-width: 1.25rem;
        height: 1.25rem;
        padding: 0 0.35rem;
        margin-right: var(--space-3);
        border: 1px solid var(--border-default);
        border-radius: var(--radius-xs);
        background: var(--surface-secondary);
        color: var(--text-muted);
        font-size: 0.7rem;
        font-family: var(--font-mono);
        font-weight: var(--font-semibold);
        pointer-events: none;
      }
    `,
  ],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => SearchFieldComponent),
      multi: true,
    },
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SearchFieldComponent implements ControlValueAccessor {
  readonly placeholder = input<string>('Pesquisar...');
  readonly ariaLabel = input<string>('');
  readonly value = model<string>('');
  readonly searchChange = output<string>();

  isFocused = false;

  private onChange: (val: string) => void = () => {};
  private onTouched: () => void = () => {};

  writeValue(val: string | null): void {
    this.value.set(val || '');
  }

  registerOnChange(fn: (val: string) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  onInput(event: Event): void {
    const val = (event.target as HTMLInputElement).value;
    this.value.set(val);
    this.onChange(val);
    this.searchChange.emit(val);
  }

  onBlur(): void {
    this.isFocused = false;
    this.onTouched();
  }

  clear(): void {
    this.value.set('');
    this.onChange('');
    this.searchChange.emit('');
  }
}
