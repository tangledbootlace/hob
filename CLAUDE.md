# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

House of Burgesses Services (HOB) is a .NET 9.0 ASP.NET Core Web API project with Entity Framework Core, MassTransit message bus, and a focus on observability, distributed tracing, and microservices architecture.

### Directory Structure

```
hob/
├── src/                          # Application source code
│   ├── hob.sln                   # .NET solution file
│   ├── HOB.API/                  # ASP.NET Core Web API
│   ├── HOB.Data/                 # Entity Framework Core data layer
│   ├── HOB.Worker/               # Background worker service
│   └── HOB.Common/               # Shared library
├── hob-dashboard/                # Next.js dashboard UI
├── local-containers/             # Local development Docker setup
│   ├── docker-compose.yml
│   ├── docker-compose.infrastructure.yml
│   ├── docker-compose.service.yml
│   ├── .env.example
│   └── data/                     # Persistent data volumes
├── infrastructure/               # Production infrastructure
│   ├── terraform/
│   ├── docker-compose.production.yml
│   ├── docker-compose.infrastructure.yml
│   ├── docker-compose.services.yml
│   ├── docker-compose.worker.yml
│   ├── run-worker.sh
│   └── .env.example
├── docs/                         # Comprehensive documentation
└── .github/workflows/            # CI/CD workflows
```

### Solution Projects

- **HOB.API** (`src/HOB.API/`): ASP.NET Core Web API with CRUD endpoints
- **HOB.Data** (`src/HOB.Data/`): Entity Framework Core data layer with entities and DbContext
- **HOB.Worker** (`src/HOB.Worker/`): Console application for asynchronous report generation
- **HOB.Common** (`src/HOB.Common/`): Shared library with cross-cutting concerns

## Build and Development Commands

### Building the Project

```bash
# Navigate to source directory
cd src

# Build the solution
dotnet build hob.sln

# Build in Release configuration
dotnet build hob.sln -c Release

# Restore NuGet packages
dotnet restore hob.sln
```

### Running the Application

```bash
# Navigate to local containers directory
cd local-containers

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
- **Dashboard**: http://dashboard.hob.localhost
- **Grafana**: http://grafana.hob.localhost (admin/grafana)
- **Prometheus**: http://prometheus.hob.localhost
- **Jaeger**: http://jaeger.hob.localhost
- **RabbitMQ Management**: http://web.rabbitmq.hob.localhost (local/local)
- **Traefik Dashboard**: http://localhost:8080

### Database

- **SQL Server**: localhost:1433
- **User**: sa
- **Password**: Password123 (configurable via .env)

## Architecture

### Source Code Structure

All application code is located in the `src/` directory:

- **src/HOB.API**: Main API project containing endpoints and application-specific logic
  - `Customers/`: Customer CRUD endpoints (Create, Get, List, Update, Delete)
  - `Orders/`: Order CRUD endpoints (Create, Get, List, Update, Delete)
  - `Sales/`: Sale CRUD endpoints (Create, Get, List, Update, Delete)
  - `Reports/`: Report generation endpoint (triggers async worker)
  - `Dashboard/`: Dashboard summary endpoint
  - `Extensions/`: Service registration and endpoint mapping
  - `Dockerfile`: Container image definition
- **src/HOB.Data**: Entity Framework Core data access layer
  - `Entities/`: Domain entities (Customer, Order, Sale)
  - `HobDbContext.cs`: EF Core DbContext with fluent configuration and seed data
- **src/HOB.Worker**: Console application for asynchronous tasks
  - `Consumers/`: MassTransit message consumers
  - `Observers/`: Queue drain observer for graceful shutdown
  - `Services/`: CSV report generation service
  - `Dockerfile`: Container image definition
- **src/HOB.Common**: Shared library with cross-cutting concerns
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

See `src/HOB.API/GetTestEndpoint/` for reference implementation.

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
- `src/HOB.API/appsettings.json`: Base configuration
- `src/HOB.API/appsettings.Development.json`: Development overrides

### Adding New Endpoints

1. Create a new folder in `src/HOB.API` for your feature
2. Create Request, Response, and RequestHandler classes following MediatR pattern
3. Register the endpoint in a new or existing `WebApplicationExtensions.cs` method
4. Use `.WithOpenApi()` to include in Swagger documentation
5. Register handler via MediatR (auto-registered from assembly scan in Program.cs)

See `src/HOB.API/GetTestEndpoint/` for a reference implementation.

### Infrastructure Dependencies

The solution depends on:
- **SQL Server** (db): Database service with health checks (port 1433)
- **RabbitMQ** (rabbitmq): Message broker with health checks (port 5672, management UI on port 15672)
- **Traefik**: Reverse proxy for routing
- **Jaeger**: Distributed tracing (port 4317 for OTLP)
- **Prometheus**: Metrics collection (port 9090)
- **Grafana**: Metrics visualization (port 3000)

These are managed via Docker Compose:
- **Local Development**: `local-containers/docker-compose.infrastructure.yml`
- **Production**: `infrastructure/docker-compose.infrastructure.yml`

All services must be healthy before the application starts.

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

#### Dashboard
- `GET /api/dashboard/summary` - Get dashboard summary statistics

### Worker Service

The HOB.Worker is a console application designed to run as a scheduled job:
- **Trigger**: Cron schedule via GitHub Actions or manual execution
- **Process**: Consumes report generation messages from RabbitMQ
- **Output**: CSV reports in `/reports` directory
- **Lifecycle**: Starts → Processes messages → Waits 30s → Exits gracefully
- **Queue Drain Observer**: Monitors queue and triggers shutdown after 30s of inactivity

## Testing

Currently, no test projects exist in the solution. When adding tests:
- Use xUnit as the testing framework (standard for .NET)
- Name test projects as `[ProjectName].Tests` and place in `src/` directory
- Add test projects to `src/hob.sln`
- Run tests with: `cd src && dotnet test`

## Environment Configuration

### Local Development

Environment variables are configured in `local-containers/.env`. Copy from `local-containers/.env.example` and customize as needed. Default values are suitable for local development.

### Production Deployment

Production environment variables are provided via:
- GitHub Secrets (for sensitive data)
- GitHub Variables (for non-sensitive configuration)
- Portainer Environment Variables

See `infrastructure/.env.example` for the complete list of required variables. **Never commit secrets to version control.**

## Additional Documentation

For more detailed information, see:
- [README.md](README.md) - Project overview and quick start
- [docs/Home.md](docs/Home.md) - Comprehensive documentation hub
- [docs/Infrastructure-Setup.md](docs/Infrastructure-Setup.md) - Production setup guide
- [docs/Deployment-and-Operations.md](docs/Deployment-and-Operations.md) - Operations guide
- [docs/architecture.md](docs/architecture.md) - Architecture details
- [docs/api-endpoints.md](docs/api-endpoints.md) - API endpoint documentation

## TODO Items in Code

The following items are marked as TODO in `src/HOB.API/Program.cs`:
- Add DI services (line 20)
- Add service metrics middleware (line 22)
- Use metrics middleware (line 28)

These indicate planned OpenTelemetry metrics integration beyond the current tracing support.
