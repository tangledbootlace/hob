# HOB Infrastructure Wiki

Welcome to the House of Burgesses (HOB) infrastructure documentation. This wiki provides comprehensive guides for deploying, operating, and maintaining the HOB application on Portainer.

## 📚 Documentation

### Getting Started

- **[Infrastructure Setup Guide](Infrastructure-Setup.md)** - Complete setup instructions from scratch
  - Prerequisites and requirements
  - Server and Portainer configuration
  - GitHub Actions setup
  - Terraform configuration
  - First deployment steps

### Operations

- **[Deployment and Operations Guide](Deployment-and-Operations.md)** - Day-to-day operations
  - GitHub Actions workflows
  - Manual deployment methods
  - Worker job management
  - Monitoring and maintenance
  - Backup and recovery
  - Scaling strategies

### Configuration

- **[Making Infrastructure Changes](Infrastructure-Changes.md)** - Safely modify infrastructure
  - Understanding the infrastructure
  - Common change scenarios
  - Terraform workflow
  - Testing and validation
  - Deployment procedures

## 🏗️ Architecture Overview

### Stack Components

#### Infrastructure Stack
- **SQL Server 2022** - Primary database
- **RabbitMQ** - Message queue for async processing
- **Prometheus** - Metrics collection
- **Grafana** - Metrics visualization
- **Jaeger** - Distributed tracing
- **Traefik** - Reverse proxy and load balancer

#### Services Stack
- **HOB.API** - REST API (.NET 9.0)
- **HOB.Dashboard** - Next.js web interface

#### Worker Jobs
- **HOB.Worker** - Report generation service (scheduled)

## 🚀 Quick Start

### For First-Time Setup

1. Read [Infrastructure Setup Guide](Infrastructure-Setup.md)
2. Prepare your server with Docker and Portainer
3. Configure GitHub repository secrets and variables
4. Set up Terraform locally
5. Run initial deployment

### For Deployments

1. Push code changes to GitHub
2. GitHub Actions automatically builds Docker images
3. Deploy via GitHub Actions workflow or Terraform
4. Monitor deployment in Portainer and Grafana

### For Operations

1. Monitor services in Grafana: `http://grafana.yourdomain.com`
2. View traces in Jaeger: `http://jaeger.yourdomain.com`
3. Check RabbitMQ queues: `http://rabbitmq.yourdomain.com`
4. Access API: `http://api.yourdomain.com`
5. Access Dashboard: `http://dashboard.yourdomain.com`

## 📖 Common Tasks

### Deployment Tasks

