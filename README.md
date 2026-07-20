# \# Warehouse Management API

# 

A REST API for managing warehouse inventory, built with ASP.NET Core 8 as part of Session 02 of the Inmind Academy REST APIs lab.

##### \## Tech Stack



\- .NET 8 / ASP.NET Core Web API

\- Swashbuckle.AspNetCore (Swagger / OpenAPI)

\- In-memory storage (static `List<T>` stores, no persistence)



##### \## Project Structure

warehouse-management-api/

├── Controllers/

│   ├── ProductsController.cs

│   └── SuppliersController.cs

├── Services/

│   ├── ISupplierService.cs

│   └── SupplierService.cs

├── Models/

│   ├── Product.cs

│   ├── ProductImage.cs

│   └── Supplier.cs

├── Contracts/

│   ├── CreateProductRequest.cs

│   ├── UpdateProductQuantityRequest.cs

│   ├── UpdateProductPriceRequest.cs

│   └── CreateSupplierRequest.cs

├── Data/

│   ├── FakeWarehouseStore.cs

│   └── FakeSuppliers.cs

└── Program.cs



Products talk directly to `FakeWarehouseStore` from the controller. Suppliers go through a service layer (`ISupplierService` / `SupplierService`) so `ProductsController` can validate supplier existence via dependency injection when assigning a supplier to a product.



\## Getting Started



\### Prerequisites

\- .NET 8 SDK

\- An IDE (JetBrains Rider / VS Code / Visual Studio)



\### Run



```bash

dotnet restore

dotnet run

```



Run using the `http` or `https` launch profile (not IIS Express). On startup, navigate to:https://localhost:{port}/swagger

to access the Swagger UI and test all endpoints interactively.



\## Business Scenario



The warehouse stores products that can be added, listed, searched, updated, archived, assigned images, filtered by supplier, and filtered by stock availability. All data is in-memory and resets on restart.



\---



\## Product Endpoints



Base route: `api/products`



\### 1. Get all products GET /api/products

Returns all products sorted by `CreatedAt` descending.



\*\*Query parameters:\*\*

| Param | Type | Description |

|---|---|---|

| `onlyAvailable` | bool | If `true`, filters out archived products and products with `QuantityInStock == 0`. Default `false`. |



\### 2. Get product by id GET /api/products/{id}

Returns `404` if no product matches the given id.



\### 3. Search products GET /api/products/search?name=...\&supplier=...

Partial, case-insensitive match on `Name` and/or `SupplierName`. At least one of `name` or `supplier` must be provided, otherwise returns `400`.



\### 4. Create product POST /api/products

\*\*Body:\*\*

```json

{

\&#x20; "name": "string",

\&#x20; "sku": "string",

\&#x20; "description": "string",

\&#x20; "price": 0,

\&#x20; "quantityInStock": 0,

\&#x20; "supplierName": "string",

\&#x20; "expiryDate": "2026-01-01T00:00:00Z"

}

```

Generates `Id` and `CreatedAt`/`LastUpdatedAt` server-side. Returns `409 Conflict` if the SKU already exists (exact match, case-insensitive).



\### 5. Update quantity POST /api/products/{id}/quantity

\*\*Body:\*\* `{ "quantityInStock": 0 }`



Returns `400` if the quantity is negative. Updates `LastUpdatedAt`.



\### 6. Update price POST /api/products/{id}/price

\*\*Body:\*\* `{ "price": 0 }`



Returns `400` if the price is `<= 0`. Old/new value is logged to the console. Updates `LastUpdatedAt`.



\### 7. Upload image POST /api/products/{id}/image

`multipart/form-data`, field name `file`.



\- Only `.jpg`, `.jpeg`, `.png` accepted

\- Max size 2 MB

\- Saved to `wwwroot/uploads`



\### 8. Delete product (soft delete) DELETE /api/products/{id}

Sets `IsArchived = true`. The product is never removed from the store.



\### 9. Get warehouse server time GET /api/products/server-time

Header: `Accept-Language: en-US | fr-FR | ar-LB`



Returns the current server time formatted according to the given culture (defaults to `en-US`).



\---



\## Homework — Supplier Module \& Product-Supplier Link



\### Supplier Endpoints



Base route: `api/suppliers`



