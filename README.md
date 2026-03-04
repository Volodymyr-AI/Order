# 🛒 Orders Microservice

A production-ready **Order Management** microservice built with **Clean Architecture**, **CQRS/MediatR**, **Domain-Driven Design** patterns, and event-driven messaging via **Apache Kafka**.

---

## 📐 Architecture

```
Orders.WebAPI          → ASP.NET Core 10 REST API
Order.Application      → CQRS handlers, validators, use-cases (MediatR + FluentValidation)
Order.Core             → Domain model, value objects, domain events
Orders.Persistence     → EF Core 10 + PostgreSQL, Outbox pattern, Kafka publisher
xUnitTesting           → Unit + Integration tests (xUnit, NSubstitute, WebApplicationFactory)
```

The project follows the **Dependency Rule** strictly — inner layers have zero knowledge of outer layers.

---

## ✨ Features

| Area | Details |
|---|---|
| **Domain Model** | Rich aggregate (`CustomerOrder`) with status machine: `Draft → Confirmed → Paid / Cancelled` |
| **Value Objects** | `Money` (currency-aware, rounds away-from-zero), `Currency` (ISO 4217) |
| **CQRS** | Commands & Queries separated; FluentValidation pipeline behavior |
| **Outbox Pattern** | Domain events saved atomically with the order via EF Core `SaveChangesInterceptor`, dispatched by a background worker |
| **Kafka Integration** | `KafkaOutboxPublisher` routes events to `orders.confirmed`, `orders.paid`, `orders.cancelled` topics |
| **Redis Caching** | `GET /orders/{id}` results cached with 5-minute TTL via `IDistributedCache` decorator |
| **Idempotency** | HTTP-level idempotency keys stored in PostgreSQL (scope + identity + hash) |
| **JWT Auth** | `JwtBearer` for production; swappable `TestAuthHandler` for integration tests |
| **Correlation IDs** | Every request tagged with `X-Correlation-Id`, propagated into outbox messages and structured logs |

---

## 🗂️ Domain State Machine

```
           ┌──────────────────────┐
           │         Draft        │
           └──────────┬───────────┘
                      │ Confirm()
           ┌──────────▼───────────┐
           │      Confirmed       │──── Cancel() ───┐
           └──────────┬───────────┘                 │
                      │ Pay()                        │
           ┌──────────▼───────────┐    ┌────────────▼──────────┐
           │         Paid         │    │       Cancelled        │
           └──────────────────────┘    └───────────────────────┘
```

`CustomerOrderConfirmed` and `CustomerOrderPaid` domain events are emitted and published to Kafka via the Transactional Outbox pattern.

---

## 🚀 Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker & Docker Compose](https://www.docker.com/)

### 1. Start infrastructure

```bash
docker compose up -d
```

This starts **PostgreSQL** (`:5433`), **Redis** (`:6379`), **Kafka** + **Zookeeper**.

### 2. Apply migrations

```bash
dotnet ef database update --project Orders.Persistence --startup-project Orders.WebAPI
```

### 3. Configure secrets

```bash
cd Orders.WebAPI
dotnet user-secrets set "Jwt:Key" "your-256-bit-secret-key-here"
dotnet user-secrets set "ConnectionStrings:OrdersDb" "Host=localhost;Port=5433;Database=orders;Username=postgres;Password=postgres"
```

### 4. Run the API

```bash
dotnet run --project Orders.WebAPI
```

Scalar UI (OpenAPI): **https://localhost:7138/scalar**

---

## 🔌 API Endpoints

| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| `POST` | `/api/orders` | — | Create a new order |
| `GET` | `/api/orders/{id}` | ✅ JWT | Get order details *(Redis cached)* |
| `POST` | `/api/orders/{id}/confirm` | ✅ JWT | Confirm order |
| `POST` | `/api/orders/{id}/pay` | ✅ JWT | Pay for order |
| `POST` | `/api/orders/{id}/cancel` | ✅ JWT | Cancel order |

> **Debug only:** `GET /dev/token` — generates a test JWT + userId pair.

### Example: Create Order

```http
POST /api/orders
Content-Type: application/json

{
  "customerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "storeId": 1,
  "items": [
    {
      "productId": "7cb7cd7c-b3e0-4c0c-a0f1-1a1ab2c3d4e5",
      "nameSnapshot": "Coffee Beans 1kg",
      "unitPriceAmount": 12.50,
      "currencyCode": "USD",
      "quantity": 2
    }
  ]
}
```

---

## ⚡ Redis Caching

Order read queries are cached in Redis using a **decorator pattern** around `GetOrderQueryHandler`:

