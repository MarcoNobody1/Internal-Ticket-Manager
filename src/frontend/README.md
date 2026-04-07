# Frontend Workspace

This folder contains the real Angular workspace baseline for Internal Ticket Manager.

## Current Structure

- `src/main.ts` — Angular bootstrap entry
- `src/app/app.config.ts` — root providers for router, HTTP client, interceptor, and animations
- `src/app/core/theme/` — local theme preference service for PrimeNG light/dark mode
- `src/app/app.routes.ts` — login/public and workspace/protected route configuration
- `src/app/core/auth/` — auth service, guard, interceptor, and auth models
- `src/app/features/login/` — PrimeNG-based login page
- `src/app/features/workspace/` — protected shell with fixed left rail, responsive drawer, and toolbar actions
- `proxy.conf.json` — local Angular proxy for `/api` calls to the backend
- `src/app/core/`, `src/app/features/`, `src/app/shared/` — reserved structure for future cross-cutting, feature, and shared UI code

## Current Baseline

The frontend is intentionally modest:
- standalone Angular app
- router with public/protected auth flow
- PrimeNG 18 + PrimeIcons UI shell and feature screens
- PrimeNG Aura configured with a class-based light/dark theme switcher (`.app-dark`)
- session-based JWT persistence for the current browser tab
- local theme preference persisted in `localStorage`
- bearer token interceptor and auth guard
- protected workspace with projects, tickets, comments, and logout
- role-aware projects UI (admin write actions only)
- ticket create/edit form with project assignment, status, and priority controls
- no state-management complexity

## Tooling Note

This workspace now runs on Angular 18 + PrimeNG 18, which is compatible with the current Node 22 development environment used in this repository.

- The workspace keeps TypeScript on the Angular-supported range declared by Angular 18 tooling (`>=5.4 <5.6`).
- `baseUrl` was intentionally removed from `tsconfig.json` because the app does not use path aliases and newer TypeScript versions mark that pattern as deprecated for future releases.
- `rootDir` is explicitly set to `./src` in the root/app/spec tsconfig files to keep editor diagnostics and build output layout aligned.
- Deprecated compatibility flags such as `downlevelIteration` and legacy `moduleResolution: node` were removed in favor of modern Angular 18 settings.

## UI Stack Notes

- PrimeNG is configured in `src/app/app.config.ts` with `providePrimeNG(...)` and the Aura preset from `@primeng/themes`.
- Dark mode is enabled through PrimeNG's `darkModeSelector` option, and the app toggles the `.app-dark` class on the root element instead of relying on ad-hoc CSS overrides.
- PrimeIcons are loaded through `angular.json` so the standalone feature screens can use consistent iconography.
- Angular Material may still exist in `package.json` temporarily, but the active user-facing shell and key screens are now PrimeNG-based.

## Local Auth Demo Notes

- `ng serve` uses `proxy.conf.json`, so frontend code can call `/api/auth/login` directly during local development.
- Demo credentials come from the backend database seeding path:
  - `admin.demo / AdminDemo123!`
  - `developer.demo / DeveloperDemo123!`

## Package Management and Angular Commands

- Use **Bun** for dependency management inside `src/frontend`.
- Use **Angular CLI (`ng`)** for Angular-specific operations.

Examples:

```powershell
bun install
bun run build
bun run test -- --watch=false --browsers=ChromeHeadless
```

```powershell
ng serve
ng generate component features/example/example-page
ng test --watch=false --browsers=ChromeHeadless
```
