# Arif Platform - Configuration Guide

## Overview

This guide provides comprehensive information about configuring the Arif Platform for different environments and use cases. The platform uses a hierarchical configuration system that supports environment-specific settings, feature flags, and runtime configuration changes.

## Configuration Architecture

### Configuration Hierarchy
1. **Default Configuration**: Built-in default values
2. **Environment Configuration**: Environment-specific settings (Development, Staging, Production)
3. **User Configuration**: User-provided configuration files
4. **Environment Variables**: Runtime environment variables
5. **Command Line Arguments**: Override any configuration at runtime

### Configuration Sources
- **appsettings.json**: Base configuration file
- **appsettings.{Environment}.json**: Environment-specific overrides
- **Environment Variables**: Runtime configuration
- **Azure Key Vault**: Secure configuration storage
- **Configuration Management Service**: Centralized configuration management

## Core Configuration

### Database Configuration

#### SQL Server Configuration
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ArifPlatform;User Id=sa;Password=YourPassword;MultipleActiveResultSets=true;TrustServerCertificate=true;",
    "ReadOnlyConnection": "Server=localhost;Database=ArifPlatform;User Id=readonly;Password=ReadOnlyPassword;MultipleActiveResultSets=true;TrustServerCertificate=true;"
  },
  "Database": {
    "CommandTimeout": 30,
    "EnableSensitiveDataLogging": false,
    "EnableDetailedErrors": false,
    "MaxRetryCount": 3,
    "MaxRetryDelay": "00:00:30"
  }
}
```

#### Entity Framework Configuration
```json
{
  "EntityFramework": {
    "EnableServiceProviderCaching": true,
    "EnableSensitiveDataLogging": false,
    "LogLevel": "Information",
    "MigrationsAssembly": "Arif.Platform.Shared.Infrastructure"
  }
}
```

### Authentication and Security Configuration

#### JWT Configuration
```json
{
  "JwtSettings": {
    "SecretKey": "ArifPlatformSecretKeyForJWTTokenGeneration2024!@#$%",
    "Issuer": "https://arif-platform.com",
    "Audience": "arif-platform-users",
    "ExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7,
    "ClockSkew": "00:05:00",
    "ValidateIssuer": true,
    "ValidateAudience": true,
    "ValidateLifetime": true,
    "ValidateIssuerSigningKey": true
  }
}
```

#### Zero-Trust Security Configuration
```json
{
  "ZeroTrustSecurity": {
    "Enabled": true,
    "TrustScoreThreshold": 0.7,
    "DeviceFingerprintingEnabled": true,
    "ContinuousMonitoringEnabled": true,
    "AdaptiveAccessEnabled": true,
    "ThreatIntelligenceEnabled": true,
    "SessionTimeout": "02:00:00",
    "MaxConcurrentSessions": 5,
    "RequireMFAForSensitiveOperations": true,
    "TrustScoreFactors": {
      "LocationWeight": 0.2,
      "DeviceWeight": 0.3,
      "BehaviorWeight": 0.3,
      "TimeWeight": 0.2
    }
  }
}
```

## Enterprise Features Configuration

### AI and Machine Learning Configuration

#### OpenAI Configuration
```json
{
  "OpenAI": {
    "ApiKey": "your-openai-api-key",
    "OrganizationId": "your-organization-id",
    "DefaultModel": "gpt-4",
    "MaxTokens": 4000,
    "Temperature": 0.7,
    "TopP": 1.0,
    "FrequencyPenalty": 0.0,
    "PresencePenalty": 0.0,
    "RequestTimeout": "00:02:00",
    "MaxRetries": 3,
    "RateLimiting": {
      "RequestsPerMinute": 60,
      "TokensPerMinute": 150000
    }
  }
}
```

#### Vector Database Configuration
```json
{
  "VectorDatabase": {
    "Provider": "Pinecone",
    "Pinecone": {
      "ApiKey": "your-pinecone-api-key",
      "Environment": "us-west1-gcp",
      "IndexName": "arif-platform-vectors",
      "Dimension": 1536,
      "Metric": "cosine"
    },
    "Weaviate": {
      "Endpoint": "http://localhost:8080",
      "ApiKey": "your-weaviate-api-key",
      "ClassName": "ArifDocument"
    }
  }
}
```

### Enterprise Integrations Configuration

#### SAP Integration Configuration
```json
{
  "SAP": {
    "Enabled": false,
    "ConnectionString": "ASHOST=sap-server;SYSNR=00;CLIENT=100",
    "Username": "SAP_USER",
    "Password": "SAP_PASSWORD",
    "Language": "EN",
    "PoolSize": 5,
    "Timeout": "00:00:30"
  }
}
```

#### Oracle Integration Configuration
```json
{
  "Oracle": {
    "Enabled": false,
    "ConnectionString": "Data Source=oracle-server:1521/XE;User Id=arif_user;Password=password;",
    "CommandTimeout": 30,
    "PoolSize": 10
  }
}
```

### Monitoring and Observability Configuration

#### Application Insights Configuration
```json
{
  "ApplicationInsights": {
    "InstrumentationKey": "your-instrumentation-key",
    "ConnectionString": "InstrumentationKey=your-key;IngestionEndpoint=https://your-region.in.applicationinsights.azure.com/",
    "EnableAdaptiveSampling": true,
    "EnableQuickPulseMetricStream": true,
    "SamplingSettings": {
      "MaxTelemetryItemsPerSecond": 20,
      "EvaluationInterval": "00:00:15"
    }
  }
}
```

#### OpenTelemetry Configuration
```json
{
  "OpenTelemetry": {
    "ServiceName": "arif-platform",
    "ServiceVersion": "1.0.0",
    "Tracing": {
      "Enabled": true,
      "Exporters": ["Console", "Jaeger", "OTLP"],
      "Jaeger": {
        "AgentHost": "localhost",
        "AgentPort": 6831
      }
    },
    "Metrics": {
      "Enabled": true,
      "Exporters": ["Console", "Prometheus"]
    }
  }
}
```

## Environment-Specific Configuration

### Development Environment
```json
{
  "Environment": "Development",
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft": "Information"
    }
  },
  "Database": {
    "EnableSensitiveDataLogging": true,
    "EnableDetailedErrors": true
  },
  "Authentication": {
    "RequireEmailConfirmation": false,
    "AllowSelfRegistration": true
  }
}
```

### Production Environment
```json
{
  "Environment": "Production",
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft": "Error"
    }
  },
  "Database": {
    "EnableSensitiveDataLogging": false,
    "EnableDetailedErrors": false
  },
  "Authentication": {
    "RequireEmailConfirmation": true,
    "AllowSelfRegistration": false
  },
  "Security": {
    "EnforceHttps": true,
    "RequireStrictTransportSecurity": true,
    "EnableContentSecurityPolicy": true
  }
}
```

## Feature Flags Configuration

### Feature Flag Management
```json
{
  "FeatureFlags": {
    "Provider": "ConfigurationService",
    "RefreshInterval": "00:05:00",
    "Flags": {
      "EnableAdvancedAI": {
        "Enabled": true,
        "Description": "Enable advanced AI features",
        "Environments": ["Development", "Staging", "Production"]
      },
      "EnableZeroTrustSecurity": {
        "Enabled": true,
        "Description": "Enable zero-trust security features",
        "Environments": ["Production"]
      },
      "EnableEnterpriseIntegrations": {
        "Enabled": true,
        "Description": "Enable enterprise system integrations",
        "Environments": ["Production"],
        "UserTargeting": {
          "Enabled": true,
          "Rules": [
            {
              "Attribute": "TenantType",
              "Operator": "Equals",
              "Value": "Enterprise"
            }
          ]
        }
      }
    }
  }
}
```

## Security Configuration

### HTTPS and SSL Configuration
```json
{
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://localhost:5000"
      },
      "Https": {
        "Url": "https://localhost:5001",
        "Certificate": {
          "Path": "certificates/arif-platform.pfx",
          "Password": "certificate-password"
        }
      }
    }
  }
}
```

### CORS Configuration
```json
{
  "Cors": {
    "PolicyName": "ArifCorsPolicy",
    "AllowedOrigins": [
      "https://admin.arif-platform.com",
      "https://tenant.arif-platform.com",
      "https://agent.arif-platform.com"
    ],
    "AllowedMethods": ["GET", "POST", "PUT", "DELETE", "OPTIONS"],
    "AllowedHeaders": ["*"],
    "AllowCredentials": true,
    "PreflightMaxAge": 86400
  }
}
```

## Configuration Best Practices

### Security Best Practices
1. **Never store secrets in configuration files**
2. **Use environment variables or secure vaults for sensitive data**
3. **Encrypt configuration files containing sensitive information**
4. **Implement configuration validation and sanitization**
5. **Use different configurations for different environments**

### Performance Best Practices
1. **Cache frequently accessed configuration values**
2. **Use connection pooling for database connections**
3. **Configure appropriate timeout values**
4. **Optimize logging levels for production**
5. **Use async/await patterns for I/O operations**

---

*This configuration guide is regularly updated to reflect the latest platform capabilities and best practices.*
