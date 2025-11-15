# HOB.API Infrastructure

This directory contains infrastructure-as-code for deploying the HOB API and related services to Portainer.

## Files

- **docker-compose.infrastructure.yml** - Infrastructure services (database, RabbitMQ, monitoring, Traefik)
- **docker-compose.services.yml** - Application services (API, Dashboard)

## Stack Deployment Order

1. **Infrastructure Stack** - Deploy first, provides foundational services
   - SQL Server
   - RabbitMQ
   - Prometheus
   - Grafana
   - Jaeger
   - Traefik

2. **Services Stack** - Deploy after infrastructure is healthy
   - HOB.API
   - HOB.Dashboard

## Environment Variables

### Infrastructure Stack

| Variable | Description | Default |
|----------|-------------|---------|
| SA_PASSWORD | SQL Server SA password | - |
| RABBITMQ_DEFAULT_USER | RabbitMQ username | local |
| RABBITMQ_DEFAULT_PASS | RabbitMQ password | - |
| GF_SECURITY_ADMIN_PASSWORD | Grafana admin password | - |
| PROMETHEUS_DOMAIN | Prometheus domain | prometheus.hob.localhost |
| GRAFANA_DOMAIN | Grafana domain | grafana.hob.localhost |
| JAEGER_DOMAIN | Jaeger domain | jaeger.hob.localhost |
| RABBITMQ_DOMAIN | RabbitMQ management domain | rabbitmq.hob.localhost |
| DATA_VOLUME_PATH | Host path for config data | /opt/hob/data |

### Services Stack

| Variable | Description | Default |
|----------|-------------|---------|
| ASPNETCORE_ENVIRONMENT | ASP.NET environment | Production |
| DB_PASSWORD | Database password (same as SA_PASSWORD) | - |
| RABBITMQ_USER | RabbitMQ username | local |
| RABBITMQ_PASSWORD | RabbitMQ password | - |
| HOB_API_DOMAIN | API domain | hob.api.localhost |
| HOB_DASHBOARD_DOMAIN | Dashboard domain | dashboard.hob.localhost |
| IMAGE_TAG | Docker image tag | latest |
| DOCKER_REGISTRY | Docker registry | ghcr.io |

## Manual Deployment

### Using Portainer UI

1. Navigate to Stacks in Portainer
2. Click "Add stack"
3. Name: `hob-infrastructure-production`
4. Build method: Web editor
5. Paste contents of `docker-compose.infrastructure.yml`
6. Add environment variables
7. Deploy

Repeat for `hob-services-production` using `docker-compose.services.yml`

### Using Terraform

See `/infrastructure/terraform/README.md` for Terraform deployment instructions.

## Volumes

Infrastructure stack creates the following named volumes:
- `prometheus-data` - Prometheus metrics data
- `grafana-data` - Grafana dashboards and settings
- `mssql-data` - SQL Server database files
- `mssql-log` - SQL Server transaction logs
- `mssql-backup` - SQL Server backup location
- `rabbitmq-data` - RabbitMQ message data
- `traefik-certs` - Traefik SSL certificates

## Networks

Both stacks use the `hob-network` bridge network for inter-service communication.

## Health Checks

All services include health checks:
- **SQL Server**: Validates connection with SA credentials
- **RabbitMQ**: Pings RabbitMQ diagnostics
- **API**: HTTP GET to `/health` endpoint
- **Dashboard**: HTTP GET to `/api/health` endpoint

## Monitoring

Access monitoring tools:
- **Grafana**: http://${GRAFANA_DOMAIN}
- **Prometheus**: http://${PROMETHEUS_DOMAIN}
- **Jaeger**: http://${JAEGER_DOMAIN}
- **RabbitMQ Management**: http://${RABBITMQ_DOMAIN}
- **Traefik Dashboard**: http://your-server:8080

## Troubleshooting

### Stack won't start

1. Check Portainer logs for the stack
2. Verify environment variables are set correctly
3. Ensure volumes and networks can be created
4. Check that ports 80, 443, 1433, 4317, 4318, 8080 are available

### Services can't connect to infrastructure

1. Verify infrastructure stack is healthy
2. Check that `hob-network` exists and is accessible
3. Review service logs for connection errors
4. Validate connection strings and credentials

### Database connection issues

1. Confirm SQL Server container is running
2. Test connection: `docker exec hob-db /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'your-password' -C -Q "SELECT 1"`
3. Check firewall rules
4. Verify connection string format

## Security Considerations

- Change default passwords in production
- Use secrets management for credentials
- Configure SSL/TLS certificates for Traefik
- Restrict Traefik dashboard access
- Enable authentication for Prometheus and Grafana
- Use network policies to restrict service communication
