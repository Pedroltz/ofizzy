import { TestBed } from '@angular/core/testing';
import { ResponsiveLayoutService } from './responsive-layout.service';

describe('ResponsiveLayoutService', () => {
  it('reports a mobile viewport', () => {
    const original = window.matchMedia;
    Object.defineProperty(window, 'matchMedia', {
      configurable: true,
      value: () => ({
        matches: true,
        addEventListener: () => undefined,
        removeEventListener: () => undefined,
      }),
    });
    TestBed.resetTestingModule();
    const service = TestBed.inject(ResponsiveLayoutService);
    expect(service.isMobile()).toBe(true);
    Object.defineProperty(window, 'matchMedia', { configurable: true, value: original });
  });
});
