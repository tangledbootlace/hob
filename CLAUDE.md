# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

House of Burgesses Services (HOB) is a .NET 9.0 ASP.NET Core Web API project with Entity Framework Core, MassTransit message bus, and a focus on observability, distributed tracing, and microservices architecture.

### Directory Structure

```
hob/
├── src/                                    # Application source code
│   ├── back-end-dotnet/                    # .NET backend projects
│   │   ├── hob.sln                         # .NET solution file
│   │   ├── HOB.API/                        # ASP.NET Core Web API
│   │   ├── HOB.Data/                       # Entity Framework Core data layer
│   │   ├── HOB.Worker/                     # Background worker service
│   │   └── HOB.Common/                     # Shared library
│   └── front-end-nextjs/                   # Next.js frontend projects
│       └── hob-dashboard/                  # Next.js dashboard UI
├── local-containers/                       # Local development Docker setup
│   ├── docker-compose.yml
│   ├── docker-compose.infrastructure.yml
│   ├── docker-compose.service.yml
│   ├── .env.example
│   └── data/                               # Persistent data volumes
├── infrastructure/                         # Production infrastructure
│   ├── terraform/
│   ├── docker-compose.production.yml
│   ├── docker-compose.infrastructure.yml
│   ├── docker-compose.services.yml
│   ├── docker-compose.worker.yml
│   ├── run-worker.sh
│   └── .env.example
├── docs/                                   # Comprehensive documentation
└── .github/workflows/                      # CI/CD workflows
```

### Solution Projects

- **HOB.API** (`src/back-end-dotnet/HOB.API/`): ASP.NET Core Web API with CRUD endpoints
- **HOB.Data** (`src/back-end-dotnet/HOB.Data/`): Entity Framework Core data layer with entities and DbContext
- **HOB.Worker** (`src/back-end-dotnet/HOB.Worker/`): Console application for asynchronous report generation
- **HOB.Common** (`src/back-end-dotnet/HOB.Common/`): Shared library with cross-cutting concerns
- **hob-dashboard** (`src/front-end-nextjs/hob-dashboard/`): Next.js dashboard UI

## Build and Development Commands

### Building the Project