| Method | Route | Description |

|---|---|---|

| GET | `/api/suppliers` | Get all suppliers |

| GET | `/api/suppliers/{id}` | Get a supplier by id (`404` if not found) |

| POST | `/api/suppliers` | Create a new supplier |

| DELETE | `/api/suppliers/{id}` | Deactivate a supplier (soft delete — sets `IsActive = false`) |



\*\*Create supplier body:\*\*

```json

{

\&#x20; "name": "string",

\&#x20; "country": "string",

\&#x20; "contactEmail": "string",

\&#x20; "phoneNumber": "string"

}

```

`Id` is generated server-side, `IsActive` defaults to `true`.



\### Assign Supplier to Product POST /api/products/{id}/assign-supplier/{supplierId}

\*\*Validation:\*\*

\- `404` if the product does not exist

\- `404` if the supplier does not exist

\- `400` if the product is archived (`IsArchived == true`)



On success, sets `product.SupplierId` and updates `LastUpdatedAt`.

## Session 03 — DDD Architecture Refactor

### Why This Refactor

This session restructures the same project — same repository, same public API behavior — into
four layers with a strict dependency direction:
`Warehouse.Domain` has zero project references and zero NuGet package references — it doesn't
know ASP.NET Core exists, doesn't know EF Core exists, doesn't know how data is stored. Every
other layer depends on it; it depends on nothing.

### Layer Responsibilities

|Layer|Responsibility|Depends on|Contains|
|-|-|-|-|
|**Warehouse.Domain**|Core business rules and entities|*(nothing)*|`Product`, `Supplier`, `StockMovement`, `WarehouseItem`, `ProductImage`, repository interfaces (`IProductRepository`, `ISupplierRepository`), `IFileStorage`, `DomainException`|
|**Warehouse.Application**|Orchestrates use cases; contains no business rules of its own|Domain|MediatR `Command`/`Query` + `Handler` pairs (one folder per use case), `ProductDto`/`SupplierDto`, `NotFoundException`, `ValidationException`, `ConflictException`|
|**Warehouse.Infrastructure**|Technical implementation details|Domain|`ProductRepository`, `SupplierRepository` (in-memory), `LocalFileStorage`|
|**Warehouse.Presentation**|HTTP entry point only|Application, Infrastructure (composition root only — see note below)|Controllers, `Contracts` (request DTOs), `ExceptionHandlingMiddleware`, `Program.cs`|

Business rules that used to live as scattered `if` statements across controller actions now live
once, on the domain entities themselves:

* Product name and SKU are required — enforced in `Product.Create`
* Price must be greater than zero — enforced in `Product.Create` and `Product.UpdatePrice`
* Quantity cannot be negative — enforced in `Product.Create` and `Product.UpdateQuantity`
* Archived products cannot be updated — enforced in every `Product` mutation method
* Inactive suppliers cannot be assigned to products — enforced in `Product.AssignSupplier`

### CQRS with MediatR

Product and Supplier use cases are split into **Commands** (create, update, archive, assign,
deactivate — anything that changes state) and **Queries** (get, list, search — anything that only
reads state), each with its own `IRequest`/`IRequestHandler` pair, dispatched via MediatR.

> \*\*Note:\*\* MediatR is pinned to version `\[12.5.0]` in `Warehouse.Application.csproj`. Versions
> 13.0.0 and later require a commercial license; 12.5.0 is the last version released under the
> free Apache 2.0 license.

### Refactored Endpoints

All Session 02 endpoints preserve their original behavior and status codes. Internally, every
action now does: bind HTTP input → `\_mediator.Send(...)` → return status code. No business logic
or direct storage access remains in any controller.

**Products** (`api/products`)

|Method|Route|Use Case (Command/Query)|
|-|-|-|
|GET|`/api/products`|`ListProductsQuery`|
|GET|`/api/products/{id}`|`GetProductByIdQuery`|
|GET|`/api/products/search`|`SearchProductsQuery`|
|POST|`/api/products`|`CreateProductCommand`|
|POST|`/api/products/{id}/quantity`|`UpdateProductQuantityCommand`|
|POST|`/api/products/{id}/price`|`UpdateProductPriceCommand`|
|POST|`/api/products/{id}/image`|`AddProductImageCommand`|
|DELETE|`/api/products/{id}`|`ArchiveProductCommand` (soft delete)|
|GET|`/api/products/server-time`|*(no use case — pure computation, stays in controller)*|
|POST|`/api/products/{id}/assign-supplier/{supplierId}`|`AssignSupplierToProductCommand`|

