# AGENTS.md

Guidance for AI coding agents working on OrderCore.

## Project Intent

OrderCore is an order processing study project that is evolving toward a realistic distributed backend. The current system already has a layered .NET backend, PostgreSQL persistence through Entity Framework Core, and a React/Vite frontend. Planned capabilities include RabbitMQ, Redis, Docker, and Azure.

Prefer small, consistent changes that strengthen the existing architecture. Do not introduce large frameworks, cross-cutting abstractions, or speculative infrastructure unless the current task clearly needs them.

## Repository Shape

- `OrderCore.sln`: solution entry point.
- `src/OrderCore.Api`: ASP.NET Core API, controllers, middleware, application startup.
- `src/OrderCore.Application`: use cases, DTOs, repository abstractions, application exceptions.
- `src/OrderCore.Domain`: entities, domain rules, enums, shared base types.
- `src/OrderCore.Infrastructure`: EF Core `DbContext`, entity configurations, migrations, repository implementations.
- `src/OrderCore.Contracts`: reserved for shared contracts as the system grows.
- `tests/OrderCore.UnitTests`: unit tests for domain and application behavior.
- `tests/OrderCore.IntegrationTests`: integration test project, currently minimal.
- `web/ordercore-web`: React/Vite frontend.
- `docs`: project documentation for humans and agents.

## Architecture Rules

- Keep controllers thin. Controllers should receive requests, call an application service, and translate the result to HTTP.
- Keep business rules close to the domain entities when the rule belongs to entity state or invariants.
- Keep use-case orchestration in `OrderCore.Application` services named with the current pattern, such as `CreateCustomerService` and `GetCustomerByIdService`.
- Keep persistence behind repository abstractions in `OrderCore.Application.Abstractions.Repositories`.
- Implement repository details in `OrderCore.Infrastructure.Repositories`.
- Register infrastructure dependencies in `OrderCore.Infrastructure.DependencyInjection.ServiceCollectionExtensions`.
- Register application services in `src/OrderCore.Api/Program.cs` until a dedicated application DI extension exists.
- Use DTOs at API/application boundaries. Do not expose EF entities directly through controllers.
- Use `CancellationToken` in async service, repository, and controller methods.

## Error Handling

- Use application exceptions from `OrderCore.Application.Common.Exceptions` for expected application failures.
- `ValidationException` maps to `400 Bad Request`.
- `BusinessRuleException` maps to `409 Conflict`.
- `NotFoundException` maps to `404 Not Found`.
- Domain constructors/methods currently throw `ArgumentException` or `InvalidOperationException` for invariant violations.
- Let `GlobalExceptionMiddleware` centralize API error responses.

## Backend Conventions

- Target framework is `net10.0`.
- Nullable reference types and implicit usings are enabled.
- Services expose an `ExecuteAsync(...)` method.
- Repository methods are async and persist changes inside write methods such as `AddAsync`.
- EF Core configurations live under `src/OrderCore.Infrastructure/Persistence/Configurations`.
- Migrations live under `src/OrderCore.Infrastructure/Migrations`.
- When adding a new feature, follow the existing vertical grouping by resource:
  - `Application/<Resource>/Commands`
  - `Application/<Resource>/Queries`
  - `Application/<Resource>/Dtos`
  - API controller route under `api/<resource>`

## Frontend Conventions

- Frontend lives in `web/ordercore-web`.
- Use React Router routes declared in `src/App.tsx`.
- API helpers live under `src/api`.
- Shared layout lives in `src/components/layout.tsx`.
- The current API base URL is `https://localhost:7171` in `src/api/http.ts`.
- Keep UI changes consistent with the existing pages before introducing new component systems.

## Tests

- Unit tests use xUnit, FluentAssertions, and Moq.
- Domain tests should validate entity invariants and state transitions.
- Application tests should mock repository abstractions and verify use-case behavior.
- Add or update tests when changing domain rules, service orchestration, validation behavior, repository contracts, or API-visible behavior.
- Prefer focused tests with names following the existing `Should_..._When_...` style.

## Useful Commands

Run from the repository root unless noted.

```powershell
dotnet restore OrderCore.sln
dotnet build OrderCore.sln
dotnet test OrderCore.sln
dotnet run --project src\OrderCore.Api\OrderCore.Api.csproj
```

Frontend:

```powershell
Set-Location web\ordercore-web
npm install
npm run dev
npm run build
npm run lint
```

Entity Framework migrations:

```powershell
dotnet ef migrations add <MigrationName> --project src\OrderCore.Infrastructure --startup-project src\OrderCore.Api
dotnet ef database update --project src\OrderCore.Infrastructure --startup-project src\OrderCore.Api
```

## Change Discipline

- Always work from a feature branch. Before making changes, check the current branch and create or switch to a new branch when needed.
- Use the `feature/` prefix for new branches unless the task explicitly requires another naming convention.
- Do not commit or push directly to `main`; direct pushes to `main` are not allowed.
- Inspect nearby files before editing.
- Match existing naming, folder layout, and coding style.
- Keep changes scoped to the requested behavior.
- Do not reformat unrelated files.
- Do not revert user changes or unrelated work.
- Update documentation when behavior, commands, routes, architecture, or setup steps change.
- If a task reveals a missing convention, add it here or in `docs/DEVELOPMENT.md` instead of relying on memory.
