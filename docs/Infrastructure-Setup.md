# Infrastructure Setup Guide

This guide walks you through setting up the HOB application infrastructure on your Portainer server.

## Table of Contents

- [Prerequisites](#prerequisites)
- [Initial Server Setup](#initial-server-setup)
- [Portainer Configuration](#portainer-configuration)
- [GitHub Configuration](#github-configuration)
- [Terraform Setup](#terraform-setup)
- [First Deployment](#first-deployment)
- [Verification](#verification)
- [Troubleshooting](#troubleshooting)

## Prerequisites

### Required Software

- **Docker**: Version 20.10 or higher
- **Portainer**: Version 2.0 or higher
- **Terraform**: Version 1.0 or higher (for Terraform deployments)
- **Git**: For cloning the repository

### Server Requirements

- **Operating System**: Linux (Ubuntu 20.04+ recommended)
- **CPU**: 2+ cores recommended
- **RAM**: 8GB minimum, 16GB recommended
- **Disk Space**: 50GB+ available
- **Network**: Open ports 80, 443, 1433 (SQL Server), 4317-4318 (Jaeger)

### Access Requirements

- SSH access to your server
- Portainer admin credentials
- GitHub account with repository access
- Domain names (or use localhost for testing)

## Initial Server Setup

### 1. Install Docker

```bash
# Update package index
sudo apt-get update

# Install Docker
curl -fsSL https://get.docker.com -o get-docker.sh
sudo sh get-docker.sh

# Add your user to docker group
sudo usermod -aG docker $USER

# Verify installation
docker --version
```

### 2. Install Portainer

```bash
# Create Portainer volume
docker volume create portainer_data

# Run Portainer
docker run -d \
  -p 8000:8000 \
  -p 9443:9443 \
  --name portainer \
  --restart=always \
  -v /var/run/docker.sock:/var/run/docker.sock \
  -v portainer_data:/data \
  portainer/portainer-ce:latest

# Access Portainer at https://your-server:9443
```

### 3. Configure Initial Portainer Setup

1. Navigate to `https://your-server:9443`
2. Create admin account
3. Connect to local Docker environment
4. Note your endpoint ID (usually `1` for local)

### 4. Create Docker Network

The HOB application requires a dedicated Docker network:

```bash
docker network create hob-network
```

### 5. Create Required Directories

```bash
# Create data directories
sudo mkdir -p /opt/hob/data/{prometheus,grafana/datasources,grafana/dashboards}
sudo mkdir -p /opt/hob/reports

# Set permissions
sudo chown -R $USER:$USER /opt/hob
chmod -R 755 /opt/hob
```

### 6. Configure Prometheus

Create Prometheus configuration:

```bash
cat > /opt/hob/data/prometheus/prometheus.yml <<EOF
global:
  scrape_interval: 15s
  evaluation_interval: 15s

scrape_configs:
  - job_name: 'hob-api'
    static_configs:
      - targets: ['hob-api:8080']
    metrics_path: '/metrics'

  - job_name: 'prometheus'
    static_configs:
      - targets: ['localhost:9090']
EOF
```

### 7. Configure Grafana Data Sources

Create Grafana Prometheus datasource:

```bash
cat > /opt/hob/data/grafana/datasources/prometheus.yml <<EOF
apiVersion: 1

datasources:
  - name: Prometheus
    type: prometheus
    access: proxy
    url: http://hob-prometheus:9090
    isDefault: true
    editable: true
EOF
```

## Portainer Configuration

### Environment Variables Setup

In Portainer, you'll need to configure environment variables for each stack. Here are the required variables:

#### Infrastructure Stack Variables

| Variable | Description | Example |
|----------|-------------|---------|
| `SA_PASSWORD` | SQL Server SA password | `YourSecurePassword123!` |
| `RABBITMQ_DEFAULT_USER` | RabbitMQ username | `hob` |
| `RABBITMQ_DEFAULT_PASS` | RabbitMQ password | `SecureRabbitMQPass` |
| `GF_SECURITY_ADMIN_PASSWORD` | Grafana admin password | `SecureGrafanaPass` |
| `PROMETHEUS_DOMAIN` | Prometheus domain | `prometheus.hob.yourdomain.com` |
| `GRAFANA_DOMAIN` | Grafana domain | `grafana.hob.yourdomain.com` |
| `JAEGER_DOMAIN` | Jaeger domain | `jaeger.hob.yourdomain.com` |
| `RABBITMQ_DOMAIN` | RabbitMQ management domain | `rabbitmq.hob.yourdomain.com` |
| `DATA_VOLUME_PATH` | Host path for data | `/opt/hob/data` |

#### Services Stack Variables

| Variable | Description | Example |
|----------|-------------|---------|
| `ASPNETCORE_ENVIRONMENT` | ASP.NET environment | `Production` |
| `DB_PASSWORD` | Database password | `YourSecurePassword123!` |
| `RABBITMQ_USER` | RabbitMQ username | `hob` |
| `RABBITMQ_PASSWORD` | RabbitMQ password | `SecureRabbitMQPass` |
| `HOB_API_DOMAIN` | API domain | `api.hob.yourdomain.com` |
| `HOB_DASHBOARD_DOMAIN` | Dashboard domain | `hob.yourdomain.com` |
| `IMAGE_TAG` | Docker image tag | `latest` |
| `DOCKER_REGISTRY` | Docker registry | `ghcr.io` |

## GitHub Configuration

### 1. Repository Secrets

Navigate to your GitHub repository → Settings → Secrets and variables → Actions → New repository secret

Add the following secrets:

#### Portainer Secrets
- `PORTAINER_URL`: Your Portainer URL (e.g., `https://portainer.yourdomain.com`)
- `PORTAINER_USERNAME`: Portainer admin username
- `PORTAINER_PASSWORD`: Portainer admin password
- `PORTAINER_ENDPOINT_ID`: Portainer endpoint ID (usually `1`)

#### Server Secrets
- `SERVER_HOST`: Your server hostname or IP
- `SERVER_USER`: SSH username
- `SERVER_SSH_KEY`: SSH private key for server access

#### Application Secrets
- `DB_PASSWORD`: SQL Server SA password
- `RABBITMQ_USER`: RabbitMQ username
- `RABBITMQ_PASSWORD`: RabbitMQ password
- `GRAFANA_ADMIN_PASSWORD`: Grafana admin password

### 2. Repository Variables

Navigate to Settings → Secrets and variables → Actions → Variables tab → New repository variable

Add the following variables:

- `HOB_API_DOMAIN`: API domain
- `HOB_DASHBOARD_DOMAIN`: Dashboard domain
- `GRAFANA_DOMAIN`: Grafana domain
- `PROMETHEUS_DOMAIN`: Prometheus domain
- `JAEGER_DOMAIN`: Jaeger domain
- `RABBITMQ_DOMAIN`: RabbitMQ domain
- `DOCKER_REGISTRY`: Docker registry (default: `ghcr.io`)
- `WORKER_SCHEDULE`: Cron schedule (default: `*/5 * * * *`)
- `DATA_VOLUME_PATH`: Data volume path (default: `/opt/hob/data`)
- `REPORTS_VOLUME_PATH`: Reports volume path (default: `/opt/hob/reports`)

### 3. GitHub Packages Access

Ensure GitHub Actions can push to GitHub Container Registry:

1. Go to Settings → Actions → General
2. Under "Workflow permissions", select "Read and write permissions"
3. Click "Save"

### 4. SSH Key Setup (for Worker Schedule)

Generate an SSH key for GitHub Actions:

```bash
# On your local machine
ssh-keygen -t ed25519 -C "github-actions" -f ~/.ssh/github-actions

# Copy public key to server
ssh-copy-id -i ~/.ssh/github-actions.pub user@your-server

# Copy private key content
cat ~/.ssh/github-actions
```

Add the private key content to `SERVER_SSH_KEY` secret in GitHub.

## Terraform Setup

### 1. Install Terraform

```bash
# On your local machine
wget https://releases.hashicorp.com/terraform/1.6.0/terraform_1.6.0_linux_amd64.zip
unzip terraform_1.6.0_linux_amd64.zip
sudo mv terraform /usr/local/bin/
terraform --version
```

### 2. Configure Terraform Variables

```bash
# Navigate to infrastructure directory
cd infrastructure/terraform

# Copy example variables
cp terraform.tfvars.example terraform.tfvars

# Edit with your configuration
nano terraform.tfvars
```

Example `terraform.tfvars`:

```hcl
portainer_url         = "https://portainer.yourdomain.com"
portainer_username    = "admin"
portainer_password    = "your-secure-password"
portainer_endpoint_id = 1

environment = "production"

hob_api_domain       = "api.hob.yourdomain.com"
hob_dashboard_domain = "hob.yourdomain.com"
grafana_domain       = "grafana.hob.yourdomain.com"
prometheus_domain    = "prometheus.hob.yourdomain.com"
jaeger_domain        = "jaeger.hob.yourdomain.com"
rabbitmq_domain      = "rabbitmq.hob.yourdomain.com"

db_password            = "YourSecurePassword123!"
rabbitmq_user          = "hob"
rabbitmq_password      = "SecureRabbitMQPassword"
grafana_admin_password = "SecureGrafanaPassword"

docker_registry          = "ghcr.io"
docker_registry_username = "your-github-username"
docker_registry_password = "your-github-token"
image_tag                = "latest"

worker_schedule      = "*/5 * * * *"
data_volume_path     = "/opt/hob/data"
reports_volume_path  = "/opt/hob/reports"
```

### 3. Initialize Terraform

```bash
terraform init
```

### 4. Validate Configuration

```bash
terraform validate
terraform fmt
```

## First Deployment

### Option 1: Deploy via Terraform (Recommended)

```bash
# Plan deployment
terraform plan

# Apply configuration
terraform apply

# Review outputs
terraform output
```

### Option 2: Deploy via Portainer UI

#### Deploy Infrastructure Stack

1. Log in to Portainer
2. Navigate to **Stacks** → **Add stack**
3. Name: `hob-infrastructure-production`
4. Build method: **Web editor**
5. Copy contents of `HOB.API/infrastructure/docker-compose.infrastructure.yml`
6. Add environment variables (see Portainer Configuration above)
7. Click **Deploy the stack**

#### Deploy Services Stack

1. Wait for infrastructure stack to be healthy
2. Navigate to **Stacks** → **Add stack**
3. Name: `hob-services-production`
4. Build method: **Web editor**
5. Copy contents of `HOB.API/infrastructure/docker-compose.services.yml`
6. Add environment variables
7. Click **Deploy the stack**

### Option 3: Deploy via GitHub Actions

1. Go to your GitHub repository → Actions
2. Select "Deploy Infrastructure to Portainer"
3. Click "Run workflow"
4. Select environment: `production`
5. Select action: `apply`
6. Click "Run workflow"

## Verification

### 1. Check Container Status

```bash
# List all HOB containers
docker ps --filter "label=com.hob.stack"

# Check specific stack
docker ps --filter "label=com.hob.stack=infrastructure"
docker ps --filter "label=com.hob.stack=services"
```

### 2. Verify Network Connectivity

```bash
# Inspect network
docker network inspect hob-network

# Check container connectivity
docker exec hob-api ping -c 3 hob-db
docker exec hob-api ping -c 3 hob-rabbitmq
```

### 3. Test Service Health

```bash
# API health check
curl http://your-api-domain/health

# Dashboard health check
curl http://your-dashboard-domain/api/health

# Database connection
docker exec hob-db /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'your-password' -C -Q "SELECT 1"

# RabbitMQ status
docker exec hob-rabbitmq rabbitmqctl status
```

### 4. Access Web Interfaces

Open your browser and verify access to:

- ✅ API: `http://your-api-domain`
- ✅ Dashboard: `http://your-dashboard-domain`
- ✅ Grafana: `http://grafana.yourdomain.com` (admin / your-password)
- ✅ Prometheus: `http://prometheus.yourdomain.com`
- ✅ Jaeger: `http://jaeger.yourdomain.com`
- ✅ RabbitMQ: `http://rabbitmq.yourdomain.com` (your-user / your-password)
- ✅ Traefik: `http://your-server:8080`

### 5. Check Logs

```bash
# Infrastructure logs
docker logs hob-db
docker logs hob-rabbitmq
docker logs hob-prometheus

# Service logs
docker logs hob-api
docker logs hob-dashboard

# Follow logs
docker logs -f hob-api
```

## Troubleshooting

### Containers Won't Start

**Symptoms**: Containers exit immediately or restart continuously

**Solutions**:
1. Check container logs: `docker logs <container-name>`
2. Verify environment variables are set correctly
3. Ensure required ports are available
4. Check volume permissions: `ls -la /opt/hob`
5. Verify network exists: `docker network ls | grep hob`

### Database Connection Failures

**Symptoms**: API can't connect to database

**Solutions**:
1. Verify database is running: `docker ps | grep hob-db`
2. Test connection: `docker exec hob-db /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'password' -C -Q "SELECT 1"`
3. Check connection string in API logs
4. Verify password matches in both infrastructure and services stacks
5. Ensure containers are on same network

### RabbitMQ Connection Issues

**Symptoms**: Worker or API can't connect to RabbitMQ

**Solutions**:
1. Check RabbitMQ status: `docker exec hob-rabbitmq rabbitmqctl status`
2. Verify credentials match across stacks
3. Check RabbitMQ logs: `docker logs hob-rabbitmq`
4. Access management UI and verify vhost `/` exists
5. Ensure port 5672 is accessible within network

### Terraform Deployment Fails

**Symptoms**: `terraform apply` fails with errors

**Solutions**:
1. Verify Portainer credentials: `curl -k https://your-portainer/api/users/admin/check`
2. Check endpoint ID is correct
3. Validate docker-compose syntax: `docker-compose -f HOB.API/infrastructure/docker-compose.infrastructure.yml config`
4. Review Terraform state: `terraform state list`
5. Check Portainer API logs

### SSL/TLS Certificate Issues

**Symptoms**: HTTPS not working or certificate errors

**Solutions**:
1. Configure Traefik with Let's Encrypt (see Traefik documentation)
2. Mount certificate files to Traefik container
3. Update Traefik labels with TLS configuration
4. Verify domain DNS records point to server
5. Check certificate expiration dates

### Worker Not Running

**Symptoms**: Worker jobs not executing on schedule

**Solutions**:
1. Check GitHub Actions workflow status
2. Verify SSH access from GitHub to server
3. Review worker-schedule.yml workflow logs
4. Test SSH connection manually
5. Check server logs during scheduled time

## Next Steps

After successful infrastructure setup:

1. ✅ Read [Deployment and Operations Guide](Deployment-and-Operations.md)
2. ✅ Review [Making Infrastructure Changes](Infrastructure-Changes.md)
3. ✅ Set up monitoring and alerting
4. ✅ Configure backups for databases and volumes
5. ✅ Implement SSL/TLS certificates
6. ✅ Review security hardening recommendations

## Security Checklist

Before going to production, ensure:

- [ ] All default passwords changed
- [ ] SSL/TLS certificates configured
- [ ] Traefik dashboard access restricted
- [ ] Database exposed only to internal network
- [ ] RabbitMQ management UI protected
- [ ] Grafana and Prometheus authentication enabled
- [ ] Server firewall configured
- [ ] SSH key-based authentication only
- [ ] GitHub secrets properly configured
- [ ] Regular backup schedule established
- [ ] Monitoring and alerting configured

## Getting Help

If you encounter issues not covered in this guide:

1. Check the [Troubleshooting](#troubleshooting) section
2. Review container logs
3. Consult project documentation
4. Check GitHub Issues for similar problems
5. Contact the development team
