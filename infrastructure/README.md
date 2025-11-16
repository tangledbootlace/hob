# HOB Infrastructure

This directory contains all Infrastructure-as-Code (IaC) resources for deploying the House of Burgesses application.

## Quick Start

### Prerequisites

- Portainer server already set up and accessible
- GitHub Secrets and Variables configured (see below)
- Terraform 1.9.0+ installed (for local deployment)

### Deployment Steps

```bash
# 1. Validate configuration
./validate.sh

# 2. Initialize Terraform (first time only)
cd terraform
terraform init

# 3. Review deployment plan
terraform plan

# 4. Deploy infrastructure
terraform apply
```

### Teardown Steps

```bash
# Basic teardown (preserves volumes)
./teardown.sh

# Complete teardown (destroys all data)
./teardown.sh --destroy-volumes
```

## Directory Structure

```
infrastructure/
├── README.md                              # This file
├── .env.example                           # Environment variables template
├── docker-compose.infrastructure.yml      # Infrastructure stack (DB, RabbitMQ, Monitoring)
├── docker-compose.services.yml            # Services stack (API, Dashboard)
├── docker-compose.worker.yml              # Worker configuration
├── docker-compose.production.yml          # Production overrides
├── validate.sh                            # Pre-deployment validation script
├── teardown.sh                            # Infrastructure teardown script
├── run-worker.sh                          # Manual worker execution script
└── terraform/                             # Terraform configuration
    ├── main.tf                            # Main Terraform resources
    ├── variables.tf                       # Variable definitions
    ├── provider.tf                        # Provider configuration
    ├── outputs.tf                         # Output values
    ├── versions.tf                        # Version constraints
    └── terraform.tfvars.example           # Terraform variables template
```

## Configuration Files

### Environment Configuration (.env)

Create from `.env.example`:

```bash
cp .env.example .env
# Edit .env with your values
nano .env
```

**Required Variables:**
- `PORTAINER_URL` - Portainer server URL
- `PORTAINER_USERNAME` - Portainer admin username
- `PORTAINER_PASSWORD` - Portainer admin password
- `DB_PASSWORD` - SQL Server password (strong password required)
- `RABBITMQ_USER` - RabbitMQ username
- `RABBITMQ_PASSWORD` - RabbitMQ password (min 8 chars)
- `GRAFANA_ADMIN_PASSWORD` - Grafana password (min 8 chars)
- `HOB_API_DOMAIN` - API domain name
- `HOB_DASHBOARD_DOMAIN` - Dashboard domain name
- `GRAFANA_DOMAIN` - Grafana domain name
- `PROMETHEUS_DOMAIN` - Prometheus domain name
- `JAEGER_DOMAIN` - Jaeger domain name
- `RABBITMQ_DOMAIN` - RabbitMQ domain name

### Terraform Configuration (terraform.tfvars)

Create from `terraform/terraform.tfvars.example`:

```bash
cd terraform
cp terraform.tfvars.example terraform.tfvars
# Edit terraform.tfvars with your values
nano terraform.tfvars
```

## Scripts

### validate.sh - Pre-Deployment Validation

Checks that all prerequisites are met before deployment.

```bash
# Run validation
./validate.sh

# Use custom environment file
./validate.sh --env-file /path/to/.env
```

**Checks Performed:**
- Terraform and Docker installation
- Configuration files present
- Required environment variables set
- Password strength validation
- Volume paths exist and writable
- Portainer connectivity
- Terraform initialization status

**Exit Codes:**
- `0` - All checks passed or warnings only
- `1` - One or more errors found

### teardown.sh - Infrastructure Teardown

Safely destroys all infrastructure resources.

```bash
# Interactive teardown (preserves volumes)
./teardown.sh

# Destroy volumes too (⚠️ DATA LOSS!)
./teardown.sh --destroy-volumes

# Non-interactive mode
./teardown.sh --auto-approve

# Combine options
./teardown.sh --auto-approve --destroy-volumes
```

**What Gets Destroyed:**
- All Portainer stacks (services, infrastructure)
- Docker containers and networks
- Optionally: persistent volumes (with `--destroy-volumes`)

**What's Preserved:**
- Terraform state files (for recovery)
- Configuration files (.env, terraform.tfvars)
- Persistent volumes (unless `--destroy-volumes` used)

### run-worker.sh - Manual Worker Execution

Executes the worker job manually (alternative to scheduled execution).

```bash
./run-worker.sh
```

## GitHub Actions Workflows

### Deployment Workflows

Located in `.github/workflows/`:

1. **build-and-push.yml** - Builds Docker images and pushes to registry
2. **deploy-services.yml** - Deploys services to Portainer via Terraform
3. **deploy-infrastructure.yml.disabled** - Infrastructure deployment (disabled for safety)
4. **worker-schedule.yml.disabled** - Scheduled worker execution (disabled, needs configuration)

### Teardown Workflow

**teardown-infrastructure.yml** - Manual infrastructure teardown

To use:
1. Go to GitHub **Actions** tab
2. Select "Teardown Infrastructure (Manual)"
3. Click "Run workflow"
4. Configure options:
   - Environment: production/staging/dev
   - Destroy volumes: true/false
   - Confirmation: Type "DESTROY"
5. Click "Run workflow"

## Required GitHub Secrets

Configure in: **Settings → Secrets and variables → Actions → Secrets**