**Suppliers** (`api/suppliers`)

|Method|Route|Use Case (Command/Query)|
|-|-|-|
|GET|`/api/suppliers`|`ListSuppliersQuery`|
|GET|`/api/suppliers/{id}`|`GetSupplierByIdQuery`|
|POST|`/api/suppliers`|`CreateSupplierCommand`|
|DELETE|`/api/suppliers/{id}`|`DeactivateSupplierCommand` (soft delete)|

### Error Handling

A single `ExceptionHandlingMiddleware` maps domain/application exceptions to HTTP status codes
centrally, instead of repeating `try/catch` in every controller action:

|Exception|Status Code|
|-|-|
|`NotFoundException`|404|
|`ConflictException`|409|
|`ValidationException`|400|
|`DomainException`|400|



### Tests

Two test projects added under `tests/`:

* **Warehouse.Domain.Tests** — domain rule tests, no mocking required
* **Warehouse.Application.Tests** — use case tests, mocked repositories via NSubstitute

|Test|Result|
|-|-|
|`Create\_WithZeroOrNegativePrice\_Throws`|✅|
|`UpdatePrice\_WithZeroOrNegativeValue\_Throws`|✅|
|`Create\_WithNegativeQuantity\_Throws`|✅|
|`UpdateQuantity\_WithNegativeValue\_Throws`|✅|
|`UpdatePrice\_OnArchivedProduct\_Throws`|✅|
|`UpdateQuantity\_OnArchivedProduct\_Throws`|✅|
|`AssignSupplier\_WithInactiveSupplier\_Throws`|✅|
|`AssignSupplier\_WithActiveSupplier\_Succeeds`|✅|
|`Handle\_WithValidRequest\_CallsRepositoryAddAsync`|✅|
|`Handle\_WithDuplicateSku\_ThrowsAndDoesNotCallAddAsync`|✅|

**10/10 passed.** Run with:

```bash
dotnet test
```

!\[Test results](docs/screenshots/session03-test-results.png)

## Session 04 — Databases, EF Core & LINQ

### Overview

This session connects the warehouse system to a real Postgres database, built twice:

- **Database First** (`session-04-database-connection-db-first`) — scaffolded from an existing schema.
- **Code First** (`session-04-database-connection-code-first`) — migrations generated from the existing Session 03 Domain entities. **This is the branch that will be merged.**

Both connect to Postgres running in Docker.

```bash
docker run --name Warehouse-DB \
  -e POSTGRES_USER=username \
  -e POSTGRES_PASSWORD=1234 \
  -e POSTGRES_DB=postgresdb \
  -p 5434:5432 \
  -d postgres:18
```

---

### Database First

Scaffolded directly from a manually-created `WarehouseDbFirst` database (`Products`, `Suppliers`, `ProductImages` tables) using:

```bash
dotnet ef dbcontext scaffold "Host=localhost;Port=5434;Database=WarehouseDbFirst;Username=username;Password=1234" \
  Npgsql.EntityFrameworkCore.PostgreSQL -o Models
```

This generates plain, convention-based entity classes and a `WarehouseDbFirstContext` — deliberately **not** the same as the Session 03 Domain entities. Since these scaffolded types have no business invariants and would require Application to reference Infrastructure directly to route them through MediatR (violating the Domain-only dependency rule), the five required LINQ endpoints were added directly onto `ProductsController` on this branch only, injecting `WarehouseDbFirstContext` alongside `IMediator`.

**Endpoints added (DB First branch only):**

| Method | Route | LINQ concept demonstrated |
|---|---|---|
| GET | `/api/products/by-supplier?supplierName=...&sortOrder=asc\|desc` | Filtering + conditional sorting |
| GET | `/api/products/group-by-expiry-year` | `GroupBy` |
| GET | `/api/products/group-by-expiry-year-and-country` | `GroupBy` on a composite key, joined with Supplier |
| GET | `/api/products/count` | Aggregation (`CountAsync`) |
| GET | `/api/products/paged?pageNumber=...&pageSize=...` | Server-side pagination (`Skip`/`Take`) |

