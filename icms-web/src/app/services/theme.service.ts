import { Injectable, effect, signal } from '@angular/core';

export type Theme = 'light' | 'dark';

const STORAGE_KEY = 'icms-theme';
const HTML_CLASS = 'dark';

/**
 * Owns the app light/dark theme. Persists the choice in localStorage and
 * toggles the `dark` class on <html> so Tailadmin's `dark:` Tailwind variants
 * (configured via `@custom-variant dark`) apply. Matches Tailadmin's class-based
 * dark-mode behaviour.
 */
@Injectable({ providedIn: 'root' })
export class ThemeService {
  readonly theme = signal<Theme>('light');

  constructor() {
    const stored = localStorage.getItem(STORAGE_KEY);
    if (stored === 'dark' || stored === 'light') {
      this.theme.set(stored);
    } else if (window.matchMedia?.('(prefers-color-scheme: dark)').matches) {
      this.theme.set('dark');
    }

    effect(() => {
      const value = this.theme();
      document.documentElement.classList.toggle(HTML_CLASS, value === 'dark');
      document.documentElement.style.colorScheme = value;
      localStorage.setItem(STORAGE_KEY, value);
    });
  }

  toggle(): void {
    this.theme.set(this.theme() === 'dark' ? 'light' : 'dark');
  }

  setTheme(theme: Theme): void {
    this.theme.set(theme);
  }
}