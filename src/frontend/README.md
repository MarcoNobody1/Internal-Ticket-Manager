# Frontend Workspace

This folder contains the real Angular workspace baseline for Internal Ticket Manager.

## Current Structure

- `src/main.ts` — Angular bootstrap entry
- `src/app/app.config.ts` — root providers with router setup
- `src/app/app.routes.ts` — root route configuration
- `src/app/app.component.*` — application shell with router outlet
- `src/app/features/home/` — neutral home page used only to prove the shell and routing baseline
- `src/app/core/`, `src/app/features/`, `src/app/shared/` — reserved structure for future cross-cutting, feature, and shared UI code

## Current Baseline

The frontend is intentionally thin:
- standalone Angular app
- router baseline
- neutral shell/home page
- no auth flows
- no ticket screens
- no state-management complexity

## Tooling Note

This workspace was generated with Angular CLI 16.2.1. The local bootstrap environment warned that Node 22 is unsupported for that CLI version, so use a supported Node LTS version for normal install/run work.