| Secret Name | Description | Example |
|-------------|-------------|---------|
| `PORTAINER_URL` | Portainer server URL | `https://portainer.example.com` |
| `PORTAINER_USERNAME` | Portainer admin username | `admin` |
| `PORTAINER_PASSWORD` | Portainer admin password | `SecurePassword123!` |
| `PORTAINER_ENDPOINT_ID` | Portainer endpoint ID | `1` |
| `DB_PASSWORD` | SQL Server SA password | `StrongDBPass123!` |
| `RABBITMQ_USER` | RabbitMQ username | `hob` |
| `RABBITMQ_PASSWORD` | RabbitMQ password | `SecureRabbitPass` |
| `GRAFANA_ADMIN_PASSWORD` | Grafana admin password | `SecureGrafanaPass` |

## Required GitHub Variables

Configure in: **Settings → Secrets and variables → Actions → Variables**

| Variable Name | Description | Example |
|---------------|-------------|---------|
| `HOB_API_DOMAIN` | API domain | `api.hob.example.com` |
| `HOB_DASHBOARD_DOMAIN` | Dashboard domain | `hob.example.com` |
| `GRAFANA_DOMAIN` | Grafana domain | `grafana.hob.example.com` |
| `PROMETHEUS_DOMAIN` | Prometheus domain | `prometheus.hob.example.com` |
| `JAEGER_DOMAIN` | Jaeger domain | `jaeger.hob.example.com` |
| `RABBITMQ_DOMAIN` | RabbitMQ domain | `rabbitmq.hob.example.com` |
| `DOCKER_REGISTRY` | Docker registry | `ghcr.io` (default) |
| `DATA_VOLUME_PATH` | Data volume path | `/opt/hob/data` (default) |
| `REPORTS_VOLUME_PATH` | Reports volume path | `/opt/hob/reports` (default) |

## Idempotency

All infrastructure operations are idempotent:

- **terraform apply** - Can be run multiple times; only applies changes
- **terraform destroy** - Can be run multiple times; safe if resources don't exist
- **validate.sh** - Read-only checks, always safe to run
- **teardown.sh** - Handles missing resources gracefully

## Deployment Sequence

### Full Deployment

```
1. validate.sh         → Pre-deployment checks
2. terraform init      → Initialize Terraform
3. terraform plan      → Review changes
4. terraform apply     → Deploy infrastructure
5. Verify deployment   → Check all services are running
```

### CI/CD Deployment

```
1. build-and-push      → Build Docker images
2. deploy-services     → Deploy via Terraform to Portainer
3. Verify deployment   → Check all services are running
```

## Teardown Sequence

### Safe Teardown

```
1. Backup data         → Export important data
2. teardown.sh         → Destroy infrastructure (preserves volumes)
3. Verify teardown     → Check resources are removed
```

### Complete Teardown

```
1. Backup data         → Export important data
2. teardown.sh --destroy-volumes → Destroy everything
3. Clean Terraform state → Remove .terraform and state files
4. Verify teardown     → Check resources and volumes are removed
```

## Troubleshooting

### Validation Fails

```bash
# Check detailed error messages
./validate.sh

# Fix issues identified
# Common fixes:
# - Install Terraform: brew install terraform (macOS) or apt install terraform (Linux)
# - Create .env file: cp .env.example .env
# - Set environment variables in .env
# - Fix password strength
```

### Terraform Apply Fails

```bash
# Check Terraform state
cd terraform
terraform state list

# View specific resource
terraform state show portainer_stack.infrastructure

# Check Portainer connectivity
curl -k https://your-portainer/api/status

# Validate Terraform configuration
terraform validate

# Force unlock if state is locked
terraform force-unlock <lock-id>
```

### Teardown Fails

```bash
# Try targeted destroy
cd terraform
terraform destroy -target=portainer_stack.services
terraform destroy -target=portainer_stack.infrastructure

# If Terraform fails, use emergency teardown
# See docs/Infrastructure-Setup.md → Emergency Teardown section
```

## Documentation

- [Infrastructure Setup Guide](../docs/Infrastructure-Setup.md) - Complete setup instructions
- [Deployment and Operations](../docs/Deployment-and-Operations.md) - Operations guide
- [Infrastructure Changes](../docs/Infrastructure-Changes.md) - Making changes guide
- [Architecture](../docs/architecture.md) - System architecture
- [API Endpoints](../docs/api-endpoints.md) - API documentation

## Security Best Practices

- ✅ Use strong passwords (validated by scripts)
- ✅ Never commit secrets to Git
- ✅ Use GitHub Secrets for sensitive data
- ✅ Enable `prevent_destroy` in Terraform for production
- ✅ Require confirmation for teardown operations
- ✅ Regular backups of persistent volumes
- ✅ SSL/TLS certificates for all public services
- ✅ Restrict Traefik dashboard access
- ✅ Use least privilege for service accounts

## Components

### Infrastructure Stack
- SQL Server 2022
- RabbitMQ with Management UI
- Prometheus
- Grafana
- Jaeger
- Traefik reverse proxy

### Services Stack
- HOB.API - REST API
- HOB.Dashboard - Next.js UI

### Worker
- Scheduled via GitHub Actions (every 5 minutes)
- Processes report generation messages
- Auto-shuts down when queue is empty

## Support

For issues or questions:
1. Check [Troubleshooting](#troubleshooting) section above
2. Review [Infrastructure Setup Guide](../docs/Infrastructure-Setup.md)
3. Check GitHub Issues
4. Contact the development team
