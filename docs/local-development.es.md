# Entorno local de desarrollo

**Idiomas:** [English](./local-development.en.md) | [Español](./local-development.es.md)

Esta guía es para cualquiera que quiera:
- clonar el repositorio
- instalar las herramientas que le falten
- reproducir el mismo entorno local con SQL Server
- levantar backend y frontend en local
- desmontarlo todo cuando termine

---

## Antes de empezar

Abre una terminal en la carpeta donde quieras clonar el proyecto:

```powershell
git clone <repo-url>
cd Internal-Ticket-Manager
```

Desde aquí en adelante:
- **raíz del repositorio** = `Internal-Ticket-Manager`
- **carpeta frontend** = `Internal-Ticket-Manager\src\frontend`

---

## 1. Instalar Bun

Ejecuta este comando exacto en **PowerShell**:

```powershell
powershell -c "irm bun.sh/install.ps1 | iex"
```

Reinicia la terminal.

Después verifica que Bun funcione:

```powershell
bun --version
```

Resultado esperado: un número de versión tipo `1.x.x`.

---

## 2. Instalar Angular CLI globalmente

Ejecuta:

```powershell
npm install -g @angular/cli
ng version
```

Resultado esperado: salida con la versión de Angular CLI.

> Usa **Angular CLI** para comandos propios de Angular. Usa **Bun** para la gestión de paquetes.

---

## 3. Instalar Docker Desktop si hace falta

Si Docker no está instalado todavía, ejecuta:

```powershell
winget install -e --id Docker.DockerDesktop --source winget
```

Después:
1. abre Docker Desktop
2. espera a que el engine quede en ejecución
3. reinicia la terminal si `docker` no se reconoce al instante

Verifica Docker:

```powershell
docker --version
docker compose version
```

---

## 4. Bun vs Angular CLI

### Estos comandos se ejecutan desde `src/frontend`

Primero cambia de carpeta:

```powershell
cd src\frontend
```

### Usa Bun para paquetes y scripts

```powershell
bun install
bun run build
bun run test -- --watch=false --browsers=ChromeHeadless
```

### Usa Angular CLI para operaciones Angular

```powershell
ng serve
ng generate component features/example/example-page
ng test --watch=false --browsers=ChromeHeadless
```

---

## 5. Preparar dependencias del frontend

Abre una terminal en la **carpeta frontend**:

```powershell
cd <ruta>\Internal-Ticket-Manager\src\frontend
```

Instala dependencias con Bun:

```powershell
bun install
```

Verifica que exista el lockfile de Bun:

```powershell
Get-ChildItem bun.lock
```

Si todavía existe un lockfile antiguo de npm en local, elimínalo:

```powershell
Remove-Item package-lock.json
```

---

## 6. Crear el entorno local con Docker

Abre una terminal en la **raíz del repositorio**:

```powershell
cd <ruta>\Internal-Ticket-Manager
```

Crea el archivo local de entorno:

```powershell
Copy-Item .env.example .env
```

Edita `.env` y elige una contraseña robusta para `sa`.

Valores sugeridos:

```env
SQLSERVER_SA_PASSWORD=ChangeThisToAStrongPassword!2026
SQLSERVER_PORT=1433
SQLSERVER_DATABASE=TicketingDb
```

Levanta SQL Server:

```powershell
docker compose up -d
```

Verifica el estado:

```powershell
docker compose ps
docker compose logs sqlserver
```

Espera hasta que el contenedor esté `healthy`.

---

## 7. Crear la base de datos

Todavía desde la **raíz del repositorio**, ejecuta:

```powershell
docker exec internal-ticket-manager-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "<TU_PASSWORD_DEL_.env>" -C -Q "IF DB_ID(N'TicketingDb') IS NULL CREATE DATABASE [TicketingDb];"
```

Verifica que exista:

```powershell
docker exec internal-ticket-manager-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "<TU_PASSWORD_DEL_.env>" -C -Q "SET NOCOUNT ON; SELECT name FROM sys.databases WHERE name = 'TicketingDb';"
```

Resultado esperado: `TicketingDb`.

---

## 8. Configurar la connection string de la API

Todavía desde la **raíz del repositorio**, ejecuta:

