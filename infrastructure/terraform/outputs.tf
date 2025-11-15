output "infrastructure_stack_id" {
  description = "Portainer stack ID for infrastructure"
  value       = portainer_stack.infrastructure.id
}

output "infrastructure_stack_name" {
  description = "Portainer stack name for infrastructure"
  value       = portainer_stack.infrastructure.name
}

output "services_stack_id" {
  description = "Portainer stack ID for services"
  value       = portainer_stack.services.id
}

output "services_stack_name" {
  description = "Portainer stack name for services"
  value       = portainer_stack.services.name
}

output "api_url" {
  description = "HOB API URL"
  value       = "http://${var.hob_api_domain}"
}

output "dashboard_url" {
  description = "HOB Dashboard URL"
  value       = "http://${var.hob_dashboard_domain}"
}

output "grafana_url" {
  description = "Grafana URL"
  value       = "http://${var.grafana_domain}"
}

output "prometheus_url" {
  description = "Prometheus URL"
  value       = "http://${var.prometheus_domain}"
}

output "jaeger_url" {
  description = "Jaeger URL"
  value       = "http://${var.jaeger_domain}"
}

output "rabbitmq_management_url" {
  description = "RabbitMQ Management UI URL"
  value       = "http://${var.rabbitmq_domain}"
}

output "worker_schedule" {
  description = "Worker cron schedule"
  value       = var.worker_schedule
}
