# Local Development Setup

**Languages:** [English](./local-development.en.md) | [Español](./local-development.es.md)

This guide is for anyone who wants to:
- clone the repository
- install missing tools
- reproduce the same local SQL Server environment
- run the backend and frontend locally
- clean everything up when finished

---

## Before You Start

Open a terminal in the folder where you want to clone the project:

```powershell
git clone <repo-url>
cd Internal-Ticket-Manager
```

From this point on:
- **repository root** means `Internal-Ticket-Manager`
- **frontend folder** means `Internal-Ticket-Manager\src\frontend`

---

## 1. Install Bun

Run this exact command in **PowerShell**:

```powershell
powershell -c "irm bun.sh/install.ps1 | iex"
```

Restart the terminal.

Then verify Bun works:

```powershell
bun --version
```

Expected result: a version number like `1.x.x`.

---

## 2. Install Angular CLI globally

Run:

```powershell
npm install -g @angular/cli
ng version
```

Expected result: Angular CLI version output.

> Use **Angular CLI** for Angular-specific commands. Use **Bun** for package management.

---

## 3. Install Docker Desktop if Needed

If Docker is not installed yet, run:

```powershell
winget install -e --id Docker.DockerDesktop --source winget
```

Then:
1. open Docker Desktop
2. wait until the engine is running
3. restart the terminal if `docker` is not recognized immediately

Verify Docker:

```powershell
docker --version
docker compose version
```

---

## 4. Bun vs Angular CLI

### Run these commands from `src/frontend`

Change directory first:

```powershell
cd src\frontend
```

### Use Bun for package management and scripts

```powershell
bun install
bun run build
bun run test -- --watch=false --browsers=ChromeHeadless
```

### Use Angular CLI for Angular operations

```powershell
ng serve
ng generate component features/example/example-page
ng test --watch=false --browsers=ChromeHeadless
```

---

## 5. Frontend Dependency Setup

Open a terminal in the **frontend folder**:

```powershell
cd <path>\Internal-Ticket-Manager\src\frontend
```

Install dependencies with Bun:

```powershell
bun install
```

Verify the Bun lockfile exists:

```powershell
Get-ChildItem bun.lock
```

If an old npm lockfile still exists locally, remove it:

```powershell
Remove-Item package-lock.json
```

---

## 6. Create the Local Docker Environment

Open a terminal in the **repository root**:

```powershell
cd <path>\Internal-Ticket-Manager
```

Create your local environment file:

```powershell
Copy-Item .env.example .env
```

Edit `.env` and choose a strong SA password.

Suggested values:

```env
SQLSERVER_SA_PASSWORD=ChangeThisToAStrongPassword!2026
SQLSERVER_PORT=1433
SQLSERVER_DATABASE=TicketingDb
```

Start SQL Server:

```powershell
docker compose up -d
```

Check status:

```powershell
docker compose ps
docker compose logs sqlserver
```

Wait until the container is `healthy`.

---

## 7. Create the Database

Still from the **repository root**, run:

```powershell
docker exec internal-ticket-manager-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "<YOUR_PASSWORD_FROM_.env>" -C -Q "IF DB_ID(N'TicketingDb') IS NULL CREATE DATABASE [TicketingDb];"
```

Verify it exists:

```powershell
docker exec internal-ticket-manager-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "<YOUR_PASSWORD_FROM_.env>" -C -Q "SET NOCOUNT ON; SELECT name FROM sys.databases WHERE name = 'TicketingDb';"
```

Expected result: `TicketingDb`.

---

## 8. Configure the API Connection String

Still from the **repository root**, run:

```powershell
dotnet user-secrets set "ConnectionStrings:TicketingDb" "Server=localhost,1433;Database=TicketingDb;User Id=sa;Password=<YOUR_PASSWORD_FROM_.env>;Encrypt=True;TrustServerCertificate=True;" --project src/backend/Api/InternalTicketManager.Api.csproj
```

