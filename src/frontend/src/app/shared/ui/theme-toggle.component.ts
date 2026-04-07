import { Component, inject, input } from '@angular/core';
import { ButtonModule } from 'primeng/button';

import { ThemeService } from '../../core/theme/theme.service';

@Component({
  selector: 'itm-theme-toggle',
  standalone: true,
  imports: [ButtonModule],
  template: `
    <button
      pButton
      type="button"
      [text]="text()"
      [rounded]="rounded()"
      [severity]="severity()"
      [icon]="icon()"
      [label]="label()"
      [class]="styleClass()"
      [attr.aria-label]="buttonLabel()"
      (click)="themeService.toggleTheme()"></button>
  `
})
export class ThemeToggleComponent {
  readonly rounded = input(false);
  readonly showLabel = input(true);
  readonly text = input(false);
  readonly severity = input<'secondary' | 'contrast'>('secondary');
  readonly styleClass = input('');

  readonly themeService = inject(ThemeService);

  icon(): string {
    return this.themeService.isDark() ? 'pi pi-sun' : 'pi pi-moon';
  }

  label(): string {
    return this.showLabel() ? this.buttonLabel() : '';
  }

  buttonLabel(): string {
    return this.themeService.isDark() ? 'Light mode' : 'Dark mode';
  }
}
