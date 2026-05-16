# Copilot Instructions

Use the repository-level guidance in `AGENTS.md` as the source of truth for this project.

Key reminders:

- Keep controllers thin and delegate behavior to application services.
- Keep domain invariants inside domain entities.
- Keep persistence behind repository abstractions.
- Add focused xUnit tests with FluentAssertions and Moq when changing domain or application behavior.
- Update `docs/` and `AGENTS.md` when conventions, commands, routes, or architecture change.

