import { TestBed } from '@angular/core/testing';

import { THEME_DARK_CLASS, THEME_STORAGE_KEY, ThemeService } from './theme.service';

describe('ThemeService', () => {
  let themeService: ThemeService;

  beforeEach(() => {
    localStorage.clear();
    document.documentElement.classList.remove(THEME_DARK_CLASS);
    document.documentElement.style.colorScheme = 'light';

    TestBed.configureTestingModule({});
    themeService = TestBed.inject(ThemeService);
  });

  afterEach(() => {
    localStorage.clear();
    document.documentElement.classList.remove(THEME_DARK_CLASS);
    document.documentElement.style.colorScheme = 'light';
  });

  it('defaults to dark mode and enables the dark selector', () => {
    expect(themeService.activeTheme()).toBe('dark');
    expect(document.documentElement.classList.contains(THEME_DARK_CLASS)).toBeTrue();
    expect(document.documentElement.style.colorScheme).toBe('dark');
  });

  it('restores the stored dark preference during startup', () => {
    TestBed.resetTestingModule();
    localStorage.setItem(THEME_STORAGE_KEY, 'dark');

    TestBed.configureTestingModule({});
    themeService = TestBed.inject(ThemeService);

    expect(themeService.activeTheme()).toBe('dark');
    expect(document.documentElement.classList.contains(THEME_DARK_CLASS)).toBeTrue();
    expect(document.documentElement.style.colorScheme).toBe('dark');
  });

  it('persists and applies the toggled theme', () => {
    themeService.toggleTheme();

    expect(themeService.activeTheme()).toBe('light');
    expect(localStorage.getItem(THEME_STORAGE_KEY)).toBe('light');
    expect(document.documentElement.classList.contains(THEME_DARK_CLASS)).toBeFalse();
  });
});
