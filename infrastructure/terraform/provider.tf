terraform {
  required_version = ">= 1.0"

  required_providers {
    portainer = {
      source  = "portainer/portainer"
      version = "~> 2.0"
    }
  }

  # Optional: Configure remote state backend
  # backend "s3" {
  #   bucket = "your-terraform-state-bucket"
  #   key    = "hob/portainer/terraform.tfstate"
  #   region = "us-east-1"
  # }
}

provider "portainer" {
  url      = var.portainer_url
  username = var.portainer_username
  password = var.portainer_password
}
