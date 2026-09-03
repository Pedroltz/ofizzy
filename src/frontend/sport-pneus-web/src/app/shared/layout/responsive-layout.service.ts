import { DestroyRef, Injectable, inject, signal } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class ResponsiveLayoutService {
  private readonly query =
    typeof window === 'undefined' ? null : window.matchMedia('(max-width: 640px)');
  readonly isMobile = signal(this.query?.matches ?? false);

  constructor() {
    if (!this.query) return;
    const update = (event: MediaQueryListEvent): void => this.isMobile.set(event.matches);
    this.query.addEventListener('change', update);
    inject(DestroyRef).onDestroy(() => this.query?.removeEventListener('change', update));
  }
}
