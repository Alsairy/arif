# Arif Platform - Deployment Guide

## Overview

This guide provides comprehensive instructions for deploying the Arif Platform in various environments, from development to production. The platform supports multiple deployment strategies including Docker, Kubernetes, and cloud-native deployments.

## Prerequisites

### System Requirements
- **Operating System**: Linux (Ubuntu 20.04+ recommended), Windows Server 2019+, or macOS 10.15+
- **CPU**: Minimum 4 cores, Recommended 8+ cores
- **Memory**: Minimum 16GB RAM, Recommended 32GB+ RAM
- **Storage**: Minimum 100GB SSD, Recommended 500GB+ SSD
- **Network**: High-speed internet connection for cloud deployments

### Software Dependencies
- **.NET 8 SDK**: Latest version
- **Docker**: Version 20.10+
- **Docker Compose**: Version 2.0+
- **Kubernetes**: Version 1.24+ (for K8s deployments)
- **Helm**: Version 3.8+ (for Helm deployments)
- **Terraform**: Version 1.0+ (for infrastructure provisioning)

### Cloud Provider Requirements
- **Azure**: Active subscription with appropriate permissions
- **AWS**: Active account with IAM permissions
- **GCP**: Active project with necessary APIs enabled

## Development Environment Setup

### Local Development with Docker Compose

1. **Clone the Repository**
```bash
git clone https://github.com/your-org/arif-platform.git
cd arif-platform
```

2. **Environment Configuration**
```bash
# Copy environment templates
cp backend/src/services/Authentication/Arif.Platform.Authentication.Api/appsettings.Development.json.template \
   backend/src/services/Authentication/Arif.Platform.Authentication.Api/appsettings.Development.json

# Update configuration files with your settings
```

3. **Start Services**
```bash
# Start all services with Docker Compose
docker-compose up -d

# Verify services are running
docker-compose ps
```

4. **Initialize Database**
```bash
# Run database migrations
docker-compose exec auth-service dotnet ef database update
docker-compose exec ai-orchestration-service dotnet ef database update
```

5. **Access Services**
- API Gateway: http://localhost:5000
- Authentication Service: http://localhost:5001
- AI Orchestration Service: http://localhost:5002
- Admin Dashboard: http://localhost:3000
- Tenant Dashboard: http://localhost:3001

### Local Development without Docker

1. **Install Dependencies**
```bash
# Install .NET 8 SDK
wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt-get update
sudo apt-get install -y dotnet-sdk-8.0

# Install SQL Server (or use SQL Server in Docker)
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=YourStrong@Passw0rd" \
   -p 1433:1433 --name sqlserver --hostname sqlserver \
   -d mcr.microsoft.com/mssql/server:2022-latest

# Install Redis
sudo apt-get install redis-server
```

2. **Build and Run Services**
```bash
# Build the solution
cd backend
dotnet build Arif.Platform.sln

# Run services individually
cd src/gateways/Arif.Platform.ApiGateway
dotnet run --urls="http://localhost:5000"

cd ../services/Authentication/Arif.Platform.Authentication.Api
dotnet run --urls="http://localhost:5001"
```

## Production Deployment

### Kubernetes Deployment

#### Prerequisites
- Kubernetes cluster (AKS, EKS, GKE, or on-premises)
- kubectl configured to access your cluster
- Helm 3.8+ installed

#### Deployment Steps

1. **Create Namespace**
```bash
kubectl apply -f k8s/namespace.yaml
```

2. **Configure Secrets**
```bash
# Create secrets for database connections, JWT keys, etc.
kubectl create secret generic arif-platform-secrets \
  --from-literal=ConnectionStrings__DefaultConnection="Server=sql-server;Database=ArifPlatform;User Id=sa;Password=YourPassword;" \
  --from-literal=JwtSettings__SecretKey="YourJWTSecretKey" \
  --namespace=arif-platform
```

3. **Deploy Infrastructure Services**
```bash
# Deploy SQL Server
kubectl apply -f k8s/sqlserver-deployment.yaml

# Deploy Redis
kubectl apply -f k8s/redis-deployment.yaml

# Wait for infrastructure to be ready
kubectl wait --for=condition=available --timeout=300s deployment/sqlserver -n arif-platform
kubectl wait --for=condition=available --timeout=300s deployment/redis -n arif-platform
```

4. **Deploy Application Services**
```bash
# Deploy API Gateway
kubectl apply -f k8s/api-gateway-deployment.yaml

# Deploy Authentication Service
kubectl apply -f k8s/auth-service-deployment.yaml

# Deploy all microservices
kubectl apply -f k8s/microservices-deployment.yaml

# Verify deployments
kubectl get pods -n arif-platform
```

