# Making Infrastructure Changes

This guide explains how to safely make changes to the HOB infrastructure configuration.

## Table of Contents

- [Understanding the Infrastructure](#understanding-the-infrastructure)
- [Making Changes Safely](#making-changes-safely)
- [Common Change Scenarios](#common-change-scenarios)
- [Terraform Workflow](#terraform-workflow)
- [Docker Compose Changes](#docker-compose-changes)
- [Environment Variable Changes](#environment-variable-changes)
- [GitHub Actions Changes](#github-actions-changes)
- [Testing Changes](#testing-changes)
- [Applying Changes](#applying-changes)

## Understanding the Infrastructure

### Infrastructure Layers

The HOB infrastructure consists of three main layers:

1. **Infrastructure Stack**
   - Database (SQL Server)
   - Message Queue (RabbitMQ)
   - Monitoring (Prometheus, Grafana, Jaeger)
   - Reverse Proxy (Traefik)

2. **Services Stack**
   - API (HOB.API)
   - Dashboard (HOB.Dashboard)

3. **Worker Jobs**
   - Report Generator (HOB.Worker)
   - Scheduled via GitHub Actions

### File Structure

```
hob/
├── infrastructure/
│   └── terraform/              # Terraform configurations
│       ├── provider.tf         # Provider setup
│       ├── variables.tf        # Variable definitions
│       ├── main.tf            # Main resources
│       ├── outputs.tf         # Output values
│       └── terraform.tfvars   # Variable values (gitignored)
├── HOB.API/
│   └── infrastructure/
│       ├── docker-compose.infrastructure.yml
│       └── docker-compose.services.yml
├── HOB.Worker/
│   └── infrastructure/
│       ├── docker-compose.worker.yml
│       └── run-worker.sh
└── .github/
    └── workflows/             # CI/CD workflows
        ├── build-and-push.yml
        ├── deploy-infrastructure.yml
        ├── deploy-services.yml
        └── worker-schedule.yml
```

## Making Changes Safely

### Change Process

1. **Plan**: Identify what needs to change
2. **Test**: Test changes locally or in staging
3. **Review**: Review changes with team
4. **Apply**: Deploy to production
5. **Verify**: Confirm changes work as expected
6. **Monitor**: Watch for issues
7. **Document**: Update documentation

### Development Workflow

```bash
# 1. Create feature branch
git checkout -b feature/infrastructure-change

# 2. Make changes to configuration files
# Edit infrastructure files...

# 3. Test locally
docker-compose -f docker-compose.production.yml up -d

# 4. Commit changes
git add .
git commit -m "feat: add new monitoring dashboard"

# 5. Push to GitHub
git push origin feature/infrastructure-change

# 6. Create pull request
# Review changes with team

# 7. Merge to main
# Deploy via GitHub Actions
```

## Common Change Scenarios

### Add New Environment Variable

**Files to modify**:
1. `infrastructure/terraform/variables.tf`
2. `infrastructure/terraform/main.tf`
3. `HOB.API/infrastructure/docker-compose.*.yml`

**Example: Add CORS allowed origins**

1. Add variable definition:

```hcl
# infrastructure/terraform/variables.tf
variable "cors_allowed_origins" {
  description = "Allowed CORS origins"
  type        = string
  default     = "*"
}
```

2. Add to stack environment:

```hcl
# infrastructure/terraform/main.tf
resource "portainer_stack" "services" {
  # ... existing config ...

  env = [
    # ... existing vars ...
    {
      name  = "CORS_ALLOWED_ORIGINS"
      value = var.cors_allowed_origins
    }
  ]
}
```

3. Add to docker-compose:

```yaml
# HOB.API/infrastructure/docker-compose.services.yml
services:
  hob-api:
    environment:
      - CORS_ALLOWED_ORIGINS=${CORS_ALLOWED_ORIGINS}
```

4. Add to GitHub Actions:

```yaml
# .github/workflows/deploy-services.yml
- name: Deploy Services
  run: |
    cat > terraform.tfvars <<EOF
    # ... existing vars ...
    cors_allowed_origins = "${{ vars.CORS_ALLOWED_ORIGINS }}"
    EOF
```

5. Add GitHub repository variable:
   - Go to Settings → Secrets and variables → Actions → Variables
   - New repository variable: `CORS_ALLOWED_ORIGINS`

### Add New Service to Infrastructure

**Example: Add Redis cache**

1. Update infrastructure docker-compose:

```yaml
# HOB.API/infrastructure/docker-compose.infrastructure.yml
services:
  redis:
    image: redis:7-alpine
    container_name: hob-redis
    restart: unless-stopped
    command: redis-server --requirepass ${REDIS_PASSWORD}
    volumes:
      - redis-data:/data
    networks:
      - hob-network
    healthcheck:
      test: ["CMD", "redis-cli", "ping"]
      interval: 10s
      timeout: 3s
      retries: 5
    labels:
      - "com.hob.stack=infrastructure"

volumes:
  redis-data:
    driver: local
```

2. Add connection string to services:

```yaml
# HOB.API/infrastructure/docker-compose.services.yml
services:
  hob-api:
    environment:
      - ConnectionStrings__Redis=redis:6379,password=${REDIS_PASSWORD}
    depends_on:
      - redis
```

3. Update Terraform variables:

```hcl
# infrastructure/terraform/variables.tf
variable "redis_password" {
  description = "Redis password"
  type        = string
  sensitive   = true
  default     = ""
}
```

4. Add to Terraform main:

```hcl
# infrastructure/terraform/main.tf
resource "portainer_stack" "infrastructure" {
  env = [
    # ... existing vars ...
    {
      name  = "REDIS_PASSWORD"
      value = var.redis_password
    }
  ]
}
```

### Change Resource Limits

**Example: Increase API memory limit**

```yaml
# HOB.API/infrastructure/docker-compose.services.yml
services:
  hob-api:
    deploy:
      resources:
        limits:
          cpus: '2.0'        # Changed from 1.0
          memory: 4G          # Changed from 2G
        reservations:
          cpus: '1.0'
          memory: 2G          # Changed from 1G
```

### Update Service Ports

**Example: Change API port**

1. Update environment variable:

```yaml
# HOB.API/infrastructure/docker-compose.services.yml
services:
  hob-api:
    environment:
      - ASPNETCORE_URLS=http://+:9000  # Changed from 8080
```

2. Update Traefik labels:

```yaml
services:
  hob-api:
    labels:
      - "traefik.http.services.hob-api.loadbalancer.server.port=9000"
```

### Add New Monitoring Dashboard

**Example: Add custom Grafana dashboard**

1. Create dashboard JSON:

```bash
# Create dashboard file
cat > /opt/hob/data/grafana/dashboards/custom-dashboard.json <<EOF
{
  "dashboard": {
    "title": "Custom HOB Dashboard",
    "panels": [...]
  }
}
EOF
```

2. Dashboard auto-loads via provisioning configuration.

### Change Database Configuration

**Example: Increase max connections**

```yaml
# HOB.API/infrastructure/docker-compose.infrastructure.yml
services:
  db:
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=${SA_PASSWORD}
      - MSSQL_PID=Developer
      - MSSQL_MEMORY_LIMIT_MB=4096  # Add memory limit
    command:
      - /opt/mssql/bin/sqlservr
      - --max-connections=500       # Add max connections
```

### Modify Worker Schedule

**Change from 5 minutes to 10 minutes**:

```yaml
# .github/workflows/worker-schedule.yml
on:
  schedule:
    - cron: '*/10 * * * *'  # Changed from */5
```

Commit and push to apply new schedule.

## Terraform Workflow

### Making Terraform Changes

1. **Edit Terraform files**:

```bash
cd infrastructure/terraform
nano variables.tf  # Add new variables
nano main.tf       # Modify resources
```

2. **Format and validate**:

```bash
terraform fmt
terraform validate
```

3. **Preview changes**:

```bash
terraform plan
```

4. **Review plan output**:
   - Check what will be created/modified/destroyed
   - Verify no unexpected changes
   - Confirm changes match intent

5. **Apply changes**:

```bash
terraform apply
```

6. **Verify outputs**:

```bash
terraform output
```

### Terraform State Management

**View current state**:

```bash
terraform state list
terraform state show portainer_stack.infrastructure
```

**Import existing resources**:

```bash
terraform import portainer_stack.infrastructure <stack-id>
```

**Remove resource from state** (without destroying):

```bash
terraform state rm portainer_stack.infrastructure
```

### Terraform Best Practices

1. **Always run `terraform plan` before `apply`**
2. **Review plan output carefully**
3. **Use variables for all configurable values**
4. **Keep `terraform.tfvars` in `.gitignore`**
5. **Use remote state for team environments**
6. **Tag resources for identification**
7. **Document all variable changes**

## Docker Compose Changes

### Local Testing

Before deploying changes, test locally:

```bash
# Test infrastructure stack
docker-compose -f HOB.API/infrastructure/docker-compose.infrastructure.yml config
docker-compose -f HOB.API/infrastructure/docker-compose.infrastructure.yml up -d

# Test services stack
docker-compose -f HOB.API/infrastructure/docker-compose.services.yml config
docker-compose -f HOB.API/infrastructure/docker-compose.services.yml up -d

# Check logs
docker-compose -f HOB.API/infrastructure/docker-compose.services.yml logs -f

# Clean up
docker-compose -f HOB.API/infrastructure/docker-compose.services.yml down
docker-compose -f HOB.API/infrastructure/docker-compose.infrastructure.yml down
```

### Validate Syntax

```bash
# Validate docker-compose syntax
docker-compose -f docker-compose.infrastructure.yml config

# Check for errors
docker-compose -f docker-compose.services.yml config > /dev/null && echo "Valid" || echo "Invalid"
```

### Update Existing Stack

After changing docker-compose files:

1. **Via Portainer**:
   - Go to stack
   - Update docker-compose content
   - Click "Update the stack"

2. **Via Terraform**:
   ```bash
   terraform apply
   ```

3. **Via Docker Compose**:
   ```bash
   docker-compose -f docker-compose.yml up -d
   ```

## Environment Variable Changes

### Secrets vs Variables

**Use Secrets for**:
- Passwords
- API keys
- Tokens
- Certificates
- Private keys

**Use Variables for**:
- Domain names
- Feature flags
- Non-sensitive configuration
- Resource limits
- Schedule configuration

### Add New Secret

1. Go to GitHub repository → Settings → Secrets and variables → Actions
2. Click "New repository secret"
3. Name: `NEW_SECRET_NAME`
4. Value: `secret-value`
5. Click "Add secret"

### Add New Variable

1. Go to GitHub repository → Settings → Secrets and variables → Actions → Variables
2. Click "New repository variable"
3. Name: `NEW_VARIABLE_NAME`
4. Value: `variable-value`
5. Click "Add variable"

### Use in Workflow

```yaml
# .github/workflows/deploy-services.yml
steps:
  - name: Deploy
    env:
      SECRET_VALUE: ${{ secrets.NEW_SECRET_NAME }}
      VARIABLE_VALUE: ${{ vars.NEW_VARIABLE_NAME }}
    run: |
      echo "Secret: ${SECRET_VALUE}"
      echo "Variable: ${VARIABLE_VALUE}"
```

### Rotate Secrets

1. **Generate new secret value**
2. **Update GitHub secret**
3. **Update server configuration** (if stored locally)
4. **Redeploy services** to pick up new value
5. **Verify services work with new secret**
6. **Revoke old secret**

## GitHub Actions Changes

### Modify Workflow

1. **Edit workflow file**:

```yaml
# .github/workflows/deploy-services.yml
on:
  workflow_dispatch:
    inputs:
      environment:
        description: 'Environment'
        required: true
        type: choice
        options:
          - production
          - staging
          - development
          - test  # New option
```

2. **Commit and push**:

```bash
git add .github/workflows/deploy-services.yml
git commit -m "feat: add test environment to workflow"
git push
```

3. **Test workflow**:
   - Go to Actions
   - Select workflow
   - Click "Run workflow"
   - Verify new option appears

### Add New Workflow

1. **Create workflow file**:

```bash
cat > .github/workflows/database-backup.yml <<'EOF'
name: Database Backup

on:
  schedule:
    - cron: '0 2 * * *'  # Daily at 2 AM
  workflow_dispatch:

jobs:
  backup:
    runs-on: ubuntu-latest
    steps:
      - name: Backup database
        run: |
          # Backup logic here
EOF
```

2. **Commit and push**:

```bash
git add .github/workflows/database-backup.yml
git commit -m "feat: add database backup workflow"
git push
```

### Workflow Best Practices

1. **Use meaningful names**
2. **Add descriptions to inputs**
3. **Include manual trigger for testing**
4. **Add step summaries for visibility**
5. **Handle errors gracefully**
6. **Use secrets for sensitive data**
7. **Test workflows in feature branches**

## Testing Changes

### Test Infrastructure Locally

```bash
# Set environment variables
export SA_PASSWORD="TestPassword123!"
export RABBITMQ_USER="test"
export RABBITMQ_PASSWORD="test"
export GRAFANA_DOMAIN="grafana.localhost"
# ... set other vars ...

# Start infrastructure
docker-compose -f HOB.API/infrastructure/docker-compose.infrastructure.yml up -d

# Wait for health checks
sleep 30

# Verify services
docker ps --filter "label=com.hob.stack=infrastructure"

# Test database connection
docker exec hob-db /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "${SA_PASSWORD}" -C -Q "SELECT 1"

# Test RabbitMQ
docker exec hob-rabbitmq rabbitmqctl status

# Clean up
docker-compose -f HOB.API/infrastructure/docker-compose.infrastructure.yml down -v
```

### Test in Staging Environment

1. **Create staging stack in Portainer**:
   - Name: `hob-infrastructure-staging`
   - Use staging environment variables
   - Deploy to test endpoint

2. **Deploy via GitHub Actions**:
   - Select "staging" environment
   - Monitor deployment

3. **Run smoke tests**:
   - Check all services are healthy
   - Test API endpoints
   - Verify monitoring works
   - Check worker executions

4. **Monitor for issues**:
   - Watch logs for errors
   - Check metrics in Grafana
   - Review traces in Jaeger

### Validation Checklist

Before applying to production:

- [ ] Changes tested locally
- [ ] Changes tested in staging
- [ ] Terraform plan reviewed
- [ ] Docker compose syntax validated
- [ ] Environment variables configured
- [ ] Secrets rotated if needed
- [ ] Documentation updated
- [ ] Team notified of changes
- [ ] Rollback plan prepared
- [ ] Monitoring configured for new components

## Applying Changes

### Production Deployment Process

1. **Pre-deployment**:
   ```bash
   # Backup database
   /opt/hob/scripts/backup-db.sh

   # Backup volumes
   /opt/hob/scripts/backup-volumes.sh

   # Document current state
   docker ps > pre-deployment-state.txt
   ```

2. **Deploy via GitHub Actions**:
   - Go to Actions → Deploy Infrastructure
   - Select "production" environment
   - Choose "plan" to preview
   - Review plan output
   - Run again with "apply"
   - Monitor deployment

3. **Verify deployment**:
   ```bash
   # Check container status
   docker ps --filter "label=com.hob.stack"

   # Test API
   curl http://api.yourdomain.com/health

   # Check logs
   docker logs hob-api --tail 100
   ```

4. **Monitor for issues**:
   - Watch Grafana dashboards
   - Check error rates
   - Review application logs
   - Monitor resource usage

5. **Post-deployment**:
   ```bash
   # Document new state
   docker ps > post-deployment-state.txt

   # Update runbook
   # Notify team
   ```

### Rollback Procedure

If issues occur:

1. **Immediate rollback**:
   ```bash
   # Via GitHub Actions
   # Deploy with previous image tag

   # Via Terraform
   cd infrastructure/terraform
   terraform apply -var="image_tag=v1.2.2"
   ```

2. **Verify rollback**:
   ```bash
   docker ps
   docker logs hob-api
   curl http://api.yourdomain.com/health
   ```

3. **Investigate issues**:
   - Review deployment logs
   - Check configuration differences
   - Test changes in staging
   - Fix issues and redeploy

## Change Documentation

### Document Changes

Always document infrastructure changes:

1. **Update CLAUDE.md**:
   - Add new environment variables
   - Document new services
   - Update architecture section

2. **Update README files**:
   - Update infrastructure README
   - Update project-specific README files
   - Add troubleshooting notes

3. **Update Wiki**:
   - Update relevant wiki pages
   - Add new sections if needed
   - Include examples

4. **Create Change Log**:
   ```markdown
   ## 2024-01-15 - Add Redis Cache

   ### Changes
   - Added Redis service to infrastructure stack
   - Updated API to use Redis for caching
   - Added `REDIS_PASSWORD` environment variable

   ### Deployment
   - Deployed via Terraform
   - No downtime required

   ### Verification
   - Redis container running
   - API cache hit rate: 85%
   ```

### Track Changes in Git

```bash
# Meaningful commit messages
git commit -m "feat(infrastructure): add Redis cache service"
git commit -m "fix(worker): increase memory limit to 2GB"
git commit -m "chore(terraform): update Portainer provider to v2.1"

# Tag releases
git tag -a v1.3.0 -m "Release v1.3.0 - Add Redis caching"
git push origin v1.3.0
```

## Best Practices

1. **Test changes in non-production first**
2. **Use version control for all changes**
3. **Document changes thoroughly**
4. **Create backups before major changes**
5. **Monitor deployments closely**
6. **Have rollback plan ready**
7. **Communicate changes to team**
8. **Review changes with peers**
9. **Update documentation immediately**
10. **Learn from issues and improve process**

## Getting Help

For infrastructure change questions:
1. Review this guide
2. Check [Infrastructure Setup Guide](Infrastructure-Setup.md)
3. Review [Deployment and Operations Guide](Deployment-and-Operations.md)
4. Test in local/staging first
5. Consult with team
6. Document your learnings
