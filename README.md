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

&#x20; "name": "string",

&#x20; "sku": "string",

&#x20; "description": "string",

&#x20; "price": 0,

&#x20; "quantityInStock": 0,

&#x20; "supplierName": "string",

&#x20; "expiryDate": "2026-01-01T00:00:00Z"

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

&#x20; "name": "string",

&#x20; "country": "string",

&#x20; "contactEmail": "string",

&#x20; "phoneNumber": "string"

}

```

`Id` is generated server-side, `IsActive` defaults to `true`.



\### Assign Supplier to Product POST /api/products/{id}/assign-supplier/{supplierId}

\*\*Validation:\*\*

\- `404` if the product does not exist

\- `404` if the supplier does not exist

\- `400` if the product is archived (`IsArchived == true`)



On success, sets `product.SupplierId` and updates `LastUpdatedAt`.

