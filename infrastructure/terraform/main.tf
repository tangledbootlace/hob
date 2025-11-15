locals {
  stack_name_infrastructure = "hob-infrastructure-${var.environment}"
  stack_name_services       = "hob-services-${var.environment}"
  stack_name_worker         = "hob-worker-${var.environment}"

  common_labels = {
    "com.hob.environment" = var.environment
    "com.hob.managed-by"  = "terraform"
  }
}

# Infrastructure Stack (Database, RabbitMQ, Monitoring)
resource "portainer_stack" "infrastructure" {
  name         = local.stack_name_infrastructure
  endpoint_id  = var.portainer_endpoint_id

  env = [
    {
      name  = "SA_PASSWORD"
      value = var.db_password
    },
    {
      name  = "RABBITMQ_DEFAULT_USER"
      value = var.rabbitmq_user
    },
    {
      name  = "RABBITMQ_DEFAULT_PASS"
      value = var.rabbitmq_password
    },
    {
      name  = "GF_SECURITY_ADMIN_PASSWORD"
      value = var.grafana_admin_password
    },
    {
      name  = "PROMETHEUS_DOMAIN"
      value = var.prometheus_domain
    },
    {
      name  = "GRAFANA_DOMAIN"
      value = var.grafana_domain
    },
    {
      name  = "JAEGER_DOMAIN"
      value = var.jaeger_domain
    },
    {
      name  = "RABBITMQ_DOMAIN"
      value = var.rabbitmq_domain
    },
    {
      name  = "DATA_VOLUME_PATH"
      value = var.data_volume_path
    }
  ]

  file_content = templatefile("${path.module}/../../HOB.API/infrastructure/docker-compose.infrastructure.yml", {
    prometheus_domain = var.prometheus_domain
    grafana_domain    = var.grafana_domain
    jaeger_domain     = var.jaeger_domain
    rabbitmq_domain   = var.rabbitmq_domain
    data_volume_path  = var.data_volume_path
  })
}

# Services Stack (API, Dashboard)
resource "portainer_stack" "services" {
  name         = local.stack_name_services
  endpoint_id  = var.portainer_endpoint_id

  depends_on = [portainer_stack.infrastructure]

  env = [
    {
      name  = "ASPNETCORE_ENVIRONMENT"
      value = var.environment == "production" ? "Production" : "Development"
    },
    {
      name  = "DB_PASSWORD"
      value = var.db_password
    },
    {
      name  = "RABBITMQ_USER"
      value = var.rabbitmq_user
    },
    {
      name  = "RABBITMQ_PASSWORD"
      value = var.rabbitmq_password
    },
    {
      name  = "HOB_API_DOMAIN"
      value = var.hob_api_domain
    },
    {
      name  = "HOB_DASHBOARD_DOMAIN"
      value = var.hob_dashboard_domain
    },
    {
      name  = "IMAGE_TAG"
      value = var.image_tag
    },
    {
      name  = "DOCKER_REGISTRY"
      value = var.docker_registry
    }
  ]

  file_content = templatefile("${path.module}/../../HOB.API/infrastructure/docker-compose.services.yml", {
    hob_api_domain       = var.hob_api_domain
    hob_dashboard_domain = var.hob_dashboard_domain
    image_tag            = var.image_tag
    docker_registry      = var.docker_registry
    environment          = var.environment == "production" ? "Production" : "Development"
  })
}

# Note: Worker is deployed via GitHub Actions on a cron schedule
# This resource serves as documentation of the worker configuration
# but is not actively deployed by Terraform
resource "null_resource" "worker_documentation" {
  triggers = {
    schedule = var.worker_schedule
  }

  provisioner "local-exec" {
    command = "echo 'Worker job is scheduled via GitHub Actions with schedule: ${var.worker_schedule}'"
  }
}
