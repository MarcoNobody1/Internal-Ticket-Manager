# Internal Ticket Manager

Internal Ticket Manager is a technical interview project for managing internal support and development tickets.

The goal is to demonstrate strong fundamentals with a clean, explainable architecture using **.NET 8 Web API** on the backend and **Angular** on the frontend, without pretending the platform is further along than it really is.

## Current Status

This repository is currently in its **foundation/bootstrap stage**.

What exists today:
- a realistic project README
- an intentional repository scaffold for backend, frontend, tests, and docs
- project standards and SDD artifacts that define how the work will be delivered

What is intentionally **not** generated yet:
- `.NET` solution or project files
- Angular workspace files
- authentication and authorization
- database setup and EF Core models
- ticket management features
- CI/CD, Docker, or deployment assets

That boundary is deliberate: this first increment is about making the project direction clear and defendable before framework code is generated.

## Project Goal

Build a small but solid **modular monolith** for internal ticket management that is easy to explain in an interview.

The finished demo should showcase:
- clean REST API design
- practical service-layer business logic
- EF Core persistence with readable queries
- JWT-based authentication with a minimal role model
- Angular screens for listing, creating, and updating tickets
- focused validation, testing, and documentation

## Intended Stack

### Backend
- **.NET 8 Web API**
- **Entity Framework Core**
- **SQL Server** (or a similar relational database if the setup changes later)
- JWT authentication

### Frontend
- **Angular**
- Standalone components
- Angular Router
- Reactive Forms
- HTTP services, guards, interceptors, and RxJS

## Intended Architecture

The target shape is a **clean modular monolith**, optimized for clarity over ceremony.

Planned backend structure:

```text
src/backend/
  Api/
  Application/
  Domain/
  Infrastructure/
```

Planned frontend direction:

```text
src/frontend/
  app/
    core/
    features/
    shared/
```

This does **not** mean heavy enterprise patterns will be added by default. The project explicitly avoids unnecessary CQRS, event buses, MediatR-by-fashion, fake DDD complexity, or global frontend state unless real complexity justifies it.

## Repository Structure

```text
.
├── docs/              # Project notes and future architecture/API documentation
├── src/
│   ├── backend/       # Planned .NET backend area
│   └── frontend/      # Planned Angular frontend area
├── tests/
│   ├── backend/       # Planned backend tests
│   └── frontend/      # Planned frontend tests
├── AGENTS.md          # Project standards and delivery rules for contributors/agents
└── README.md
```

## Planned MVP Scope

The expected MVP is intentionally practical and interview-friendly:
- authenticate users with a minimal role model (`Admin`, `Developer`)
- create tickets
- list tickets with filtering and pagination
- update ticket status and assignment
- validate requests clearly
- expose clean API documentation
- provide a simple Angular UI for the main flows

## What This Increment Does

This increment only establishes the project foundation:
- replaces the placeholder README with realistic documentation
- creates visible scaffold directories for backend, frontend, tests, and docs
- keeps placeholders lightweight and intentional

## What Is Deferred

The following work is deferred to later increments:
- generating the actual `.NET 8` solution and API project
- generating the Angular workspace and application shell
- implementing auth, persistence, domain models, and ticket endpoints
- adding tests with real runners/frameworks
- environment configuration, CI/CD, and containerization

## Setup Notes

There is nothing to run yet.

At this stage, the repository is documentation-first by design. Once the backend and frontend workspaces are generated in later increments, this README will be updated with:
- local setup instructions
- environment variable requirements
- database migration commands
- API and frontend run commands
- testing commands

## Documentation Direction

Supporting documentation will live under `docs/` as the project grows. That area is intended for:
- architecture notes
- API decisions
- authentication notes
- setup guidance
- delivery and testing notes

## Why This Approach

This repository is meant to be defendable in an interview.

So instead of dumping generated framework output immediately, the project starts with a clear structure and honest documentation. That makes it easier to explain:
- what the system is supposed to become
- what has been done already
- what was intentionally deferred
- why complexity is being introduced only when it earns its place

## Next Likely Increment

The next sensible step is to bootstrap the actual application workspaces:
1. create the `.NET 8` solution and backend projects
2. create the Angular workspace
3. wire up the initial folder structure and baseline configuration

Until then, this repository should be read as a **project foundation**, not as a finished application.
