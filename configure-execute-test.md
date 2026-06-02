# Configure, Execute, and Test Guide

This project can be started end-to-end with one command using Docker Compose.

## Scope

The Compose stack in `template/backend` starts:

- Frontend (`template/frontend`, served by Nginx)
- WebApi (`Ambev.DeveloperEvaluation.WebApi`)
- PostgreSQL
- RabbitMQ

## 1) Prerequisites

- Docker Desktop
- .NET 8 SDK (for tests)
- Node.js 20+ (optional, for frontend local development/tests)

## 2) Quick start (one command)

From repository root:

```powershell
cd .\template\backend
docker compose up --build -d
```

What this does:

- Builds WebApi and Frontend images
- Starts all services in background
- Runs API with `ASPNETCORE_ENVIRONMENT=Docker`
- Applies pending database migrations during API startup
- Waits for PostgreSQL and RabbitMQ health checks before starting WebApi

Main URLs:

- Frontend: `http://localhost:4200`
- API Swagger: `http://localhost:8080/swagger`
- Health: `http://localhost:8080/health`
- RabbitMQ UI: `http://localhost:15672` (user: `developer`, password: `ev@luAt10n`)

Sales API examples: [docs/sales-api.md](../docs/sales-api.md)

## 3) Operational commands

View running services:

```powershell
cd .\template\backend
docker compose ps
```

Follow API logs:

```powershell
cd .\template\backend
docker compose logs -f ambev.developerevaluation.webapi
```

Stop and remove containers:

```powershell
cd .\template\backend
docker compose down
```

Reset database volume:

```powershell
cd .\template\backend
docker compose down -v
```

Rebuild after code changes:

```powershell
cd .\template\backend
docker compose up --build -d
```

## 4) Run tests

Backend:

```powershell
cd .\template\backend
dotnet test .\Ambev.DeveloperEvaluation.sln --configuration Release
```

Frontend:

```powershell
cd .\template\frontend
npm ci
npm test -- --watch=false --browsers=ChromeHeadless
```

Coverage (Windows PowerShell):

```powershell
cd .\template\backend
.\coverage-report.ps1
```

## 5) Verify event flow (Rebus + RabbitMQ)

1. Start the stack with `docker compose up --build -d`.
2. Create/update/cancel a sale using `/api/Sales` endpoints.
3. Check WebApi logs:
   - `SaleCreated event received`
   - `SaleModified event received`
   - `SaleCancelled event received`
   - `ItemCancelled event received`
4. Optionally inspect queue/exchange metrics in RabbitMQ UI.

## 6) Optional local-only development mode

If you need to run WebApi directly on host (without WebApi container), use:

```powershell
cd .\template\backend
docker compose up -d ambev.developerevaluation.database ambev.developerevaluation.rabbitmq
dotnet run --project .\src\Ambev.DeveloperEvaluation.WebApi\Ambev.DeveloperEvaluation.WebApi.csproj --launch-profile https
```

More EF and local commands are in [docs/ef-core-commands.md](../docs/ef-core-commands.md) and [docs/run-local-webapi-with-containers.md](../docs/run-local-webapi-with-containers.md).

Design notes: [docs/design-patterns.md](../docs/design-patterns.md)
