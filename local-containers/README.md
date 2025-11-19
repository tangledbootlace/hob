# Local Development Containers

This directory contains Docker Compose configurations for running the HOB application stack locally.

## Overview

The local container setup consists of three Docker Compose files:

- `docker-compose.yml` - Main orchestration file that includes both infrastructure and services
- `docker-compose.infrastructure.yml` - Infrastructure services (database, message broker, observability)
- `docker-compose.service.yml` - Application services (API, worker, dashboard)

## Prerequisites

### Docker Installation

You'll need Docker Engine installed on your system. Docker Desktop is **not recommended for Linux VMs** (especially Proxmox VMs) as it requires KVM virtualization support.

#### Linux (Recommended: Docker Engine)

Install Docker Engine directly instead of Docker Desktop:

```bash
# Install Docker Engine (Debian/Ubuntu)
curl -fsSL https://get.docker.com -o get-docker.sh
sudo sh get-docker.sh

# Add your user to the docker group
sudo usermod -aG docker $USER

# Log out and back in, or run:
newgrp docker

# Verify installation
docker --version
docker compose version
```

**Note for Linux users switching from Docker Desktop to Docker Engine:**

If you previously had Docker Desktop installed, you may need to clean up the credential helper configuration:

```bash
# Edit ~/.docker/config.json and remove the "credsStore" entry
# Change from:
# {"credsStore": "desktop"}
# To:
# {}

# Or run this command to fix it:
echo '{}' > ~/.docker/config.json
```

#### macOS

```bash
# Install Docker Desktop (recommended for macOS)
brew install --cask docker
# Or download from: https://www.docker.com/products/docker-desktop
```

#### Windows

Download and install Docker Desktop from https://www.docker.com/products/docker-desktop

## Setup Steps

### 1. Create Docker Network

The HOB application requires a custom Docker network for service communication:

```bash
docker network create hob-network
```

**Important:** This network must be created before starting any containers. All services communicate through this network.

### 2. Configure Environment Variables

Copy the example environment file and customize as needed:

```bash
cp .env.example .env
```

Review and modify the `.env` file if you want to change:
- Database passwords
- RabbitMQ credentials
- API ports
- Connection strings

**Default values in `.env.example` are suitable for local development.**

### 3. Start the Containers

Start all services (infrastructure + application):

```bash
docker compose up
```

Or run in detached mode:

```bash
docker compose up -d
```

To rebuild containers after code changes:

```bash
docker compose up --build
```

### 4. Start Only Infrastructure

If you want to run the application services manually (e.g., for debugging), you can start only the infrastructure:

```bash
docker compose -f docker-compose.infrastructure.yml up -d
```

Then run the API and dashboard manually:

```bash
# Terminal 1: Run the API
cd ../src/back-end-dotnet/HOB.API
dotnet run

# Terminal 2: Run the dashboard
cd ../src/front-end-nextjs/hob-dashboard
npm run dev
```

## Accessing Services

All services are routed through Traefik reverse proxy:

| Service | URL | Notes |
|---------|-----|-------|
| API | http://hob.api.localhost | Main REST API |
| Swagger UI | http://hob.api.localhost/swagger | API documentation (Development only) |
| Dashboard | http://dashboard.hob.localhost | Next.js frontend |
| Grafana | http://grafana.hob.localhost | Metrics visualization (admin/grafana) |
| Prometheus | http://prometheus.hob.localhost | Metrics collection |
| Jaeger | http://jaeger.hob.localhost | Distributed tracing |
| RabbitMQ Management | http://web.rabbitmq.hob.localhost | Message broker UI (local/local) |
| Traefik Dashboard | http://localhost:8080 | Reverse proxy dashboard |

### Database Access

- **Host:** localhost
- **Port:** 1433
- **Database:** HOB
- **User:** sa
- **Password:** Password123 (configurable in `.env`)

Example connection string:
```
Server=localhost,1433;Database=HOB;User Id=sa;Password=Password123;TrustServerCertificate=True;
```

## Managing Containers

### View Running Containers

```bash
docker ps
```

### View Container Logs

```bash
# All logs
docker compose logs

# Follow logs in real-time
docker compose logs -f

# Specific service
docker logs hob.api
docker logs hob-dashboard

# Follow specific service
docker logs -f hob.api
```

### Stop All Containers

```bash
docker compose down
```

### Stop and Remove Volumes (Clean Slate)

```bash
docker compose down -v
```

**Warning:** This will delete all database data and require re-initialization.

### Restart a Specific Service

```bash
docker compose restart hob.api
```

### Rebuild a Specific Service

```bash
docker compose up -d --build hob.api
```

## Container Architecture

### Infrastructure Services

- **traefik** - Reverse proxy and load balancer
- **db** - Microsoft SQL Server 2022 (with health checks)
- **rabbitmq** - RabbitMQ message broker (with management UI)
- **prometheus** - Metrics collection and storage
- **grafana** - Metrics visualization dashboards
- **jaeger** - Distributed tracing backend

### Application Services

- **hob.api** - .NET 9.0 Web API (depends on db, rabbitmq)
- **hob-worker** - Background worker for async tasks (depends on db, rabbitmq)
- **hob-dashboard** - Next.js 16 frontend (depends on hob.api)

## Troubleshooting

### Port Conflicts

If you get "port already in use" errors:

```bash
# Check what's using the port (Linux/macOS)
sudo lsof -i :80
sudo lsof -i :1433

# Windows
netstat -ano | findstr :80
```

### Network Issues

If services can't communicate:

```bash
# Verify the network exists
docker network ls | grep hob-network

# If missing, create it:
docker network create hob-network

# Check which containers are on the network
docker network inspect hob-network
```

### Database Connection Issues

If the API can't connect to the database:

1. Check database container is healthy:
   ```bash
   docker ps --filter name=db
   ```

2. Verify database credentials in `.env` match your configuration

3. Check API logs:
   ```bash
   docker logs hob.api
   ```

### Permission Denied (Linux)

If you get "permission denied" when running Docker commands:

```bash
# Make sure your user is in the docker group
groups

# If not listed, add yourself:
sudo usermod -aG docker $USER

# Then log out and back in, or run:
newgrp docker
```

### Container Build Failures

If containers fail to build:

1. Clear Docker build cache:
   ```bash
   docker builder prune
   ```

2. Remove old images:
   ```bash
   docker compose down --rmi all
   ```

3. Rebuild from scratch:
   ```bash
   docker compose build --no-cache
   docker compose up
   ```

### Gateway Timeout (504)

If you get 504 errors when accessing services through Traefik:

1. Verify all containers are on the `hob-network`
2. Check Traefik labels in `docker-compose.service.yml` include port configuration
3. Ensure service health checks are passing (for db and rabbitmq)

## Data Persistence

Container data is persisted in the `data/` directory:

- `data/mssql/data/` - SQL Server database files
- `data/prometheus/` - Prometheus configuration and metrics
- `data/grafana/` - Grafana dashboards and data sources

These directories are created automatically when you start the containers.

## Next Steps

After starting the containers:

1. Access the dashboard at http://dashboard.hob.localhost
2. Explore the API documentation at http://hob.api.localhost/swagger
3. View metrics in Grafana at http://grafana.hob.localhost (admin/grafana)
4. Monitor traces in Jaeger at http://jaeger.hob.localhost

For more information, see:
- [Main README](../README.md)
- [Documentation Hub](../docs/Home.md)
- [Infrastructure Setup](../docs/Infrastructure-Setup.md)
