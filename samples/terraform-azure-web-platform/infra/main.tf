locals {
  prefix = "${var.workload_name}-${var.environment}"
  required_tags = {
    environment = var.environment
    managed-by  = "terraform"
    workload    = var.workload_name
  }
  tags = merge(local.required_tags, var.tags)
}

resource "azurerm_resource_group" "main" {
  name     = "rg-${local.prefix}"
  location = var.location
  tags     = local.tags
}

resource "azurerm_log_analytics_workspace" "main" {
  name                = "log-${local.prefix}"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  sku                 = "PerGB2018"
  retention_in_days   = 30
  tags                = local.tags
}

resource "azurerm_application_insights" "main" {
  name                = "appi-${local.prefix}"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  workspace_id        = azurerm_log_analytics_workspace.main.id
  application_type    = "web"
  tags                = local.tags
}

resource "azurerm_service_plan" "main" {
  name                = "plan-${local.prefix}"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  os_type             = "Linux"
  sku_name            = var.environment == "prod" ? "P1v3" : "B1"
  tags                = local.tags
}

resource "azurerm_linux_web_app" "main" {
  name                = "app-${local.prefix}"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  service_plan_id     = azurerm_service_plan.main.id
  https_only          = true
  tags                = local.tags

  identity { type = "SystemAssigned" }

  site_config {
    always_on                         = var.environment == "prod"
    minimum_tls_version               = "1.2"
    health_check_path                 = "/health/ready"
    health_check_eviction_time_in_min = 5

    application_stack {
      dotnet_version = "10.0"
    }
  }

  app_settings = {
    APPLICATIONINSIGHTS_CONNECTION_STRING = azurerm_application_insights.main.connection_string
    ASPNETCORE_ENVIRONMENT                = title(var.environment)
  }

  lifecycle {
    precondition {
      condition     = var.environment != "prod" || azurerm_service_plan.main.sku_name == "P1v3"
      error_message = "Production must use the P1v3 service plan."
    }
  }
}
