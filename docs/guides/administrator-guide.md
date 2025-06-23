# Arif Platform - Administrator Guide

## Overview

This comprehensive guide provides system administrators with detailed instructions for deploying, configuring, and maintaining the Arif Generative AI Chatbot Platform. The platform is built on .NET 8 microservices architecture with enterprise-grade security and scalability features.

## Table of Contents

1. [System Requirements](#system-requirements)
2. [Installation and Deployment](#installation-and-deployment)
3. [Configuration Management](#configuration-management)
4. [Security Configuration](#security-configuration)
5. [Monitoring and Logging](#monitoring-and-logging)
6. [Backup and Recovery](#backup-and-recovery)
7. [Troubleshooting](#troubleshooting)
8. [Maintenance Procedures](#maintenance-procedures)

## System Requirements

### Minimum Hardware Requirements

#### Production Environment
- **CPU**: 8 cores (Intel Xeon or AMD EPYC)
- **RAM**: 32 GB
- **Storage**: 500 GB SSD (with additional storage for backups)
- **Network**: 1 Gbps connection

#### Development Environment
- **CPU**: 4 cores
- **RAM**: 16 GB
- **Storage**: 100 GB SSD
- **Network**: 100 Mbps connection

### Software Requirements

#### Operating System
- **Linux**: Ubuntu 20.04 LTS or later, CentOS 8 or later
- **Windows**: Windows Server 2019 or later
- **Container Platform**: Docker 20.10+ and Kubernetes 1.21+

#### Runtime Dependencies
- **.NET Runtime**: .NET 8.0 or later
- **Database**: SQL Server 2019 or later, PostgreSQL 13+
- **Cache**: Redis 6.0 or later
- **Message Queue**: Azure Service Bus or RabbitMQ 3.8+
- **Web Server**: Nginx 1.18+ or IIS 10+

## Installation and Deployment

### Docker Deployment (Recommended)

#### Prerequisites
```bash
# Install Docker and Docker Compose
curl -fsSL https://get.docker.com -o get-docker.sh
sudo sh get-docker.sh
sudo curl -L "https://github.com/docker/compose/releases/download/v2.20.0/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
sudo chmod +x /usr/local/bin/docker-compose
```

#### Quick Start Deployment
```bash
# Clone the repository
git clone https://github.com/your-org/arif-platform.git
cd arif-platform

# Configure environment variables
cp .env.example .env
nano .env

# Start all services
docker-compose up -d

# Verify deployment
docker-compose ps
```

#### Environment Configuration (.env)
```bash
# Database Configuration
DB_HOST=sqlserver
DB_PORT=1433
DB_NAME=ArifPlatform
DB_USER=sa
DB_PASSWORD=YourStrongPassword123!

# Redis Configuration
REDIS_HOST=redis
REDIS_PORT=6379
REDIS_PASSWORD=YourRedisPassword

# JWT Configuration
JWT_SECRET_KEY=YourJWTSecretKey256BitMinimum
JWT_ISSUER=ArifPlatform
JWT_AUDIENCE=ArifPlatformUsers
JWT_EXPIRY_MINUTES=60

# AI Service Configuration
OPENAI_API_KEY=your-openai-api-key
AZURE_OPENAI_ENDPOINT=https://your-resource.openai.azure.com/
AZURE_OPENAI_API_KEY=your-azure-openai-key

# External Integrations
TWILIO_ACCOUNT_SID=your-twilio-sid
TWILIO_AUTH_TOKEN=your-twilio-token
FACEBOOK_APP_SECRET=your-facebook-secret
SLACK_BOT_TOKEN=your-slack-token

# Monitoring
APPLICATION_INSIGHTS_KEY=your-app-insights-key
LOG_LEVEL=Information
```

### Kubernetes Deployment

#### Prerequisites
```bash
# Install kubectl
curl -LO "https://dl.k8s.io/release/$(curl -L -s https://dl.k8s.io/release/stable.txt)/bin/linux/amd64/kubectl"
sudo install -o root -g root -m 0755 kubectl /usr/local/bin/kubectl

# Install Helm
curl https://raw.githubusercontent.com/helm/helm/main/scripts/get-helm-3 | bash
```

#### Deploy with Helm
```bash
# Add Arif Platform Helm repository
helm repo add arif-platform ./helm/arif-platform
helm repo update

# Create namespace
kubectl create namespace arif-platform

# Install with custom values
helm install arif-platform arif-platform/arif-platform \
  --namespace arif-platform \
  --values values-production.yaml
```

#### Kubernetes Configuration (values-production.yaml)
```yaml
# Global Configuration
global:
  imageRegistry: your-registry.com
  imageTag: "1.0.0"
  storageClass: "fast-ssd"

# Database Configuration
database:
  enabled: true
  type: sqlserver
  host: "sqlserver.database.windows.net"
  port: 1433
  name: "ArifPlatform"
  username: "arifadmin"
  password: "YourStrongPassword123!"

# Redis Configuration
redis:
  enabled: true
  host: "redis-cluster.cache.windows.net"
  port: 6380
  ssl: true
  password: "YourRedisPassword"

# API Gateway Configuration
apiGateway:
  replicaCount: 3
  resources:
    requests:
      cpu: 500m
      memory: 1Gi
    limits:
      cpu: 1000m
      memory: 2Gi
  service:
    type: LoadBalancer
    port: 80

# Microservices Configuration
services:
  authentication:
    replicaCount: 2
    resources:
      requests:
        cpu: 250m
        memory: 512Mi
      limits:
        cpu: 500m
        memory: 1Gi

  aiOrchestration:
    replicaCount: 3
    resources:
      requests:
        cpu: 500m
        memory: 1Gi
      limits:
        cpu: 1000m
        memory: 2Gi

# Ingress Configuration
ingress:
  enabled: true
  className: "nginx"
  annotations:
    cert-manager.io/cluster-issuer: "letsencrypt-prod"
    nginx.ingress.kubernetes.io/rate-limit: "100"
  hosts:
    - host: api.arif.platform
      paths:
        - path: /
          pathType: Prefix
  tls:
    - secretName: arif-platform-tls
      hosts:
        - api.arif.platform
```

### Manual .NET Deployment

#### Prerequisites
```bash
# Install .NET 8 SDK
wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt-get update
sudo apt-get install -y dotnet-sdk-8.0
```

#### Build and Deploy Services
```bash
# Build all services
cd backend
dotnet build Arif.Platform.sln --configuration Release

# Publish API Gateway
dotnet publish src/gateways/Arif.Platform.ApiGateway/Arif.Platform.ApiGateway.csproj \
  --configuration Release \
  --output /opt/arif-platform/api-gateway

# Publish Authentication Service
dotnet publish src/services/Authentication/Arif.Platform.Authentication.Api/Arif.Platform.Authentication.Api.csproj \
  --configuration Release \
  --output /opt/arif-platform/auth-service

# Create systemd service files
sudo tee /etc/systemd/system/arif-api-gateway.service > /dev/null <<EOF
[Unit]
Description=Arif Platform API Gateway
After=network.target

[Service]
Type=notify
ExecStart=/usr/bin/dotnet /opt/arif-platform/api-gateway/Arif.Platform.ApiGateway.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=arif-api-gateway
User=arif
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://localhost:5000

[Install]
WantedBy=multi-user.target
EOF

# Enable and start services
sudo systemctl enable arif-api-gateway
sudo systemctl start arif-api-gateway
sudo systemctl status arif-api-gateway
```

## Configuration Management

### Application Configuration

#### appsettings.json Structure
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ArifPlatform;Trusted_Connection=true;",
    "Redis": "localhost:6379"
  },
  "Jwt": {
    "SecretKey": "YourJWTSecretKey256BitMinimum",
    "Issuer": "ArifPlatform",
    "Audience": "ArifPlatformUsers",
    "ExpiryMinutes": 60
  },
  "OpenAI": {
    "ApiKey": "your-openai-api-key",
    "Model": "gpt-4",
    "MaxTokens": 150,
    "Temperature": 0.7
  },
  "AzureOpenAI": {
    "Endpoint": "https://your-resource.openai.azure.com/",
    "ApiKey": "your-azure-openai-key",
    "DeploymentName": "gpt-4"
  },
  "Integrations": {
    "Twilio": {
      "AccountSid": "your-twilio-sid",
      "AuthToken": "your-twilio-token",
      "PhoneNumber": "+1234567890"
    },
    "Facebook": {
      "AppId": "your-facebook-app-id",
      "AppSecret": "your-facebook-secret",
      "VerifyToken": "your-verify-token"
    }
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Arif.Platform": "Debug"
    }
  },
  "ApplicationInsights": {
    "InstrumentationKey": "your-app-insights-key"
  }
}
```

#### Environment-Specific Configuration
```bash
# Development
export ASPNETCORE_ENVIRONMENT=Development

# Staging
export ASPNETCORE_ENVIRONMENT=Staging

# Production
export ASPNETCORE_ENVIRONMENT=Production
```

### Database Configuration

#### SQL Server Setup
```sql
-- Create database
CREATE DATABASE ArifPlatform;
GO

-- Create application user
CREATE LOGIN ArifPlatformUser WITH PASSWORD = 'YourStrongPassword123!';
GO

USE ArifPlatform;
GO

CREATE USER ArifPlatformUser FOR LOGIN ArifPlatformUser;
GO

-- Grant permissions
ALTER ROLE db_datareader ADD MEMBER ArifPlatformUser;
ALTER ROLE db_datawriter ADD MEMBER ArifPlatformUser;
ALTER ROLE db_ddladmin ADD MEMBER ArifPlatformUser;
GO
```

#### Database Migration
```bash
# Install Entity Framework tools
dotnet tool install --global dotnet-ef

# Run migrations
cd src/shared/Arif.Platform.Shared.Infrastructure
dotnet ef database update --startup-project ../../gateways/Arif.Platform.ApiGateway

# Create new migration
dotnet ef migrations add InitialCreate --startup-project ../../gateways/Arif.Platform.ApiGateway
```

### Redis Configuration

#### Redis Setup
```bash
# Install Redis
sudo apt update
sudo apt install redis-server

# Configure Redis
sudo nano /etc/redis/redis.conf

# Key configurations:
# bind 127.0.0.1
# port 6379
# requirepass YourRedisPassword
# maxmemory 2gb
# maxmemory-policy allkeys-lru

# Restart Redis
sudo systemctl restart redis-server
sudo systemctl enable redis-server
```

## Security Configuration

### SSL/TLS Configuration

#### Nginx SSL Configuration
```nginx
server {
    listen 443 ssl http2;
    server_name api.arif.platform;

    ssl_certificate /etc/ssl/certs/arif-platform.crt;
    ssl_certificate_key /etc/ssl/private/arif-platform.key;
    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_ciphers ECDHE-RSA-AES256-GCM-SHA512:DHE-RSA-AES256-GCM-SHA512:ECDHE-RSA-AES256-GCM-SHA384:DHE-RSA-AES256-GCM-SHA384;
    ssl_prefer_server_ciphers off;

    location / {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_cache_bypass $http_upgrade;
    }
}
```

### Firewall Configuration

#### UFW (Ubuntu Firewall)
```bash
# Enable UFW
sudo ufw enable

# Allow SSH
sudo ufw allow ssh

# Allow HTTP/HTTPS
sudo ufw allow 80/tcp
sudo ufw allow 443/tcp

# Allow specific service ports (internal only)
sudo ufw allow from 10.0.0.0/8 to any port 5000:5009

# Check status
sudo ufw status verbose
```

### API Security

#### Rate Limiting Configuration
```json
{
  "RateLimiting": {
    "GlobalLimiter": {
      "PermitLimit": 1000,
      "Window": "00:01:00",
      "ReplenishmentPeriod": "00:00:10",
      "TokensPerPeriod": 100
    },
    "AuthenticationLimiter": {
      "PermitLimit": 10,
      "Window": "00:01:00"
    }
  }
}
```

## Monitoring and Logging

### Application Insights Configuration

#### Enable Application Insights
```csharp
// Program.cs
builder.Services.AddApplicationInsightsTelemetry(options =>
{
    options.InstrumentationKey = builder.Configuration["ApplicationInsights:InstrumentationKey"];
});
```

### Structured Logging

#### Serilog Configuration
```json
{
  "Serilog": {
    "Using": ["Serilog.Sinks.Console", "Serilog.Sinks.File", "Serilog.Sinks.ApplicationInsights"],
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
          "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "/var/log/arif-platform/log-.txt",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 30
        }
      },
      {
        "Name": "ApplicationInsights",
        "Args": {
          "instrumentationKey": "your-app-insights-key"
        }
      }
    ]
  }
}
```

### Health Checks

#### Configure Health Checks
```csharp
// Program.cs
builder.Services.AddHealthChecks()
    .AddSqlServer(connectionString)
    .AddRedis(redisConnectionString)
    .AddUrlGroup(new Uri("https://api.openai.com/v1/models"), "OpenAI");

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
```

### Monitoring Dashboard

#### Grafana Dashboard Configuration
```json
{
  "dashboard": {
    "title": "Arif Platform Monitoring",
    "panels": [
      {
        "title": "Request Rate",
        "type": "graph",
        "targets": [
          {
            "expr": "rate(http_requests_total[5m])",
            "legendFormat": "{{method}} {{status}}"
          }
        ]
      },
      {
        "title": "Response Time",
        "type": "graph",
        "targets": [
          {
            "expr": "histogram_quantile(0.95, rate(http_request_duration_seconds_bucket[5m]))",
            "legendFormat": "95th percentile"
          }
        ]
      }
    ]
  }
}
```

## Backup and Recovery

### Database Backup

#### Automated SQL Server Backup
```sql
-- Create backup job
EXEC dbo.sp_add_job
    @job_name = N'Arif Platform Daily Backup';

EXEC dbo.sp_add_jobstep
    @job_name = N'Arif Platform Daily Backup',
    @step_name = N'Backup Database',
    @command = N'BACKUP DATABASE [ArifPlatform] TO DISK = N''/backup/ArifPlatform_$(ESCAPE_SQUOTE(STRTRAN(REPLACE(CONVERT(varchar, GETDATE(), 120), '':'', ''''), '' '', ''''))) .bak''
    WITH FORMAT, INIT, COMPRESSION, CHECKSUM;';

-- Schedule job
EXEC dbo.sp_add_schedule
    @schedule_name = N'Daily at 2 AM',
    @freq_type = 4,
    @freq_interval = 1,
    @active_start_time = 020000;

EXEC dbo.sp_attach_schedule
    @job_name = N'Arif Platform Daily Backup',
    @schedule_name = N'Daily at 2 AM';
```

#### Backup Script
```bash
#!/bin/bash
# backup-arif-platform.sh

BACKUP_DIR="/backup/arif-platform"
DATE=$(date +%Y%m%d_%H%M%S)
DB_NAME="ArifPlatform"

# Create backup directory
mkdir -p $BACKUP_DIR

# Database backup
sqlcmd -S localhost -U sa -P "$DB_PASSWORD" -Q "BACKUP DATABASE [$DB_NAME] TO DISK = N'$BACKUP_DIR/database_$DATE.bak' WITH FORMAT, INIT, COMPRESSION, CHECKSUM;"

# Redis backup
redis-cli --rdb $BACKUP_DIR/redis_$DATE.rdb

# Configuration backup
tar -czf $BACKUP_DIR/config_$DATE.tar.gz /opt/arif-platform/config

# Clean old backups (keep 30 days)
find $BACKUP_DIR -name "*.bak" -mtime +30 -delete
find $BACKUP_DIR -name "*.rdb" -mtime +30 -delete
find $BACKUP_DIR -name "*.tar.gz" -mtime +30 -delete

echo "Backup completed: $DATE"
```

### Disaster Recovery

#### Recovery Procedures
```bash
# 1. Stop all services
sudo systemctl stop arif-*

# 2. Restore database
sqlcmd -S localhost -U sa -P "$DB_PASSWORD" -Q "RESTORE DATABASE [ArifPlatform] FROM DISK = N'/backup/arif-platform/database_20240101_020000.bak' WITH REPLACE;"

# 3. Restore Redis data
sudo systemctl stop redis-server
sudo cp /backup/arif-platform/redis_20240101_020000.rdb /var/lib/redis/dump.rdb
sudo chown redis:redis /var/lib/redis/dump.rdb
sudo systemctl start redis-server

# 4. Restore configuration
sudo tar -xzf /backup/arif-platform/config_20240101_020000.tar.gz -C /

# 5. Start all services
sudo systemctl start arif-*

# 6. Verify system health
curl -f http://localhost:5000/health
```

## Troubleshooting

### Common Issues

#### Service Won't Start
```bash
# Check service status
sudo systemctl status arif-api-gateway

# Check logs
sudo journalctl -u arif-api-gateway -f

# Check configuration
dotnet /opt/arif-platform/api-gateway/Arif.Platform.ApiGateway.dll --dry-run
```

#### Database Connection Issues
```bash
# Test database connectivity
sqlcmd -S localhost -U ArifPlatformUser -P "YourPassword" -Q "SELECT 1"

# Check connection string
grep -r "ConnectionStrings" /opt/arif-platform/*/appsettings.json

# Verify database exists
sqlcmd -S localhost -U sa -P "$DB_PASSWORD" -Q "SELECT name FROM sys.databases WHERE name = 'ArifPlatform'"
```

#### High Memory Usage
```bash
# Check memory usage by service
ps aux | grep dotnet | sort -k4 -nr

# Monitor garbage collection
dotnet-counters monitor --process-id $(pgrep -f "Arif.Platform.ApiGateway") --counters System.Runtime

# Adjust memory limits in systemd
sudo systemctl edit arif-api-gateway
# Add:
# [Service]
# MemoryLimit=2G
```

### Performance Tuning

#### Database Optimization
```sql
-- Update statistics
UPDATE STATISTICS [dbo].[Users];
UPDATE STATISTICS [dbo].[ChatSessions];
UPDATE STATISTICS [dbo].[ChatMessages];

-- Rebuild indexes
ALTER INDEX ALL ON [dbo].[Users] REBUILD;
ALTER INDEX ALL ON [dbo].[ChatSessions] REBUILD;
ALTER INDEX ALL ON [dbo].[ChatMessages] REBUILD;

-- Check query performance
SELECT TOP 10
    total_worker_time/execution_count AS avg_cpu_time,
    total_elapsed_time/execution_count AS avg_elapsed_time,
    execution_count,
    SUBSTRING(st.text, (qs.statement_start_offset/2)+1,
        ((CASE qs.statement_end_offset
            WHEN -1 THEN DATALENGTH(st.text)
            ELSE qs.statement_end_offset
        END - qs.statement_start_offset)/2) + 1) AS statement_text
FROM sys.dm_exec_query_stats qs
CROSS APPLY sys.dm_exec_sql_text(qs.sql_handle) st
ORDER BY total_worker_time/execution_count DESC;
```

## Maintenance Procedures

### Regular Maintenance Tasks

#### Weekly Tasks
```bash
#!/bin/bash
# weekly-maintenance.sh

# Update system packages
sudo apt update && sudo apt upgrade -y

# Clean up logs older than 30 days
find /var/log/arif-platform -name "*.log" -mtime +30 -delete

# Restart services to clear memory
sudo systemctl restart arif-*

# Run database maintenance
sqlcmd -S localhost -U sa -P "$DB_PASSWORD" -i /opt/arif-platform/scripts/weekly-maintenance.sql

# Check disk space
df -h | grep -E "(/$|/var|/opt)"

# Generate health report
curl -s http://localhost:5000/health | jq '.' > /var/log/arif-platform/health-$(date +%Y%m%d).json
```

#### Monthly Tasks
```bash
#!/bin/bash
# monthly-maintenance.sh

# Update SSL certificates
sudo certbot renew --nginx

# Archive old logs
tar -czf /backup/logs/arif-platform-logs-$(date +%Y%m).tar.gz /var/log/arif-platform/*.log
rm /var/log/arif-platform/*.log.1

# Database integrity check
sqlcmd -S localhost -U sa -P "$DB_PASSWORD" -Q "DBCC CHECKDB('ArifPlatform') WITH NO_INFOMSGS;"

# Performance baseline
/opt/arif-platform/scripts/performance-baseline.sh

# Security scan
nmap -sS -O localhost
```

### Update Procedures

#### Application Updates
```bash
#!/bin/bash
# update-arif-platform.sh

VERSION=$1
if [ -z "$VERSION" ]; then
    echo "Usage: $0 <version>"
    exit 1
fi

# Backup current version
sudo cp -r /opt/arif-platform /opt/arif-platform.backup.$(date +%Y%m%d)

# Download new version
wget https://releases.arif.platform/v$VERSION/arif-platform-$VERSION.tar.gz
tar -xzf arif-platform-$VERSION.tar.gz

# Stop services
sudo systemctl stop arif-*

# Update binaries
sudo cp -r arif-platform-$VERSION/* /opt/arif-platform/

# Run database migrations
cd /opt/arif-platform/migrations
dotnet ef database update

# Start services
sudo systemctl start arif-*

# Verify deployment
sleep 30
curl -f http://localhost:5000/health

echo "Update to version $VERSION completed successfully"
```

## Support and Resources

### Log Locations
- **Application Logs**: `/var/log/arif-platform/`
- **System Logs**: `/var/log/syslog`
- **Nginx Logs**: `/var/log/nginx/`
- **Database Logs**: `/var/opt/mssql/log/`

### Configuration Files
- **Application Settings**: `/opt/arif-platform/*/appsettings.json`
- **Nginx Configuration**: `/etc/nginx/sites-available/arif-platform`
- **Systemd Services**: `/etc/systemd/system/arif-*.service`

### Useful Commands
```bash
# Check all Arif services status
sudo systemctl status arif-* --no-pager

# View real-time logs
sudo journalctl -f -u arif-api-gateway

# Test API endpoints
curl -H "Authorization: Bearer $JWT_TOKEN" http://localhost:5000/api/auth/profile

# Monitor system resources
htop
iotop
nethogs
```

### Emergency Contacts
- **Technical Support**: support@arif.platform
- **Emergency Hotline**: +1-800-ARIF-911
- **Status Page**: https://status.arif.platform

This administrator guide provides comprehensive instructions for managing the Arif Platform. For additional support or specific deployment scenarios, please contact our technical support team.
