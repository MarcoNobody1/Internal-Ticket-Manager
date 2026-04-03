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
- `POST /api/auth/login` issues a demo JWT
- `GET /api/auth/me` proves protected identity access
- local SQL Server development is prepared through Docker Compose + connection string configuration

## Deferred Work

This increment does **not** include:
- EF Core `DbContext`, migrations, and real ticket persistence yet
- ticket entities, DTOs, services, or endpoints
- frontend login flow and protected Angular screens

The goal here is a clean modular-monolith starting point that is easy to explain and safe to grow.
