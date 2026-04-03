# Internal Ticket Manager

Internal Ticket Manager is a defendable interview demo for a small internal support/development ticket platform built with **.NET 8 Web API** and **Angular**.

The repository now includes the real runtime baseline: a .NET solution with modular backend projects and an Angular workspace with a standalone shell. It is intentionally thin on purpose.

## Current Status

What exists today:
- `InternalTicketManager.sln` with `Api`, `Application`, `Domain`, and `Infrastructure` projects
- a minimal API host with Swagger in development and `GET /api/health`
- an Angular workspace in `src/frontend` with a standalone shell and router baseline
- updated documentation that reflects the runnable bootstrap stage

What is still intentionally deferred:
- JWT authentication and authorization
- EF Core setup, database models, and migrations
- ticket modules, business workflows, and frontend feature screens
- CI/CD, containers, and deployment automation

## Project Goal

Build a small but solid modular monolith that demonstrates strong fundamentals without fake enterprise complexity.

The intended MVP will eventually cover:
- JWT auth with a minimal role model (`Admin`, `Developer`)
- ticket creation, listing, filtering, and status updates
- explicit DTOs, validation, and service-layer logic
- practical Angular screens built with router, forms, and HTTP services

## Runtime Baseline Structure

```text
.
├── InternalTicketManager.sln
├── docs/
├── src/
│   ├── backend/
│   │   ├── Api/
│   │   ├── Application/
│   │   ├── Domain/
│   │   ├── Infrastructure/
│   │   └── README.md
│   └── frontend/
│       ├── src/app/
│       │   ├── core/
│       │   ├── features/
│       │   └── shared/
│       └── README.md
├── tests/
└── README.md
```

## Run Commands

### Backend API

```bash
dotnet restore InternalTicketManager.sln
dotnet run --project src/backend/Api/InternalTicketManager.Api.csproj
```

Expected baseline behavior:
- Swagger UI available in development
- `GET /api/health` returns `{ "status": "ok" }`

### Frontend Angular Shell

```bash
npm install --prefix src/frontend
npm start --prefix src/frontend
```

Expected baseline behavior:
- Angular dev server starts from `src/frontend`
- the root route renders a neutral home page through the router outlet

## Tooling Notes

- The backend projects are normalized to **.NET 8** targets, but the local machine used for bootstrap only exposed **.NET 9 template defaults**. The templates were generated with the available SDK and then aligned back to the required `net8.0` target framework.
- The local machine also has **Angular CLI 16.2.1** with **Node 22**, and the CLI warns that this Node version is unsupported. The workspace was generated successfully, but a supported Node LTS version should be used for normal day-to-day frontend install/run work.
- Per project rules, this increment did **not** run a build after the normalization changes.

## Deferred Scope

This bootstrap increment does **not** add:
- auth wiring
- ticket endpoints or business logic
- EF Core models or persistence setup
- test projects or CI/CD

That boundary is deliberate. The current goal is a clean runnable baseline, not a half-built product.

## Why This Shape

The project stays close to framework defaults where that helps clarity, then trims template noise that would be awkward to defend in an interview. That gives the repo a real starting point without pretending sample code is product functionality.