5. **Configure Ingress**
```bash
# Install NGINX Ingress Controller (if not already installed)
helm upgrade --install ingress-nginx ingress-nginx \
  --repo https://kubernetes.github.io/ingress-nginx \
  --namespace ingress-nginx --create-namespace

# Apply ingress configuration
kubectl apply -f k8s/ingress.yaml
```

#### Helm Deployment

1. **Install with Helm**
```bash
# Add Helm repository (if using external chart)
helm repo add arif-platform https://charts.arif-platform.com
helm repo update

# Install using local chart
helm install arif-platform ./helm/arif-platform \
  --namespace arif-platform \
  --create-namespace \
  --values helm/arif-platform/values.yaml
```

2. **Customize Values**
```yaml
# helm/arif-platform/values.yaml
global:
  imageRegistry: "your-registry.com"
  imagePullSecrets:
    - name: registry-secret

database:
  enabled: true
  host: "sql-server"
  port: 1433
  database: "ArifPlatform"

redis:
  enabled: true
  host: "redis"
  port: 6379

ingress:
  enabled: true
  className: "nginx"
  hosts:
    - host: api.arif-platform.com
      paths:
        - path: /
          pathType: Prefix
```

### Cloud Deployments

#### Azure Deployment

1. **Infrastructure Provisioning with Terraform**
```bash
cd terraform
terraform init
terraform plan -var-file="azure.tfvars"
terraform apply -var-file="azure.tfvars"
```

2. **Azure Container Apps Deployment**
```bash
# Create resource group
az group create --name arif-platform-rg --location eastus

# Create container app environment
az containerapp env create \
  --name arif-platform-env \
  --resource-group arif-platform-rg \
  --location eastus

# Deploy services
az containerapp create \
  --name api-gateway \
  --resource-group arif-platform-rg \
  --environment arif-platform-env \
  --image your-registry.azurecr.io/arif-platform/api-gateway:latest \
  --target-port 5000 \
  --ingress external
```

3. **Azure Kubernetes Service (AKS)**
```bash
# Create AKS cluster
az aks create \
  --resource-group arif-platform-rg \
  --name arif-platform-aks \
  --node-count 3 \
  --node-vm-size Standard_D4s_v3 \
  --enable-addons monitoring \
  --generate-ssh-keys

# Get credentials
az aks get-credentials --resource-group arif-platform-rg --name arif-platform-aks

# Deploy using kubectl or Helm
kubectl apply -f k8s/
```

#### AWS Deployment

1. **Infrastructure Provisioning**
```bash
cd terraform
terraform init
terraform plan -var-file="aws.tfvars"
terraform apply -var-file="aws.tfvars"
```

2. **Amazon EKS Deployment**
```bash
# Create EKS cluster
eksctl create cluster \
  --name arif-platform \
  --region us-west-2 \
  --nodegroup-name standard-workers \
  --node-type m5.large \
  --nodes 3 \
  --nodes-min 1 \
  --nodes-max 4

# Deploy application
kubectl apply -f k8s/
```

3. **AWS App Runner Deployment**
```bash
# Create App Runner service for each microservice
aws apprunner create-service \
  --service-name arif-platform-api-gateway \
  --source-configuration '{
    "ImageRepository": {
      "ImageIdentifier": "your-account.dkr.ecr.us-west-2.amazonaws.com/arif-platform/api-gateway:latest",
      "ImageConfiguration": {
        "Port": "5000"
      },
      "ImageRepositoryType": "ECR"
    },
    "AutoDeploymentsEnabled": true
  }'
```

#### Google Cloud Platform (GCP) Deployment

1. **Google Kubernetes Engine (GKE)**
```bash
# Create GKE cluster
gcloud container clusters create arif-platform \
  --zone us-central1-a \
  --num-nodes 3 \
  --machine-type n1-standard-4 \
  --enable-autoscaling \
  --min-nodes 1 \
  --max-nodes 10

# Get credentials
gcloud container clusters get-credentials arif-platform --zone us-central1-a

# Deploy application
kubectl apply -f k8s/
```

2. **Cloud Run Deployment**
```bash
# Deploy each service to Cloud Run
gcloud run deploy api-gateway \
  --image gcr.io/your-project/arif-platform/api-gateway:latest \
  --platform managed \
  --region us-central1 \
  --allow-unauthenticated \
  --port 5000
```

## Configuration Management

### Environment Variables

#### Required Environment Variables
```bash
# Database Configuration
ConnectionStrings__DefaultConnection="Server=localhost;Database=ArifPlatform;Trusted_Connection=true;"

# JWT Configuration
JwtSettings__SecretKey="YourSecretKeyHere"
JwtSettings__Issuer="https://arif-platform.com"
JwtSettings__Audience="arif-platform-users"
JwtSettings__ExpirationMinutes="60"

# Redis Configuration
Redis__ConnectionString="localhost:6379"

# Azure Storage (if using Azure)
AzureStorage__ConnectionString="DefaultEndpointsProtocol=https;AccountName=..."

# OpenAI Configuration
OpenAI__ApiKey="your-openai-api-key"
OpenAI__OrganizationId="your-org-id"

# Monitoring Configuration
ApplicationInsights__InstrumentationKey="your-app-insights-key"
```

