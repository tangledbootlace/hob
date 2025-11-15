# HOB Infrastructure

This directory contains Terraform configuration for deploying the House of Burgesses Services to a Portainer stack.

## Directory Structure

```
infrastructure/
├── terraform/           # Root Terraform configuration
│   ├── provider.tf     # Provider configuration
│   ├── variables.tf    # Variable definitions
│   ├── main.tf         # Main stack resources
│   ├── outputs.tf      # Output values
│   └── terraform.tfvars.example  # Example variables
```

## Quick Start

1. Copy `terraform.tfvars.example` to `terraform.tfvars`:
   ```bash
   cd infrastructure/terraform
   cp terraform.tfvars.example terraform.tfvars
   ```

2. Edit `terraform.tfvars` with your configuration

3. Initialize Terraform:
   ```bash
   terraform init
   ```

4. Plan the deployment:
   ```bash
   terraform plan
   ```

5. Apply the configuration:
   ```bash
   terraform apply
   ```

## Documentation

For detailed setup and usage instructions, see the [Wiki](../.wiki):

- [Infrastructure Setup](../.wiki/Infrastructure-Setup.md)
- [Deployment Guide](../.wiki/Deployment-and-Operations.md)
- [Making Infrastructure Changes](../.wiki/Infrastructure-Changes.md)

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

## Security

- Never commit `terraform.tfvars` or any files with credentials
- Use environment variables or secret management for sensitive values
- Configure remote state backend for production
- Rotate passwords regularly