---

### Code First (merged)

The real Session 03 `Product`/`Supplier` Domain entities were mapped to Postgres via EF Core migrations — no new entity classes, no duplication.

**WarehouseDbContext** (`Warehouse.Infrastructure/Persistence/CodeFirst/WarehouseDbContext.cs`) required minimal explicit Fluent API configuration, specifically because of choices made in Session 3's rich domain model:

- `Product.SupplierId` has no navigation property on either side (a deliberate Session 3 decoupling decision) — EF's convention-based FK discovery can't detect it without being told explicitly via `HasForeignKey`.
- `ProductImage` has no `Id` property by design uses a composite key (`ProductId`, `FileName`) instead.

No custom configuration was needed for entity IDs — EF Core's default convention already treats `string` primary keys as client-generated (unlike `int`/`Guid`, there's no built-in generation strategy for `string`), matching how `Product.Create()`/`Supplier.Create()` already assign IDs before the entity reaches the database.

**Migration:**

```bash
dotnet ef migrations add InitialCreate \
  --project Warehouse.Infrastructure \
  --startup-project Warehouse.Presentation \
  --context WarehouseDbContext

dotnet ef database update \
  --project Warehouse.Infrastructure \
  --startup-project Warehouse.Presentation \
  --context WarehouseDbContext
```

#### Repositories now backed by EF Core

`ProductRepository`/`SupplierRepository` (`Warehouse.Infrastructure/Persistence/CodeFirst/`) were updated in place to implement `IProductRepository`/`ISupplierRepository` against `WarehouseDbContext`, replacing the Session 3 in-memory `List<T>` implementations entirely

Key changes from the in-memory version:

- **`UpdateAsync` now does real work.** In-memory, this was a documented no-op (mutating a reference type in a `List<T>` needs no explicit save). Against a real database, it calls `SaveChangesAsync()`. Since `WarehouseDbContext` is scoped per-request, the same context instance tracks an entity across a handler's `GetByIdAsync` → domain-method mutation → `UpdateAsync` sequence — EF's change tracker detects mutations made via private setters through domain methods (e.g. `product.UpdatePrice(...)`) just as it would public ones, so no explicit `.Update()` call is needed.
- **`GetBySkuAsync` uses `EF.Functions.ILike`**, not `.Equals(..., OrdinalIgnoreCase)` — `StringComparison.OrdinalIgnoreCase` has no SQL translation; `ILike` is Npgsql's case-insensitive match, translating to Postgres's native `ILIKE`.
- **`GetByIdAsync` eager-loads the `_images` backing field** (`.Include("_images")`) — required specifically for `AddProductImageHandler`, which fetches a product and mutates its image collection; without eager loading, EF wouldn't be tracking that collection and the new image wouldn't persist.
- **Read-only queries use `.AsNoTracking()`** (`GetAllAsync`, `GetBySkuAsync`) to skip unnecessary change-tracking overhead.

**DI registration changed from `AddSingleton` to `AddScoped`**

```csharp
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ISupplierRepository, SupplierRepository>();
```

`AddDbContext` registers `WarehouseDbContext` as **Scoped** by default. A `Singleton` repository depending on a `Scoped` context throws `InvalidOperationException: Cannot consume scoped service ... from singleton` at startup — this is a hard DI lifetime rule, not a style choice.

#### AutoMapper + ViewModels

`ProductDto`/`SupplierDto` were renamed to **`ProductViewModel`/`SupplierViewModel`** (plain mutable classes, not records), mapped from Domain entities via AutoMapper instead of the previous static `FromDomain(...)` factory methods:

```csharp
public class ProductMappingProfile : Profile
{
    public ProductMappingProfile()
    {
        CreateMap<Product, ProductViewModel>();
    }
}
```

Every handler returning a ViewModel injects `IMapper` and calls `_mapper.Map<ProductViewModel>(product)`. Handlers with no return value (`ArchiveProductHandler`, `DeactivateSupplierHandler` — both `IRequestHandler<TCommand>` with no generic result type) needed no AutoMapper changes at all, since there's nothing to map for a `204 No Content` response.