```
GET /api/orders/{id}
        │
        ▼
CachedGetOrderQueryHandler
        │
        ├─── Cache HIT  → return cached OrderDetailsDto (< 1ms)
        │
        └─── Cache MISS → GetOrderQueryHandler → PostgreSQL → cache result (TTL: 5 min)
```

Cache is **invalidated automatically** on any mutation (confirm / pay / cancel) via `OrderCacheKeys.InvalidateOrderAsync`.

```bash
# Redis connection string in appsettings.json
"Redis": {
  "ConnectionString": "localhost:6379"
}
```

---

## 📨 Kafka Topics

| Topic | Trigger |
|---|---|
| `orders.confirmed` | `CustomerOrderConfirmed` domain event |
| `orders.paid` | `CustomerOrderPaid` domain event |
| `orders.cancelled` | *(reserved)* |

Messages carry `event_type` and `correlationId` headers for downstream tracing.

---

## 🧪 Testing

```bash
dotnet test
```

The test suite includes **three layers**:

**Domain tests** — pure unit tests for aggregate logic, value objects and invariants.

**Application tests** — handler tests with mocked repositories (NSubstitute), FluentValidation tests.

**Presentation/Integration tests** — full `WebApplicationFactory` tests using **SQLite in-memory** database, covering HTTP status codes, authentication (JWT + TestScheme), authorization (owner vs. other user), and business rule validation.

```
xUnitTesting/
├── DomainTests/
│   ├── CustomerOrderTests.cs     # aggregate state machine, domain events
│   └── MoneyTests.cs             # rounding, currency guards
├── ApplicationTests/
│   ├── CreateOrderHandlerTests.cs
│   └── CreateOrderCommandValidatorTests.cs
└── PresentationTests/
    ├── OrdersEndpointsTests.cs           # full CRUD + auth flows
    ├── OrdersEndpointsValidationTests.cs # ValidationProblemDetails shape
    └── OrdersJwtAuthTests.cs             # real JWT token validation
```

---

## 🏗️ Tech Stack

| Layer | Technology |
|---|---|
| Runtime | .NET 10 / ASP.NET Core 10 |
| ORM | Entity Framework Core 10 |
| Database | PostgreSQL 16 |
| Cache | Redis 7 (`StackExchange.Redis`) |
| Messaging | Apache Kafka (`Confluent.Kafka`) |
| Mediator | MediatR 14 |
| Validation | FluentValidation 12 |
| Auth | JWT Bearer / custom TestAuthHandler |
| Testing | xUnit, NSubstitute, WebApplicationFactory, SQLite |
| API Docs | Scalar (OpenAPI) |

---

## 🗄️ Database Schema

```
customer_orders
  id, customer_id, store_id
  total_amount, total_currency
  status, currency_code
  created_at, confirmed_at, paid_at, cancelled_at

order_items
  order_id (FK), product_id (PK)
  name_snapshot, quantity
  unit_price_amount, unit_price_currency_code

outbox_messages
  id, type, payload_json, correlation_id
  occurred_at, processed_at, attempts, last_error

idempotency_keys
  id, scope, identity_type, identity_id, key
  request_hash, status, response_code, response_body
  created_at, completed_at
```

---

## 📁 Project Structure

```
Orders.sln
├── Order.Core/                  # Domain (no dependencies)
│   ├── BaseModels/              # CustomerOrder, OrderItem, Money, Currency
│   └── DomainEvents/            # CustomerOrderConfirmed, CustomerOrderPaid
├── Order.Application/           # Use-cases
│   ├── Orders/Commands/         # CreateOrder, ConfirmOrder, PayOrder, CancelOrder
│   ├── Orders/Queries/          # GetOrder (with Redis decorator)
│   └── Common/                  # ValidationBehavior, Exceptions, Cache
├── Orders.Persistence/          # Infrastructure
│   ├── Configurations/          # EF Core entity configs
│   ├── Repositories/            # OrderRepository, EfOutboxStore
│   ├── Kafka/                   # KafkaOutboxPublisher
│   └── Migrations/
├── Orders.WebAPI/               # Presentation
│   ├── Controllers/
│   ├── Middlewares/             # ExceptionHandling, CorrelationId, ClientId
│   ├── Auth/                    # CurrentUser, TestAuthHandler
│   ├── Idempotency/
│   └── Workers/                 # OutboxDispatcherBackgroundService
└── xUnitTesting/
    ├── DomainTests/
    ├── ApplicationTests/
    └── PresentationTests/
```

---

## 🔒 Security

- All mutating endpoints require JWT authentication
- Ownership is verified on every operation (403 Forbidden for non-owners)
- Correlation IDs are generated server-side if not supplied by the client
- Idempotency keys prevent duplicate order processing

---

## 📄 License

MIT
