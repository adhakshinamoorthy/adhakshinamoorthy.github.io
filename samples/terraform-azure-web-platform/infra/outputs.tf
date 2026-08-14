output "resource_group_name" {
  description = "Resource group containing the web platform."
  value       = azurerm_resource_group.main.name
}

output "web_app_hostname" {
  description = "Public hostname of the deployed web application."
  value       = azurerm_linux_web_app.main.default_hostname
}

output "web_app_principal_id" {
  description = "Managed identity principal used for downstream RBAC."
  value       = azurerm_linux_web_app.main.identity[0].principal_id
}
