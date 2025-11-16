#!/bin/bash
set -euo pipefail

# HOB Infrastructure Validation Script
# This script validates that all prerequisites are met before deployment
# Usage: ./validate.sh [--env-file .env]

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
TERRAFORM_DIR="${SCRIPT_DIR}/terraform"
ENV_FILE="${SCRIPT_DIR}/.env"

# Color codes for output
RED='\033[0;31m'
YELLOW='\033[1;33m'
GREEN='\033[0;32m'
NC='\033[0m' # No Color

# Validation counters
ERRORS=0
WARNINGS=0
CHECKS=0

# Parse arguments
while [[ $# -gt 0 ]]; do
  case $1 in
    --env-file)
      ENV_FILE="$2"
      shift 2
      ;;
    *)
      echo "Unknown option: $1"
      echo "Usage: $0 [--env-file .env]"
      exit 1
      ;;
  esac
done

log_info() {
  echo -e "${GREEN}[✓]${NC} $1"
}

log_warn() {
  echo -e "${YELLOW}[⚠]${NC} $1"
  ((WARNINGS++))
}

log_error() {
  echo -e "${RED}[✗]${NC} $1"
  ((ERRORS++))
}

check() {
  ((CHECKS++))
}

echo "=========================================="
echo "  HOB Infrastructure Validation"
echo "=========================================="
echo ""

# Check 1: Terraform installation
check
echo -n "Checking Terraform installation... "
if command -v terraform &> /dev/null; then
  TF_VERSION=$(terraform version -json | grep -o '"terraform_version":"[^"]*' | cut -d'"' -f4)
  log_info "Terraform ${TF_VERSION} installed"
else
  log_error "Terraform is not installed. Please install Terraform 1.9.0 or later."
fi

# Check 2: Docker installation
check
echo -n "Checking Docker installation... "
if command -v docker &> /dev/null; then
  DOCKER_VERSION=$(docker --version | grep -o '[0-9.]*' | head -1)
  log_info "Docker ${DOCKER_VERSION} installed"
else
  log_warn "Docker is not installed locally. Required on deployment target server."
fi

# Check 3: Terraform directory
check
echo -n "Checking Terraform directory... "
if [ -d "${TERRAFORM_DIR}" ]; then
  log_info "Terraform directory exists"
else
  log_error "Terraform directory not found: ${TERRAFORM_DIR}"
fi

# Check 4: Terraform files
check
echo -n "Checking Terraform configuration files... "
REQUIRED_TF_FILES=("main.tf" "variables.tf" "provider.tf" "outputs.tf")
TF_FILES_OK=true
for file in "${REQUIRED_TF_FILES[@]}"; do
  if [ ! -f "${TERRAFORM_DIR}/${file}" ]; then
    log_error "Missing Terraform file: ${file}"
    TF_FILES_OK=false
  fi
done
if [ "$TF_FILES_OK" = true ]; then
  log_info "All Terraform configuration files present"
fi

# Check 5: Docker Compose files
check
echo -n "Checking Docker Compose files... "
REQUIRED_COMPOSE_FILES=("docker-compose.infrastructure.yml" "docker-compose.services.yml")
COMPOSE_FILES_OK=true
for file in "${REQUIRED_COMPOSE_FILES[@]}"; do
  if [ ! -f "${SCRIPT_DIR}/${file}" ]; then
    log_error "Missing Docker Compose file: ${file}"
    COMPOSE_FILES_OK=false
  fi
done
if [ "$COMPOSE_FILES_OK" = true ]; then
  log_info "All Docker Compose files present"
fi

# Check 6: Environment file
check
echo -n "Checking environment configuration... "
if [ -f "${ENV_FILE}" ]; then
  log_info "Environment file found: ${ENV_FILE}"
  source "${ENV_FILE}"
else
  log_warn "Environment file not found: ${ENV_FILE}"
  log_warn "Create one from .env.example before deployment"
fi

# Check 7: Terraform variables file
check
echo -n "Checking Terraform variables... "
if [ -f "${TERRAFORM_DIR}/terraform.tfvars" ]; then
  log_info "terraform.tfvars found"
elif [ -f "${TERRAFORM_DIR}/terraform.tfvars.example" ]; then
  log_warn "terraform.tfvars not found. Copy from terraform.tfvars.example"
else
  log_error "terraform.tfvars.example not found"
fi

# Check 8: Required environment variables (if .env loaded)
if [ -f "${ENV_FILE}" ]; then
  check
  echo "Checking required environment variables..."

  REQUIRED_VARS=(
    "PORTAINER_URL:Portainer server URL"
    "PORTAINER_USERNAME:Portainer admin username"
    "PORTAINER_PASSWORD:Portainer admin password"
    "DB_PASSWORD:Database password"
    "RABBITMQ_USER:RabbitMQ username"
    "RABBITMQ_PASSWORD:RabbitMQ password"
    "GRAFANA_ADMIN_PASSWORD:Grafana admin password"
    "HOB_API_DOMAIN:API domain"
    "HOB_DASHBOARD_DOMAIN:Dashboard domain"
  )

  for var_desc in "${REQUIRED_VARS[@]}"; do
    VAR_NAME="${var_desc%%:*}"
    VAR_DESC="${var_desc#*:}"

    if [ -z "${!VAR_NAME:-}" ]; then
      log_error "Missing: ${VAR_NAME} (${VAR_DESC})"
    fi
  done
fi

