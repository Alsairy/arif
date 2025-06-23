output "resource_group_name" {
  description = "Name of the created resource group"
  value       = azurerm_resource_group.arif_platform.name
}

output "aks_cluster_name" {
  description = "Name of the AKS cluster"
  value       = azurerm_kubernetes_cluster.arif_platform.name
}

output "aks_cluster_fqdn" {
  description = "FQDN of the AKS cluster"
  value       = azurerm_kubernetes_cluster.arif_platform.fqdn
}

output "aks_cluster_kube_config" {
  description = "Kubernetes configuration for the AKS cluster"
  value       = azurerm_kubernetes_cluster.arif_platform.kube_config_raw
  sensitive   = true
}

output "container_registry_login_server" {
  description = "Login server for the Azure Container Registry"
  value       = azurerm_container_registry.arif_platform.login_server
}

output "container_registry_admin_username" {
  description = "Admin username for the Azure Container Registry"
  value       = azurerm_container_registry.arif_platform.admin_username
  sensitive   = true
}

output "container_registry_admin_password" {
  description = "Admin password for the Azure Container Registry"
  value       = azurerm_container_registry.arif_platform.admin_password
  sensitive   = true
}

output "sql_server_fqdn" {
  description = "Fully qualified domain name of the Azure SQL Server"
  value       = azurerm_mssql_server.arif_platform.fully_qualified_domain_name
}

output "sql_database_name" {
  description = "Name of the Azure SQL Database"
  value       = azurerm_mssql_database.arif_platform.name
}

output "redis_hostname" {
  description = "Hostname of the Azure Cache for Redis"
  value       = azurerm_redis_cache.arif_platform.hostname
}

output "redis_port" {
  description = "Port of the Azure Cache for Redis"
  value       = azurerm_redis_cache.arif_platform.port
}

output "redis_primary_access_key" {
  description = "Primary access key for the Azure Cache for Redis"
  value       = azurerm_redis_cache.arif_platform.primary_access_key
  sensitive   = true
}

output "key_vault_uri" {
  description = "URI of the Azure Key Vault"
  value       = azurerm_key_vault.arif_platform.vault_uri
}

output "application_insights_instrumentation_key" {
  description = "Instrumentation key for Application Insights"
  value       = azurerm_application_insights.arif_platform.instrumentation_key
  sensitive   = true
}

output "application_insights_connection_string" {
  description = "Connection string for Application Insights"
  value       = azurerm_application_insights.arif_platform.connection_string
  sensitive   = true
}

output "log_analytics_workspace_id" {
  description = "ID of the Log Analytics Workspace"
  value       = azurerm_log_analytics_workspace.arif_platform.id
}

output "deployment_instructions" {
  description = "Instructions for deploying the Arif Platform"
  value = <<-EOT
    Deployment Instructions:
    
    1. Configure kubectl to use the AKS cluster:
       az aks get-credentials --resource-group ${azurerm_resource_group.arif_platform.name} --name ${azurerm_kubernetes_cluster.arif_platform.name}
    
    2. Configure Docker to use the Azure Container Registry:
       az acr login --name ${azurerm_container_registry.arif_platform.name}
    
    3. Build and push Docker images:
       cd ../
       ./scripts/deploy.sh production
    
    4. Access the application:
       kubectl get services -n arif-platform
    
    5. Monitor the application:
       - Azure Portal: https://portal.azure.com
       - Log Analytics: ${azurerm_log_analytics_workspace.arif_platform.portal_url}
       - Application Insights: https://portal.azure.com/#@/resource${azurerm_application_insights.arif_platform.id}
  EOT
}