#### Service-Specific Configuration

**Authentication Service**
```json
{
  "Authentication": {
    "PasswordPolicy": {
      "MinLength": 8,
      "RequireUppercase": true,
      "RequireLowercase": true,
      "RequireDigit": true,
      "RequireSpecialCharacter": true
    },
    "AccountLockout": {
      "MaxFailedAttempts": 5,
      "LockoutDuration": "00:15:00"
    }
  }
}
```

**AI Orchestration Service**
```json
{
  "AIOrchestration": {
    "DefaultModel": "gpt-4",
    "MaxTokens": 4000,
    "Temperature": 0.7,
    "VectorDatabase": {
      "Provider": "Pinecone",
      "ApiKey": "your-pinecone-api-key",
      "Environment": "us-west1-gcp"
    }
  }
}
```

### SSL/TLS Configuration

#### Certificate Management
```bash
# Using Let's Encrypt with cert-manager (Kubernetes)
kubectl apply -f https://github.com/cert-manager/cert-manager/releases/download/v1.12.0/cert-manager.yaml

# Create ClusterIssuer
kubectl apply -f - <<EOF
apiVersion: cert-manager.io/v1
kind: ClusterIssuer
metadata:
  name: letsencrypt-prod
spec:
  acme:
    server: https://acme-v02.api.letsencrypt.org/directory
    email: admin@arif-platform.com
    privateKeySecretRef:
      name: letsencrypt-prod
    solvers:
    - http01:
        ingress:
          class: nginx
EOF
```

## Monitoring and Observability

### Application Performance Monitoring

#### Azure Application Insights
```json
{
  "ApplicationInsights": {
    "InstrumentationKey": "your-instrumentation-key",
    "EnableAdaptiveSampling": true,
    "EnableQuickPulseMetricStream": true
  }
}
```

#### Prometheus and Grafana
```bash
# Install Prometheus using Helm
helm repo add prometheus-community https://prometheus-community.github.io/helm-charts
helm install prometheus prometheus-community/kube-prometheus-stack \
  --namespace monitoring \
  --create-namespace

# Access Grafana
kubectl port-forward svc/prometheus-grafana 3000:80 -n monitoring
```

### Logging Configuration

#### Structured Logging with Serilog
```json
{
  "Serilog": {
    "Using": ["Serilog.Sinks.Console", "Serilog.Sinks.File"],
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/arif-platform-.txt",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 30
        }
      }
    ]
  }
}
```

## Security Configuration

### Network Security

#### Firewall Rules
```bash
# Allow HTTP/HTTPS traffic
sudo ufw allow 80/tcp
sudo ufw allow 443/tcp

# Allow specific service ports (adjust as needed)
sudo ufw allow 5000:5010/tcp

# Enable firewall
sudo ufw enable
```

#### Network Policies (Kubernetes)
```yaml
apiVersion: networking.k8s.io/v1
kind: NetworkPolicy
metadata:
  name: arif-platform-network-policy
  namespace: arif-platform
spec:
  podSelector: {}
  policyTypes:
  - Ingress
  - Egress
  ingress:
  - from:
    - namespaceSelector:
        matchLabels:
          name: ingress-nginx
    ports:
    - protocol: TCP
      port: 5000
```

### Data Encryption

#### Database Encryption
```sql
-- Enable Transparent Data Encryption (SQL Server)
USE master;
CREATE MASTER KEY ENCRYPTION BY PASSWORD = 'YourStrongPassword';
CREATE CERTIFICATE ArifPlatformCert WITH SUBJECT = 'Arif Platform TDE Certificate';
USE ArifPlatform;
CREATE DATABASE ENCRYPTION KEY
WITH ALGORITHM = AES_256
ENCRYPTION BY SERVER CERTIFICATE ArifPlatformCert;
ALTER DATABASE ArifPlatform SET ENCRYPTION ON;
```

## Backup and Disaster Recovery

### Database Backup

#### Automated Backup Script
```bash
#!/bin/bash
# backup-database.sh

BACKUP_DIR="/backups/arif-platform"
DATE=$(date +%Y%m%d_%H%M%S)
DB_NAME="ArifPlatform"

# Create backup directory
mkdir -p $BACKUP_DIR

# Backup database
sqlcmd -S localhost -U sa -P "$SA_PASSWORD" -Q "BACKUP DATABASE [$DB_NAME] TO DISK = N'$BACKUP_DIR/${DB_NAME}_$DATE.bak'"

# Compress backup
gzip "$BACKUP_DIR/${DB_NAME}_$DATE.bak"

# Clean up old backups (keep last 30 days)
find $BACKUP_DIR -name "*.bak.gz" -mtime +30 -delete

echo "Backup completed: ${DB_NAME}_$DATE.bak.gz"
```

