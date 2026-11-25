# IncidentAgent

Smart Incident Management System. Clean Architecture .NET 10 API on PostgreSQL 17, a Blazor WebAssembly dashboard, and an Angular starter.

| App | Folder | URL |
|-----|--------|-----|
| API (ASP.NET Core minimal API, .NET 10) | `backend/IncidentAgent.Api` (+ `Domain`, `Application`, `Infrastructure`) | http://localhost:5080 |
| PostgreSQL 17 (Docker) | `docker/docker-compose.yml` | localhost:5432 |
| Blazor UI (standalone WebAssembly) | `frontend-blazor/IncidentAgent.BlazorUi` | http://localhost:5090 |
| Angular UI (standalone components, no SSR) | `frontend-angular` | http://localhost:4200 |

`IncidentAgent.slnx` at the repo root contains the .NET projects. Backend dependencies point inward: Api → Application/Infrastructure, Infrastructure → Application/Domain, Application → Domain.

## Prerequisites

- .NET SDK 10
- Docker Desktop (for PostgreSQL)
- EF Core tools: `dotnet tool install --global dotnet-ef`
- Node.js 24+ and npm
- Angular CLI 22 (`npm install -g @angular/cli`)

## First-time setup (database)

Run from the repo root. Pick your own local dev password and use the same value in both places.

```bash
cp docker/.env.example docker/.env          # then set POSTGRES_PASSWORD in docker/.env (gitignored)
docker compose -f docker/docker-compose.yml up -d

dotnet user-secrets set "ConnectionStrings:Postgres" \
  "Host=localhost;Port=5432;Database=incidentagent;Username=incidentagent;Password=<your password>" \
  --project backend/IncidentAgent.Api

dotnet ef database update -p backend/IncidentAgent.Infrastructure -s backend/IncidentAgent.Api
```

In Development the API seeds sample data on startup (categories, services, users, SLA policies, ~10 incidents). Only missing rows are inserted.

## Run

Use three terminals, all starting from the repo root.

**1. API**

```bash
dotnet run --project backend/IncidentAgent.Api
```

Endpoints:
- `GET /health` (liveness, no DB) and `GET /health/ready` (checks PostgreSQL)
- `GET /api/hello`
- `POST /api/incidents`, `GET /api/incidents` (paging + filters), `GET|PUT /api/incidents/{id}`
- `POST /api/incidents/{id}/assign`, `POST /api/incidents/{id}/status`, `POST /api/incidents/{id}/comments`, `GET /api/incidents/{id}/history`
- `GET /api/categories`, `GET /api/services`, `GET /api/users`
- `GET /openapi/v1.json` (Development only)

Smoke test: `backend/IncidentAgent.Api/IncidentAgent.Api.http` (run it in Rider's HTTP client).

**2. Blazor UI**

```bash
dotnet run --project frontend-blazor/IncidentAgent.BlazorUi
```

Open http://localhost:5090 for the dashboard (Dashboard, All Incidents, incident details, Categories, Application Services). The API URL is set in `frontend-blazor/IncidentAgent.BlazorUi/wwwroot/appsettings.json` (`ApiBaseUrl`).

**3. Angular UI**

```bash
cd frontend-angular
npm install
npm start
```

Open http://localhost:4200 and click **Call API**. The API URL is set in `frontend-angular/src/environments/environment*.ts` (`apiBaseUrl`).

## Migrations

```bash
dotnet ef migrations add <Name> -p backend/IncidentAgent.Infrastructure -s backend/IncidentAgent.Api -o Persistence/Migrations
```

## Build

```bash
dotnet build IncidentAgent.slnx
cd frontend-angular && npx ng build
```

## CORS

The API only allows browser calls from `http://localhost:5090` and `http://localhost:4200`. If you change a UI port, update the CORS policy in `backend/IncidentAgent.Api/Program.cs`.
