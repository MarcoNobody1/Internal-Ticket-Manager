import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { MessageService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { DividerModule } from 'primeng/divider';
import { InputTextModule } from 'primeng/inputtext';
import { finalize } from 'rxjs';

import { AuthRedirectReason } from '../../core/auth/auth.models';
import { AuthService } from '../../core/auth/auth.service';
import { ThemeToggleComponent } from '../../shared/ui/theme-toggle.component';

@Component({
  selector: 'itm-login-page',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    ButtonModule,
    CardModule,
    DividerModule,
    InputTextModule,
    ThemeToggleComponent
  ],
  templateUrl: './login-page.component.html',
  styleUrls: ['./login-page.component.css']
})
export class LoginPageComponent {
  isSubmitting = false;
  readonly noticeMessage = this.getNoticeMessage(this.activatedRoute.snapshot.queryParamMap.get('reason'));

  readonly loginForm = this.formBuilder.nonNullable.group({
    username: ['', [Validators.required]],
    password: ['', [Validators.required]]
  });

  constructor(
    private readonly formBuilder: FormBuilder,
    private readonly authService: AuthService,
    private readonly router: Router,
    private readonly activatedRoute: ActivatedRoute,
    private readonly messageService: MessageService
  ) {}

  submit(): void {
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    this.isSubmitting = true;

    this.authService
      .login(this.loginForm.getRawValue())
      .pipe(finalize(() => (this.isSubmitting = false)))
      .subscribe({
        next: () => {
          const returnUrl = this.activatedRoute.snapshot.queryParamMap.get('returnUrl') || '/workspace';
          void this.router.navigateByUrl(returnUrl);
        },
        error: () => {
          this.messageService.add({
            severity: 'error',
            summary: 'Login failed',
            detail: 'Check the demo credentials and try again.',
            life: 4000
          });
        }
      });
  }

  private getNoticeMessage(reason: string | null): string | null {
    switch (reason as AuthRedirectReason | null) {
      case 'authenticationRequired':
        return 'Sign in to continue to the protected workspace.';
      case 'sessionExpired':
        return 'Your session expired. Sign in again to keep working.';
      case 'signedOut':
        return 'You signed out successfully.';
      default:
        return null;
    }
  }
}
