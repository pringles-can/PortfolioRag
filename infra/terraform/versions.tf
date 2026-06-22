terraform {
  required_version = ">= 1.5"

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 4.0"
    }
  }

  # Local state for staging. For anything shared/long-lived, switch to an
  # azurerm backend (Storage Account) so state isn't only on one machine.
}
