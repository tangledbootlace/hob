# House of Burgesses Services (HOB)

A modern .NET 9.0 microservices application with observability-first architecture, featuring REST API, message-driven workflows, and comprehensive infrastructure automation.

## 🚀 Features

- **REST API** - ASP.NET Core Web API with CRUD operations for Customers, Orders, and Sales
- **Message-Driven Architecture** - MassTransit with RabbitMQ for asynchronous processing
- **Worker Service** - Background job processor for report generation
- **Next.js Dashboard** - Modern web interface for data management
- **Observability** - OpenTelemetry, Prometheus, Grafana, and Jaeger integration
- **Infrastructure as Code** - Terraform for Portainer-based deployment
- **CI/CD** - GitHub Actions for automated builds and deployments

## 📁 Project Structure

```
hob/
├── src/                          # Application source code
│   ├── hob.sln                   # .NET solution file
│   ├── HOB.API/                  # REST API project
│   ├── HOB.Data/                 # Entity Framework Core data layer
│   ├── HOB.Worker/               # Background worker service
│   └── HOB.Common/               # Shared libraries and utilities
├── hob-dashboard/                # Next.js dashboard UI
├── local-containers/             # Local development Docker setup
│   ├── docker-compose.yml        # Main compose file
│   ├── docker-compose.infrastructure.yml
│   ├── docker-compose.service.yml
│   ├── .env.example              # Environment variables template
│   └── data/                     # Persistent data volumes
├── infrastructure/               # Production infrastructure
│   ├── terraform/                # Terraform configurations
│   ├── docker-compose.production.yml
│   ├── docker-compose.infrastructure.yml
│   ├── docker-compose.services.yml
│   ├── docker-compose.worker.yml
│   ├── run-worker.sh             # Worker execution script
│   └── .env.example              # Production environment template
├── docs/                         # Comprehensive documentation
│   ├── Home.md                   # Documentation hub
│   ├── Infrastructure-Setup.md
│   ├── Deployment-and-Operations.md
│   └── ...                       # Additional guides
├── .github/workflows/            # CI/CD workflows
└── CLAUDE.md                     # Development guide for Claude Code
```

## 🏗️ Architecture

### Technology Stack

**Backend**
- .NET 9.0 (ASP.NET Core Web API)
- Entity Framework Core 9.0 (Code-First)
- MassTransit (Message Bus)
- MediatR (CQRS Pattern)

**Frontend**
- Next.js 14+ (React)
- TypeScript
- Tailwind CSS

**Infrastructure**
- SQL Server 2022
- RabbitMQ
- Prometheus & Grafana (Metrics)
- Jaeger (Distributed Tracing)
- Traefik (Reverse Proxy)

**Deployment**
- Docker & Docker Compose
- Portainer (Production)
- Terraform (IaC)
- GitHub Actions (CI/CD)

### Design Patterns

- **CQRS with MediatR** - Clean separation of commands and queries
- **Repository Pattern** - Entity Framework Core DbContext
- **Message-Driven Architecture** - Async processing via RabbitMQ
- **Observability-First** - Distributed tracing, metrics, and logging built-in

## 🚀 Quick Start

### Prerequisites

- Docker Desktop
- .NET 9.0 SDK (for local development)
- Node.js 18+ (for dashboard development)

### Local Development

1. **Clone the repository**
   ```bash
   git clone https://github.com/tangledbootlace/hob.git
   cd hob
   ```

2. **Set up environment variables**
   ```bash
   cd local-containers
   cp .env.example .env
   # Edit .env if needed
   ```

3. **Start all services**
   ```bash
   docker-compose up
   ```

4. **Access the application**
   - API: http://hob.api.localhost
   - API Docs (Swagger): http://hob.api.localhost/swagger
   - Dashboard: http://dashboard.hob.localhost
   - Grafana: http://grafana.hob.localhost (admin/grafana)
   - Prometheus: http://prometheus.hob.localhost
   - Jaeger: http://jaeger.hob.localhost
   - RabbitMQ: http://web.rabbitmq.hob.localhost (local/local)

### Building from Source

```bash
cd src
dotnet restore hob.sln
dotnet build hob.sln
dotnet run --project HOB.API
```

## 📚 Documentation

Comprehensive documentation is available in the [docs/](docs/) folder:

- **[Getting Started](docs/Infrastructure-Setup.md)** - Complete setup guide
- **[Deployment Guide](docs/Deployment-and-Operations.md)** - Production deployment
- **[Architecture](docs/architecture.md)** - System design and patterns
- **[API Documentation](docs/api-endpoints.md)** - Endpoint specifications
- **[Development Guide](CLAUDE.md)** - Contributing and development workflow

## 🔧 Development

### Running Tests

```bash
cd src
dotnet test
```

### Database Migrations

```bash
cd src/HOB.Data
dotnet ef migrations add MigrationName
dotnet ef database update
```

### Code Style

The project follows standard .NET coding conventions and includes:
- EditorConfig for consistent formatting
- TODO comments for planned enhancements (see Program.cs)

## 🌐 Production Deployment

Production deployment uses Portainer via Terraform or GitHub Actions. See the [Deployment Guide](docs/Deployment-and-Operations.md) for details.

### GitHub Actions Workflows

- **Build & Push** - Builds Docker images on every push
- **Deploy Infrastructure** - Deploys infrastructure stack via Terraform
- **Deploy Services** - Deploys application services
- **Worker Schedule** - Runs worker jobs on schedule

## 📊 Monitoring

The application includes comprehensive observability:

- **Metrics** - Prometheus scrapes /metrics endpoint
- **Dashboards** - Pre-configured Grafana dashboards
- **Tracing** - OpenTelemetry spans to Jaeger
- **Logging** - Structured logging with correlation IDs
- **Health Checks** - /health endpoints for all services

## 🤝 Contributing

1. Create a feature branch
2. Make your changes
3. Test locally with Docker Compose
4. Update documentation as needed
5. Submit a pull request

See [CLAUDE.md](CLAUDE.md) for detailed development guidelines.

## 📝 License

This project is proprietary software for House of Burgesses Services.

## 🆘 Support

- **Documentation**: [docs/Home.md](docs/Home.md)
- **Issues**: [GitHub Issues](https://github.com/tangledbootlace/hob/issues)
- **CI/CD**: [GitHub Actions](https://github.com/tangledbootlace/hob/actions)

---

**Built with ❤️ using .NET 9.0 and modern DevOps practices**
