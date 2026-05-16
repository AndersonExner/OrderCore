# Architecture

OrderCore follows a layered architecture with a clean separation between domain rules, application use cases, infrastructure, and HTTP/UI entry points.

## Layers

```text
OrderCore.Api
  depends on Application and Infrastructure

OrderCore.Application
  depends on Domain and Contracts

OrderCore.Infrastructure
  depends on Application, Domain, and Contracts

OrderCore.Domain
  has no project dependency on application, infrastructure, or API

OrderCore.Contracts
  reserved for shared contracts as the system grows
```

## Responsibility by Project

### `OrderCore.Domain`

Contains the core business model:

- `Customer`
- `Product`
- `Order`
- `OrderItem`
- `OrderStatus`
- `BaseEntity`

Domain entities protect their own invariants through constructors and methods. Examples include email normalization, product stock validation, and order status transitions.

### `OrderCore.Application`

Contains use cases and application-level contracts.

Main patterns:

- Commands mutate state, for example `CreateCustomerService`.
- Queries read state, for example `GetCustomerByIdService`.
- DTOs define application input/output shapes.
- Repository interfaces define persistence needs without depending on EF Core.
- Application exceptions express expected failures.

Application services orchestrate domain entities and repositories. They should not know how data is stored.

### `OrderCore.Infrastructure`

Contains EF Core persistence details:

- `AppDbContext`
- entity configurations
- migrations
- repository implementations
- dependency injection registration for infrastructure services

Infrastructure translates repository abstractions into database access.

### `OrderCore.Api`

Contains HTTP concerns:

- controllers
- middleware
- CORS
- Swagger
- service registration
- ASP.NET Core pipeline setup

Controllers should stay thin and delegate behavior to application services.

### `ordercore-web`

Contains the browser client:

- routes in `src/App.tsx`
- pages in `src/pages`
- API client helpers in `src/api`
- shared layout in `src/components/layout.tsx`

## Request Flow

Typical create flow:

```text
HTTP request
  -> Controller
  -> Application service
  -> Domain entity/rules
  -> Repository abstraction
  -> EF Core repository
  -> PostgreSQL
  -> DTO response
  -> HTTP response
```

Typical read flow:

```text
HTTP request
  -> Controller
  -> Query service
  -> Repository abstraction
  -> EF Core repository
  -> DTO response
  -> HTTP response
```

## Error Flow

Expected application failures use exceptions from `OrderCore.Application.Common.Exceptions`.

The API middleware maps them to HTTP responses:

- `ValidationException`: `400 Bad Request`
- `BusinessRuleException`: `409 Conflict`
- `NotFoundException`: `404 Not Found`
- `ArgumentException`: `400 Bad Request`
- unhandled exceptions: `500 Internal Server Error`

## Adding a New Feature

Use the current resource-oriented pattern:

1. Add or update domain behavior in `OrderCore.Domain` when there is a business invariant.
2. Add DTOs in `OrderCore.Application/<Resource>/Dtos`.
3. Add command/query service in `OrderCore.Application/<Resource>/Commands` or `Queries`.
4. Add repository abstraction methods if persistence access is needed.
5. Implement repository methods in `OrderCore.Infrastructure/Repositories`.
6. Add or update EF Core configuration and migrations when the database shape changes.
7. Register new services in `Program.cs` or the appropriate DI extension.
8. Expose the behavior through an API controller.
9. Update frontend API helpers and pages when the UI needs the capability.
10. Add tests for domain rules and application behavior.
11. Update docs when routes, behavior, setup, or architecture change.