# Check 9: Password strength
if [ -f "${ENV_FILE}" ]; then
  check
  echo "Checking password strength..."

  # Check DB password
  if [ -n "${DB_PASSWORD:-}" ]; then
    if [[ ${#DB_PASSWORD} -lt 8 ]]; then
      log_warn "DB_PASSWORD is too short (minimum 8 characters)"
    elif [[ ! "$DB_PASSWORD" =~ [A-Z] ]] || [[ ! "$DB_PASSWORD" =~ [a-z] ]] || [[ ! "$DB_PASSWORD" =~ [0-9] ]] || [[ ! "$DB_PASSWORD" =~ [@\$!%*?&] ]]; then
      log_warn "DB_PASSWORD should contain uppercase, lowercase, number, and special character"
    else
      log_info "DB_PASSWORD meets strength requirements"
    fi
  fi

  # Check RabbitMQ password
  if [ -n "${RABBITMQ_PASSWORD:-}" ]; then
    if [[ ${#RABBITMQ_PASSWORD} -lt 8 ]]; then
      log_warn "RABBITMQ_PASSWORD is too short (minimum 8 characters)"
    else
      log_info "RABBITMQ_PASSWORD meets minimum length"
    fi
  fi

  # Check Grafana password
  if [ -n "${GRAFANA_ADMIN_PASSWORD:-}" ]; then
    if [[ ${#GRAFANA_ADMIN_PASSWORD} -lt 8 ]]; then
      log_warn "GRAFANA_ADMIN_PASSWORD is too short (minimum 8 characters)"
    else
      log_info "GRAFANA_ADMIN_PASSWORD meets minimum length"
    fi
  fi
fi

# Check 10: Volume paths
if [ -f "${ENV_FILE}" ]; then
  check
  echo "Checking volume paths..."

  if [ -n "${DATA_VOLUME_PATH:-}" ]; then
    if [ -d "${DATA_VOLUME_PATH}" ]; then
      if [ -w "${DATA_VOLUME_PATH}" ]; then
        log_info "DATA_VOLUME_PATH exists and is writable: ${DATA_VOLUME_PATH}"
      else
        log_warn "DATA_VOLUME_PATH exists but is not writable: ${DATA_VOLUME_PATH}"
      fi
    else
      log_warn "DATA_VOLUME_PATH does not exist (will be created): ${DATA_VOLUME_PATH}"
    fi
  else
    log_warn "DATA_VOLUME_PATH not set (default: /opt/hob/data)"
  fi

  if [ -n "${REPORTS_VOLUME_PATH:-}" ]; then
    if [ -d "${REPORTS_VOLUME_PATH}" ]; then
      if [ -w "${REPORTS_VOLUME_PATH}" ]; then
        log_info "REPORTS_VOLUME_PATH exists and is writable: ${REPORTS_VOLUME_PATH}"
      else
        log_warn "REPORTS_VOLUME_PATH exists but is not writable: ${REPORTS_VOLUME_PATH}"
      fi
    else
      log_warn "REPORTS_VOLUME_PATH does not exist (will be created): ${REPORTS_VOLUME_PATH}"
    fi
  else
    log_warn "REPORTS_VOLUME_PATH not set (default: /opt/hob/reports)"
  fi
fi

# Check 11: Portainer connectivity (if credentials available)
if [ -n "${PORTAINER_URL:-}" ] && [ -n "${PORTAINER_USERNAME:-}" ] && [ -n "${PORTAINER_PASSWORD:-}" ]; then
  check
  echo -n "Checking Portainer connectivity... "

  if command -v curl &> /dev/null; then
    HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" "${PORTAINER_URL}/api/status" || echo "000")
    if [ "$HTTP_CODE" = "200" ]; then
      log_info "Portainer is accessible at ${PORTAINER_URL}"
    elif [ "$HTTP_CODE" = "000" ]; then
      log_error "Cannot reach Portainer at ${PORTAINER_URL} (connection failed)"
    else
      log_warn "Portainer returned HTTP ${HTTP_CODE} at ${PORTAINER_URL}"
    fi
  else
    log_warn "curl not available, skipping Portainer connectivity check"
  fi
fi

# Check 12: Terraform init status
check
echo -n "Checking Terraform initialization... "
if [ -d "${TERRAFORM_DIR}/.terraform" ]; then
  log_info "Terraform is initialized"
else
  log_warn "Terraform is not initialized. Run 'terraform init' first."
fi

# Check 13: Git repository status
check
echo -n "Checking Git repository... "
if git rev-parse --git-dir > /dev/null 2>&1; then
  CURRENT_BRANCH=$(git rev-parse --abbrev-ref HEAD)
  log_info "Git repository detected (branch: ${CURRENT_BRANCH})"
else
  log_warn "Not a Git repository"
fi

# Summary
echo ""
echo "=========================================="
echo "  Validation Summary"
echo "=========================================="
echo "Total checks: ${CHECKS}"
echo -e "${RED}Errors: ${ERRORS}${NC}"
echo -e "${YELLOW}Warnings: ${WARNINGS}${NC}"
echo ""

if [ ${ERRORS} -gt 0 ]; then
  echo -e "${RED}❌ Validation FAILED${NC}"
  echo "Please fix the errors above before deploying."
  exit 1
elif [ ${WARNINGS} -gt 0 ]; then
  echo -e "${YELLOW}⚠️  Validation passed with warnings${NC}"
  echo "Review the warnings above. You may proceed with caution."
  exit 0
else
  echo -e "${GREEN}✅ Validation PASSED${NC}"
  echo "All checks passed! Ready for deployment."
  echo ""
  echo "Next steps:"
  echo "  1. cd ${TERRAFORM_DIR}"
  echo "  2. terraform plan"
  echo "  3. terraform apply"
  exit 0
fi