```powershell
dotnet user-secrets set "ConnectionStrings:TicketingDb" "Server=localhost,1433;Database=TicketingDb;User Id=sa;Password=<TU_PASSWORD_DEL_.env>;Encrypt=True;TrustServerCertificate=True;" --project src/backend/Api/InternalTicketManager.Api.csproj
```

Verifica que ha quedado guardada:

```powershell
dotnet user-secrets list --project src/backend/Api/InternalTicketManager.Api.csproj
```

---

## 9. Aplicar la migración existente de EF Core

El proyecto ya incluye la primera migración de persistencia:

- nombre de la migración: `InitialTicketingPersistence`

Desde la **raíz del repositorio**, ejecuta:

```powershell
dotnet tool restore
dotnet ef database update --project src/backend/Infrastructure/InternalTicketManager.Infrastructure.csproj --startup-project src/backend/Api/InternalTicketManager.Api.csproj
```

Tablas actuales esperadas:
- `Projects`
- `Tickets`
- `Comments`

---

## 10. Verificar la baseline actual

### Backend

Desde la **raíz del repositorio**:

```powershell
dotnet build InternalTicketManager.sln
dotnet test tests/backend/InternalTicketManager.Api.IntegrationTests/InternalTicketManager.Api.IntegrationTests.csproj
dotnet run --project src/backend/Api/InternalTicketManager.Api.csproj
```

### Frontend

Abre una segunda terminal en la **carpeta frontend**:

```powershell
cd <ruta>\Internal-Ticket-Manager\src\frontend
bun install
bun run build
bun run test -- --watch=false --browsers=ChromeHeadless
ng serve
```

---

## 11. Probar el backend con Postman

El repositorio ya incluye una colección de Postman lista para importar en:

```text
docs/postman/InternalTicketManager.postman_collection.json
```

### 11.1 Importar la colección

1. Abre **Postman**.
2. Pulsa **Import**.
3. Selecciona:

```text
Internal-Ticket-Manager/docs/postman/InternalTicketManager.postman_collection.json
```

### 11.2 Verificar las variables de la colección

Después de importarla, la colección ya incluye estas variables:

- `baseUrl` = `http://localhost:5215`
- `token`
- `projectId`
- `ticketId`

### 11.3 Arrancar el backend antes de lanzar requests

Desde la **raíz del repositorio**:

```powershell
dotnet run --project src/backend/Api/InternalTicketManager.Api.csproj
```

### 11.4 Orden recomendado de pruebas

En Postman, lanza los requests en este orden:

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

### 11.5 Variables que la colección rellena automáticamente

La colección guarda automáticamente:

- `token` después del login
- `projectId` después de crear un proyecto
- `ticketId` después de crear un ticket

Así puedes probar el flujo CRUD sin copiar identificadores a mano entre peticiones.

---

## 12. Flujo futuro de EF Core migrations

Desde la **raíz del repositorio**:

```powershell
dotnet tool restore
dotnet ef migrations add InitialCreate --project src/backend/Infrastructure/InternalTicketManager.Infrastructure.csproj --startup-project src/backend/Api/InternalTicketManager.Api.csproj
dotnet ef database update --project src/backend/Infrastructure/InternalTicketManager.Infrastructure.csproj --startup-project src/backend/Api/InternalTicketManager.Api.csproj
```

El `DbContext` real ya existe. Utiliza este flujo cuando el modelo cambie y necesites crear una nueva migración.

---

## 13. Desmontaje / limpieza

### Parar contenedores pero conservar datos

Ejecuta desde la **raíz del repositorio**:

```powershell
docker compose down
```

### Parar contenedores y borrar el volumen persistente de SQL Server

Ejecuta desde la **raíz del repositorio**:

```powershell
docker compose down -v
```

### Borrar el `.env` local

Ejecuta desde la **raíz del repositorio**:

```powershell
Remove-Item .env
```

### Borrar el secret local de la API

Ejecuta desde la **raíz del repositorio**:

```powershell
dotnet user-secrets remove "ConnectionStrings:TicketingDb" --project src/backend/Api/InternalTicketManager.Api.csproj
```

### Borrar dependencias instaladas del frontend

Ejecuta desde la **raíz del repositorio**:

```powershell
Remove-Item -Recurse -Force src/frontend/node_modules
```
