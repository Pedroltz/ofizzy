import { Injectable, computed, effect, inject, signal } from '@angular/core';
import { DOCUMENT } from '@angular/common';
import {
  ActiveTheme,
  THEME_STORAGE_KEY,
  ThemePreference,
} from './theme.models';

@Injectable({
  providedIn: 'root',
})
export class ThemeService {
  private readonly document = inject(DOCUMENT);

  private readonly systemIsDark = signal(this.readSystemDarkPreference());
  readonly preference = signal<ThemePreference>(this.readStoredPreference());

  readonly activeTheme = computed<ActiveTheme>(() => {
    const pref = this.preference();
    if (pref === 'system') {
      return this.systemIsDark() ? 'dark' : 'light';
    }
    return pref;
  });

  readonly isDark = computed(() => this.activeTheme() === 'dark');

  private mediaQueryList: MediaQueryList | null = null;
  private readonly mediaListener = (e: MediaQueryListEvent) => {
    this.systemIsDark.set(e.matches);
  };

  constructor() {
    this.initSystemListener();

    // Effect to apply data-theme attribute on document root whenever activeTheme changes
    effect(() => {
      const theme = this.activeTheme();
      this.applyTheme(theme);
    });
  }

  setPreference(preference: ThemePreference): void {
    this.preference.set(preference);
    try {
      if (typeof window !== 'undefined' && window.localStorage) {
        window.localStorage.setItem(THEME_STORAGE_KEY, preference);
      }
    } catch {
      // Ignore storage errors (e.g. private browsing or sandboxed iframe)
    }
  }

  toggle(): void {
    const current = this.activeTheme();
    this.setPreference(current === 'dark' ? 'light' : 'dark');
  }

  private applyTheme(theme: ActiveTheme): void {
    if (!this.document?.documentElement) return;
    this.document.documentElement.setAttribute('data-theme', theme);
    this.document.documentElement.classList.toggle('dark', theme === 'dark');
  }

  private readStoredPreference(): ThemePreference {
    try {
      if (typeof window !== 'undefined' && window.localStorage) {
        const stored = window.localStorage.getItem(THEME_STORAGE_KEY);
        if (stored === 'light' || stored === 'dark' || stored === 'system') {
          return stored;
        }
      }
    } catch {
      // Fall back to default
    }
    return 'system';
  }

  private readSystemDarkPreference(): boolean {
    if (typeof window !== 'undefined' && window.matchMedia) {
      return window.matchMedia('(prefers-color-scheme: dark)').matches;
    }
    return false;
  }

  private initSystemListener(): void {
    if (typeof window !== 'undefined' && window.matchMedia) {
      this.mediaQueryList = window.matchMedia('(prefers-color-scheme: dark)');
      this.systemIsDark.set(this.mediaQueryList.matches);
      if (this.mediaQueryList.addEventListener) {
        this.mediaQueryList.addEventListener('change', this.mediaListener);
      } else if ('addListener' in this.mediaQueryList) {
        // Fallback for older browsers
        (this.mediaQueryList as { addListener: (cb: (e: MediaQueryListEvent) => void) => void }).addListener(this.mediaListener);
      }
    }
  }
}
