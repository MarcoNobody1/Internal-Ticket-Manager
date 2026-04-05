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
      import('./features/workspace/workspace-page.component').then((m) => m.WorkspacePageComponent),
    children: [
      {
        path: '',
        pathMatch: 'full',
        redirectTo: 'projects'
      },
      {
        path: 'projects',
        loadComponent: () =>
          import('./features/projects/projects-page.component').then((m) => m.ProjectsPageComponent)
      }
    ]
  },
  {
    path: '**',
    redirectTo: 'workspace'
  }
];
