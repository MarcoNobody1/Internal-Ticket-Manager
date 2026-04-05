# Internal Ticket Manager

[![.NET](https://img.shields.io/badge/.NET-8-512BD4)](#)
[![Angular](https://img.shields.io/badge/Angular-18-DD0031)](#)
[![Bun](https://img.shields.io/badge/Bun-1.3.11-black)](#)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-2022-CC2927)](#)
[![Docker](https://img.shields.io/badge/Docker-Compose-2496ED)](#)

> Demo técnica, limpia y defendible para entrevista, construida con **.NET 8 Web API**, **Angular**, **Bun** y **SQL Server 2022**.

**Idiomas:** [English](./README.md) | [Español](./README.es.md)

---

## Visión general

Este repositorio está planteado intencionalmente como un **monolito modular pequeño**.

Lo que ya existe:
- autenticación JWT apoyada en usuarios y roles persistidos
- CRUD admin-only para usuarios persistidos con asignación de un solo rol (`Admin` / `Developer`)
- endpoint de salud y endpoint protegido de identidad
- base real de persistencia con creación automática del esquema local en Development
- CRUD de proyectos
- CRUD base de tickets para listar, ver detalle, crear y actualizar
- shell Angular standalone con login funcional, workspace y pantalla admin de usuarios
- gestión de dependencias frontend con Bun
- infraestructura local con Docker Compose + SQL Server 2022
- documentación para reproducir el entorno local

Lo que sigue diferido a propósito:
- delete de tickets, filtros, paginación, comentarios y endpoints dedicados de workflow
- pantallas funcionales de tickets más allá del workspace protegido actual
- CI/CD y despliegue

---

## Requisitos previos

Antes de intentar levantar el proyecto en local, asegúrate de que tu equipo tiene instaladas estas herramientas:

- **Git**
- **.NET 8 SDK**
- **Node.js 22.x**
- **Bun**
- **Angular CLI 18.x**
- **Docker Desktop** con Docker Compose
- **Visual Studio Code** (opcional, pero recomendable si quieres usar el arranque cómodo con un clic)

Comandos recomendados para verificarlo:

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

## Estructura del repositorio

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

## ¿Quieres reproducir este repositorio en local?

Si tu objetivo es clonar el repositorio y tener **el mismo entorno local**, **no te quedes solo con este README**.

Ve a la guía específica:

- **English:** [`docs/local-development.en.md`](./docs/local-development.en.md)
- **Español:** [`docs/local-development.es.md`](./docs/local-development.es.md)

Esa guía explica:
- qué herramientas tienes que instalar primero
- en qué **ruta exacta** abrir la terminal
- el **orden exacto de comandos**
- cómo levantar Docker y SQL Server
- cómo crear `TicketingDb`
- cómo la API crea el esquema local y deja sembrados los usuarios por defecto
- cómo configurar la connection string de la API
- cómo correr backend y frontend
- cómo importar y usar la colección de Postman para probar el CRUD del backend
- cómo desmontar todo al terminar

---

## Resumen rápido de arranque

### 1. Clonar el repositorio

Abre una terminal en la carpeta donde quieras guardar el proyecto y ejecuta:

```powershell
git clone <repo-url>
cd Internal-Ticket-Manager
```

### 2. Leer la guía completa de entorno local

Elige idioma:

- [`docs/local-development.en.md`](./docs/local-development.en.md)
- [`docs/local-development.es.md`](./docs/local-development.es.md)

### 3. Comandos principales una vez hecho el setup

#### Backend API

Abre una terminal en la **raíz del repositorio**:

```powershell
cd <ruta>\Internal-Ticket-Manager
dotnet run --project src/backend/Api/InternalTicketManager.Api.csproj
```

Endpoints base esperados:
- `GET /api/health`
- `POST /api/auth/login`
- `GET /api/auth/me`
- `GET /api/users`
- `GET /api/users/{id}`
- `POST /api/users`
- `PUT /api/users/{id}`
- `DELETE /api/users/{id}`
- `GET /api/projects`
- `GET /api/projects/{id}`
- `POST /api/projects`
- `PUT /api/projects/{id}`
- `GET /api/tickets`
- `GET /api/tickets/{id}`
- `POST /api/tickets`
- `PUT /api/tickets/{id}`

#### Frontend

Abre una terminal en el **workspace frontend**:

```powershell
cd <ruta>\Internal-Ticket-Manager\src\frontend
ng serve
```

Notas del flujo auth/frontend:
- Angular Material sigue siendo la base visual.
- Las credenciales demo seed siguen siendo:
  - `admin.demo / AdminDemo123!`
  - `developer.demo / DeveloperDemo123!`
- Rutas principales:
  - `/login`
  - `/workspace/projects`
  - `/workspace/tickets`
  - `/workspace/tickets/:ticketId`
  - `/workspace/users` — solo para admins

#### Flujo de arranque desde Visual Studio Code

Si abres el repositorio en VSCode, ahora puedes arrancar la aplicación sin abrir manualmente varias terminales.

Configuraciones disponibles en VSCode:

- `Full App: API + Frontend`
- `Backend: API (.NET)`
- `Frontend: Angular`

Archivos de VSCode añadidos para este flujo:

- `.vscode/tasks.json`
- `.vscode/launch.json`

Uso recomendado:

1. Abre la raíz del repositorio en VSCode.
2. Pulsa `F5` o abre **Run and Debug**.
3. Elige `Full App: API + Frontend`.
4. VSCode arrancará el backend, levantará el frontend Angular en `http://localhost:4201` y abrirá el navegador automáticamente.

---

## Reglas de tooling

### Usa Bun para la gestión de paquetes del frontend

Ejecuta esto desde:

```text
src/frontend
```

Ejemplos:

```powershell
bun install
bun run build
bun run test -- --watch=false --browsers=ChromeHeadless
```

### Usa Angular CLI para operaciones propias de Angular

También desde:

```text
src/frontend
```

Ejemplos:

```powershell
ng serve
ng generate component features/example/example-page
ng test --watch=false --browsers=ChromeHeadless
```

---

## Infraestructura local

SQL Server local corre con Docker Compose:

- imagen: `mcr.microsoft.com/mssql/server:2022-latest`
- base de datos: `TicketingDb`
- puerto expuesto: `1433`
- volumen persistente habilitado

Las configuraciones locales se reparten así:
- `.env` para variables de Docker
- `.NET user-secrets` para la connection string de la API

---

## Por qué esta solución encaja con el proyecto

Esta base es simple y defendible:

- un solo contenedor local de SQL Server
- un `docker-compose.yml` reproducible
- Bun para gestión de paquetes frontend
- Angular CLI solo para operaciones Angular
- `user-secrets` para no subir credenciales locales reales
- creación automática del esquema local y usuarios demo sembrados para facilitar el onboarding

---

## Índice de documentación

- [README in English](./README.md)
- [Local setup guide (English)](./docs/local-development.en.md)
- [Guía de entorno local (Español)](./docs/local-development.es.md)
- [Colección de Postman](./docs/postman/InternalTicketManager.postman_collection.json)
