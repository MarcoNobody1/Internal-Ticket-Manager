import { DOCUMENT } from '@angular/common';
import { Injectable, computed, inject, signal } from '@angular/core';

export type ThemePreference = 'light' | 'dark';

export const THEME_STORAGE_KEY = 'itm-theme-preference';
export const THEME_DARK_CLASS = 'app-dark';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private readonly document = inject(DOCUMENT);

  readonly activeTheme = signal<ThemePreference>('dark');
  readonly isDark = computed(() => this.activeTheme() === 'dark');

  constructor() {
    this.initializeTheme();
  }

  toggleTheme(): void {
    this.setTheme(this.isDark() ? 'light' : 'dark');
  }

  setTheme(theme: ThemePreference): void {
    this.activeTheme.set(theme);
    this.applyTheme(theme);
    this.persistTheme(theme);
  }

  private initializeTheme(): void {
    const storedTheme = this.readStoredTheme();

    this.activeTheme.set(storedTheme);
    this.applyTheme(storedTheme);
  }

  private applyTheme(theme: ThemePreference): void {
    const rootElement = this.document.documentElement;

    rootElement.classList.toggle(THEME_DARK_CLASS, theme === 'dark');
    rootElement.style.colorScheme = theme;
  }

  private persistTheme(theme: ThemePreference): void {
    localStorage.setItem(THEME_STORAGE_KEY, theme);
  }

  private readStoredTheme(): ThemePreference {
    const storedTheme = localStorage.getItem(THEME_STORAGE_KEY);

    return storedTheme === 'light' ? 'light' : 'dark';
  }
}
