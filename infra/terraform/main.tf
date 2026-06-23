locals {
  prefix = "${var.name_prefix}-${var.environment}"

  # Npgsql connection string. SSL is required by Azure Postgres; staging trusts
  # the server cert rather than shipping the root CA (tighten for production).
  postgres_connection_string = join("", [
    "Host=${azurerm_postgresql_flexible_server.main.fqdn};",
    "Port=5432;",
    "Database=${azurerm_postgresql_flexible_server_database.main.name};",
    "Username=${var.postgres_admin_login};",
    "Password=${var.postgres_admin_password};",
    "SSL Mode=Require;Trust Server Certificate=true",
  ])
}

resource "azurerm_resource_group" "main" {
  name     = "${local.prefix}-rg"
  location = var.location
}

# ---- Container registry (hosts the API image) ----
resource "azurerm_container_registry" "main" {
  name                = "${var.name_prefix}${var.environment}acr" # globally unique, alphanumeric
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  sku                 = "Basic"
  admin_enabled       = true # simple auth for staging; prefer managed identity for prod
}

# ---- Observability (required by the Container Apps environment) ----
resource "azurerm_log_analytics_workspace" "main" {
  name                = "${local.prefix}-law"
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  sku                 = "PerGB2018"
  retention_in_days   = 30
}

resource "azurerm_container_app_environment" "main" {
  name                       = "${local.prefix}-cae"
  resource_group_name        = azurerm_resource_group.main.name
  location                   = azurerm_resource_group.main.location
  log_analytics_workspace_id = azurerm_log_analytics_workspace.main.id
}

# ---- PostgreSQL Flexible Server (+ pgvector) ----
resource "azurerm_postgresql_flexible_server" "main" {
  name                          = "${local.prefix}-pg"
  resource_group_name           = azurerm_resource_group.main.name
  location                      = azurerm_resource_group.main.location
  version                       = "16"
  administrator_login           = var.postgres_admin_login
  administrator_password        = var.postgres_admin_password
  storage_mb                    = 32768
  sku_name                      = "B_Standard_B1ms"
  zone                          = "1"
  public_network_access_enabled = true # locked down by firewall rules below
}

# Allowlist the pgvector extension so the app's migration can CREATE EXTENSION vector.
resource "azurerm_postgresql_flexible_server_configuration" "vector" {
  name      = "azure.extensions"
  server_id = azurerm_postgresql_flexible_server.main.id
  value     = "VECTOR"
}

resource "azurerm_postgresql_flexible_server_database" "main" {
  name      = "portfoliorag"
  server_id = azurerm_postgresql_flexible_server.main.id
  collation = "en_US.utf8"
  charset   = "utf8"
}

# Lets Azure-internal services (the Container App) reach Postgres.
resource "azurerm_postgresql_flexible_server_firewall_rule" "azure_services" {
  name             = "AllowAzureServices"
  server_id        = azurerm_postgresql_flexible_server.main.id
  start_ip_address = "0.0.0.0"
  end_ip_address   = "0.0.0.0"
}

# Lets you connect from your own machine (psql / inspection).
resource "azurerm_postgresql_flexible_server_firewall_rule" "client" {
  name             = "AllowClientIp"
  server_id        = azurerm_postgresql_flexible_server.main.id
  start_ip_address = var.client_ip
  end_ip_address   = var.client_ip
}

# ---- Container App (the API) ----
resource "azurerm_container_app" "api" {
  name                         = "${local.prefix}-api"
  resource_group_name          = azurerm_resource_group.main.name
  container_app_environment_id = azurerm_container_app_environment.main.id
  revision_mode                = "Single"

  registry {
    server               = azurerm_container_registry.main.login_server
    username             = azurerm_container_registry.main.admin_username
    password_secret_name = "acr-password"
  }

  secret {
    name  = "acr-password"
    value = azurerm_container_registry.main.admin_password
  }

  secret {
    name  = "openai-api-key"
    value = var.openai_api_key
  }

  secret {
    name  = "ingest-api-key"
    value = var.ingest_api_key
  }

  secret {
    name  = "postgres-connection-string"
    value = local.postgres_connection_string
  }

  template {
    min_replicas = 0 # scale to zero when idle (cheaper; cold start on first request)
    max_replicas = 1

    container {
      name   = "api"
      image  = var.container_image
      cpu    = 0.5
      memory = "1Gi"

      env {
        name  = "ASPNETCORE_ENVIRONMENT"
        value = "Staging"
      }

      env {
        name  = "ASPNETCORE_HTTP_PORTS"
        value = "8080"
      }

      # Migrates on each cold-start boot; idempotent, safe with scale-to-zero.
      env {
        name  = "Database__MigrateOnStartup"
        value = "true"
      }

      env {
        name        = "ConnectionStrings__Postgres"
        secret_name = "postgres-connection-string"
      }

      env {
        name        = "OpenAI__ApiKey"
        secret_name = "openai-api-key"
      }

      env {
        name        = "Ingestion__ApiKey"
        secret_name = "ingest-api-key"
      }
    }
  }

  ingress {
    external_enabled = true
    target_port      = 8080
    transport        = "auto"

    traffic_weight {
      percentage      = 100
      latest_revision = true
    }

    # An Allow rule makes everything else implicitly denied -> private to you.
    ip_security_restriction {
      name             = "allow-client-ip"
      action           = "Allow"
      ip_address_range = "${var.client_ip}/32"
    }
  }

  # The pgvector allowlist and Azure-services firewall rule must exist before
  # the app boots, since startup migration connects and runs CREATE EXTENSION
  # vector. Without this they can be created in parallel and the first boot
  # crash-loops until they land.
  depends_on = [
    azurerm_postgresql_flexible_server_configuration.vector,
    azurerm_postgresql_flexible_server_firewall_rule.azure_services,
  ]
}
