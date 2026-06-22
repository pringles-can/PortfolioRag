provider "azurerm" {
  # azurerm v4 requires an explicit subscription id. Set it here via the
  # variable, or export ARM_SUBSCRIPTION_ID before running terraform.
  subscription_id = var.subscription_id

  features {}
}
