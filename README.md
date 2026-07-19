# OrderCore

OrderCore is a distributed backend system designed to simulate a real-world order processing workflow.

The system focuses on:
- event-driven architecture
- scalability
- resiliency
- clean architecture principles

## Current Tech Stack

- .NET 10
- ASP.NET Core
- Entity Framework Core
- PostgreSQL
- React
- Vite
- Docker

## Planned

- RabbitMQ
- Redis
- Azure

## Run With Docker

```powershell
docker compose up --build
```

Services:

```text
Frontend: http://localhost:5173
API:      http://localhost:5282
Swagger:  http://localhost:5282/swagger
Postgres: localhost:5432
```

The API applies EF Core migrations automatically when running through Docker Compose.

## Status

🚧 In progress

## Documentation

- [AI agent instructions](AGENTS.md)
- [Project overview](docs/PROJECT_OVERVIEW.md)
- [Architecture](docs/ARCHITECTURE.md)
- [Development guide](docs/DEVELOPMENT.md)
