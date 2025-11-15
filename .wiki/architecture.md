# Architecture Overview

## System Components

### 1. HOB.API (ASP.NET Core Web API)
- **Purpose**: REST API for CRUD operations and message publishing
- **Responsibilities**:
  - Expose CRUD endpoints for Orders, Customers, Sales
  - Publish report generation messages to RabbitMQ via MassTransit
  - Handle HTTP requests with MediatR pattern
  - Distributed tracing and metrics via OpenTelemetry

### 2. HOB.Data (Class Library)
- **Purpose**: Data access layer with Entity Framework Core
- **Responsibilities**:
  - Entity definitions (Order, Customer, Sale)
  - DbContext configuration
  - Database migrations
  - Repository pattern (optional, can use DbContext directly with MediatR)

### 3. HOB.Worker (Console Application)
- **Purpose**: Azure Container App Job for asynchronous report generation
- **Responsibilities**:
  - MassTransit consumer for report generation messages
  - Queue drain observer for graceful shutdown
  - CSV report generation
  - Cron-triggered execution model

### 4. HOB.Common.Library (Shared Library)
- **Purpose**: Cross-cutting concerns
- **Current Features**: Health checks, telemetry, metrics, problem details
- **Will Add**: Shared message contracts for MassTransit

## Data Model

### Entities

```
Customer
├── CustomerId (PK)
├── Name
├── Email
├── Phone
├── CreatedAt
└── Orders (Navigation)

Order
├── OrderId (PK)
├── CustomerId (FK)
├── OrderDate
├── TotalAmount
├── Status
├── Customer (Navigation)
└── Sales (Navigation)

Sale
├── SaleId (PK)
├── OrderId (FK)
├── ProductName
├── Quantity
├── UnitPrice
├── TotalPrice
├── Order (Navigation)
└── CreatedAt
```

## Message-Driven Workflow

### Report Generation Flow

1. Manager triggers report via API endpoint: `POST /api/reports/generate`
2. API publishes `GenerateReportCommand` message to RabbitMQ
3. Worker consumer receives message
4. Worker queries database for Orders, Customers, Sales
5. Worker generates CSV report with relationship data
6. Worker saves CSV to local filesystem
7. Worker completes message processing
8. Queue drain observer monitors for 30s idle time
9. Worker gracefully exits when queue is empty

### Queue Drain Observer Behavior

- **Idle Timeout**: 30 seconds
- **Reset Condition**: Message received
- **Pause Condition**: Message being processed
- **Action**: Graceful shutdown when timeout reached

## Technology Stack

- **.NET 9.0**: Runtime and framework
- **Entity Framework Core**: ORM for SQL Server
- **MassTransit**: Message bus abstraction over RabbitMQ
- **RabbitMQ**: Message broker
- **SQL Server**: Relational database
- **OpenTelemetry**: Distributed tracing
- **Prometheus**: Metrics collection
- **Docker Compose**: Local development orchestration

## Configuration

### Connection Strings

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=db;Database=HOB;User Id=sa;Password=Password123;TrustServerCertificate=True;",
    "RabbitMQ": "amqp://local:local@rabbitmq:5672"
  }
}
```

### MassTransit Configuration

- **Transport**: RabbitMQ
- **Queue Naming**: `hob-report-generation`
- **Retry Policy**: Exponential backoff (3 retries)
- **Circuit Breaker**: Enabled

## Deployment Model

### HOB.API
- **Type**: Long-running web service
- **Platform**: Azure Container Apps or Kubernetes
- **Scaling**: Horizontal (multiple instances)

### HOB.Worker
- **Type**: Azure Container App Job
- **Trigger**: Cron schedule
- **Scaling**: Single instance per execution
- **Lifecycle**: Start → Process messages → Drain queue → Exit

## Security Considerations

- SQL injection prevention: Use parameterized queries (EF Core handles this)
- Connection string security: Use Azure Key Vault in production
- Message validation: Validate all incoming messages
- API authentication: Add JWT/OAuth in future iterations

## Observability

### Tracing
- All API requests traced via OpenTelemetry
- Database queries traced via EF Core instrumentation
- MassTransit message flow traced

### Metrics
- API request rates and latency
- Database query performance
- Message processing rates
- Worker execution duration

### Health Checks
- Database connectivity
- RabbitMQ connectivity
- Worker heartbeat (future enhancement)
