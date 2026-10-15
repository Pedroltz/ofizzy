export type ThemePreference = 'light' | 'dark' | 'system';
export type ActiveTheme = 'light' | 'dark';

export interface ThemeOption {
  readonly value: ThemePreference;
  readonly label: string;
  readonly icon: string;
}

export const THEME_OPTIONS: readonly ThemeOption[] = [
  { value: 'system', label: 'Sistema', icon: 'pi pi-desktop' },
  { value: 'light', label: 'Claro', icon: 'pi pi-sun' },
  { value: 'dark', label: 'Escuro', icon: 'pi pi-moon' },
] as const;

export const THEME_STORAGE_KEY = 'workshop-theme';
