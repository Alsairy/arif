terraform {
  required_version = ">= 1.0"
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.0"
    }
    kubernetes = {
      source  = "hashicorp/kubernetes"
      version = "~> 2.0"
    }
    helm = {
      source  = "hashicorp/helm"
      version = "~> 2.0"
    }
  }
}

provider "azurerm" {
  features {}
}

resource "azurerm_resource_group" "arif_platform" {
  name     = var.resource_group_name
  location = var.location

  tags = {
    Environment = var.environment
    Project     = "ArifPlatform"
    ManagedBy   = "Terraform"
  }
}

resource "azurerm_kubernetes_cluster" "arif_platform" {
  name                = var.cluster_name
  location            = azurerm_resource_group.arif_platform.location
  resource_group_name = azurerm_resource_group.arif_platform.name
  dns_prefix          = "${var.cluster_name}-dns"

  default_node_pool {
    name                = "default"
    node_count          = var.node_count
    vm_size             = var.node_vm_size
    type                = "VirtualMachineScaleSets"
    availability_zones  = ["1", "2", "3"]
    enable_auto_scaling = true
    min_count          = var.min_node_count
    max_count          = var.max_node_count

    upgrade_settings {
      max_surge = "10%"
    }
  }

  identity {
    type = "SystemAssigned"
  }

  network_profile {
    network_plugin    = "azure"
    load_balancer_sku = "standard"
  }

  oms_agent {
    log_analytics_workspace_id = azurerm_log_analytics_workspace.arif_platform.id
  }

  tags = {
    Environment = var.environment
    Project     = "ArifPlatform"
    ManagedBy   = "Terraform"
  }
}

resource "azurerm_log_analytics_workspace" "arif_platform" {
  name                = "${var.cluster_name}-logs"
  location            = azurerm_resource_group.arif_platform.location
  resource_group_name = azurerm_resource_group.arif_platform.name
  sku                 = "PerGB2018"
  retention_in_days   = 30

  tags = {
    Environment = var.environment
    Project     = "ArifPlatform"
    ManagedBy   = "Terraform"
  }
}

resource "azurerm_container_registry" "arif_platform" {
  name                = var.acr_name
  resource_group_name = azurerm_resource_group.arif_platform.name
  location            = azurerm_resource_group.arif_platform.location
  sku                 = "Standard"
  admin_enabled       = true

  tags = {
    Environment = var.environment
    Project     = "ArifPlatform"
    ManagedBy   = "Terraform"
  }
}

resource "azurerm_role_assignment" "aks_acr_pull" {
  scope                = azurerm_container_registry.arif_platform.id
  role_definition_name = "AcrPull"
  principal_id         = azurerm_kubernetes_cluster.arif_platform.kubelet_identity[0].object_id
}

resource "azurerm_mssql_server" "arif_platform" {
  name                         = var.sql_server_name
  resource_group_name          = azurerm_resource_group.arif_platform.name
  location                     = azurerm_resource_group.arif_platform.location
  version                      = "12.0"
  administrator_login          = var.sql_admin_username
  administrator_login_password = var.sql_admin_password

  tags = {
    Environment = var.environment
    Project     = "ArifPlatform"
    ManagedBy   = "Terraform"
  }
}

resource "azurerm_mssql_database" "arif_platform" {
  name           = var.sql_database_name
  server_id      = azurerm_mssql_server.arif_platform.id
  collation      = "SQL_Latin1_General_CP1_CI_AS"
  license_type   = "LicenseIncluded"
  max_size_gb    = 20
  sku_name       = "S1"
  zone_redundant = false

  tags = {
    Environment = var.environment
    Project     = "ArifPlatform"
    ManagedBy   = "Terraform"
  }
}

resource "azurerm_redis_cache" "arif_platform" {
  name                = var.redis_name
  location            = azurerm_resource_group.arif_platform.location
  resource_group_name = azurerm_resource_group.arif_platform.name
  capacity            = 1
  family              = "C"
  sku_name            = "Standard"
  enable_non_ssl_port = false
  minimum_tls_version = "1.2"

  redis_configuration {
    maxmemory_reserved = 10
    maxmemory_delta    = 2
    maxmemory_policy   = "allkeys-lru"
  }

  tags = {
    Environment = var.environment
    Project     = "ArifPlatform"
    ManagedBy   = "Terraform"
  }
}

resource "azurerm_application_insights" "arif_platform" {
  name                = "${var.cluster_name}-insights"
  location            = azurerm_resource_group.arif_platform.location
  resource_group_name = azurerm_resource_group.arif_platform.name
  workspace_id        = azurerm_log_analytics_workspace.arif_platform.id
  application_type    = "web"

  tags = {
    Environment = var.environment
    Project     = "ArifPlatform"
    ManagedBy   = "Terraform"
  }
}

resource "azurerm_key_vault" "arif_platform" {
  name                = var.key_vault_name
  location            = azurerm_resource_group.arif_platform.location
  resource_group_name = azurerm_resource_group.arif_platform.name
  tenant_id           = data.azurerm_client_config.current.tenant_id
  sku_name            = "standard"

  access_policy {
    tenant_id = data.azurerm_client_config.current.tenant_id
    object_id = data.azurerm_client_config.current.object_id

    key_permissions = [
      "Get", "List", "Update", "Create", "Import", "Delete", "Recover", "Backup", "Restore"
    ]

    secret_permissions = [
      "Get", "List", "Set", "Delete", "Recover", "Backup", "Restore"
    ]

    certificate_permissions = [
      "Get", "List", "Update", "Create", "Import", "Delete", "Recover", "Backup", "Restore", "ManageContacts", "ManageIssuers", "GetIssuers", "ListIssuers", "SetIssuers", "DeleteIssuers"
    ]
  }

  tags = {
    Environment = var.environment
    Project     = "ArifPlatform"
    ManagedBy   = "Terraform"
  }
}

data "azurerm_client_config" "current" {}

provider "kubernetes" {
  host                   = azurerm_kubernetes_cluster.arif_platform.kube_config.0.host
  client_certificate     = base64decode(azurerm_kubernetes_cluster.arif_platform.kube_config.0.client_certificate)
  client_key             = base64decode(azurerm_kubernetes_cluster.arif_platform.kube_config.0.client_key)
  cluster_ca_certificate = base64decode(azurerm_kubernetes_cluster.arif_platform.kube_config.0.cluster_ca_certificate)
}

provider "helm" {
  kubernetes {
    host                   = azurerm_kubernetes_cluster.arif_platform.kube_config.0.host
    client_certificate     = base64decode(azurerm_kubernetes_cluster.arif_platform.kube_config.0.client_certificate)
    client_key             = base64decode(azurerm_kubernetes_cluster.arif_platform.kube_config.0.client_key)
    cluster_ca_certificate = base64decode(azurerm_kubernetes_cluster.arif_platform.kube_config.0.cluster_ca_certificate)
  }
}

resource "helm_release" "arif_platform" {
  name       = "arif-platform"
  chart      = "../helm/arif-platform"
  namespace  = "arif-platform"
  create_namespace = true

  values = [
    file("../helm/arif-platform/values.yaml")
  ]

  set {
    name  = "global.environment"
    value = var.environment
  }

  set_sensitive {
    name  = "security.jwt.secretKey"
    value = var.jwt_secret_key
  }

  depends_on = [
    azurerm_kubernetes_cluster.arif_platform,
    azurerm_role_assignment.aks_acr_pull
  ]
}
