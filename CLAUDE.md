# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

House of Burgesses Services (HOB) is a .NET 9.0 ASP.NET Core Web API project with a focus on observability, distributed tracing, and microservices architecture.

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
- **HOB.Common.Library**: Shared library with cross-cutting concerns
  - `Observability/Healthchecks`: Health check configuration
  - `Shared`: Common utilities (problem details, Swagger, exception handling)

### Key Architectural Patterns

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

The API depends on:
- **SQL Server** (db): Database service with health checks
- **RabbitMQ** (rabbitmq): Message broker with health checks

These are managed via Docker Compose and must be healthy before the API starts.

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
