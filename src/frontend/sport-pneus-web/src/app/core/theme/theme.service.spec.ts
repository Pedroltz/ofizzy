import { TestBed } from '@angular/core/testing';
import { DOCUMENT } from '@angular/common';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { ThemeService } from './theme.service';
import { THEME_STORAGE_KEY } from './theme.models';

describe('ThemeService', () => {
  let service: ThemeService;
  let doc: Document;
  let matchMediaListeners: Array<(e: MediaQueryListEvent) => void> = [];
  let matchesDark = false;

  beforeEach(() => {
    localStorage.clear();
    matchMediaListeners = [];
    matchesDark = false;

    // Mock matchMedia
    vi.stubGlobal('matchMedia', vi.fn().mockImplementation((query: string) => ({
      matches: matchesDark,
      media: query,
      onchange: null,
      addEventListener: vi.fn((event: string, listener: (e: MediaQueryListEvent) => void) => {
        if (event === 'change') {
          matchMediaListeners.push(listener);
        }
      }),
      removeEventListener: vi.fn(),
      dispatchEvent: vi.fn(),
    })));

    TestBed.configureTestingModule({
      providers: [ThemeService],
    });

    doc = TestBed.inject(DOCUMENT);
    service = TestBed.inject(ThemeService);
  });

  it('should default to system preference and resolve to light when system is light', () => {
    expect(service.preference()).toBe('system');
    expect(service.activeTheme()).toBe('light');
    expect(service.isDark()).toBe(false);
  });

  it('should resolve to dark when system is dark and preference is system', () => {
    matchesDark = true;
    TestBed.resetTestingModule();
    TestBed.configureTestingModule({ providers: [ThemeService] });
    const darkService = TestBed.inject(ThemeService);

    expect(darkService.preference()).toBe('system');
    expect(darkService.activeTheme()).toBe('dark');
    expect(darkService.isDark()).toBe(true);
  });

  it('should allow manually setting light preference and persisting to localStorage', () => {
    service.setPreference('light');

    expect(service.preference()).toBe('light');
    expect(service.activeTheme()).toBe('light');
    expect(localStorage.getItem(THEME_STORAGE_KEY)).toBe('light');
  });

  it('should allow manually setting dark preference and persisting to localStorage', () => {
    service.setPreference('dark');

    expect(service.preference()).toBe('dark');
    expect(service.activeTheme()).toBe('dark');
    expect(localStorage.getItem(THEME_STORAGE_KEY)).toBe('dark');
  });

  it('should read persisted preference from localStorage on initialization', () => {
    localStorage.setItem(THEME_STORAGE_KEY, 'dark');

    TestBed.resetTestingModule();
    TestBed.configureTestingModule({ providers: [ThemeService] });
    const restoredService = TestBed.inject(ThemeService);

    expect(restoredService.preference()).toBe('dark');
    expect(restoredService.activeTheme()).toBe('dark');
  });

  it('should update activeTheme when system preference changes at runtime in system mode', () => {
    expect(service.preference()).toBe('system');
    expect(service.activeTheme()).toBe('light');

    // Simulate OS switching to dark theme
    matchMediaListeners.forEach((listener) =>
      listener({ matches: true } as MediaQueryListEvent)
    );

    expect(service.activeTheme()).toBe('dark');
    expect(service.isDark()).toBe(true);

    // Simulate OS switching back to light theme
    matchMediaListeners.forEach((listener) =>
      listener({ matches: false } as MediaQueryListEvent)
    );

    expect(service.activeTheme()).toBe('light');
    expect(service.isDark()).toBe(false);
  });

  it('should NOT change activeTheme on system change if explicit light/dark preference is set', () => {
    service.setPreference('light');
    expect(service.activeTheme()).toBe('light');

    // OS changes to dark
    matchMediaListeners.forEach((listener) =>
      listener({ matches: true } as MediaQueryListEvent)
    );

    expect(service.activeTheme()).toBe('light');
  });

  it('should toggle between light and dark', () => {
    service.setPreference('light');
    expect(service.activeTheme()).toBe('light');

    service.toggle();
    expect(service.preference()).toBe('dark');
    expect(service.activeTheme()).toBe('dark');

    service.toggle();
    expect(service.preference()).toBe('light');
    expect(service.activeTheme()).toBe('light');
  });

  it('should update DOM documentElement with data-theme and dark class', () => {
    TestBed.flushEffects();

    service.setPreference('dark');
    TestBed.flushEffects();

    expect(doc.documentElement.getAttribute('data-theme')).toBe('dark');
    expect(doc.documentElement.classList.contains('dark')).toBe(true);

    service.setPreference('light');
    TestBed.flushEffects();

    expect(doc.documentElement.getAttribute('data-theme')).toBe('light');
    expect(doc.documentElement.classList.contains('dark')).toBe(false);
  });
});
