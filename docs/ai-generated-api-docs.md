# Warehouse Management API - Comprehensive Documentation

Welcome to the documentation for the **Warehouse Management API** system. This repository contains a production-ready, distributed, microservice-enabled API built using **Domain-Driven Design (DDD)**, **Command Query Responsibility Segregation (CQRS)** with **MediatR**, **PostgreSQL**, **Redis**, **RabbitMQ**, **MinIO**, **Hangfire**, and **Serilog**.

---

## Table of Contents

1. [API README & System Overview](#1-api-readme--system-overview)
2. [Prerequisites & Local Environment Setup](#2-prerequisites--local-environment-setup)
3. [Architecture Notes & Design Patterns](#3-architecture-notes--design-patterns)
4. [Endpoint Summaries Matrix](#4-endpoint-summaries-matrix)
5. [Request Examples](#5-request-examples)
6. [Response Examples & Error Formats](#6-response-examples--error-formats)
7. [Testing Strategy](#7-testing-strategy)

---

## 1. API README & System Overview

### Core Objectives & Business Scenario
The **Warehouse Management API** handles warehouse inventory management, product lifecycle tracking, supplier relations, stock level adjustments, document and image attachments, automated stock threshold alerts, and expiration notifications.

### Technology Stack & Frameworks

| Category | Component / Library | Purpose & Context |
| :--- | :--- | :--- |
| **Framework** | .NET 8 / ASP.NET Core Web API | Core runtime & web host |
| **Architecture** | Domain-Driven Design (DDD) & CQRS | Clean separation of Domain, Application, Infrastructure, Presentation |
| **Mediator** | MediatR | CQRS command/query dispatching and pipeline behaviors |
| **Database** | PostgreSQL + EF Core 9 (Npgsql) | Primary relational database persistence |
| **Caching** | Redis (`StackExchange.Redis`) | Query caching & performance optimization with automatic pipeline invalidation |
| **Messaging** | RabbitMQ | Asynchronous event-driven communication between microservices |
| **Object Storage** | MinIO S3 Object Storage | Distributed file storage for product images and supplier documents |
| **Background Jobs** | Hangfire | Periodic background job scheduling (e.g. product expiration checks) |
| **Security & Auth** | Firebase Authentication + JWT Bearer | Role-Based Access Control (RBAC) supporting `ApiUser` and `AdminOnly` policies |
| **Logging & Metrics** | Serilog + Custom Middleware | Structured logging, request timing, correlation ID tracking, and cache tracking |
| **Health Monitoring**| Health Checks & HealthChecks UI | Endpoint & infrastructure status checks at `/health` and `/health-ui` |
| **Testing** | xUnit, Moq, FluentAssertions, `WebApplicationFactory` | Unit testing and integration testing suite |

### Solution Project Structure

```
warehouse-management-api-SamerD/
├── docker-compose.yaml                        # Root RabbitMQ compose configuration
├── warehouse-management-api/
│   ├── docker-compose.yaml                    # MinIO object storage compose configuration
│   ├── Warehouse.Domain/                      # Core domain entities, value objects, exceptions, repository contracts
│   ├── Warehouse.Application/                 # CQRS Commands/Queries, DTOs, ViewModels, Integration Events, Pipeline Behaviors
│   ├── Warehouse.Infrastructure/              # EF Core DbContext, Repositories, MinIO Storage, RabbitMQ Publisher, Firebase Auth
│   ├── Warehouse.Presentation/                # ASP.NET Core API Controllers, Middleware, Filters, Program.cs
│   └── tests/                                 # Unit & Integration tests for Domain, Application, and API
└── Warehouse.Notifications.Api/               # Microservice for consuming RabbitMQ events and presenting notifications
    ├── Notifications.Domain/
    ├── Notifications.Application/
    ├── Notifications.Infrastructure/
    └── Notifications.Presentation/            # Notifications REST API controllers
```

---

## 2. Prerequisites & Local Environment Setup

### Prerequisites
- **.NET 8.0 SDK**
- **Docker Desktop** (or Docker Engine + Docker Compose)
- **PostgreSQL Server** (or running via local container)
- **Firebase Project Credentials** (for JWT authentication verification)

### Environment Variables (`.env`)
Create a `.env` file in the root or presentation directory with the following configuration:

```env
# Database & Redis
DefaultConnection=Host=localhost;Database=WarehouseDb;Username=postgres;Password=postgres
Redis=localhost:6379

# MinIO Configuration
MINIO_ENDPOINT=localhost:9000
MINIO_ACCESS_KEY=minioadmin
MINIO_SECRET_KEY=minioadmin
MINIO_USE_SSL=false
MINIO_BUCKET=warehouse-attachments

# RabbitMQ Configuration
RABBITMQ_HOST=localhost
RABBITMQ_USER=warehouse
RABBITMQ_PASS=warehouse

# Firebase Configuration
PROJECT_ID=your-firebase-project-id
API_KEY=your-firebase-api-key
AUTHDOMAIN=your-firebase-auth-domain
```

### Infrastructure Startup via Docker Compose

1. **Launch RabbitMQ:**
   ```bash
   docker-compose -f docker-compose.yaml up -d
   ```
   *RabbitMQ Management UI is accessible at `http://localhost:15672` (User: `warehouse`, Password: `warehouse`).*

2. **Launch MinIO Object Storage:**
   ```bash
   docker-compose -f warehouse-management-api/docker-compose.yaml up -d
   ```
   *MinIO Console is accessible at `http://localhost:9001` (User: `minioadmin`, Password: `minioadmin`).*

3. **Apply Database Migrations:**
   ```bash
   cd warehouse-management-api/Warehouse.Infrastructure
   dotnet ef database update --startup-project ../Warehouse.Presentation
   ```

4. **Run Main API:**
   ```bash
   cd warehouse-management-api/Warehouse.Presentation
   dotnet run
   ```
   *Swagger UI available at: `https://localhost:7153/swagger` or `http://localhost:5042/swagger`*
   *Health Checks UI available at: `https://localhost:7153/health-ui`*

5. **Run Notifications Microservice:**
   ```bash
   cd Warehouse.Notifications.Api/Notifications.Presentation
   dotnet run
   ```

---

## 3. Architecture Notes & Design Patterns

### Architectural Diagram

```
[ HTTP Clients / Swagger ]
          │
          ▼
┌────────────────────────────────────────────────────────────────────────┐
│ Warehouse.Presentation (API Layer)                                     │
│  - Controllers & Route Handlers                                        │
│  - ExceptionHandlingMiddleware & IdCorrelationMiddleware              │
│  - Firebase JwtBearer Authentication & Policy Authorization            │
└──────────────────────────────────┬─────────────────────────────────────┘
                                   │
                                   ▼
┌────────────────────────────────────────────────────────────────────────┐
│ Warehouse.Application (CQRS Layer via MediatR)                         │
│  - Commands & Queries                                                  │
│  - ProductCacheInvalidationBehavior (Pipeline Behavior)               │
│  - Mapper Profiles & ViewModels                                        │
└──────────────┬───────────────────┬──────────────────────┬──────────────┘
               │                   │                      │
               ▼                   ▼                      ▼
┌─────────────────────────┐ ┌──────────────┐ ┌───────────────────────────┐
│ Warehouse.Domain        │ │ Redis Cache  │ │ Infrastructure           │
│  - Aggregates (Product, │ │  - Query     │ │  - WarehouseDbContext     │
│    Supplier)            │ │    Caching   │ │    (PostgreSQL)          │
│  - Entities & Value     │ │  - Key       │ │  - MinioFileStorage       │
│    Objects              │ │    Tracking  │ │    (Object Storage)      │
│  - Domain Exceptions    │ └──────────────┘ │  - RabbitMqPublisher      │
└─────────────────────────┘                  └─────────────┬─────────────┘
                                                           │
                                                           ▼ (Integration Events)
                                                   ┌──────────────┐
                                                   │   RabbitMQ   │
                                                   └───────┬──────┘
                                                           │
                                                           ▼
                                            ┌───────────────────────────┐
                                            │ Notifications Microservice│
                                            └───────────────────────────┘
```

### Tactical Domain-Driven Design (DDD) Patterns
- **Aggregates & Aggregate Roots:**
  - `Product` ([Product.cs](file:///c:/Users/salim/OneDrive/Desktop/INMIND/warehouse-management-api-SamerD/warehouse-management-api/Warehouse.Domain/Products/Product.cs)): Controls inventory stock counts, prices, expiration dates, image attachments, and stock movements. Enforces invariants such as non-negative stock and positive pricing.
  - `Supplier` ([Supplier.cs](file:///c:/Users/salim/OneDrive/Desktop/INMIND/warehouse-management-api-SamerD/warehouse-management-api/Warehouse.Domain/Suppliers/Supplier.cs)): Manages supplier contact metadata and attached verification documents (`SupplierDocument`).
- **Value Objects & Child Entities:**
  - `ProductImage` ([ProductImage.cs](file:///c:/Users/salim/OneDrive/Desktop/INMIND/warehouse-management-api-SamerD/warehouse-management-api/Warehouse.Domain/Products/ProductImage.cs)): Keyed compositely by `ProductId` + `FileName`.
  - `StockMovement` ([StockMovement.cs](file:///c:/Users/salim/OneDrive/Desktop/INMIND/warehouse-management-api-SamerD/warehouse-management-api/Warehouse.Domain/Products/StockMovement.cs)): Audit trail record capturing stock adjustments (Type: `In`, `Out`, `Damage`, `Audit`).
- **Domain Exceptions:** Strongly-typed business exceptions (e.g., `ProductNotFoundException`, `DuplicateSkuException`, `InsufficientStockException`, `InvalidProductPriceException`) handled automatically by `ExceptionHandlingMiddleware`.

### CQRS & MediatR Pipeline Architecture
- **Queries (Read Operations):** Return lightweight ViewModels (`ProductViewModel`, `SupplierViewModel`, `InventoryDashboardViewModel`). Formatted and stored in Redis cache for ultra-low latency.
- **Commands (Write Operations):** Perform state mutations through repository units of work and trigger targeted cache invalidation.
- **MediatR Pipeline Behavior (`ProductCacheInvalidationBehavior`):** Automatically detects commands implementing `IInvalidatesProductCache` and flushes invalid product cache entries upon successful command execution.

### Event-Driven Messaging & Microservices
When significant domain state changes occur, `RabbitMqEventPublisher` publishes integration events to RabbitMQ:
- `ProductCreatedEvent`
- `StockAdjustedEvent`
- `StockLowDetectedEvent`
- `WarehouseFileUploadedEvent`

The `Warehouse.Notifications.Api` microservice consumes these messages asynchronously, creating notifications accessible via `/api/notifications`.

### Object Storage & Caching Layer
- **MinIO S3 Integration:** Files (images and PDF/doc attachments) are uploaded via stream to MinIO buckets using `MinioFileStorage` ([MinioFileStorage.cs](file:///c:/Users/salim/OneDrive/Desktop/INMIND/warehouse-management-api-SamerD/warehouse-management-api/Warehouse.Infrastructure/Storage/MinioFileStorage.cs)), avoiding heavy database bloat.
- **Redis Cache & Tracking:** Distributed caching via `IDistributedCache` with stats tracked in `CacheStatisticsTracker` ([CacheController.cs](file:///c:/Users/salim/OneDrive/Desktop/INMIND/warehouse-management-api-SamerD/warehouse-management-api/Warehouse.Presentation/Controllers/CacheController.cs)).

---

## 4. Endpoint Summaries Matrix

### Main Warehouse API (`warehouse-management-api`)

| HTTP Method | Route Endpoint | Policy / Authorization | Description |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/products` | `ApiUser` | Returns list of products. Query param `onlyAvailable=true` filters out archived/out-of-stock items. |
| `GET` | `/api/products/{id}` | `ApiUser` | Retrieves product details by ID. |
| `GET` | `/api/products/search` | `ApiUser` | Searches products by partial `name` or `supplier`. |
| `GET` | `/api/products/expiring-soon` | `ApiUser` | Returns products expiring within specified `withinDays` query parameter. |
| `POST` | `/api/products` | `AdminOnly` | Creates a new product aggregate root. |
| `POST` | `/api/products/{id}/quantity` | `AdminOnly` | Updates the stock quantity of a product. |
| `POST` | `/api/products/{id}/price` | `AdminOnly` | Updates product price. |
| `POST` | `/api/products/{id}/image` | `AdminOnly` | Uploads an image attachment for a product to MinIO. |
| `GET` | `/api/products/{id}/images/{fileName}/download` | `ApiUser` | Downloads a product image file from MinIO. |
| `POST` | `/api/products/{id}/assign-supplier/{supplierId}` | `AdminOnly` | Links a supplier to a product. |
| `DELETE` | `/api/products/{id}` | `AdminOnly` | Soft-deletes/archives a product. |
| `GET` | `/api/products/server-time` | `ApiUser` | Returns server time formatted based on `Accept-Language` header (`en-US`, `fr-FR`, `ar-LB`). |
| `GET` | `/api/suppliers` | `ApiUser` | Returns list of all suppliers. |
| `GET` | `/api/suppliers/{id}` | `ApiUser` | Retrieves supplier details by ID. |
| `POST` | `/api/suppliers` | `AdminOnly` | Creates a new supplier record. |
| `DELETE` | `/api/suppliers/{id}` | `AdminOnly` | Deactivates a supplier record. |
| `POST` | `/api/suppliers/{id}/documents` | `AdminOnly` | Uploads a supplier compliance document to MinIO. |
| `GET` | `/api/suppliers/{id}/documents/{documentId}` | `ApiUser` | Downloads a supplier document file. |
| `DELETE` | `/api/suppliers/{id}/documents/{documentId}` | `AdminOnly` | Deletes a supplier document file. |
| `GET` | `/api/inventory/dashboard` | `ApiUser` | Returns inventory dashboard metrics (total items, low stock counts, total valuation). |
| `POST` | `/api/stock-adjustments` | `AdminOnly` | Records stock movement adjustments (`In`, `Out`, `Damage`, `Audit`). |
| `GET` | `/api/cache/statistics` | `AdminOnly` | Returns Redis cache performance statistics (hits, misses, active keys). |
| `GET` | `/health` | Anonymous | Health check status endpoint for system services. |
| `GET` | `/health-ui` | Anonymous | Interactive Health Checks Web Dashboard UI. |

### Notifications Microservice (`Warehouse.Notifications.Api`)

| HTTP Method | Route Endpoint | Policy / Authorization | Description |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/notifications` | Anonymous / Standard | Lists system notifications generated by RabbitMQ background integration events. |
| `GET` | `/api/notifications/{id}` | Anonymous / Standard | Retrieves details for a specific notification by ID. |
| `POST` | `/api/notifications/{id}/read` | Anonymous / Standard | Marks a notification as read. |

---

## 5. Request Examples

### 1. Create Product (`POST /api/products`)

```http
POST /api/products HTTP/1.1
Host: localhost:7153
Authorization: Bearer eyJhbGciOiJSUzI1NiIsImtpZCI6...
Content-Type: application/json
X-Correlation-ID: req-98765-abc

{
  "name": "Wireless Ergonomic Mouse",
  "sku": "MOUSE-LOGI-MX3",
  "description": "High precision wireless optical mouse with dual connectivity",
  "price": 99.99,
  "quantityInStock": 150,
  "expiryDate": "2028-12-31T23:59:59Z"
}
```

### 2. Stock Adjustment (`POST /api/stock-adjustments`)

```http
POST /api/stock-adjustments HTTP/1.1
Host: localhost:7153
Authorization: Bearer eyJhbGciOiJSUzI1NiIsImtpZCI6...
Content-Type: application/json

{
  "productId": "prod-10293847",
  "adjustmentType": "In",
  "quantity": 50,
  "reason": "Restock shipment received from vendor"
}
```

### 3. Create Supplier (`POST /api/suppliers`)

```http
POST /api/suppliers HTTP/1.1
Host: localhost:7153
Authorization: Bearer eyJhbGciOiJSUzI1NiIsImtpZCI6...
Content-Type: application/json

{
  "name": "Logitech International",
  "country": "Switzerland",
  "contactEmail": "supply@logitech.com",
  "phoneNumber": "+41-21-863-5111"
}
```

### 4. Upload Product Image (`POST /api/products/{id}/image`)

```http
POST /api/products/prod-10293847/image HTTP/1.1
Host: localhost:7153
Authorization: Bearer eyJhbGciOiJSUzI1NiIsImtpZCI6...
Content-Type: multipart/form-data; boundary=----WebKitFormBoundary7MA4YWxkTrZu0gW

------WebKitFormBoundary7MA4YWxkTrZu0gW
Content-Disposition: form-data; name="file"; filename="mouse_hero.jpg"
Content-Type: image/jpeg

(binary file content...)
------WebKitFormBoundary7MA4YWxkTrZu0gW--
```

### 5. Mark Notification as Read (`POST /api/notifications/{id}/read`)

```http
POST /api/notifications/notif-550e8400-e29b-41d4-a716-446655440000/read HTTP/1.1
Host: localhost:5080
Content-Length: 0
```

---

## 6. Response Examples & Error Formats

### 1. Successful Product Retrieval (`200 OK`)

```json
{
  "id": "prod-10293847",
  "name": "Wireless Ergonomic Mouse",
  "sku": "MOUSE-LOGI-MX3",
  "description": "High precision wireless optical mouse with dual connectivity",
  "price": 99.99,
  "quantityInStock": 200,
  "supplierId": "sup-44029",
  "supplierName": "Logitech International",
  "expiryDate": "2028-12-31T23:59:59Z",
  "isArchived": false,
  "createdAt": "2026-08-07T12:00:00Z",
  "lastUpdatedAt": "2026-08-07T14:30:00Z",
  "images": [
    {
      "fileName": "mouse_hero.jpg",
      "contentType": "image/jpeg",
      "sizeBytes": 245800,
      "uploadedAt": "2026-08-07T13:00:00Z"
    }
  ]
}
```

### 2. Successful Inventory Dashboard (`200 OK`)

```json
{
  "totalProducts": 1420,
  "totalSuppliers": 85,
  "totalQuantityInStock": 45200,
  "totalInventoryValuation": 1245900.50,
  "lowStockProductCount": 12,
  "expiringProductCount": 5
}
```

### 3. Duplicate SKU Conflict (`409 Conflict`)

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.8",
  "title": "Conflict",
  "status": 409,
  "detail": "A product with SKU 'MOUSE-LOGI-MX3' already exists.",
  "instance": "/api/products",
  "correlationId": "req-98765-abc"
}
```

### 4. Validation Error (`400 Bad Request`)

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Validation Failed",
  "status": 400,
  "detail": "One or more validation errors occurred.",
  "errors": {
    "Price": [
      "The Price field must be greater than 0."
    ],
    "Sku": [
      "The Sku field is required."
    ]
  },
  "correlationId": "req-98765-abc"
}
```

### 5. Unauthorized Access (`401 Unauthorized`)

```json
{
  "type": "https://tools.ietf.org/html/rfc7235#section-3.1",
  "title": "Unauthorized",
  "status": 401,
  "detail": "Bearer token is missing or expired.",
  "correlationId": "req-98765-abc"
}
```

---

## 7. Testing Strategy

The project features a full test suite built using **xUnit**, **Moq**, and **Fluent Assertions**:

- **Domain Tests (`Warehouse.Domain.Tests`):** Validates aggregate root invariants, value object behavior, and domain rule calculations.
- **Application Tests (`Warehouse.Application.Tests`):** Tests MediatR Command and Query handlers using mock repositories and mock message publishers.
- **Unit Tests (`Warehouse.Api.UnitTests`):** Tests API controller actions and request mapping logic.
- **Integration Tests (`Warehouse.Api.IntegrationTests`):** Uses `WebApplicationFactory<Program>` to execute end-to-end HTTP request flows against an in-memory/test database harness.

### Executing Tests

To run the complete test suite:

```bash
cd warehouse-management-api
dotnet test --logger "console;verbosity=detailed"
```

To run coverage reports:

```bash
dotnet test --collect:"XPlat Code Coverage"
```
