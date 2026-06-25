output "acr_login_server" {
  description = "ACR hostname to tag/push the API image to."
  value       = azurerm_container_registry.main.login_server
}

output "acr_name" {
  description = "ACR name (for: az acr login --name <acr_name>)."
  value       = azurerm_container_registry.main.name
}

output "resource_group" {
  description = "Resource group name."
  value       = azurerm_resource_group.main.name
}

output "app_url" {
  description = "Public (IP-restricted) URL of the API."
  value       = "https://${azurerm_container_app.api.ingress[0].fqdn}"
}

output "postgres_fqdn" {
  description = "PostgreSQL server hostname."
  value       = azurerm_postgresql_flexible_server.main.fqdn
}
