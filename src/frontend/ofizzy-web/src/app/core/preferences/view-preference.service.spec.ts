import { TestBed } from '@angular/core/testing';
import { beforeEach, describe, expect, it } from 'vitest';
import {
  VIEW_PREFERENCES_STORAGE_KEY,
  ViewPreferenceService,
} from './view-preference.service';

describe('ViewPreferenceService', () => {
  let service: ViewPreferenceService;
  let mockStorage: Record<string, string> = {};

  const mockLocalStorage = {
    getItem: (key: string) => mockStorage[key] ?? null,
    setItem: (key: string, value: string) => {
      mockStorage[key] = String(value);
    },
    removeItem: (key: string) => {
      delete mockStorage[key];
    },
    clear: () => {
      mockStorage = {};
    },
    length: 0,
    key: () => null,
  };

  beforeEach(() => {
    mockStorage = {};
    Object.defineProperty(window, 'localStorage', {
      value: mockLocalStorage,
      writable: true,
      configurable: true,
    });
    Object.defineProperty(globalThis, 'localStorage', {
      value: mockLocalStorage,
      writable: true,
      configurable: true,
    });
    mockLocalStorage.clear();

    TestBed.configureTestingModule({
      providers: [ViewPreferenceService],
    });
    service = TestBed.inject(ViewPreferenceService);
  });

  it('should return defaultMode when no preference is saved', () => {
    const customersMode = service.getSignal('customers', 'table');
    expect(customersMode()).toBe('table');
  });

  it('should read persisted preference from localStorage', () => {
    mockStorage[VIEW_PREFERENCES_STORAGE_KEY] = JSON.stringify({
      customers: 'cards',
      vehicles: 'table',
    });

    TestBed.resetTestingModule();
    TestBed.configureTestingModule({ providers: [ViewPreferenceService] });
    const freshService = TestBed.inject(ViewPreferenceService);

    expect(freshService.getSignal('customers')()).toBe('cards');
    expect(freshService.getSignal('vehicles')()).toBe('table');
  });

  it('should persist to localStorage when signal is changed via set()', () => {
    const modeSignal = service.getSignal('work-orders', 'table');
    expect(modeSignal()).toBe('table');

    modeSignal.set('cards');
    expect(modeSignal()).toBe('cards');

    const stored = JSON.parse(mockStorage[VIEW_PREFERENCES_STORAGE_KEY]);
    expect(stored['work-orders']).toBe('cards');
  });

  it('should persist to localStorage when signal is changed via update()', () => {
    const modeSignal = service.getSignal('customers', 'table');
    modeSignal.update((curr) => (curr === 'table' ? 'cards' : 'table'));

    expect(modeSignal()).toBe('cards');
    const stored = JSON.parse(mockStorage[VIEW_PREFERENCES_STORAGE_KEY]);
    expect(stored['customers']).toBe('cards');
  });

  it('should return the same cached signal instance on subsequent calls', () => {
    const sig1 = service.getSignal('customers');
    const sig2 = service.getSignal('customers');
    expect(sig1).toBe(sig2);
  });

  it('should handle corrupt localStorage JSON safely', () => {
    mockStorage[VIEW_PREFERENCES_STORAGE_KEY] = 'not-a-valid-json';

    TestBed.resetTestingModule();
    TestBed.configureTestingModule({ providers: [ViewPreferenceService] });
    const freshService = TestBed.inject(ViewPreferenceService);

    expect(freshService.getSignal('customers', 'table')()).toBe('table');
  });
});
