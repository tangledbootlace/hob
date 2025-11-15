# Deployment and Operations Guide

This guide covers day-to-day deployment operations, monitoring, and maintenance of the HOB application.

## Table of Contents

- [Deployment Methods](#deployment-methods)
- [GitHub Actions Workflows](#github-actions-workflows)
- [Manual Deployments](#manual-deployments)
- [Worker Operations](#worker-operations)
- [Monitoring](#monitoring)
- [Maintenance](#maintenance)
- [Backup and Recovery](#backup-and-recovery)
- [Scaling](#scaling)
- [Rollback Procedures](#rollback-procedures)

## Deployment Methods

The HOB application supports three deployment methods:

1. **Automated via GitHub Actions** (Recommended for production)
2. **Terraform CLI** (For infrastructure changes)
3. **Manual via Portainer UI** (For troubleshooting)

## GitHub Actions Workflows

### Build and Push Docker Images

**Workflow**: `.github/workflows/build-and-push.yml`

**Triggers**:
- Push to `main` or `develop` branches
- New version tags (`v*`)
- Pull requests to `main`
- Manual dispatch

**What it does**:
1. Builds Docker images for API, Worker, and Dashboard
2. Tags images with branch name, SHA, and semantic version
3. Pushes to GitHub Container Registry
4. Uses layer caching for faster builds

**Manual Execution**:

```bash
# Via GitHub UI
1. Navigate to Actions → Build and Push Docker Images
2. Click "Run workflow"
3. Select branch
4. Choose whether to push images
5. Click "Run workflow"
```

**Image Tags**:
- `latest` - Latest build from main branch
- `develop` - Latest build from develop branch
- `main-<sha>` - Specific commit from main
- `v1.2.3` - Semantic version tags

### Deploy Infrastructure

**Workflow**: `.github/workflows/deploy-infrastructure.yml`

**Triggers**:
- Manual dispatch only (safety measure)

**What it does**:
1. Runs Terraform to deploy infrastructure stack
2. Creates/updates Portainer stacks
3. Configures environment variables
4. Outputs deployment URLs

**Manual Execution**:

```bash
# Via GitHub UI
1. Navigate to Actions → Deploy Infrastructure to Portainer
2. Click "Run workflow"
3. Select environment (production/staging/development)
4. Choose Terraform action (plan/apply/destroy)
5. Click "Run workflow"
```

**Terraform Actions**:
- `plan` - Preview changes without applying
- `apply` - Deploy infrastructure changes
- `destroy` - Tear down infrastructure (use with caution!)

### Deploy Services

**Workflow**: `.github/workflows/deploy-services.yml`

**Triggers**:
- Successful build on `main` branch (automatic)
- Manual dispatch

**What it does**:
1. Deploys API and Dashboard services
2. Pulls specified image tag
3. Updates Portainer stacks
4. Provides deployment summary

**Manual Execution**:

```bash
# Via GitHub UI
1. Navigate to Actions → Deploy Services to Portainer
2. Click "Run workflow"
3. Select environment
4. Specify image tag (e.g., "v1.2.3", "latest")
5. Choose what to deploy:
   - Infrastructure stack: yes/no
   - Services stack: yes/no
6. Click "Run workflow"
```

**Deployment Options**:
- Deploy infrastructure only
- Deploy services only
- Deploy both infrastructure and services

### Worker Scheduled Job

**Workflow**: `.github/workflows/worker-schedule.yml`

**Triggers**:
- Cron schedule: Every 5 minutes (`*/5 * * * *`)
- Manual dispatch

**What it does**:
1. Connects to server via SSH
2. Pulls latest worker image
3. Runs worker container
4. Processes report generation queue
5. Auto-shuts down when queue is empty
6. Cleans up old worker containers

**Manual Execution**:

```bash
# Via GitHub UI
1. Navigate to Actions → Worker Scheduled Job
2. Click "Run workflow"
3. Select environment
4. Click "Run workflow"

# Via server SSH
ssh user@server
cd /path/to/hob
export DOCKER_REGISTRY="ghcr.io"
export IMAGE_TAG="latest"
export DB_PASSWORD="your-password"
export RABBITMQ_USER="hob"
export RABBITMQ_PASSWORD="your-password"
export REPORTS_VOLUME_PATH="/opt/hob/reports"
./HOB.Worker/infrastructure/run-worker.sh
```

## Manual Deployments

### Deploy via Terraform

```bash
# Navigate to terraform directory
cd infrastructure/terraform

# Initialize (first time only)
terraform init

# Preview changes
terraform plan

# Deploy infrastructure stack
terraform apply

# Deploy services stack only
terraform apply -target=portainer_stack.services

# View outputs
terraform output

# Destroy everything (careful!)
terraform destroy
```

### Deploy via Portainer UI

#### Update Existing Stack

1. Log in to Portainer
2. Navigate to **Stacks**
3. Click on stack name (e.g., `hob-services-production`)
4. Click **Editor** tab
5. Make changes to docker-compose file
6. Update environment variables if needed
7. Click **Update the stack**
8. Enable "Pull latest image versions"
9. Click **Update**

#### Create New Stack

1. Navigate to **Stacks** → **Add stack**
2. Name: `hob-<stack-type>-<environment>`
3. Build method: **Web editor**
4. Paste docker-compose content
5. Add environment variables
6. Click **Deploy the stack**

### Deploy via Docker Compose

```bash
# Clone repository on server
git clone https://github.com/tangledbootlace/hob.git
cd hob

# Set environment variables
export DOCKER_REGISTRY="ghcr.io"
export IMAGE_TAG="v1.2.3"
export SA_PASSWORD="your-db-password"
# ... set other variables ...

# Deploy infrastructure
docker-compose -f HOB.API/infrastructure/docker-compose.infrastructure.yml up -d

# Wait for health checks
docker-compose -f HOB.API/infrastructure/docker-compose.infrastructure.yml ps

# Deploy services
docker-compose -f HOB.API/infrastructure/docker-compose.services.yml up -d

# Check status
docker ps --filter "label=com.hob.stack"
```

## Worker Operations

### Understanding Worker Lifecycle

The worker operates as a scheduled job:

1. **Startup**: Container starts, connects to RabbitMQ
2. **Processing**: Consumes messages, generates reports
3. **Monitoring**: Queue drain observer watches for activity
4. **Idle Detection**: 30 seconds with no messages
5. **Shutdown**: Graceful exit, container removed

### Monitor Worker Executions

```bash
# List recent worker runs
docker ps -a --filter "label=com.hob.service=report-generator"

# View specific run logs
docker logs hob-worker-<run-id>

# Check reports generated
ls -lah /opt/hob/reports

# Monitor in real-time (during scheduled run)
watch -n 1 'docker ps --filter "label=com.hob.service=report-generator"'
```

### Trigger Worker Manually

```bash
# Via GitHub Actions (recommended)
# Go to Actions → Worker Scheduled Job → Run workflow

# Via server SSH
ssh user@server
docker run --rm \
  --name hob-worker-manual-$(date +%s) \
  --network hob-network \
  -e DOTNET_ENVIRONMENT=Production \
  -e "ConnectionStrings__DefaultConnection=Server=hob-db;Database=HOB;User Id=sa;Password=PASSWORD;TrustServerCertificate=True;" \
  -e "ConnectionStrings__RabbitMQ=amqp://user:pass@hob-rabbitmq:5672" \
  -e "ReportSettings__OutputDirectory=/reports" \
  -v /opt/hob/reports:/reports \
  ghcr.io/tangledbootlace/hob-worker:latest
```

### Change Worker Schedule

Edit `.github/workflows/worker-schedule.yml`:

```yaml
on:
  schedule:
    # Every 5 minutes (default)
    - cron: '*/5 * * * *'

    # Every 10 minutes
    # - cron: '*/10 * * * *'

    # Every hour
    # - cron: '0 * * * *'

    # Daily at 2 AM
    # - cron: '0 2 * * *'

    # Every Monday at 9 AM
    # - cron: '0 9 * * 1'
```

Commit and push changes to apply new schedule.

### Worker Troubleshooting

**Worker exits immediately**:
```bash
# Check logs for errors
docker logs hob-worker-<run-id>

# Verify RabbitMQ is accessible
docker exec hob-api ping -c 3 hob-rabbitmq

# Test RabbitMQ connection
docker exec hob-rabbitmq rabbitmqctl list_queues
```

**Reports not generating**:
```bash
# Check queue has messages
# Access RabbitMQ management UI: http://rabbitmq.yourdomain.com
# Navigate to Queues → report-generation queue

# Verify reports volume is writable
ls -la /opt/hob/reports
touch /opt/hob/reports/test.txt
rm /opt/hob/reports/test.txt

# Check worker permissions
docker run --rm -v /opt/hob/reports:/reports alpine ls -la /reports
```

## Monitoring

### Application Monitoring

#### Grafana Dashboards

Access: `http://grafana.yourdomain.com`
Login: `admin` / your-password

**Available Dashboards**:
- API Performance Metrics
- Request Rates and Latency
- Error Rates
- Database Connections
- RabbitMQ Queue Depth

**Create Custom Dashboard**:
1. Log in to Grafana
2. Click **+** → **Dashboard**
3. Add panel
4. Select Prometheus data source
5. Write PromQL query
6. Save dashboard

#### Prometheus Metrics

Access: `http://prometheus.yourdomain.com`

**Useful Queries**:

```promql
# Request rate
rate(http_requests_total[5m])

# Error rate
rate(http_requests_total{status=~"5.."}[5m])

# Response time (95th percentile)
histogram_quantile(0.95, rate(http_request_duration_seconds_bucket[5m]))

# Active connections
http_requests_in_progress

# Database connections
sql_server_connections_active
```

#### Jaeger Tracing

Access: `http://jaeger.yourdomain.com`

**View Traces**:
1. Select service: `HOB.API` or `HOB.Worker`
2. Set time range
3. Click **Find Traces**
4. Click on trace to view details

**Trace Information**:
- Request flow across services
- Database query performance
- Message bus interactions
- Error details and stack traces

### Infrastructure Monitoring

#### Container Health

```bash
# Check all container health
docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"

# View unhealthy containers
docker ps --filter "health=unhealthy"

# Inspect container health
docker inspect --format='{{.State.Health.Status}}' hob-api
```

#### Resource Usage

```bash
# Real-time resource usage
docker stats

# Specific containers
docker stats hob-api hob-db hob-rabbitmq

# Memory usage
docker stats --no-stream --format "table {{.Name}}\t{{.MemUsage}}"

# CPU usage
docker stats --no-stream --format "table {{.Name}}\t{{.CPUPerc}}"
```

#### Disk Usage

```bash
# Overall disk usage
df -h

# Docker disk usage
docker system df

# Volume usage
docker system df -v

# HOB data directories
du -sh /opt/hob/*
du -sh /opt/hob/reports/*
```

#### Log Management

```bash
# View logs
docker logs hob-api
docker logs hob-api --since 1h
docker logs hob-api --tail 100

# Follow logs
docker logs -f hob-api

# Filter logs
docker logs hob-api 2>&1 | grep ERROR
docker logs hob-api 2>&1 | grep -i exception

# Export logs
docker logs hob-api > api-logs.txt

# Check log sizes
ls -lh /var/lib/docker/containers/*/*-json.log
```

### RabbitMQ Monitoring

Access: `http://rabbitmq.yourdomain.com`
Login: your-user / your-password

**Monitor Queues**:
1. Navigate to **Queues** tab
2. Check message rates
3. View ready vs unacknowledged messages
4. Monitor consumer count

**Check Connections**:
1. Navigate to **Connections** tab
2. Verify API and Worker connections
3. Check channel count

**Exchange Activity**:
1. Navigate to **Exchanges** tab
2. View message rates
3. Check bindings

### Database Monitoring

```bash
# Connect to SQL Server
docker exec -it hob-db /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'your-password' -C

# Check database size
SELECT
    name,
    size * 8 / 1024 AS size_mb
FROM sys.master_files
WHERE database_id = DB_ID('HOB')
GO

# Active connections
SELECT
    DB_NAME(dbid) as DBName,
    COUNT(dbid) as NumberOfConnections,
    loginame as LoginName
FROM sys.sysprocesses
WHERE dbid > 0
GROUP BY dbid, loginame
GO

# Current queries
SELECT
    session_id,
    status,
    command,
    cpu_time,
    total_elapsed_time
FROM sys.dm_exec_requests
WHERE session_id > 50
GO

# Exit
EXIT
```

## Maintenance

### Update Docker Images

```bash
# Pull latest images
docker pull ghcr.io/tangledbootlace/hob-api:latest
docker pull ghcr.io/tangledbootlace/hob-worker:latest
docker pull ghcr.io/tangledbootlace/hob-dashboard:latest

# Update via Portainer
# 1. Go to stack
# 2. Enable "Pull latest image versions"
# 3. Click "Update stack"

# Update via docker-compose
docker-compose pull
docker-compose up -d
```

### Database Maintenance

```bash
# Backup database
docker exec hob-db /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'password' -C -Q "BACKUP DATABASE [HOB] TO DISK = N'/var/opt/mssql/backup/HOB-$(date +%Y%m%d-%H%M%S).bak' WITH NOFORMAT, NOINIT, NAME = 'HOB-full', SKIP, NOREWIND, NOUNLOAD, STATS = 10"

# List backups
docker exec hob-db ls -lh /var/opt/mssql/backup/

# Restore database
docker exec hob-db /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'password' -C -Q "RESTORE DATABASE [HOB] FROM DISK = N'/var/opt/mssql/backup/HOB-20231215-100000.bak' WITH FILE = 1, NOUNLOAD, REPLACE, STATS = 5"

# Shrink database (if needed)
docker exec hob-db /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'password' -C -Q "DBCC SHRINKDATABASE (HOB, 10);"
```

### Clean Up Resources

```bash
# Remove stopped containers
docker container prune -f

# Remove unused images
docker image prune -a -f

# Remove unused volumes (careful!)
docker volume prune -f

# Remove unused networks
docker network prune -f

# Clean old worker containers
docker ps -a --filter "label=com.hob.service=report-generator" \
  --format "{{.Names}}" | sort -r | tail -n +6 | \
  xargs -r docker rm -f

# Clean old reports (older than 30 days)
find /opt/hob/reports -type f -mtime +30 -delete

# Clean Docker system
docker system prune -a --volumes -f
```

### Rotate Logs

```bash
# Configure Docker log rotation (add to /etc/docker/daemon.json)
cat > /etc/docker/daemon.json <<EOF
{
  "log-driver": "json-file",
  "log-opts": {
    "max-size": "10m",
    "max-file": "3"
  }
}
EOF

# Restart Docker
sudo systemctl restart docker

# Manual log cleanup
find /var/lib/docker/containers -name "*.log" -exec truncate -s 0 {} \;
```

## Backup and Recovery

### Full Backup Strategy

**Daily Backups**:
- Database backups
- Configuration files
- Prometheus data
- Grafana dashboards

**Weekly Backups**:
- Complete volume snapshots
- Docker images
- SSL certificates

### Backup Database

```bash
# Create backup script
cat > /opt/hob/scripts/backup-db.sh <<'EOF'
#!/bin/bash
BACKUP_DIR="/opt/hob/backups/database"
TIMESTAMP=$(date +%Y%m%d-%H%M%S)
mkdir -p ${BACKUP_DIR}

docker exec hob-db /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P "${DB_PASSWORD}" -C \
  -Q "BACKUP DATABASE [HOB] TO DISK = N'/var/opt/mssql/backup/HOB-${TIMESTAMP}.bak'"

docker cp hob-db:/var/opt/mssql/backup/HOB-${TIMESTAMP}.bak ${BACKUP_DIR}/

# Keep only last 7 days
find ${BACKUP_DIR} -type f -mtime +7 -delete
EOF

chmod +x /opt/hob/scripts/backup-db.sh

# Add to crontab
crontab -e
# Add: 0 2 * * * /opt/hob/scripts/backup-db.sh
```

### Backup Volumes

```bash
# Backup script
cat > /opt/hob/scripts/backup-volumes.sh <<'EOF'
#!/bin/bash
BACKUP_DIR="/opt/hob/backups/volumes"
TIMESTAMP=$(date +%Y%m%d-%H%M%S)
mkdir -p ${BACKUP_DIR}

# Stop services
docker-compose -f /path/to/docker-compose.yml stop

# Backup volumes
tar -czf ${BACKUP_DIR}/prometheus-${TIMESTAMP}.tar.gz -C /var/lib/docker/volumes/hob_prometheus-data/_data .
tar -czf ${BACKUP_DIR}/grafana-${TIMESTAMP}.tar.gz -C /var/lib/docker/volumes/hob_grafana-data/_data .
tar -czf ${BACKUP_DIR}/rabbitmq-${TIMESTAMP}.tar.gz -C /var/lib/docker/volumes/hob_rabbitmq-data/_data .

# Start services
docker-compose -f /path/to/docker-compose.yml start

# Cleanup old backups
find ${BACKUP_DIR} -type f -mtime +14 -delete
EOF

chmod +x /opt/hob/scripts/backup-volumes.sh
```

### Restore Database

```bash
# Copy backup file to container
docker cp /opt/hob/backups/database/HOB-20231215-100000.bak hob-db:/var/opt/mssql/backup/

# Restore
docker exec hob-db /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P 'password' -C \
  -Q "RESTORE DATABASE [HOB] FROM DISK = N'/var/opt/mssql/backup/HOB-20231215-100000.bak' WITH REPLACE"
```

## Scaling

### Horizontal Scaling

Scale API instances:

```bash
# Via docker-compose
docker-compose -f docker-compose.services.yml up -d --scale hob-api=3

# Via Portainer
# 1. Navigate to Services
# 2. Select hob-api
# 3. Scale service to desired replicas
```

Update Traefik for load balancing (automatic with labels).

### Vertical Scaling

Adjust resource limits in docker-compose:

```yaml
services:
  hob-api:
    deploy:
      resources:
        limits:
          cpus: '2.0'
          memory: 2G
        reservations:
          cpus: '1.0'
          memory: 1G
```

### Database Scaling

For production, consider:
- SQL Server Always On Availability Groups
- Read replicas
- Partitioning large tables

## Rollback Procedures

### Rollback Service Deployment

```bash
# Via GitHub Actions
# 1. Go to Deploy Services workflow
# 2. Run workflow with previous image tag
# 3. Specify tag: v1.2.2 (previous version)

# Via Portainer
# 1. Go to stack
# 2. Update IMAGE_TAG environment variable
# 3. Enable "Pull latest image versions"
# 4. Update stack

# Via Terraform
cd infrastructure/terraform
terraform apply -var="image_tag=v1.2.2"
```

### Rollback Infrastructure Changes

```bash
# Via Terraform state
terraform state list
terraform state show portainer_stack.infrastructure
terraform apply -target=portainer_stack.infrastructure

# Manual rollback
# 1. Go to Portainer
# 2. Stop current stack
# 3. Recreate with previous configuration
```

### Emergency Rollback

```bash
# Quick rollback to last known good state
cd infrastructure/terraform

# Check previous state
terraform show

# Rollback to specific version
terraform apply -var="image_tag=v1.2.2" -auto-approve

# Verify
docker ps --filter "label=com.hob.stack=services"
docker logs hob-api
```

## Best Practices

1. **Always test in staging before production**
2. **Use semantic versioning for images**
3. **Monitor deployments for at least 15 minutes**
4. **Keep previous image tags available for quick rollback**
5. **Schedule maintenance during low-traffic periods**
6. **Backup before major changes**
7. **Document all configuration changes**
8. **Review logs after deployments**
9. **Test rollback procedures regularly**
10. **Maintain deployment runbooks**

## Getting Help

For deployment issues:
1. Check [Infrastructure Setup Guide](Infrastructure-Setup.md)
2. Review [Making Infrastructure Changes](Infrastructure-Changes.md)
3. Check container logs and metrics
4. Review GitHub Actions workflow logs
5. Consult Portainer stack events
6. Contact development team