- [Deploy Infrastructure](Infrastructure-Setup.md#first-deployment)
- [Deploy Services](Deployment-and-Operations.md#github-actions-workflows)
- [Run Worker Manually](Deployment-and-Operations.md#worker-operations)
- [Rollback Deployment](Deployment-and-Operations.md#rollback-procedures)

### Configuration Tasks

- [Add Environment Variable](Infrastructure-Changes.md#add-new-environment-variable)
- [Add New Service](Infrastructure-Changes.md#add-new-service-to-infrastructure)
- [Change Resource Limits](Infrastructure-Changes.md#change-resource-limits)
- [Update Worker Schedule](Infrastructure-Changes.md#modify-worker-schedule)

### Maintenance Tasks

- [Backup Database](Deployment-and-Operations.md#backup-database)
- [Rotate Logs](Deployment-and-Operations.md#rotate-logs)
- [Clean Up Resources](Deployment-and-Operations.md#clean-up-resources)
- [Update Docker Images](Deployment-and-Operations.md#update-docker-images)

## 🛠️ Terraform Configuration

### Directory Structure

```
infrastructure/
└── terraform/
    ├── provider.tf              # Portainer provider configuration
    ├── variables.tf             # Variable definitions
    ├── main.tf                  # Stack resources
    ├── outputs.tf               # Output values
    ├── terraform.tfvars.example # Example configuration
    └── terraform.tfvars         # Your configuration (gitignored)
```

### Quick Commands

```bash
# Navigate to Terraform directory
cd infrastructure/terraform

# Initialize Terraform
terraform init

# Preview changes
terraform plan

# Deploy infrastructure
terraform apply

# View outputs
terraform output
```

## 🐳 Docker Compose Files

### Project-Specific Infrastructure

- `HOB.API/infrastructure/docker-compose.infrastructure.yml` - Infrastructure services
- `HOB.API/infrastructure/docker-compose.services.yml` - Application services
- `HOB.Worker/infrastructure/docker-compose.worker.yml` - Worker job
- `docker-compose.production.yml` - Combined configuration for local testing

## 🔄 GitHub Actions Workflows

### Available Workflows

1. **Build and Push Docker Images** (`.github/workflows/build-and-push.yml`)
   - Builds API, Worker, Dashboard images
   - Pushes to GitHub Container Registry
   - Triggered on push to main/develop

2. **Deploy Infrastructure** (`.github/workflows/deploy-infrastructure.yml`)
   - Deploys infrastructure stack via Terraform
   - Manual trigger only
   - Supports plan/apply/destroy

3. **Deploy Services** (`.github/workflows/deploy-services.yml`)
   - Deploys API and Dashboard services
   - Auto-deploys on successful build
   - Manual trigger for specific versions

4. **Worker Scheduled Job** (`.github/workflows/worker-schedule.yml`)
   - Runs every 5 minutes (configurable)
   - Processes report generation queue
   - Manual trigger available

## 📊 Monitoring and Observability

### Access Points

| Service | URL | Credentials |
|---------|-----|-------------|
| Grafana | `http://grafana.yourdomain.com` | admin / your-password |
| Prometheus | `http://prometheus.yourdomain.com` | None |
| Jaeger | `http://jaeger.yourdomain.com` | None |
| RabbitMQ | `http://rabbitmq.yourdomain.com` | your-user / your-password |
| Traefik | `http://your-server:8080` | None |

### Key Metrics

- **API Performance**: Request rates, latency, error rates
- **Database**: Connection pool, query performance
- **RabbitMQ**: Queue depth, message rates
- **System**: CPU, memory, disk usage
- **Worker**: Execution count, duration, success rate

## 🔒 Security

### Required Secrets (GitHub)

- `PORTAINER_URL` - Portainer server URL
- `PORTAINER_USERNAME` - Portainer admin username
- `PORTAINER_PASSWORD` - Portainer admin password
- `PORTAINER_ENDPOINT_ID` - Portainer endpoint ID
- `SERVER_HOST` - Server hostname/IP
- `SERVER_USER` - SSH username
- `SERVER_SSH_KEY` - SSH private key
- `DB_PASSWORD` - Database SA password
- `RABBITMQ_USER` - RabbitMQ username
- `RABBITMQ_PASSWORD` - RabbitMQ password
- `GRAFANA_ADMIN_PASSWORD` - Grafana admin password

### Required Variables (GitHub)

- `HOB_API_DOMAIN` - API domain
- `HOB_DASHBOARD_DOMAIN` - Dashboard domain
- `GRAFANA_DOMAIN` - Grafana domain
- `PROMETHEUS_DOMAIN` - Prometheus domain
- `JAEGER_DOMAIN` - Jaeger domain
- `RABBITMQ_DOMAIN` - RabbitMQ domain
- `WORKER_SCHEDULE` - Cron schedule
- `DATA_VOLUME_PATH` - Data directory path
- `REPORTS_VOLUME_PATH` - Reports directory path

## 🆘 Troubleshooting

### Common Issues

1. **Containers won't start**
   - Check logs: `docker logs <container-name>`
   - Verify environment variables
   - Check port availability
   - Review [Infrastructure Setup - Troubleshooting](Infrastructure-Setup.md#troubleshooting)

2. **Database connection failures**
   - Verify SQL Server is running
   - Test connection string
   - Check network connectivity
   - See [Deployment and Operations - Monitoring](Deployment-and-Operations.md#database-monitoring)

3. **Worker not executing**
   - Check GitHub Actions workflow
   - Verify SSH connectivity
   - Review worker logs
   - See [Deployment and Operations - Worker Operations](Deployment-and-Operations.md#worker-operations)

4. **Terraform errors**
   - Validate configuration: `terraform validate`
   - Check Portainer credentials
   - Review state: `terraform state list`
   - See [Infrastructure Changes - Terraform Workflow](Infrastructure-Changes.md#terraform-workflow)

## 📞 Getting Help

If you need assistance:

1. **Check Documentation**
   - Review relevant wiki pages
   - Check troubleshooting sections
   - Review example configurations

2. **Review Logs**
   - Container logs: `docker logs <container>`
   - GitHub Actions logs
   - Portainer stack events

3. **Verify Configuration**
   - Check environment variables
   - Validate docker-compose syntax
   - Review Terraform plan

4. **Contact Team**
   - Create GitHub issue
   - Contact development team
   - Review existing issues

## 📝 Contributing

When updating infrastructure:

1. Create feature branch
2. Make changes
3. Test locally and in staging
4. Update documentation
5. Create pull request
6. Deploy to production

See [Making Infrastructure Changes](Infrastructure-Changes.md) for detailed workflow.

## 🔗 Additional Resources

### External Documentation

- [Portainer Documentation](https://docs.portainer.io/)
- [Terraform Portainer Provider](https://registry.terraform.io/providers/portainer/portainer/latest/docs)
- [Docker Compose Reference](https://docs.docker.com/compose/compose-file/)
- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [Traefik Documentation](https://doc.traefik.io/traefik/)

### Project Documentation

- [CLAUDE.md](../CLAUDE.md) - Project overview and development guide
- [README.md](../README.md) - Repository README
- [Infrastructure README](../infrastructure/README.md) - Infrastructure overview

## 📅 Maintenance Schedule

### Daily
- Worker executions (every 5 minutes)
- Automated deployments (on code push)
- Log monitoring

### Weekly
- Review metrics and dashboards
- Check disk usage
- Clean up old containers
- Review security alerts

### Monthly
- Rotate credentials
- Update dependencies
- Review and archive old reports
- Backup verification

### Quarterly
- Security audit
- Performance review
- Cost optimization review
- Documentation update

---

**Last Updated**: 2024-01-15
**Maintained By**: Development Team
**Version**: 1.0.0
