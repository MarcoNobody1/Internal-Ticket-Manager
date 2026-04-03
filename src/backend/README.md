# Backend Workspace

This folder now contains the real backend bootstrap for the project.

## Projects

- `Api/` — ASP.NET Core entry point, controller wiring, Swagger in development, and the bootstrap health endpoint
- `Application/` — application-layer project reserved for future use cases and orchestration
- `Domain/` — domain-layer project reserved for entities and business rules
- `Infrastructure/` — infrastructure-layer project reserved for persistence and external integrations

## Current Baseline

The backend is intentionally minimal:
- solution/project references are in place
- `Program.cs` only wires controllers and Swagger
- `GET /api/health` returns `{ "status": "ok" }`

## Deferred Work

This increment does **not** include:
- JWT auth
- EF Core or database setup
- ticket entities, DTOs, services, or endpoints
- tests

The goal here is a clean modular-monolith starting point that is easy to explain and safe to grow.
