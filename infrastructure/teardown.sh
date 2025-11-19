#!/bin/bash
set -euo pipefail

# HOB Infrastructure Teardown Script
# This script safely destroys all infrastructure resources managed by Terraform
# Usage: ./teardown.sh [--auto-approve] [--destroy-volumes]

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
TERRAFORM_DIR="${SCRIPT_DIR}/terraform"

# Color codes for output
RED='\033[0;31m'
YELLOW='\033[1;33m'
GREEN='\033[0;32m'
NC='\033[0m' # No Color

# Parse arguments
AUTO_APPROVE=false
DESTROY_VOLUMES=false

while [[ $# -gt 0 ]]; do
  case $1 in
    --auto-approve)
      AUTO_APPROVE=true
      shift
      ;;
    --destroy-volumes)
      DESTROY_VOLUMES=true
      shift
      ;;
    *)
      echo "Unknown option: $1"
      echo "Usage: $0 [--auto-approve] [--destroy-volumes]"
      exit 1
      ;;
  esac
done

log_info() {
  echo -e "${GREEN}[INFO]${NC} $1"
}

log_warn() {
  echo -e "${YELLOW}[WARN]${NC} $1"
}

log_error() {
  echo -e "${RED}[ERROR]${NC} $1"
}

# Check if Terraform is installed
if ! command -v terraform &> /dev/null; then
  log_error "Terraform is not installed. Please install Terraform first."
  exit 1
fi

# Change to Terraform directory
cd "${TERRAFORM_DIR}"

# Check if Terraform is initialized
if [ ! -d ".terraform" ]; then
  log_error "Terraform is not initialized. Run 'terraform init' first."
  exit 1
fi

# Display warning
log_warn "=========================================="
log_warn "  HOB INFRASTRUCTURE TEARDOWN"
log_warn "=========================================="
echo ""
log_warn "This will destroy the following resources:"
echo "  - HOB Services (API, Dashboard)"
echo "  - HOB Infrastructure (Database, RabbitMQ, Monitoring)"
echo "  - All data in ephemeral volumes"
echo ""

if [ "$DESTROY_VOLUMES" = true ]; then
  log_warn "⚠️  --destroy-volumes flag detected!"
  log_warn "This will also delete persistent volumes containing:"
  echo "  - Database data"
  echo "  - Generated reports"
  echo "  - Prometheus metrics history"
  echo "  - Grafana dashboards and settings"
  echo ""
fi

# Safety confirmation
if [ "$AUTO_APPROVE" = false ]; then
  echo -n "Type 'yes' to confirm destruction: "
  read -r confirmation
  if [ "$confirmation" != "yes" ]; then
    log_info "Teardown cancelled."
    exit 0
  fi
fi

log_info "Starting teardown process..."
echo ""

# Run terraform destroy
log_info "Running Terraform destroy..."
if [ "$AUTO_APPROVE" = true ]; then
  terraform destroy -auto-approve
else
  terraform destroy
fi

# Check if destroy was successful
if [ $? -eq 0 ]; then
  log_info "Terraform resources destroyed successfully."
else
  log_error "Terraform destroy failed. Please check the errors above."
  exit 1
fi

# Optionally destroy volumes
if [ "$DESTROY_VOLUMES" = true ]; then
  log_warn "Destroying persistent volumes..."

  # Source the .env file if it exists to get volume paths
  if [ -f "${SCRIPT_DIR}/.env" ]; then
    source "${SCRIPT_DIR}/.env"

    if [ -n "${DATA_VOLUME_PATH:-}" ] && [ -d "${DATA_VOLUME_PATH}" ]; then
      log_warn "Removing data volume: ${DATA_VOLUME_PATH}"
      sudo rm -rf "${DATA_VOLUME_PATH}"
    fi

    if [ -n "${REPORTS_VOLUME_PATH:-}" ] && [ -d "${REPORTS_VOLUME_PATH}" ]; then
      log_warn "Removing reports volume: ${REPORTS_VOLUME_PATH}"
      sudo rm -rf "${REPORTS_VOLUME_PATH}"
    fi
  else
    log_warn ".env file not found. Skipping volume cleanup."
    log_warn "Default volume paths:"
    echo "  - /opt/hob/data"
    echo "  - /opt/hob/reports"
    echo ""
    echo -n "Remove these directories? (yes/no): "
    read -r vol_confirm
    if [ "$vol_confirm" = "yes" ]; then
      [ -d "/opt/hob/data" ] && sudo rm -rf /opt/hob/data
      [ -d "/opt/hob/reports" ] && sudo rm -rf /opt/hob/reports
      log_info "Default volumes removed."
    fi
  fi
fi

# Clean up Terraform state (optional)
log_info "Terraform state files remain for recovery purposes."
log_info "To completely remove Terraform state, run:"
echo "  cd ${TERRAFORM_DIR}"
echo "  rm -rf .terraform terraform.tfstate* .terraform.lock.hcl"
echo ""

log_info "=========================================="
log_info "  TEARDOWN COMPLETE"
log_info "=========================================="
echo ""
log_info "To redeploy, run:"
echo "  cd ${TERRAFORM_DIR}"
echo "  terraform init"
echo "  terraform plan"
echo "  terraform apply"
