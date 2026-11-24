# IncidentAgent

Starter for a future Smart Incident Management System. Right now it is only a "Hello World" that proves the three apps run and talk to each other.

| App | Folder | URL |
|-----|--------|-----|
| API (ASP.NET Core minimal API, .NET 10) | `backend/IncidentAgent.Api` | http://localhost:5080 |
| Blazor UI (standalone WebAssembly) | `frontend-blazor/IncidentAgent.BlazorUi` | http://localhost:5090 |
| Angular UI (standalone components, no SSR) | `frontend-angular` | http://localhost:4200 |

`IncidentAgent.slnx` at the repo root contains the two .NET projects.

## Prerequisites

- .NET SDK 10
- Node.js 24+ and npm
- Angular CLI 22 (`npm install -g @angular/cli`)

## Run

Use three terminals, all starting from the repo root.

**1. API**

```bash
dotnet run --project backend/IncidentAgent.Api
```

Endpoints:
- `GET http://localhost:5080/api/hello`
- `GET http://localhost:5080/health`
- `GET http://localhost:5080/openapi/v1.json` (Development only)

**2. Blazor UI**

```bash
dotnet run --project frontend-blazor/IncidentAgent.BlazorUi
```

Open http://localhost:5090 and click **Call API**. The API URL is set in `frontend-blazor/IncidentAgent.BlazorUi/wwwroot/appsettings.json` (`ApiBaseUrl`).

**3. Angular UI**

```bash
cd frontend-angular
npm install
npm start
```

Open http://localhost:4200 and click **Call API**. The API URL is set in `frontend-angular/src/environments/environment*.ts` (`apiBaseUrl`).

## Build

```bash
dotnet build IncidentAgent.slnx
cd frontend-angular && npx ng build
```

## CORS

The API only allows browser calls from `http://localhost:5090` and `http://localhost:4200`. If you change a UI port, update the CORS policy in `backend/IncidentAgent.Api/Program.cs`.
