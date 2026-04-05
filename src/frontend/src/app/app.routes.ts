import { Routes } from '@angular/router';

import { adminGuard, authGuard, publicOnlyGuard } from './core/auth/auth.guard';

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
        path: 'account',
        loadComponent: () => import('./features/account/account-page.component').then((m) => m.AccountPageComponent)
      },
      {
        path: 'access-denied',
        loadComponent: () =>
          import('./features/access-denied/access-denied-page.component').then((m) => m.AccessDeniedPageComponent)
      },
      {
        path: 'projects',
        loadComponent: () =>
          import('./features/projects/projects-page.component').then((m) => m.ProjectsPageComponent)
      },
      {
        path: 'projects/:projectId',
        loadComponent: () =>
          import('./features/projects/project-details-page.component').then((m) => m.ProjectDetailsPageComponent)
      },
      {
        path: 'tickets',
        loadComponent: () =>
          import('./features/tickets/tickets-page.component').then((m) => m.TicketsPageComponent)
      },
      {
        path: 'tickets/:ticketId',
        loadComponent: () =>
          import('./features/tickets/ticket-details-page.component').then((m) => m.TicketDetailsPageComponent)
      },
      {
        path: 'users',
        canActivate: [adminGuard],
        loadComponent: () => import('./features/users/users-page.component').then((m) => m.UsersPageComponent)
      }
    ]
  },
  {
    path: '**',
    redirectTo: 'workspace'
  }
];
