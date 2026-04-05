# Backend Workspace

This folder now contains the real backend bootstrap for the project.

## Projects

- `Api/` — ASP.NET Core entry point, controller wiring, Swagger in development, and the bootstrap health endpoint
- `Application/` — application-layer project reserved for future use cases and orchestration
- `Domain/` — domain-layer project reserved for entities and business rules
- `Infrastructure/` — infrastructure-layer project reserved for persistence and external integrations

## Current Baseline

The backend is intentionally small but already useful:
- solution/project references are in place
- `Program.cs` wires controllers, Swagger, JWT bearer auth, and authorization
- `GET /api/health` returns `{ "status": "ok" }`
- `POST /api/auth/login` authenticates persisted demo users and issues a JWT
- `GET /api/auth/me` proves protected identity access
- `GET/POST/PUT` project endpoints run on top of EF Core persistence
- `GET/POST/PUT` ticket endpoints cover the first useful CRUD slice
- local SQL Server development is prepared through Docker Compose + connection string configuration
- in Development, the app ensures the schema exists and seeds the default auth roles/users automatically

## Deferred Work

This increment does **not** include:
- ticket delete, filtering, pagination, comments, or dedicated status/assignment endpoints yet
- frontend login flow and protected Angular screens

The goal here is a clean modular-monolith starting point that is easy to explain and safe to grow.
