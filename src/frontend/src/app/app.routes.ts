import { Routes } from '@angular/router';

import { authGuard, publicOnlyGuard } from './core/auth/auth.guard';

export const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    redirectTo: 'workspace'
  },
  {
    path: 'login',
    canActivate: [publicOnlyGuard],
    loadComponent: () =>
      import('./features/login/login-page.component').then((m) => m.LoginPageComponent)
  },
  {
    path: 'workspace',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/workspace/workspace-page.component').then((m) => m.WorkspacePageComponent)
  },
  {
    path: '**',
    redirectTo: 'workspace'
  }
];
