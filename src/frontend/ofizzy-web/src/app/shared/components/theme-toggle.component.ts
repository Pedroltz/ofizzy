import {
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  HostListener,
  inject,
  input,
  signal,
} from '@angular/core';
import { ThemeService } from '../../core/theme/theme.service';
import { THEME_OPTIONS, ThemePreference } from '../../core/theme/theme.models';

@Component({
  selector: 'app-theme-toggle',
  template: `
    @if (mode() === 'switch') {
      <button
        type="button"
        class="theme-switch"
        (click)="toggle()"
        [attr.aria-label]="activeTheme() === 'dark' ? 'Ativar tema claro' : 'Ativar tema escuro'"
        [attr.aria-checked]="activeTheme() === 'dark'"
        role="switch"
        [title]="
          activeTheme() === 'dark'
            ? 'Tema escuro ativo (clique para alternar)'
            : 'Tema claro ativo (clique para alternar)'
        "
      >
        <span
          class="theme-switch__track"
          [class.theme-switch__track--dark]="activeTheme() === 'dark'"
        >
          <span class="theme-switch__icon sun" aria-hidden="true"><i class="pi pi-sun"></i></span>
          <span class="theme-switch__thumb">
            <i
              [class]="activeTheme() === 'dark' ? 'pi pi-moon' : 'pi pi-sun'"
              aria-hidden="true"
            ></i>
          </span>
          <span class="theme-switch__icon moon" aria-hidden="true"><i class="pi pi-moon"></i></span>
        </span>
      </button>
    } @else {
      <div class="theme-toggle">
        <button
          type="button"
          class="theme-toggle__btn"
          (click)="isOpen.set(!isOpen())"
          [attr.aria-expanded]="isOpen()"
          aria-haspopup="menu"
          aria-label="Alternar tema de aparência"
          [title]="'Tema: ' + currentLabel()"
        >
          <i [class]="currentIcon()" aria-hidden="true"></i>
          <span class="theme-toggle__label">{{ currentLabel() }}</span>
          <i
            class="pi pi-chevron-down theme-toggle__chevron"
            [class.rotated]="isOpen()"
            aria-hidden="true"
          ></i>
        </button>

        @if (isOpen()) {
          <div
            class="theme-toggle__menu surface-card"
            [class.theme-toggle__menu--below]="placement() === 'bottom'"
            role="menu"
            aria-label="Opções de tema"
          >
            @for (opt of options; track opt.value) {
              <button
                type="button"
                class="theme-toggle__option"
                [class.active]="preference() === opt.value"
                role="menuitemradio"
                [attr.aria-checked]="preference() === opt.value"
                (click)="selectTheme(opt.value)"
              >
                <i [class]="opt.icon" aria-hidden="true"></i>
                <span>{{ opt.label }}</span>
                @if (preference() === opt.value) {
                  <i class="pi pi-check theme-toggle__check" aria-hidden="true"></i>
                }
              </button>
            }
          </div>
        }
      </div>
    }
  `,
  styles: [
    `
      /* Switch Style (Header) */
      .theme-switch {
        display: inline-flex;
        align-items: center;
        justify-content: center;
        min-height: 44px;
        min-width: 44px;
        padding: 0;
        background: transparent;
        border: none;
        cursor: pointer;
        color: inherit;
      }

      .theme-switch:focus-visible {
        outline: none;
      }

      .theme-switch:focus-visible .theme-switch__track {
        box-shadow: var(--focus-ring);
      }

      .theme-switch__track {
        position: relative;
        display: flex;
        align-items: center;
        justify-content: space-between;
        width: 3.25rem;
        height: 1.85rem;
        padding: 0 0.4rem;
        background: var(--surface-hover);
        border: 1px solid var(--border-strong);
        border-radius: var(--radius-full);
        transition:
          background-color var(--transition-fast),
          border-color var(--transition-fast);
        user-select: none;
      }

      .theme-switch:hover .theme-switch__track {
        border-color: var(--primary);
      }

      .theme-switch__track--dark {
        background: var(--surface-secondary);
        border-color: var(--primary);
      }

      .theme-switch__icon {
        display: flex;
        align-items: center;
        justify-content: center;
        font-size: 0.7rem;
        z-index: 1;
        pointer-events: none;
        transition: color var(--transition-fast);
      }

      .theme-switch__icon.sun {
        color: var(--warning);
      }

      .theme-switch__icon.moon {
        color: var(--text-muted);
      }

      .theme-switch__track--dark .theme-switch__icon.moon {
        color: var(--primary);
      }

      .theme-switch__thumb {
        position: absolute;
        left: 2px;
        top: 2px;
        width: 1.5rem;
        height: 1.5rem;
        border-radius: var(--radius-full);
        background: var(--surface-primary);
        border: 1px solid var(--border-default);
        box-shadow: 0 1px 3px rgba(0, 0, 0, 0.2);
        display: flex;
        align-items: center;
        justify-content: center;
        font-size: 0.72rem;
        color: var(--primary);
        transition:
          transform var(--transition-fast),
          background-color var(--transition-fast),
          border-color var(--transition-fast);
        z-index: 2;
      }

      .theme-switch__track--dark .theme-switch__thumb {
        transform: translateX(1.4rem);
        background: var(--primary);
        color: var(--primary-text);
        border-color: var(--primary);
      }

      /* Dropdown Style (Auth / Settings) */
      .theme-toggle {
        position: relative;
        display: inline-block;
      }

      .theme-toggle__btn {
        display: inline-flex;
        align-items: center;
        gap: var(--space-2);
        min-height: 44px;
        padding: 0.4rem 0.65rem;
        border: 1px solid var(--border-default);
        border-radius: var(--radius-sm);
        background: var(--surface-primary);
        color: var(--text-primary);
        font-size: var(--text-xs);
        font-weight: var(--font-medium);
        cursor: pointer;
        transition: all var(--transition-fast);
      }

      .theme-toggle__btn:hover {
        background: var(--surface-hover);
        border-color: var(--border-strong);
      }

      .theme-toggle__btn:focus-visible {
        outline: none;
        box-shadow: var(--focus-ring);
      }

      .theme-toggle__label {
        font-size: var(--text-xs);
      }

      .theme-toggle__chevron {
        font-size: 0.65rem;
        color: var(--text-muted);
        transition: transform var(--transition-fast);
      }

      .theme-toggle__chevron.rotated {
        transform: rotate(180deg);
      }

      .theme-toggle__menu {
        position: absolute;
        bottom: calc(100% + var(--space-1));
        left: 0;
        z-index: 50;
        min-width: 140px;
        padding: var(--space-1);
        background: var(--surface-primary);
        border: 1px solid var(--border-subtle);
        border-radius: var(--radius-md);
        box-shadow: var(--shadow-dropdown);
        display: flex;
        flex-direction: column;
        gap: 2px;
        animation: menuFadeIn var(--transition-fast) ease-out;
      }

      @keyframes menuFadeIn {
        from {
          opacity: 0;
          transform: translateY(4px);
        }
        to {
          opacity: 1;
          transform: translateY(0);
        }
      }

      .theme-toggle__option {
        min-height: 44px;
        display: flex;
        align-items: center;
        gap: var(--space-2);
        width: 100%;
        padding: 0.45rem 0.65rem;
        border: none;
        border-radius: var(--radius-xs);
        background: transparent;
        color: var(--text-secondary);
        font-size: var(--text-xs);
        font-weight: var(--font-medium);
        text-align: left;
        cursor: pointer;
        transition:
          background-color var(--transition-fast),
          color var(--transition-fast);
      }

      .theme-toggle__option:hover {
        background: var(--surface-hover);
        color: var(--text-primary);
      }

      .theme-toggle__option.active {
        background: var(--primary-soft);
        color: var(--primary);
        font-weight: var(--font-semibold);
      }

      .theme-toggle__check {
        margin-left: auto;
        font-size: 0.75rem;
      }

      .theme-toggle__menu--below {
        bottom: auto;
        top: calc(100% + var(--space-1));
        left: auto;
        right: 0;
      }
    `,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ThemeToggleComponent {
  readonly mode = input<'dropdown' | 'switch'>('dropdown');
  readonly placement = input<'top' | 'bottom'>('top');
  private readonly themeService = inject(ThemeService);
  private readonly elementRef = inject(ElementRef);

  readonly isOpen = signal(false);
  readonly options = THEME_OPTIONS;

  readonly preference = this.themeService.preference;
  readonly activeTheme = this.themeService.activeTheme;

  currentLabel(): string {
    const pref = this.preference();
    const opt = this.options.find((o) => o.value === pref);
    return opt ? opt.label : 'Sistema';
  }

  currentIcon(): string {
    const pref = this.preference();
    if (pref === 'system') {
      return this.activeTheme() === 'dark' ? 'pi pi-moon' : 'pi pi-desktop';
    }
    return pref === 'dark' ? 'pi pi-moon' : 'pi pi-sun';
  }

  toggle(): void {
    this.themeService.toggle();
  }

  selectTheme(theme: ThemePreference): void {
    this.themeService.setPreference(theme);
    this.isOpen.set(false);
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    if (!this.elementRef.nativeElement.contains(event.target)) {
      this.isOpen.set(false);
    }
  }

  @HostListener('document:keydown.escape')
  onEscape(): void {
    this.isOpen.set(false);
  }
}