Verify it was stored:

```powershell
dotnet user-secrets list --project src/backend/Api/InternalTicketManager.Api.csproj
```

---

## 9. Apply the Existing EF Core Migration

The project already contains the first persistence migration:

- migration name: `InitialTicketingPersistence`

From the **repository root**, run:

```powershell
dotnet tool restore
dotnet ef database update --project src/backend/Infrastructure/InternalTicketManager.Infrastructure.csproj --startup-project src/backend/Api/InternalTicketManager.Api.csproj
```

Expected current tables:
- `Projects`
- `Tickets`
- `Comments`

---

## 10. Verify the Current Baseline

### Backend

From the **repository root**:

```powershell
dotnet build InternalTicketManager.sln
dotnet test tests/backend/InternalTicketManager.Api.IntegrationTests/InternalTicketManager.Api.IntegrationTests.csproj
dotnet run --project src/backend/Api/InternalTicketManager.Api.csproj
```

### Frontend

Open a second terminal in the **frontend folder**:

```powershell
cd <path>\Internal-Ticket-Manager\src\frontend
bun install
bun run build
bun run test -- --watch=false --browsers=ChromeHeadless
ng serve
```

---

## 11. Test the Backend with Postman

The repository already includes an importable Postman collection here:

```text
docs/postman/InternalTicketManager.postman_collection.json
```

### 11.1 Import the collection

1. Open **Postman**.
2. Click **Import**.
3. Select:

```text
Internal-Ticket-Manager/docs/postman/InternalTicketManager.postman_collection.json
```

### 11.2 Verify the collection variables

After import, the collection already includes these variables:

- `baseUrl` = `http://localhost:5215`
- `token`
- `projectId`
- `ticketId`

### 11.3 Start the backend before sending requests

From the **repository root**:

```powershell
dotnet run --project src/backend/Api/InternalTicketManager.Api.csproj
```

### 11.4 Recommended request order

In Postman, run the requests in this order:

1. `Health / Get Health`
2. `Auth / Login (Admin Demo)`
3. `Auth / Get Current User`
4. `Projects / Create Project`
5. `Projects / Get Projects`
6. `Projects / Get Project By Id`
7. `Projects / Update Project`
8. `Tickets / Create Ticket`
9. `Tickets / Get Tickets`
10. `Tickets / Get Ticket By Id`
11. `Tickets / Update Ticket`

### 11.5 Variable behavior already included in the collection

The collection automatically stores:

- `token` after login
- `projectId` after creating a project
- `ticketId` after creating a ticket

That means you can test the CRUD flow without manually copying IDs between requests.

---

## 12. Future EF Core Migrations Workflow

From the **repository root**:

```powershell
dotnet tool restore
dotnet ef migrations add InitialCreate --project src/backend/Infrastructure/InternalTicketManager.Infrastructure.csproj --startup-project src/backend/Api/InternalTicketManager.Api.csproj
dotnet ef database update --project src/backend/Infrastructure/InternalTicketManager.Infrastructure.csproj --startup-project src/backend/Api/InternalTicketManager.Api.csproj
```

The real `DbContext` already exists. Use this flow whenever the model changes and a new migration is needed.

---

## 13. Tear Down / Clean Up

### Stop containers but keep database data

Run from the **repository root**:

```powershell
docker compose down
```

### Stop containers and delete persisted SQL Server data

Run from the **repository root**:

```powershell
docker compose down -v
```

### Remove the local `.env`

Run from the **repository root**:

```powershell
Remove-Item .env
```

### Remove the API development secret

Run from the **repository root**:

```powershell
dotnet user-secrets remove "ConnectionStrings:TicketingDb" --project src/backend/Api/InternalTicketManager.Api.csproj
```

### Remove frontend installed dependencies

Run from the **repository root**:

```powershell
Remove-Item -Recurse -Force src/frontend/node_modules
```
