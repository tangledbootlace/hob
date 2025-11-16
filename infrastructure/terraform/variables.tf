variable "portainer_url" {
  description = "Portainer server URL"
  type        = string
  sensitive   = false
}

variable "portainer_username" {
  description = "Portainer admin username"
  type        = string
  sensitive   = true
}

variable "portainer_password" {
  description = "Portainer admin password"
  type        = string
  sensitive   = true
}

variable "portainer_endpoint_id" {
  description = "Portainer endpoint ID where stacks will be deployed"
  type        = number
  default     = 1
}

variable "environment" {
  description = "Environment name (dev, staging, production)"
  type        = string
  default     = "production"
}

variable "hob_api_domain" {
  description = "Domain for the HOB API"
  type        = string
  default     = "hob.api.localhost"
}

variable "hob_dashboard_domain" {
  description = "Domain for the HOB Dashboard"
  type        = string
  default     = "dashboard.hob.localhost"
}

variable "grafana_domain" {
  description = "Domain for Grafana"
  type        = string
  default     = "grafana.hob.localhost"
}

variable "prometheus_domain" {
  description = "Domain for Prometheus"
  type        = string
  default     = "prometheus.hob.localhost"
}

variable "jaeger_domain" {
  description = "Domain for Jaeger"
  type        = string
  default     = "jaeger.hob.localhost"
}

variable "rabbitmq_domain" {
  description = "Domain for RabbitMQ management UI"
  type        = string
  default     = "web.rabbitmq.hob.localhost"
}

variable "db_password" {
  description = "SQL Server SA password (must be strong: min 8 chars, uppercase, lowercase, number, special char)"
  type        = string
  sensitive   = true

  validation {
    condition     = can(regex("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[@$!%*?&])[A-Za-z\\d@$!%*?&]{8,}$", var.db_password))
    error_message = "Password must be at least 8 characters and contain uppercase, lowercase, number, and special character."
  }
}

variable "rabbitmq_user" {
  description = "RabbitMQ username"
  type        = string
  sensitive   = false
}

variable "rabbitmq_password" {
  description = "RabbitMQ password (minimum 8 characters recommended)"
  type        = string
  sensitive   = true

  validation {
    condition     = length(var.rabbitmq_password) >= 8
    error_message = "RabbitMQ password must be at least 8 characters long."
  }
}

variable "grafana_admin_password" {
  description = "Grafana admin password (minimum 8 characters recommended)"
  type        = string
  sensitive   = true

  validation {
    condition     = length(var.grafana_admin_password) >= 8
    error_message = "Grafana admin password must be at least 8 characters long."
  }
}

variable "docker_registry" {
  description = "Docker registry for application images"
  type        = string
  default     = "ghcr.io"
}

variable "docker_registry_username" {
  description = "Docker registry username"
  type        = string
  sensitive   = true
  default     = ""
}

variable "docker_registry_password" {
  description = "Docker registry password/token"
  type        = string
  sensitive   = true
  default     = ""
}

variable "image_tag" {
  description = "Docker image tag to deploy"
  type        = string
  default     = "latest"
}

variable "worker_schedule" {
  description = "Cron schedule for worker job (every 5 minutes)"
  type        = string
  default     = "*/5 * * * *"
}

variable "data_volume_path" {
  description = "Host path for persistent data storage"
  type        = string
  default     = "/opt/hob/data"
}

variable "reports_volume_path" {
  description = "Host path for worker report output"
  type        = string
  default     = "/opt/hob/reports"
}
