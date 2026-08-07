# Warehouse Management API

A enterprise-grade RESTful microservice API for warehouse management, built with **.NET 8**, **Domain-Driven Design (DDD)**, and **CQRS (Command Query Responsibility Segregation)** using **MediatR**.

> 📖 **Comprehensive Documentation:** Full architectural details, endpoint matrix, request/response examples, and deployment guides can be found in [docs/ai-generated-api-docs.md](docs/ai-generated-api-docs.md).

---

## Tech Stack & Ecosystem

- **Framework:** .NET 8 / ASP.NET Core Web API
- **Architecture:** Domain-Driven Design (DDD) & CQRS via MediatR
- **Database & Persistence:** PostgreSQL + Entity Framework Core 9 (Npgsql)
- **Caching:** Redis (`StackExchange.Redis`) with custom MediatR pipeline cache invalidation (`ProductCacheInvalidationBehavior`)
- **Messaging & Event-Driven Microservices:** RabbitMQ (`RabbitMqEventPublisher`) integration between `warehouse-management-api` and `Warehouse.Notifications.Api`
- **Object Storage:** MinIO S3 Object Storage for product images and supplier document attachments
- **Background Jobs:** Hangfire for recurring background tasks (`ExpiryDateCheckJob`)
- **Authentication & Security:** Firebase Auth with JWT Bearer Token validation and Role-Based Access Control (`ApiUser` & `AdminOnly` policies)
- **Logging & Monitoring:** Serilog, `X-Correlation-ID` tracking, timing middleware, Health Checks & HealthChecks UI (`/health`, `/health-ui`)
- **Testing:** xUnit, Moq, Fluent Assertions, `WebApplicationFactory`

---

## Quick Start & Local Setup

### 1. Launch Infrastructure Services (Docker Compose)

Start RabbitMQ:
```bash
docker-compose -f docker-compose.yaml up -d
```

Start MinIO Object Storage:
```bash
docker-compose -f warehouse-management-api/docker-compose.yaml up -d
```

### 2. Configure Environment Variables

Ensure `.env` contains the required configuration for PostgreSQL, Redis, MinIO, RabbitMQ, and Firebase:

```env
DefaultConnection=Host=localhost;Database=WarehouseDb;Username=postgres;Password=postgres
Redis=localhost:6379
MINIO_ENDPOINT=localhost:9000
MINIO_ACCESS_KEY=minioadmin
MINIO_SECRET_KEY=minioadmin
MINIO_BUCKET=warehouse-attachments
RABBITMQ_HOST=localhost
RABBITMQ_USER=warehouse
RABBITMQ_PASS=warehouse
```

### 3. Run Database Migrations & API

```bash
cd warehouse-management-api/Warehouse.Infrastructure
dotnet ef database update --startup-project ../Warehouse.Presentation

cd ../Warehouse.Presentation
dotnet run
```

Access Swagger UI: `https://localhost:7153/swagger`  
Access Health Dashboard: `https://localhost:7153/health-ui`

---

## Documentation Sections

Detailed documentation is available in [docs/ai-generated-api-docs.md](docs/ai-generated-api-docs.md):

1. **[API README & System Overview](docs/ai-generated-api-docs.md#1-api-readme--system-overview)**
2. **[Prerequisites & Local Environment Setup](docs/ai-generated-api-docs.md#2-prerequisites--local-environment-setup)**
3. **[Architecture Notes & Design Patterns](docs/ai-generated-api-docs.md#3-architecture-notes--design-patterns)**
4. **[Endpoint Summaries Matrix](docs/ai-generated-api-docs.md#4-endpoint-summaries-matrix)**
5. **[Request Examples](docs/ai-generated-api-docs.md#5-request-examples)**
6. **[Response Examples & Error Formats](docs/ai-generated-api-docs.md#6-response-examples--error-formats)**
7. **[Testing Strategy](docs/ai-generated-api-docs.md#7-testing-strategy)**