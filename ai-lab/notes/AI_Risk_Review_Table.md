# Exercise 10 — AI Risk Review for Backend Teams

## Complete Table

| Backend Scenario | AI Risk | Mitigation Strategy |
| :--- | :--- | :--- |
| AI generates wrong validation | Business logic bypass / bug | Multi-scenario automated manual testing |
| AI generates insecure file upload | Remote code execution vulnerability | Senior developer code review & static analysis |
| AI generates incorrect EF logic | Data corruption / performance leak | Human execution plan verification & database profiling |
| AI generates missing auth mappings | Broken object-level security risk | Explicit automated integration security tests |
| AI generates incorrect MediatR command/query segregation | Unintended state mutation during read operations (CQRS violation) | Strict architectural linting and unit testing for handlers |
| AI hallucinates non-existent .NET 8 NuGet packages | Supply chain dependency attack / Build failure | Dependency vulnerability scanning and explicit package lockfiles |
| AI generates incorrect routing for product or supplier endpoints | Unauthorized data exposure or API routing conflicts | API contract testing and OpenAPI/Swagger schema validation |
| AI hardcodes database connection strings or JWT secrets | Severe secrets leakage in source control | Pre-commit hooks for secret detection and mandatory environment variable injection |
