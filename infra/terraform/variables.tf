variable "subscription_id" {
  type        = string
  description = "Azure subscription id to deploy into."
}

variable "location" {
  type        = string
  description = "Azure region for storage/data resources. Note: eastus/eastus2 are offer-restricted for PostgreSQL Flexible Server on this subscription, so these live in westus3."
  default     = "westus3"
}

variable "containerapp_location" {
  type        = string
  description = "Azure region for the Container Apps environment + app. Separate from var.location because westus3 (where Postgres must live) is AKS-capacity-constrained for managed environments. Note: westus3 AND eastus both hit ManagedEnvironmentCapacityHeavyUsageError in June 2026; capacity is transient/region-specific, so this may need to move."
  default     = "centralus"
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

variable "ask_rate_limit_per_minute" {
  type        = number
  description = "Max /ask requests per minute (fixed-window, global). Caps OpenAI spend on the public endpoint. Tune without a code change."
  default     = 10
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