> **Note:** AutoMapper is pinned to version `14.0.0` in `Warehouse.Application.csproj`. Versions 15.0.0 and later require a commercial license; 14.0.0 is the last version released under the free MIT license.

## Session 05 Lab - Harden Warehouse Management API

### Comparison between filters and middleware:
Middleware operates on HttpContext, it has no knowledge of MVC concepts, it handles HTTP concerns like: CORS, authentication, authorization, correlation ID tracking. request logging/timing. 
Filters run within the MVC action execution pipeline, they are fully aware of MVC metadata, action parameters and results and model binding state, they handle controller-specific or action-specific concerns like: result formatting, validation checks and action-level logging. 

### New middleware componenets

1-Id correlation Middleware: It extracts "X-Correlation-ID" from request headers and assigns it to "context.TraceIdentifier", then it appends it to response headers. It also begins a logging scope so all logs related to the same request share the same correlation identifier.
2-RequestTiming Middleware: It uses a stopwatch to record elapsed time for request execution, then it appends "X-response-Time" to the response header. I also added a log warning if duration surpasses 1000 ms which is an arbitrary number.
3-ExceptionHandlingMiddleware: It catches any unhandled exceptions globally, maps exception types to HTTP status cods and serializes a structured "ApiResponse" returning error code, massage and trace id while hiding raw stack trace from clients.

### New MVC Filters

1-ModelValidationFilter: It intercepts requests during the "OnactionExecuting" phase and checks if the model of the input is good according to validation, if it is not it returns a "400 Bad request". I disabled the standard MVC model validation so the pipeline uses my custom filter instead.
2-ActionLoggingFilter: It logs action details on entry and exit.

### New Endpoints

1-POST "/api/stock-adjustements": It record stock changes (increases or decreases) with a clear business reason.
2-GET "/api/inventory/dashboard": It gives an overview of products, low stock alerts, inventory values and supplier numbers.
3-GET "/api/metadata/validation/{dtoName}": Exposes metadata.

## Session 06 Lab - Observability and Performance

### 1. Localization
- **Request Localization:** Configured the application to support multiple cultures (`en` and `fr`).
- **Resource Files:** Used `SharedResources.resx` to provide localized error messages and responses.
- **Swagger Integration:** Enabled changing the request culture directly from the Swagger UI.

### 2. Structured Logging with Serilog
- **Configuration:** Replaced the default .NET logger with Serilog.
- **Sinks:** Logs are written to both the Console and rolling log files (`Logs/log-.txt`).
- **Structured Events:** Logged critical business events (e.g., product creation, stock adjustments, archiving) with structured properties for easier querying.
- **Slow Request Logging:** (Challenge) Enhanced the `RequestTimingMiddleware` to warn when any API request exceeds a 500ms execution threshold.

### 3. Caching with Redis
- **Distributed Caching:** Integrated `IDistributedCache` using `StackExchange.Redis` to cache expensive query results (`GetProductById`,`GetSupplierById`, `ListProducts`, `ListSuppliers`).
- **Cache Invalidation:** Ensured that cache entries are explicitly removed (`RemoveAsync`) whenever a write operation (Create, Update, Adjust Stock, Archive) affects the cached entities.
- **Cache Statistics:** Implemented a custom `CacheStatisticsTracker` and a `CacheController` to expose hit/miss counts, last refresh times, and currently cached keys on `/api/cache/statistics`.

### 4. Health Checks
- **Dependency Monitoring:** Added specific health checks for both the PostgreSQL database (`AspNetCore.HealthChecks.Npgsql`) and Redis (`AspNetCore.HealthChecks.Redis`).
- **Health Checks UI:** Exposed a `/health` endpoint and configured the visual dashboard at `/health-ui` to monitor system status.

### 5. Background Jobs with Hangfire
- **Recurring Tasks:** Implemented an `ExpiryDateCheckJob` scheduled to run automatically (e.g., hourly).
- **In-Memory / Postgres Storage:** Hangfire is configured to persist job states and schedules reliably.
- **Expiry Logic:** The job queries products expiring within 30 days and logs the affected products.
- **Auto-Archiving:** (Challenge) Enhanced the background job to automatically archive any product that has been expired for more than 7 days, maintaining warehouse data hygiene without manual intervention.