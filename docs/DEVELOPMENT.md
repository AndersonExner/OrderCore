# Development Guide

This document collects the routine commands and expectations for working on OrderCore.

## Prerequisites

- .NET SDK compatible with `net10.0`.
- Node.js and npm for the frontend.
- PostgreSQL for local persistence.
- EF Core CLI when creating or applying migrations.

## Backend Commands

Run from the repository root:

```powershell
dotnet restore OrderCore.sln
dotnet build OrderCore.sln
dotnet test OrderCore.sln
dotnet run --project src\OrderCore.Api\OrderCore.Api.csproj
```

The API launch profiles are:

```text
http:  http://localhost:5282
https: https://localhost:7171 and http://localhost:5282
```

Swagger is available in development at:

```text
https://localhost:7171/swagger
```

## Frontend Commands

Run from `web/ordercore-web`:

```powershell
npm install
npm run dev
npm run build
npm run lint
```

The Vite dev server defaults to:

```text
http://localhost:5173
```

The API CORS policy currently allows this frontend origin.

## Database and Migrations

The API reads the PostgreSQL connection string named `DefaultConnection`.

Common EF Core commands:

```powershell
dotnet ef migrations add <MigrationName> --project src\OrderCore.Infrastructure --startup-project src\OrderCore.Api
dotnet ef database update --project src\OrderCore.Infrastructure --startup-project src\OrderCore.Api
```

Migration files are stored under:

```text
src/OrderCore.Infrastructure/Migrations
```

Entity configurations are stored under:

```text
src/OrderCore.Infrastructure/Persistence/Configurations
```

## Testing Expectations

Use unit tests for:

- domain invariants
- domain state transitions
- application service orchestration
- validation and expected exceptions
- repository abstraction interactions

Use integration tests for:

- real database behavior
- API pipeline behavior
- serialization and middleware behavior
- multi-layer scenarios that mocks cannot validate well

The integration test project currently exists but is not yet developed.

## Documentation Expectations

Update documentation when a change affects:

- routes or payloads
- setup commands
- architecture decisions
- runtime dependencies
- database schema
- frontend API assumptions
- planned versus implemented behavior

`AGENTS.md` is the primary instruction file for AI agents. Files in `docs/` are the durable project reference.