```bash
# Navigate to backend source directory
cd src/back-end-dotnet

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

#### Backend (.NET)

- **src/back-end-dotnet/HOB.API**: Main API project containing endpoints and application-specific logic
  - `Customers/`: Customer CRUD endpoints (Create, Get, List, Update, Delete)
  - `Products/`: Product/Inventory CRUD endpoints with stock management (Create, Get, List, Update, Delete)
  - `Orders/`: Order CRUD endpoints with product validation (Create, Get, List, Update, Delete)
  - `Sales/`: Sale CRUD endpoints with inventory tracking (Create, Get, List, Update, Delete)
  - `Reports/`: Report generation endpoint (triggers async worker)
  - `Dashboard/`: Dashboard summary endpoint with low stock alerts
  - `Extensions/`: Service registration and endpoint mapping
  - `Dockerfile`: Container image definition
- **src/back-end-dotnet/HOB.Data**: Entity Framework Core data access layer
  - `Entities/`: Domain entities (Customer, Product, Order, Sale)
  - `HobDbContext.cs`: EF Core DbContext with fluent configuration and seed data
- **src/back-end-dotnet/HOB.Worker**: Console application for asynchronous tasks
  - `Consumers/`: MassTransit message consumers
  - `Observers/`: Queue drain observer for graceful shutdown
  - `Services/`: CSV report generation service
  - `Dockerfile`: Container image definition
- **src/back-end-dotnet/HOB.Common**: Shared library with cross-cutting concerns
  - `Messages/`: MassTransit message contracts
  - `Observability/Healthchecks`: Health check configuration
  - `Observability/Telemetry`: OpenTelemetry tracing
  - `Observability/Metrics`: Prometheus metrics
  - `Shared`: Common utilities (problem details, Swagger, exception handling)

#### Frontend (Next.js)

- **src/front-end-nextjs/hob-dashboard**: Next.js dashboard UI
  - `app/`: Next.js 14 app directory with routing
  - `components/`: Reusable React components
  - `Dockerfile`: Container image definition

### Key Architectural Patterns

**Entity Framework Core**: SQL Server database with Code-First migrations
- Entities: Customer, Product, Order, Sale
- Relationships:
  - Customer → Orders (one-to-many)
  - Order → Sales (one-to-many)
  - Product → Sales (one-to-many)
- Automatic inventory management: Stock quantities decrement on sale creation
- Seed data included for development (customers, products, orders, sales)
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

See `src/back-end-dotnet/HOB.API/GetTestEndpoint/` for reference implementation.

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
- `src/back-end-dotnet/HOB.API/appsettings.json`: Base configuration
- `src/back-end-dotnet/HOB.API/appsettings.Development.json`: Development overrides

### Adding New Endpoints

1. Create a new folder in `src/back-end-dotnet/HOB.API` for your feature
2. Create Request, Response, and RequestHandler classes following MediatR pattern
3. Register the endpoint in a new or existing `WebApplicationExtensions.cs` method
4. Use `.WithOpenApi()` to include in Swagger documentation
5. Register handler via MediatR (auto-registered from assembly scan in Program.cs)
6. **REQUIRED**: Create comprehensive unit tests for the request handler (see Testing Requirements section)

See `src/back-end-dotnet/HOB.API/GetTestEndpoint/` for a reference implementation.

**Note**: Pull requests without tests will not be accepted. See the Testing Requirements section below for detailed testing standards.

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

#### Products
- `POST /api/products` - Create product with inventory details
- `GET /api/products/{productId}` - Get product by ID
- `GET /api/products` - List products (with filters: search, category, lowStock, activeOnly)
- `PUT /api/products/{productId}` - Update product details and inventory
- `DELETE /api/products/{productId}` - Delete product (if no associated sales)

#### Orders
- `POST /api/orders` - Create order with sales items (validates product availability and stock)
- `GET /api/orders/{orderId}` - Get order by ID
- `GET /api/orders` - List orders (with filters: customerId, status, date range)
- `PUT /api/orders/{orderId}` - Update order status
- `DELETE /api/orders/{orderId}` - Delete order (Pending/Cancelled only)

#### Sales
- `POST /api/sales` - Create sale (validates stock, adds to existing order, decrements inventory)
- `GET /api/sales/{saleId}` - Get sale by ID
- `GET /api/sales` - List sales (with filters: orderId, productName)
- `PUT /api/sales/{saleId}` - Update sale quantity/price
- `DELETE /api/sales/{saleId}` - Delete sale (updates order total)

#### Reports
- `POST /api/reports/generate` - Generate sales report (async via worker)

#### Dashboard
- `GET /api/dashboard/summary` - Get dashboard summary statistics with low stock alerts

### Worker Service

The HOB.Worker is a console application designed to run as a scheduled job:
- **Trigger**: Cron schedule via GitHub Actions or manual execution
- **Process**: Consumes report generation messages from RabbitMQ
- **Output**: CSV reports in `/reports` directory
- **Lifecycle**: Starts → Processes messages → Waits 30s → Exits gracefully
- **Queue Drain Observer**: Monitors queue and triggers shutdown after 30s of inactivity

## Testing Requirements

**IMPORTANT: All new features and bug fixes MUST include comprehensive unit tests before opening a Pull Request.**

### Testing Standards

#### Backend (.NET)

**Testing Framework**: Use xUnit as the testing framework (standard for .NET)

**Project Structure**:
- Name test projects as `[ProjectName].Tests` (e.g., `HOB.API.Tests`, `HOB.Data.Tests`)
- Place test projects in `src/back-end-dotnet/` directory alongside the project being tested
- Add test projects to `src/back-end-dotnet/hob.sln`
- Use the following folder structure within test projects:
  ```
  HOB.API.Tests/
  ├── Customers/
  │   ├── CreateCustomerRequestHandlerTests.cs
  │   ├── UpdateCustomerRequestHandlerTests.cs
  │   └── DeleteCustomerRequestHandlerTests.cs
  ├── Products/
  │   ├── CreateProductRequestHandlerTests.cs
  │   └── ...
  └── TestHelpers/
      ├── MockDbContext.cs
      └── TestData.cs
  ```

**Required Dependencies**:
- `xUnit` - Testing framework
- `Moq` or `NSubstitute` - Mocking framework
- `FluentAssertions` - Assertion library (recommended)
- `Microsoft.EntityFrameworkCore.InMemory` - For testing with EF Core

**What Must Be Tested**:
1. **Request Handlers** - All MediatR request handlers must have tests covering:
   - Happy path scenarios
   - Edge cases (empty lists, null values, boundary conditions)
   - Error conditions (not found, validation failures, conflicts)
   - Business logic validation

2. **API Endpoints** - Test endpoint mapping and routing (integration tests)

3. **Data Layer** - Test complex queries and data transformations

4. **Validation Logic** - Test all validation rules (unique constraints, required fields, etc.)

5. **Business Rules** - Test inventory updates, order calculations, stock validation, etc.

**Test Naming Convention**:
```csharp
[MethodName]_[Scenario]_[ExpectedResult]

