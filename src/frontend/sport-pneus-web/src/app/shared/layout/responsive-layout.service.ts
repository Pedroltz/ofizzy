import { DestroyRef, Injectable, inject, signal } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class ResponsiveLayoutService {
  private readonly query =
    typeof window === 'undefined' ? null : window.matchMedia('(max-width: 640px)');
  private readonly tabletQuery =
    typeof window === 'undefined' ? null : window.matchMedia('(max-width: 900px)');
  readonly isMobile = signal(this.query?.matches ?? false);
  readonly isTabletOrSmaller = signal(this.tabletQuery?.matches ?? false);

  constructor() {
    if (!this.query) return;
    const update = (event: MediaQueryListEvent): void => this.isMobile.set(event.matches);
    const updateTablet = (event: MediaQueryListEvent): void =>
      this.isTabletOrSmaller.set(event.matches);
    this.query.addEventListener('change', update);
    this.tabletQuery?.addEventListener('change', updateTablet);
    inject(DestroyRef).onDestroy(() => {
      this.query?.removeEventListener('change', update);
      this.tabletQuery?.removeEventListener('change', updateTablet);
    });
  }
}
