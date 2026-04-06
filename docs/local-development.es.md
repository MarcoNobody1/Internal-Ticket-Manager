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

La UI del frontend utiliza ahora **PrimeNG 18 + PrimeIcons**.

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

Esto instala tanto las dependencias de Angular como la capa de UI con PrimeNG/PrimeIcons que usa el shell y las pantallas principales.

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

## 9. Arrancar la API y dejar que cree el esquema local

El repositorio no mantiene ficheros de migración de EF Core versionados.

En **Development**, la API crea automáticamente el esquema local a partir del modelo actual y siembra los datos base de autenticación.

Desde la **raíz del repositorio**, ejecuta:

```powershell
dotnet run --project src/backend/Api/InternalTicketManager.Api.csproj
```

En el primer arranque, la API hará lo siguiente:
- comprobar que el esquema existe en `TicketingDb`
- crear las tablas actuales
- sembrar los roles `Admin` y `Developer` si faltan
- sembrar los usuarios demo por defecto si faltan

Notas actuales sobre gestión admin de usuarios:
- El CRUD de `/api/users` está restringido al rol `Admin`.
- `/workspace/users` solo está disponible para admins autenticados.
- Las credenciales demo seed siguen siendo la forma soportada más simple para entrar al sistema en local.

Como el repositorio no mantiene ficheros de migración versionados, la base de datos local debe tratarse como una base de desarrollo desechable. Si el esquema cambia más adelante, la forma más simple y soportada de refrescarlo es recrear la base/volumen local y arrancar de nuevo la API.

Tablas actuales esperadas:
- `Roles`
- `Users`
- `Projects`
- `Tickets`
- `Comments`

Usuarios seed por defecto:
- `admin.demo / AdminDemo123!`
- `developer.demo / DeveloperDemo123!`

---

## 10. Verificar la baseline actual

### Backend

Desde la **raíz del repositorio**:

```powershell
dotnet build InternalTicketManager.sln
dotnet test tests/backend/InternalTicketManager.Api.IntegrationTests/InternalTicketManager.Api.IntegrationTests.csproj
dotnet run --project src/backend/Api/InternalTicketManager.Api.csproj
```

Endpoints admin de usuarios ahora cubiertos por tests de integración:
- `GET /api/users`
- `GET /api/users/{id}`
- `POST /api/users`
- `PUT /api/users/{id}`
- `DELETE /api/users/{id}`

Comportamiento actual por rol:
- `Projects`: lectura para usuarios autenticados, pero crear/editar/eliminar es solo para `Admin`.
- `Tickets`: lectura para usuarios autenticados, crear/editar para `Admin` o `Developer`, y eliminar solo para `Admin`.
- Los tickets ya se pueden asignar a uno o varios usuarios con rol `Developer`.
- El frontend incluye ahora:
  - `/workspace/account` para mostrar mejor la cuenta, la sesión y accesos directos a tickets asignados para developers
  - `/workspace/access-denied` para manejar 403 de forma intencional en la UI
  - `/workspace/projects/:projectId` para detalle de proyecto y tickets abiertos
  - flujos de tickets con selección de proyecto y asignación múltiple de developers
  - deep links `/workspace/tickets?assignedUserId=<id>` para abrir la vista filtrada desde la cuenta

Cuando la API arranca en `Development`, va a asegurar que el esquema existe y sembrar los datos de auth por defecto si faltan.

### Frontend

Abre una segunda terminal en la **carpeta frontend**:

```powershell
cd <ruta>\Internal-Ticket-Manager\src\frontend
bun install
bun run build
bun run test -- --watch=false --browsers=ChromeHeadless
ng serve
```

### Opcional: arrancar ambas partes desde VSCode

Si utilizas Visual Studio Code, el repositorio ahora incluye:

- `.vscode/tasks.json`
- `.vscode/launch.json`

Desde VSCode:

1. Abre la **raíz del repositorio**.
2. Abre **Run and Debug**.
3. Elige `Full App: API + Frontend`.
4. Pulsa `F5`.

Eso arranca:
- la API .NET
- el frontend Angular en `http://localhost:4201`
- una ventana del navegador apuntando al frontend

sin tener que abrir manualmente varias terminales.

El flujo de VSCode utiliza intencionadamente el puerto `4201`.

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
12. `Projects / Delete Project` (solo Admin, opcional)
13. `Tickets / Delete Ticket` (solo Admin, opcional)

### 11.5 Variables que la colección rellena automáticamente

La colección guarda automáticamente:

- `token` después del login
- `projectId` después de crear un proyecto
- `ticketId` después de crear un ticket

Así puedes probar el flujo CRUD sin copiar identificadores a mano entre peticiones.

---

## 12. Desmontaje / limpieza

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

Ese es también el camino recomendado si en algún momento el esquema local deja de coincidir con el modelo actual del código.

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