Examples:
- Handle_ValidProduct_CreatesProductSuccessfully
- Handle_DuplicateSKU_ThrowsInvalidOperationException
- Handle_InsufficientStock_ThrowsInvalidOperationException
```

**Running Tests**:
```bash
# Run all tests
cd src/back-end-dotnet && dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test project
dotnet test HOB.API.Tests/HOB.API.Tests.csproj

# Run tests matching a pattern
dotnet test --filter "FullyQualifiedName~Products"
```

**Example Test Structure**:
```csharp
public class CreateProductRequestHandlerTests
{
    private readonly Mock<HobDbContext> _mockDbContext;
    private readonly Mock<ILogger<CreateProductRequestHandler>> _mockLogger;
    private readonly CreateProductRequestHandler _handler;

    public CreateProductRequestHandlerTests()
    {
        _mockDbContext = new Mock<HobDbContext>();
        _mockLogger = new Mock<ILogger<CreateProductRequestHandler>>();
        _handler = new CreateProductRequestHandler(_mockDbContext.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ValidProduct_CreatesProductSuccessfully()
    {
        // Arrange
        var request = new CreateProductRequest(
            SKU: "TEST-001",
            Name: "Test Product",
            Description: "Test Description",
            UnitPrice: 10.00m,
            StockQuantity: 100,
            LowStockThreshold: 10,
            Category: "Test"
        );

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.SKU.Should().Be("TEST-001");
        _mockDbContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DuplicateSKU_ThrowsInvalidOperationException()
    {
        // Arrange - setup mock to return existing product
        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(request, CancellationToken.None)
        );
    }
}
```

#### Frontend (Next.js/React)

**Testing Framework**: Jest with React Testing Library

**Project Structure**:
- Tests are co-located with components: `ComponentName.test.tsx`
- API layer tests: `lib/api/__tests__/`
- Test utilities: `__tests__/utils/`

**What Must Be Tested**:
1. **API Client Functions** - Test all API calls with mocked responses
2. **Server Actions** - Test form actions and data mutations
3. **Components** (when complex logic exists):
   - User interactions
   - Conditional rendering
   - Form validation

**Running Tests**:
```bash
cd src/front-end-nextjs/hob-dashboard

# Run all tests
npm test

# Run tests in watch mode
npm run test:watch

# Run tests with coverage
npm run test:coverage
```

### Coverage Requirements

- **Minimum Coverage**: 70% code coverage for new code
- **Critical Paths**: 90%+ coverage for business-critical features (inventory management, order processing, payment handling)
- **Pull Request Requirement**: PR cannot be merged if tests fail or coverage drops below threshold

### Before Opening a Pull Request

**Checklist**:
- [ ] All new endpoints/handlers have corresponding unit tests
- [ ] All business logic is tested (happy path + error cases)
- [ ] Tests pass locally: `dotnet test` (backend) and `npm test` (frontend)
- [ ] Code coverage meets minimum requirements
- [ ] No test warnings or skipped tests without justification
- [ ] Integration tests added for complex workflows (optional but recommended)

### CI/CD Integration

All tests run automatically on:
- Pull request creation
- Push to PR branch
- Merge to main branch

Tests must pass before PR can be merged. See `.github/workflows/` for CI configuration.

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

The following items are marked as TODO in `src/back-end-dotnet/HOB.API/Program.cs`:
- Add DI services (line 20)
- Add service metrics middleware (line 22)
- Use metrics middleware (line 28)

These indicate planned OpenTelemetry metrics integration beyond the current tracing support.
