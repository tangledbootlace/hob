# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

House of Burgesses Services (HOB) is a .NET 9.0 ASP.NET Core Web API project with Entity Framework Core, MassTransit message bus, and a focus on observability, distributed tracing, and microservices architecture.

### Solution Projects

- **HOB.API**: ASP.NET Core Web API with CRUD endpoints
- **HOB.Data**: Entity Framework Core data layer with entities and DbContext
- **HOB.Worker**: Console application for asynchronous report generation
- **HOB.Common.Library**: Shared library with cross-cutting concerns

## Build and Development Commands

### Building the Project

```bash
# Build the solution
dotnet build hob.sln

# Build in Release configuration
dotnet build hob.sln -c Release

# Restore NuGet packages
dotnet restore hob.sln
```

### Running the Application

```bash
# Start all services (API + infrastructure)
docker-compose up

# Start in detached mode
docker-compose up -d

# Rebuild and start
docker-compose up --build

# Start only infrastructure (Prometheus, Grafana, Jaeger, DB, RabbitMQ)
docker-compose -f docker-compose.infrastructure.yml up

# Stop all services
docker-compose down
```

### Accessing Services

All services are routed through Traefik reverse proxy:

- **API**: http://hob.api.localhost
- **Swagger UI**: http://hob.api.localhost/swagger (Development only)
- **Grafana**: http://grafana.hob.localhost (admin/grafana)
- **Prometheus**: http://prometheus.hob.localhost
- **Jaeger**: http://jaeger.hob.localhost
- **RabbitMQ Management**: http://web.rabbitmq.hob.localhost (local/local)
- **Traefik Dashboard**: http://localhost:8080

### Database

- **SQL Server**: localhost:1433
- **User**: sa
- **Password**: Password123

## Architecture

### Project Structure

- **HOB.API**: Main API project containing endpoints and application-specific logic
  - `Customers/`: Customer CRUD endpoints (Create, Get, List, Update, Delete)
  - `Orders/`: Order CRUD endpoints (Create, Get, List, Update, Delete)
  - `Sales/`: Sale CRUD endpoints (Create, Get, List, Update, Delete)
  - `Reports/`: Report generation endpoint (triggers async worker)
  - `Extensions/`: Service registration and endpoint mapping
- **HOB.Data**: Entity Framework Core data access layer
  - `Entities/`: Domain entities (Customer, Order, Sale)
  - `HobDbContext.cs`: EF Core DbContext with fluent configuration and seed data
- **HOB.Worker**: Console application for asynchronous tasks
  - `Consumers/`: MassTransit message consumers
  - `Observers/`: Queue drain observer for graceful shutdown
  - `Services/`: CSV report generation service
- **HOB.Common.Library**: Shared library with cross-cutting concerns
  - `Messages/`: MassTransit message contracts
  - `Observability/Healthchecks`: Health check configuration
  - `Observability/Telemetry`: OpenTelemetry tracing
  - `Observability/Metrics`: Prometheus metrics
  - `Shared`: Common utilities (problem details, Swagger, exception handling)

### Key Architectural Patterns

**Entity Framework Core**: SQL Server database with Code-First migrations
- Entities: Customer, Order, Sale
- Relationships: Customer → Orders → Sales (one-to-many)
- Seed data included for development
- Database auto-created on API startup in Development environment

**MassTransit Message Bus**: RabbitMQ integration for asynchronous processing
- Message Publishing: API publishes GenerateReportCommand
- Message Consumption: Worker consumes messages and generates CSV reports
- Queue Drain Observer: Worker shuts down gracefully after 30s of no messages

**CQRS with MediatR**: All endpoints use the MediatR pattern with Request/Handler separation. New features should follow this structure:

```
[FeatureName]/
├── [FeatureName]Request.cs
├── [FeatureName]RequestHandler.cs
└── [FeatureName]Response.cs
```

See `GetTestEndpoint` folder for reference implementation.

**Extension Methods**: Service registration and middleware configuration use extension methods:
- `ServiceCollectionExtensions.cs` for `IServiceCollection` extensions
- `WebApplicationExtensions.cs` for `WebApplication` extensions

**Observability-First**: The application includes:
- OpenTelemetry integration for distributed tracing (Jaeger)
- Prometheus metrics exposure
- Custom exception handling that records exceptions in Activity spans
- Problem details with `problemId` from distributed trace Activity ID

### Configuration

The API listens on a port configured via environment variable `HOB_SERVICES_ContainerPort` (default: 8080).

Application settings are in:
- `appsettings.json`: Base configuration
- `appsettings.Development.json`: Development overrides

### Adding New Endpoints

1. Create a new folder in `HOB.API` for your feature
2. Create Request, Response, and RequestHandler classes following MediatR pattern
3. Register the endpoint in a new or existing `WebApplicationExtensions.cs` method
4. Use `.WithOpenApi()` to include in Swagger documentation
5. Register handler via MediatR (auto-registered from assembly scan in Program.cs)

### Infrastructure Dependencies

The solution depends on:
- **SQL Server** (db): Database service with health checks (port 1433)
- **RabbitMQ** (rabbitmq): Message broker with health checks (port 5672, management UI on port 15672)
- **Traefik**: Reverse proxy for routing
- **Jaeger**: Distributed tracing (port 4317 for OTLP)
- **Prometheus**: Metrics collection (port 9090)
- **Grafana**: Metrics visualization (port 3000)

These are managed via Docker Compose and must be healthy before the services start.

### API Endpoints

All endpoints are documented in Swagger UI at http://hob.api.localhost/swagger

#### Customers
- `POST /api/customers` - Create customer
- `GET /api/customers/{customerId}` - Get customer by ID
- `GET /api/customers` - List customers (with pagination and search)
- `PUT /api/customers/{customerId}` - Update customer
- `DELETE /api/customers/{customerId}` - Delete customer

#### Orders
- `POST /api/orders` - Create order with sales items
- `GET /api/orders/{orderId}` - Get order by ID
- `GET /api/orders` - List orders (with filters: customerId, status, date range)
- `PUT /api/orders/{orderId}` - Update order status
- `DELETE /api/orders/{orderId}` - Delete order (Pending/Cancelled only)

#### Sales
- `POST /api/sales` - Create sale (adds to existing order)
- `GET /api/sales/{saleId}` - Get sale by ID
- `GET /api/sales` - List sales (with filters: orderId, productName)
- `PUT /api/sales/{saleId}` - Update sale quantity/price
- `DELETE /api/sales/{saleId}` - Delete sale (updates order total)

#### Reports
- `POST /api/reports/generate` - Generate sales report (async via worker)

### Worker Service

The HOB.Worker is a console application designed to run as an Azure Container App Job:
- **Trigger**: Cron schedule (configure in Azure)
- **Process**: Consumes report generation messages from RabbitMQ
- **Output**: CSV reports in `/reports` directory
- **Lifecycle**: Starts → Processes messages → Waits 30s → Exits gracefully
- **Queue Drain Observer**: Monitors queue and triggers shutdown after 30s of inactivity

## Testing

Currently, no test projects exist in the solution. When adding tests:
- Use xUnit as the testing framework (standard for .NET)
- Name test projects as `[ProjectName].Tests`
- Run tests with: `dotnet test`

## TODO Items in Code

The following items are marked as TODO in `Program.cs`:
- Add DI services (line 20)
- Add service metrics middleware (line 22)
- Use metrics middleware (line 28)

These indicate planned OpenTelemetry metrics integration beyond the current tracing support.
