variable "subscription_id" {
  type        = string
  description = "Azure subscription id to deploy into."
}

variable "location" {
  type        = string
  description = "Azure region."
  default     = "eastus2"
}

variable "name_prefix" {
  type        = string
  description = "Prefix for resource names. Lowercase alphanumeric (used in the globally-unique ACR name)."
  default     = "portfoliorag"
}

variable "environment" {
  type        = string
  description = "Environment short name, e.g. staging."
  default     = "staging"
}

variable "client_ip" {
  type        = string
  description = "Your public IPv4. The app ingress is locked to this IP, and Postgres allows it for admin access. Find it with: curl ifconfig.me"
}

variable "container_image" {
  type        = string
  description = "Image the Container App runs. Defaults to a public placeholder so the first apply succeeds; re-apply with your ACR image after pushing it."
  default     = "mcr.microsoft.com/azuredocs/containerapps-helloworld:latest"
}

variable "postgres_admin_login" {
  type        = string
  description = "PostgreSQL administrator login."
  default     = "pgadmin"
}

variable "postgres_admin_password" {
  type        = string
  description = "PostgreSQL administrator password."
  sensitive   = true
}

variable "openai_api_key" {
  type        = string
  description = "OpenAI API key, injected as the OpenAI__ApiKey secret."
  sensitive   = true
}

variable "ingest_api_key" {
  type        = string
  description = "Shared secret required by POST /ingest (X-Ingest-Key header)."
  sensitive   = true
}
