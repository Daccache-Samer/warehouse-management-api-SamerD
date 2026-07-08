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

