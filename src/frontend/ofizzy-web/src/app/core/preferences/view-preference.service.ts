import { Injectable, WritableSignal, signal } from '@angular/core';

export type ViewMode = 'table' | 'cards';

export const VIEW_PREFERENCES_STORAGE_KEY = 'ofizzy_view_preferences';

@Injectable({ providedIn: 'root' })
export class ViewPreferenceService {
  private readonly memoryCache = new Map<string, WritableSignal<ViewMode>>();
  private readonly preferences: Record<string, ViewMode> = this.readStorage();

  /**
   * Returns a cached WritableSignal for the given page/scope key.
   * When updated, it automatically persists to localStorage and updates in-memory cache.
   */
  getSignal(key: string, defaultMode: ViewMode = 'table'): WritableSignal<ViewMode> {
    const existing = this.memoryCache.get(key);
    if (existing) {
      return existing;
    }

    const initial = this.preferences[key] ?? defaultMode;
    const modeSignal = signal<ViewMode>(initial);

    const originalSet = modeSignal.set.bind(modeSignal);
    const originalUpdate = modeSignal.update.bind(modeSignal);

    modeSignal.set = (value: ViewMode) => {
      originalSet(value);
      this.persist(key, value);
    };

    modeSignal.update = (updateFn: (value: ViewMode) => ViewMode) => {
      originalUpdate(updateFn);
      this.persist(key, modeSignal());
    };

    this.memoryCache.set(key, modeSignal);
    return modeSignal;
  }

  getMode(key: string, defaultMode: ViewMode = 'table'): ViewMode {
    return this.preferences[key] ?? defaultMode;
  }

  setMode(key: string, mode: ViewMode): void {
    const sig = this.memoryCache.get(key);
    if (sig) {
      sig.set(mode);
    } else {
      this.persist(key, mode);
    }
  }

  private readStorage(): Record<string, ViewMode> {
    try {
      if (typeof window !== 'undefined' && window.localStorage) {
        const raw = window.localStorage.getItem(VIEW_PREFERENCES_STORAGE_KEY);
        if (raw) {
          const parsed = JSON.parse(raw);
          if (typeof parsed === 'object' && parsed !== null) {
            return parsed;
          }
        }
      }
    } catch {
      // Ignore storage errors (e.g. private browsing, sandboxed iframe)
    }
    return {};
  }

  private persist(key: string, mode: ViewMode): void {
    if (this.preferences[key] === mode) {
      return;
    }
    this.preferences[key] = mode;
    try {
      if (typeof window !== 'undefined' && window.localStorage) {
        window.localStorage.setItem(
          VIEW_PREFERENCES_STORAGE_KEY,
          JSON.stringify(this.preferences)
        );
      }
    } catch {
      // Ignore storage errors
    }
  }
}
