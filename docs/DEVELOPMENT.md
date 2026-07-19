# Development Guide

This document collects the routine commands and expectations for working on OrderCore.

## Prerequisites

- .NET SDK compatible with `net10.0`.
- Node.js and npm for the frontend.
- PostgreSQL for local persistence.
- Docker for integration tests that use Testcontainers.
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

## Docker Compose

Run the full local stack from the repository root:

```powershell
docker compose up --build
```

The Compose stack starts:

- PostgreSQL on `localhost:5432`.
- API on `http://localhost:5282`.
- Frontend on `http://localhost:5173`.

The API container sets `Database__ApplyMigrations=true`, so EF Core migrations are applied during startup.

The Compose stack also enables the outbox worker:

```text
Outbox__Enabled=true
Outbox__PollingIntervalSeconds=10
Outbox__BatchSize=20
Outbox__MaxRetryCount=5
```

## Logging

The API uses NLog behind the standard `ILogger<T>` abstractions.

Default targets:

- Console logs for `docker compose logs api`.
- Rolling local files under the API base directory in `logs/`, or `/app/logs` inside the API container.
- Optional UDP network logging for local tools such as Log2Console.

Enable UDP network logging locally with:

```powershell
$env:ORDERCORE_LOG_NETWORK_ENABLED="true"
dotnet run --project src\OrderCore.Api\OrderCore.Api.csproj
```

The local UDP target sends Chainsaw/Log4J XML events to `udp4://127.0.0.1:9998`. In Log2Console, add a UDP receiver/listener for Chainsaw or Log4J XML/NLogViewer-compatible events on port `9998`.

Enable UDP network logging from Docker Compose with:

```powershell
$env:ORDERCORE_LOG_NETWORK_ENABLED="true"
docker compose up --build
```

Docker Compose sends UDP Chainsaw/Log4J XML events to `udp4://host.docker.internal:9998` by default so the API container can reach Log2Console on the Windows host. Override it with `ORDERCORE_LOG_NETWORK_ADDRESS` if needed.

In Docker Compose, API logs are persisted in the `ordercore-api-logs` volume and network logging is disabled by default.

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
- stock reservation and restoration behavior in order workflows

Use integration tests for:

- real database behavior
- API pipeline behavior
- serialization and middleware behavior
- multi-layer scenarios that mocks cannot validate well

The integration test project uses Testcontainers to start a PostgreSQL container and run the API against a real database. Docker must be running before executing the full solution test suite.

## Documentation Expectations

Update documentation when a change affects:

- routes or payloads
- setup commands
- architecture decisions
- runtime dependencies
- database schema
- frontend API assumptions
- planned versus implemented behavior
- durable integration behavior such as outbox messages

`AGENTS.md` is the primary instruction file for AI agents. Files in `docs/` are the durable project reference.

