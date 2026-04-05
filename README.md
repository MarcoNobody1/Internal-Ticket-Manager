# Internal Ticket Manager

[![.NET](https://img.shields.io/badge/.NET-8-512BD4)](#)
[![Angular](https://img.shields.io/badge/Angular-18-DD0031)](#)
[![Bun](https://img.shields.io/badge/Bun-1.3.11-black)](#)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-2022-CC2927)](#)
[![Docker](https://img.shields.io/badge/Docker-Compose-2496ED)](#)

> A clean, interview-defendable ticket management demo built with **.NET 8 Web API**, **Angular**, **Bun**, and **SQL Server 2022**.

**Languages:** [English](./README.md) | [Español](./README.es.md)

---

## Overview

This repository is intentionally built as a **small modular monolith**.

Current baseline includes:
- JWT auth foundation with demo users
- backend health endpoint and protected identity probe
- real EF Core persistence foundation with the first migration already created
- project CRUD endpoints
- base ticket CRUD endpoints for list/detail/create/update
- Angular standalone shell with a first login/workspace auth slice
- Bun-based frontend dependency management
- Docker Compose local SQL Server 2022 infrastructure
- reproducible local setup documentation

Deferred on purpose:
- ticket delete, filtering, pagination, comments, and dedicated workflow endpoints
- ticket screens beyond the protected workspace placeholder
- CI/CD and deployment automation

---

## Prerequisites

Before trying to run the project locally, make sure your machine has these tools installed:

- **Git**
- **.NET 8 SDK**
- **Node.js 22.x**
- **Bun**
- **Angular CLI 18.x**
- **Docker Desktop** with Docker Compose
- **Visual Studio Code** (optional, but recommended if you want the one-click launch workflow)

Recommended verification commands:

```powershell
git --version
dotnet --version
node --version
bun --version
ng version
docker --version
docker compose version
```

---

## Repository Structure

```text
.
├── InternalTicketManager.sln
├── docker-compose.yml
├── docs/
│   ├── local-development.md
│   ├── local-development.en.md
│   └── local-development.es.md
├── src/
│   ├── backend/
│   │   ├── Api/
│   │   ├── Application/
│   │   ├── Domain/
│   │   ├── Infrastructure/
│   │   └── README.md
│   └── frontend/
│       ├── bun.lock
│       ├── package.json
│       ├── angular.json
│       └── src/
└── tests/
```

---

## Want to Reproduce This Project Locally?

If your goal is to clone the repository and reproduce the same local environment, **do not rely only on this README**.

Go to the dedicated setup guide:

- **English:** [`docs/local-development.en.md`](./docs/local-development.en.md)
- **Español:** [`docs/local-development.es.md`](./docs/local-development.es.md)

That guide explains:
- which tools must be installed first
- exactly **which folder** to open the terminal in
- the **exact command order**
- how to start Docker and SQL Server
- how to create `TicketingDb`
- how to apply the existing EF Core migration and create the current tables
- how to configure the API connection string
- how to run backend and frontend locally
- how to import and use the Postman collection to test the backend CRUD
- how to tear everything down when finished

---

## Quick Start Summary

### 1. Clone the repository

Open a terminal in the folder where you want the project to live, then run:

```powershell
git clone <repo-url>
cd Internal-Ticket-Manager
```

### 2. Read the full local setup guide

Choose your language:

- [`docs/local-development.en.md`](./docs/local-development.en.md)
- [`docs/local-development.es.md`](./docs/local-development.es.md)

### 3. Main runtime commands after setup

#### Backend API

Open a terminal in the **repository root**:

```powershell
cd <path>\Internal-Ticket-Manager
dotnet run --project src/backend/Api/InternalTicketManager.Api.csproj
```

Expected baseline endpoints:
- `GET /api/health`
- `POST /api/auth/login`
- `GET /api/auth/me`
- `GET /api/projects`
- `GET /api/projects/{id}`
- `POST /api/projects`
- `PUT /api/projects/{id}`
- `GET /api/tickets`
- `GET /api/tickets/{id}`
- `POST /api/tickets`
- `PUT /api/tickets/{id}`

#### Frontend

Open a terminal in the **frontend workspace**:

```powershell
cd <path>\Internal-Ticket-Manager\src\frontend
ng serve
```

Frontend auth notes:
- Angular Material is already wired with the standard prebuilt theme.
- `ng serve` now proxies `/api/*` requests to the backend at `http://localhost:5215`.
- Demo login credentials from backend development settings:
  - `admin.demo / AdminDemo123!`
  - `developer.demo / DeveloperDemo123!`
- Routes:
  - `/login` — public login page
  - `/workspace` — protected demo area after login

#### Visual Studio Code launch workflow

If you open the repository in VSCode, you can now start the app without manually opening separate terminals.

Available VSCode configurations:

- `Full App: API + Frontend`
- `Backend: API (.NET)`
- `Frontend: Angular`

VSCode files added for this workflow:

- `.vscode/tasks.json`
- `.vscode/launch.json`

Recommended usage:

1. Open the repository root in VSCode.
2. Press `F5` or open **Run and Debug**.
3. Choose `Full App: API + Frontend`.
4. VSCode will start the backend, start the Angular frontend on `http://localhost:4201`, and open the browser automatically.

---

## Tooling Rules

### Use Bun for frontend package management

Run these from:

```text
src/frontend
```

Examples:

```powershell
bun install
bun run build
bun run test -- --watch=false --browsers=ChromeHeadless
```

### Use Angular CLI for Angular-specific operations

Also run these from:

```text
src/frontend
```

Examples:

```powershell
ng serve
ng generate component features/example/example-page
ng test --watch=false --browsers=ChromeHeadless
```

---

## Local Infrastructure

Local SQL Server is provided through Docker Compose:

- image: `mcr.microsoft.com/mssql/server:2022-latest`
- database name: `TicketingDb`
- exposed port: `1433`
- persistent volume enabled

Connection strings for local development are intended to live in:
- `.env` for Docker variables
- `.NET user-secrets` for the API connection string

---

## Why This Setup Fits the Project

This setup is intentionally simple and defendable:

- one local SQL Server container
- one reproducible Docker Compose file
- Bun for consistent frontend package management
- Angular CLI for Angular operations only
- .NET user-secrets to avoid committing real local credentials
- future-ready EF Core migration workflow without inventing fake persistence too early

---

## Documentation Index

- [README in Spanish](./README.es.md)
- [Local setup guide (English)](./docs/local-development.en.md)
- [Guía de entorno local (Español)](./docs/local-development.es.md)
- [Postman collection](./docs/postman/InternalTicketManager.postman_collection.json)
