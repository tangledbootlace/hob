#!/bin/bash
# Worker execution script for scheduled runs
# This script is called by GitHub Actions on a cron schedule

set -e

# Configuration
WORKER_IMAGE="${DOCKER_REGISTRY}/tangledbootlace/hob-worker:${IMAGE_TAG}"
CONTAINER_NAME="hob-worker-$(date +%s)"
NETWORK="hob-network"
REPORTS_PATH="${REPORTS_VOLUME_PATH:-/opt/hob/reports}"

# Required environment variables
: "${DB_PASSWORD:?DB_PASSWORD is required}"
: "${RABBITMQ_USER:?RABBITMQ_USER is required}"
: "${RABBITMQ_PASSWORD:?RABBITMQ_PASSWORD is required}"

echo "==================================="
echo "HOB Worker - Report Generation Job"
echo "==================================="
echo "Starting at: $(date)"
echo "Image: ${WORKER_IMAGE}"
echo "Container: ${CONTAINER_NAME}"
echo "Reports path: ${REPORTS_PATH}"
echo ""

# Ensure reports directory exists
mkdir -p "${REPORTS_PATH}"

# Pull latest image
echo "Pulling latest worker image..."
docker pull "${WORKER_IMAGE}"

# Run worker container
echo "Starting worker container..."
docker run \
  --name "${CONTAINER_NAME}" \
  --network "${NETWORK}" \
  --env DOTNET_ENVIRONMENT="${DOTNET_ENVIRONMENT:-Production}" \
  --env "ConnectionStrings__DefaultConnection=Server=hob-db;Database=HOB;User Id=sa;Password=${DB_PASSWORD};TrustServerCertificate=True;" \
  --env "ConnectionStrings__RabbitMQ=amqp://${RABBITMQ_USER}:${RABBITMQ_PASSWORD}@hob-rabbitmq:5672" \
  --env "ReportSettings__OutputDirectory=/reports" \
  --env "OpenTelemetry__ServiceName=HOB.Worker" \
  --env "OpenTelemetry__JaegerEndpoint=http://hob-jaeger:4317" \
  --volume "${REPORTS_PATH}:/reports" \
  --label "com.hob.stack=worker" \
  --label "com.hob.service=report-generator" \
  --label "com.hob.run-time=$(date -Iseconds)" \
  --rm \
  "${WORKER_IMAGE}"

EXIT_CODE=$?

echo ""
echo "==================================="
echo "Worker completed at: $(date)"
echo "Exit code: ${EXIT_CODE}"
echo "==================================="

# Cleanup old worker containers (keep last 5)
echo "Cleaning up old worker containers..."
docker ps -a --filter "label=com.hob.service=report-generator" --format "{{.Names}}" | \
  sort -r | tail -n +6 | xargs -r docker rm -f

exit ${EXIT_CODE}