#### Kubernetes CronJob for Backups
```yaml
apiVersion: batch/v1
kind: CronJob
metadata:
  name: database-backup
  namespace: arif-platform
spec:
  schedule: "0 2 * * *"  # Daily at 2 AM
  jobTemplate:
    spec:
      template:
        spec:
          containers:
          - name: backup
            image: mcr.microsoft.com/mssql-tools
            command:
            - /bin/bash
            - -c
            - |
              sqlcmd -S sql-server -U sa -P "$SA_PASSWORD" \
                -Q "BACKUP DATABASE [ArifPlatform] TO DISK = N'/backups/ArifPlatform_$(date +%Y%m%d_%H%M%S).bak'"
            env:
            - name: SA_PASSWORD
              valueFrom:
                secretKeyRef:
                  name: arif-platform-secrets
                  key: SA_PASSWORD
            volumeMounts:
            - name: backup-storage
              mountPath: /backups
          volumes:
          - name: backup-storage
            persistentVolumeClaim:
              claimName: backup-pvc
          restartPolicy: OnFailure
```

## Performance Optimization

### Database Optimization

#### Index Optimization
```sql
-- Create indexes for frequently queried columns
CREATE NONCLUSTERED INDEX IX_Users_Email ON Users (Email);
CREATE NONCLUSTERED INDEX IX_ChatSessions_TenantId ON ChatSessions (TenantId);
CREATE NONCLUSTERED INDEX IX_AuditLogs_Timestamp ON AuditLogs (Timestamp DESC);

-- Update statistics
UPDATE STATISTICS Users;
UPDATE STATISTICS ChatSessions;
UPDATE STATISTICS AuditLogs;
```

#### Connection Pooling
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ArifPlatform;User Id=sa;Password=YourPassword;MultipleActiveResultSets=true;Pooling=true;Min Pool Size=5;Max Pool Size=100;Connection Timeout=30;"
  }
}
```

### Application Optimization

#### Caching Configuration
```json
{
  "Redis": {
    "ConnectionString": "localhost:6379",
    "DefaultExpiration": "01:00:00",
    "KeyPrefix": "arif-platform:"
  },
  "MemoryCache": {
    "SizeLimit": 1000,
    "CompactionPercentage": 0.25
  }
}
```

## Troubleshooting

### Common Issues

#### Service Discovery Issues
```bash
# Check service registration
kubectl get services -n arif-platform

# Check endpoints
kubectl get endpoints -n arif-platform

# Check DNS resolution
kubectl run -it --rm debug --image=busybox --restart=Never -- nslookup auth-service.arif-platform.svc.cluster.local
```

#### Database Connection Issues
```bash
# Test database connectivity
sqlcmd -S localhost -U sa -P "$SA_PASSWORD" -Q "SELECT @@VERSION"

# Check connection strings in configuration
kubectl get configmap arif-platform-config -o yaml
```

#### Performance Issues
```bash
# Check resource usage
kubectl top pods -n arif-platform
kubectl top nodes

# Check application logs
kubectl logs -f deployment/api-gateway -n arif-platform
```

### Health Checks

#### Kubernetes Health Checks
```yaml
livenessProbe:
  httpGet:
    path: /health
    port: 5000
  initialDelaySeconds: 30
  periodSeconds: 10
  timeoutSeconds: 5
  failureThreshold: 3

readinessProbe:
  httpGet:
    path: /health/ready
    port: 5000
  initialDelaySeconds: 5
  periodSeconds: 5
  timeoutSeconds: 3
  failureThreshold: 3
```

## Scaling and Load Balancing

### Horizontal Pod Autoscaling
```yaml
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: api-gateway-hpa
  namespace: arif-platform
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: api-gateway
  minReplicas: 2
  maxReplicas: 10
  metrics:
  - type: Resource
    resource:
      name: cpu
      target:
        type: Utilization
        averageUtilization: 70
  - type: Resource
    resource:
      name: memory
      target:
        type: Utilization
        averageUtilization: 80
```

### Load Balancer Configuration
```yaml
apiVersion: v1
kind: Service
metadata:
  name: api-gateway-lb
  namespace: arif-platform
  annotations:
    service.beta.kubernetes.io/azure-load-balancer-internal: "false"
spec:
  type: LoadBalancer
  ports:
  - port: 80
    targetPort: 5000
    protocol: TCP
  selector:
    app: api-gateway
```

---

*This deployment guide is regularly updated to reflect the latest deployment practices and platform changes.*
