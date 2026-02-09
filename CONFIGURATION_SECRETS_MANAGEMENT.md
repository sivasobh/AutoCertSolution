# AutoCert - Configuration & Secrets Management Deep Dive
## Environment-Specific setups and Best Practices

---

## Table of Contents
1. [appsettings.json Configuration Strategy](#appssettingsjson-configuration-strategy)
2. [Vault Integration Patterns](#vault-integration-patterns)
3. [Secret Rotation & Management](#secret-rotation--management)
4. [Environment Configurations](#environment-configurations)
5. [Kubernetes Secrets Sealed](#kubernetes-secrets-sealed)
6. [ConfigMap vs Secrets Strategy](#configmap-vs-secrets-strategy)
7. [CI/CD Integration](#cicd-integration)

---

## appsettings.json Configuration Strategy

### Configuration Hierarchy

```
appsettings.json (Base - shared defaults)
└─ appsettings.{ENVIRONMENT}.json (Environment-specific)
    └─ Environment Variables (Runtime overrides)
        └─ Vault Injected Secrets (Precedence highest for sensitive data)
        
Example:
appsettings.json → appsettings.Production.json → ENV vars → Vault secrets
```

### Base Configuration (appsettings.json)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "System": "Information",
      "Microsoft": "Information"
    }
  },
  "AllowedHosts": "*",
  "Serilog": {
    "Using": ["Serilog.Sinks.Console", "Serilog.Sinks.File"],
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "System": "Warning",
        "Microsoft": "Warning",
        "Microsoft.EntityFrameworkCore": "Information"
      }
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "/var/log/autocert/app-.txt",
          "rollingInterval": "Day",
          "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}"
        }
      }
    ]
  },
  "ConnectionStrings": {
    "DefaultConnection": "User ID={{DB_USER}};Password={{DB_PASSWORD}};Host={{DB_HOST}};Port={{DB_PORT}};Database={{DB_NAME}};Pooling=true;"
  },
  "CertificateMonitoring": {
    "Enabled": true,
    "CheckIntervalMinutes": 1440,
    "TimeoutSeconds": 30,
    "RenewalBufferDays": 30,
    "RetryAttempts": 3,
    "MaxConcurrentChecks": 5
  },
  "EmailService": {
    "Enabled": true,
    "SmtpServer": "{{EMAIL_SMTP_SERVER}}",
    "SmtpPort": 587,
    "FromAddress": "noreply@autocert.com",
    "EnableSsl": true,
    "TimeoutSeconds": 10,
    "RetryAttempts": 3
  },
  "Jwt": {
    "Issuer": "autocert-api",
    "Audience": "autocert-app",
    "ExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7,
    "SecretKey": "{{JWT_SECRET_KEY}}"
  },
  "Redis": {
    "Enabled": true,
    "ConnectionString": "{{REDIS_HOST}}:{{REDIS_PORT}},password={{REDIS_PASSWORD}}",
    "Database": 0,
    "DefaultCacheDurationMinutes": 60,
    "EnableCompression": true
  },
  "Cors": {
    "AllowedOrigins": ["https://autocert.yourdomain.com"],
    "AllowedMethods": ["GET", "POST", "PUT", "DELETE", "OPTIONS"],
    "AllowedHeaders": ["*"],
    "AllowCredentials": true,
    "MaxAge": 3600
  },
  "Authentication": {
    "Provider": "JwtBearer",
    "JwtBearer": {
      "Scheme": "Bearer"
    }
  },
  "Authorization": {
    "Policies": [
      {
        "Name": "AdminOnly",
        "Roles": ["Admin"]
      },
      {
        "Name": "ManagerOrAdmin",
        "Roles": ["Manager", "Admin"]
      }
    ]
  },
  "Terraform": {
    "Enabled": true,
    "WorkingDirectory": "/app/terraform",
    "Executable": "terraform",
    "Timeout": 600
  },
  "Python": {
    "Enabled": true,
    "ScriptPath": "/app/scripts/renew_certificate.py",
    "Executable": "python3",
    "Timeout": 300
  },
  "HealthChecks": {
    "Enabled": true,
    "Path": "/health",
    "ReadinessPath": "/ready",
    "LivenessInterval": 10,
    "CheckTimeout": 5
  }
}
```

### Development Configuration (appsettings.Development.json)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "System": "Information",
      "Microsoft": "Information"
    }
  },
  "Serilog": {
    "MinimumLevel": "Debug",
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "theme": "Ansi"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "./logs/autocert-.txt",
          "rollingInterval": "Day"
        }
      }
    ]
  },
  "ConnectionStrings": {
    "DefaultConnection": "User ID=autocert;Password=changeme;Host=localhost;Port=5432;Database=autocert_db;Pooling=true;"
  },
  "Jwt": {
    "SecretKey": "development-secret-key-change-in-production-min-32-chars-please"
  },
  "Redis": {
    "ConnectionString": "localhost:6379"
  },
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:3000",
      "http://localhost:5000",
      "https://localhost:5001"
    ]
  }
}
```

### Staging Configuration (appsettings.Staging.json)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  },
  "Serilog": {
    "MinimumLevel": "Information"
  },
  "ConnectionStrings": {
    "DefaultConnection": "User ID={{DB_USER}};Password={{DB_PASSWORD}};Host=postgresql-staging;Port=5432;Database=autocert_db;Pooling=true;Connection Lifetime=300;"
  },
  "Cors": {
    "AllowedOrigins": ["https://staging-autocert.yourdomain.com"]
  },
  "Redis": {
    "ConnectionString": "redis-staging:6379,password={{REDIS_PASSWORD}}"
  }
}
```

### Production Configuration (appsettings.Production.json)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft": "Warning",
      "Microsoft.EntityFrameworkCore": "Error"
    }
  },
  "Serilog": {
    "MinimumLevel": "Warning",
    "WriteTo": [
      {
        "Name": "Console"
      },
      {
        "Name": "File",
        "Args": {
          "path": "/var/log/autocert/autocert-.txt",
          "rollingInterval": "Day",
          "rollingIntervalSize": 52428800,
          "maxRollingFiles": 30,
          "retainedFileCountLimit": 30
        }
      },
      {
        "Name": "Elasticsearch",
        "Args": {
          "nodeUriProvider": "https://elasticsearch:9200",
          "indexFormat": "autocert-{0:yyyy.MM.dd}",
          "autoRegisterTemplate": true,
          "numberOfShards": 3,
          "numberOfReplicas": 2
        }
      }
    ]
  },
  "ConnectionStrings": {
    "DefaultConnection": "User ID={{DB_USER}};Password={{DB_PASSWORD}};Host={{DB_HOST}};Port={{DB_PORT}};Database={{DB_NAME}};Pooling=true;Connection Lifetime=300;Connection Idle Lifetime=60;Minimum Pool Size=5;Maximum Pool Size=20;"
  },
  "CertificateMonitoring": {
    "CheckIntervalMinutes": 1440,
    "TimeoutSeconds": 30,
    "RenewalBufferDays": 30,
    "RetryAttempts": 3,
    "MaxConcurrentChecks": 10
  },
  "Cors": {
    "AllowedOrigins": ["https://autocert.yourdomain.com"],
    "AllowCredentials": true
  },
  "Redis": {
    "ConnectionString": "{{REDIS_HOST}}:{{REDIS_PORT}},password={{REDIS_PASSWORD}},ssl=true,abortConnect=false",
    "Database": 0,
    "DefaultCacheDurationMinutes": 60
  },
  "HealthChecks": {
    "Enabled": true
  }
}
```

---

## Vault Integration Patterns

### Pattern 1: Vault Agent Injector (Recommended)

This pattern uses Vault's Kubernetes auth and Agent Injector sidecar:

```yaml
# Deployment annotation-based configuration
metadata:
  annotations:
    vault.hashicorp.com/agent-inject: "true"
    vault.hashicorp.com/role: "autocert-role"
    
    # Database credentials
    vault.hashicorp.com/agent-inject-secret-database: "secret/data/autocert/database"
    vault.hashicorp.com/agent-inject-template-database: |
      {{`{{- with secret "secret/data/autocert/database" -}}
      DB_USER={{ .Data.data.username }}
      DB_PASSWORD={{ .Data.data.password }}
      DB_HOST={{ .Data.data.host }}
      DB_PORT={{ .Data.data.port }}
      DB_NAME={{ .Data.data.database }}
      {{- end }}`}}
    
    # Email credentials
    vault.hashicorp.com/agent-inject-secret-email: "secret/data/autocert/email"
    vault.hashicorp.com/agent-inject-template-email: |
      {{`{{- with secret "secret/data/autocert/email" -}}
      EMAIL_SMTP_SERVER={{ .Data.data.smtp_server }}
      EMAIL_SMTP_PORT={{ .Data.data.smtp_port }}
      EMAIL_USERNAME={{ .Data.data.username }}
      EMAIL_PASSWORD={{ .Data.data.password }}
      {{- end }}`}}
    
    # JWT secret
    vault.hashicorp.com/agent-inject-secret-jwt: "secret/data/autocert/jwt"
    vault.hashicorp.com/agent-inject-template-jwt: |
      {{`{{- with secret "secret/data/autocert/jwt" -}}
      JWT_SECRET_KEY={{ .Data.data.secret_key }}
      {{- end }}`}}
```

### Pattern 2: Init Container with Volume Mount

For applications that need secrets before startup:

```yaml
initContainers:
- name: vault-init
  image: vault:latest
  env:
  - name: VAULT_ADDR
    value: "http://vault.vault.svc.cluster.local:8200"
  - name: VAULT_ROLE
    value: "autocert-role"
  volumeMounts:
  - name: vault-token
    mountPath: /var/run/secrets/vault
  command:
  - sh
  - -c
  - |
    JWT=$(cat /var/run/secrets/kubernetes.io/serviceaccount/token)
    VAULT_TOKEN=$(curl -s --request POST \
      --data "{\"jwt\":\"$JWT\",\"role\":\"$VAULT_ROLE\"}" \
      $VAULT_ADDR/v1/auth/kubernetes/login | jq -r '.auth.client_token')
    echo $VAULT_TOKEN > /var/run/secrets/vault/token

containers:
- name: app
  volumeMounts:
  - name: vault-token
    mountPath: /var/run/secrets/vault
  env:
  - name: VAULT_TOKEN
    valueFrom:
      fieldRef:
        fieldPath: /var/run/secrets/vault/token

volumes:
- name: vault-token
  emptyDir:
    medium: Memory
```

### Pattern 3: External Secrets Operator

Using External Secrets Operator to sync Vault secrets to K8s Secrets:

```yaml
# Install External Secrets Operator
helm repo add external-secrets https://charts.external-secrets.io
helm install external-secrets \
  external-secrets/external-secrets \
  --namespace external-secrets-system \
  --create-namespace

# Create SecretStore
apiVersion: external-secrets.io/v1beta1
kind: SecretStore
metadata:
  name: vault-backend
  namespace: autocert-prod
spec:
  provider:
    vault:
      server: "http://vault.vault.svc.cluster.local:8200"
      auth:
        kubernetes:
          mountPath: "kubernetes"
          role: "autocert-role"
      path: "secret"

# Create ExternalSecret to sync secrets
apiVersion: external-secrets.io/v1beta1
kind: ExternalSecret
metadata:
  name: autocert-database
  namespace: autocert-prod
spec:
  refreshInterval: 1h
  secretStoreRef:
    name: vault-backend
    kind: SecretStore
  target:
    name: database-secret
    creationPolicy: Owner
  data:
  - secretKey: username
    remoteRef:
      key: autocert/database
      property: username
  - secretKey: password
    remoteRef:
      key: autocert/database
      property: password
  - secretKey: host
    remoteRef:
      key: autocert/database
      property: host
  - secretKey: port
    remoteRef:
      key: autocert/database
      property: port
```

---

## Secret Rotation & Management

### Vault Secret Rotation Strategy

```bash
# Create a script for rotating database password
#!/bin/bash
# rotate-db-password.sh

# Generate new password
NEW_PASSWORD=$(openssl rand -base64 32)

# Update PostgreSQL password
PGPASSWORD=$CURRENT_PASSWORD psql -h $DB_HOST -U postgres -c \
  "ALTER USER autocert WITH PASSWORD '$NEW_PASSWORD';"

# Update Vault
vault kv put secret/autocert/database \
  username=autocert \
  password="$NEW_PASSWORD" \
  host=$DB_HOST \
  port=5432 \
  database=autocert_db

# Restart pods to pickup new secret (via Vault Agent sidecar)
kubectl rollout restart deployment/autocert-api -n autocert-prod
kubectl rollout restart deployment/autocert-webapp -n autocert-prod

echo "Secret rotated successfully at $(date)"
```

### Automated Secret Rotation using CronJob

```yaml
apiVersion: batch/v1
kind: CronJob
metadata:
  name: secret-rotation
  namespace: autocert-prod
spec:
  schedule: "0 2 * * 0"  # Weekly at 2 AM Sunday
  jobTemplate:
    spec:
      template:
        spec:
          serviceAccountName: autocert
          containers:
          - name: rotation
            image: vault:latest
            env:
            - name: VAULT_ADDR
              value: "http://vault.vault.svc.cluster.local:8200"
            - name: VAULT_TOKEN
              valueFrom:
                secretKeyRef:
                  name: vault-token-secret
                  key: token
            command:
            - /bin/sh
            - -c
            - |
              # Rotate DB password
              NEW_PASS=$(openssl rand -base64 32)
              vault kv put secret/autocert/database password="$NEW_PASS"
              
              # Notify pods to refresh
              kubectl annotate pods -l app=autocert \
                vault.hashicorp.com/role=autocert-role \
                refresh-time="$(date +%s)" \
                --overwrite=true \
                -n autocert-prod
          restartPolicy: OnFailure
```

---

## Environment Configurations

### Pulling Configuration from appsettings at Runtime

```csharp
// In Program.cs
var builder = WebApplication.CreateBuilder(args);

// Load appsettings based on environment
var env = builder.Environment;
builder.Configuration
    .SetBasePath(app.ContentRootPath)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: false)
    .AddEnvironmentVariables();
    // .AddVault(...) if using Vault integration

// Bind configuration sections to strongly-typed options
builder.Services.Configure<CertificateMonitoringOptions>(
    builder.Configuration.GetSection("CertificateMonitoring"));

builder.Services.Configure<EmailServiceOptions>(
    builder.Configuration.GetSection("EmailService"));

builder.Services.Configure<JwtOptions>(
    builder.Configuration.GetSection("Jwt"));

builder.Services.Configure<RedisOptions>(
    builder.Configuration.GetSection("Redis"));
```

### Configuration Classes

```csharp
public class CertificateMonitoringOptions
{
    public bool Enabled { get; set; } = true;
    public int CheckIntervalMinutes { get; set; } = 1440;
    public int TimeoutSeconds { get; set; } = 30;
    public int RenewalBufferDays { get; set; } = 30;
    public int RetryAttempts { get; set; } = 3;
    public int MaxConcurrentChecks { get; set; } = 5;
}

public class EmailServiceOptions
{
    public bool Enabled { get; set; } = true;
    public string SmtpServer { get; set; }
    public int SmtpPort { get; set; } = 587;
    public string FromAddress { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public bool EnableSsl { get; set; } = true;
    public int TimeoutSeconds { get; set; } = 10;
    public int RetryAttempts { get; set; } = 3;
}

public class JwtOptions
{
    public string Issuer { get; set; }
    public string Audience { get; set; }
    public int ExpirationMinutes { get; set; } = 60;
    public int RefreshTokenExpirationDays { get; set; } = 7;
    public string SecretKey { get; set; }
}

public class RedisOptions
{
    public bool Enabled { get; set; } = true;
    public string ConnectionString { get; set; }
    public int Database { get; set; } = 0;
    public int DefaultCacheDurationMinutes { get; set; } = 60;
    public bool EnableCompression { get; set; } = true;
}
```

### Using IOptions in Service Layer

```csharp
public class CertificateService : ICertificateService
{
    private readonly IOptions<CertificateMonitoringOptions> _options;
    private readonly ILogger<CertificateService> _logger;

    public CertificateService(
        IOptions<CertificateMonitoringOptions> options,
        ILogger<CertificateService> logger)
    {
        _options = options;
        _logger = logger;
    }

    public async Task<CertificateDto> CheckCertificateAsync(Guid apiRecordId, Guid tenantId)
    {
        if (!_options.Value.Enabled)
        {
            _logger.LogWarning("Certificate monitoring is disabled");
            return null;
        }

        var timeout = TimeSpan.FromSeconds(_options.Value.TimeoutSeconds);
        var retries = _options.Value.RetryAttempts;
        
        // Use configuration values
        // ...
    }
}
```

---

## Kubernetes Secrets Sealed

### Using Sealed Secrets for Secret Management

```bash
# Install Sealed Secrets Operator
kubectl apply -f https://github.com/bitnami-labs/sealed-secrets/releases/download/v0.21.0/controller.yaml

# Create a secret
kubectl create secret generic autocert-secrets \
  --from-literal=DB_PASSWORD=mypassword \
  --from-literal=JWT_SECRET=mysecret \
  -n autocert-prod \
  --dry-run=client \
  -o yaml | kubeseal -f - > sealed-secret.yaml

# Apply sealed secret
kubectl apply -f sealed-secret.yaml
```

### Sealed Secret YAML Example

```yaml
apiVersion: bitnami.com/v1alpha1
kind: SealedSecret
metadata:
  name: autocert-secrets
  namespace: autocert-prod
spec:
  encryptedData:
    DB_PASSWORD: AgBvX2pK7w6O4kK5X1zJ3hL6mN8pQ0rS2tU3vW9xY0aZ1bC2dE3fG4hI5jK6lM7nO8pQ9rS0tU1vW2xY3zA4bC5dE6fG7hI8jK9lM0nO1pQ2rS3tU4vW5xY6zA7bC8dE9fG0hI1jK2lM3nO4pQ5rS6tU7vW8xY9zA0bC1dE2fG3hI4jK5lM6nO7pQ8rS9tU0vW1xY2zA3bC4dE5fG6hI7jK8lM9nO0pQ1rS2tU3vW4xY5zB6cD7eF8gH9iJ0kL1mM2nN3oO4pP5qQ6rR7sS8tT9uU0vV1wW2xX3yY4zA5bB6cC7dD8eE9fF0gG1hH2iI3jJ4kK5lL6mM7nN8oO9pP0qQ1rR2sS3tT4uU5vV6wW7xX8yY9zA0bB1cC2dD3eE4fF5gG6hH7iI8jJ9kK0lL1mM2nN3oO4pP5qQ6rR7sS8tT9uU0vV1wW2xX3yY4zA5bB6cC7dD8eE9fF0gG1hH2iI3jJ4kK5lL6mM7nN8oO9
    JWT_SECRET: AgCmY3pK7w6O4kK5X1zJ3hL6mN8pQ0rS2tU3vW9xY0aZ1bC2dE3fG4hI5jK6lM7nO8pQ9rS0tU1vW2xY3zA4bC5dE6fG7hI8jK9lM0nO1pQ2rS3tU4vW5xY6zA7bC8dE9fG0hI1jK2lM3nO4pQ5rS6tU7vW8xY9zA0bC1dE2fG3hI4jK5lM6nO7pQ8rS9tU0vW1xY2zA3bC4dE5fG6hI7jK8lM9nO0pQ1rS2tU3vW4xY5zB6cD7eF8gH9iJ0kL1mM2nN3oO4pP5qQ6rR7sS8tT9uU0vV1wW2xX3yY4zA5bB6cC7dD8eE9fF0gG1hH2iI3jJ4kK5lL6mM7nN8oO9pP0qQ1rR2sS3tT4uU5vV6wW7xX8yY9zA0bB1cC2dD3eE4fF5gG6hH7iI8jJ9kK0lL1mM2nN3oO4pP5qQ6rR7sS8tT9uU0vV1wW2xX3yY4zA5bB6cC7dD8eE9fF0gG1hH2iI3jJ4kK5lL6mM
  template:
    metadata:
      name: autocert-secrets
      namespace: autocert-prod
    type: Opaque
```

---

## ConfigMap vs Secrets Strategy

### When to Use Each

```
ConfigMaps:
├── Application settings (JSON, XML, YAML)
├── Feature flags
├── Non-sensitive URLs and endpoints
├── Logging configuration
└── Public API keys/IDs

Secrets (K8s):
├── Database passwords
├── API tokens
├── Private keys
├── Private API keys
├── OAuth credentials
└── SSH keys
```

### Example Configuration

```yaml
# ConfigMap for application settings
apiVersion: v1
kind: ConfigMap
metadata:
  name: autocert-app-config
  namespace: autocert-prod
data:
  appsettings.json: |
    {
      "Logging": { ... },
      "CertificateMonitoring": { ... },
      "Cors": { ... }
    }

---
# Secret for sensitive data
apiVersion: v1
kind: Secret
metadata:
  name: autocert-secrets
  namespace: autocert-prod
type: Opaque
stringData:  # Use stringData for easier creation
  DB_PASSWORD: "your-secure-password"
  JWT_SECRET_KEY: "your-jwt-secret"
  REDIS_PASSWORD: "your-redis-password"
```

---

## CI/CD Integration

### GitHub Actions Workflow with Vault

```yaml
name: Deploy to Kubernetes with Vault

on:
  push:
    branches:
      - main
      - staging

env:
  REGISTRY: myregistry.azurecr.io
  IMAGE_NAME: autocert

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest
    permissions:
      contents: read
      id-token: write

    steps:
    - uses: actions/checkout@v3

    - name: Authenticate with Azure
      uses: azure/login@v1
      with:
        client-id: ${{ secrets.AZURE_CLIENT_ID }}
        tenant-id: ${{ secrets.AZURE_TENANT_ID }}
        subscription-id: ${{ secrets.AZURE_SUBSCRIPTION_ID }}

    - name: Login to Azure Container Registry
      run: |
        az acr login --name myregistry

    - name: Build and push image
      run: |
        docker build -t ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}/api:${{ github.sha }} \
          --file src/AutoCert.API/Dockerfile .
        docker push ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}/api:${{ github.sha }}

    - name: Authenticate with Vault
      uses: hashicorp/vault-action@v2
      with:
        url: https://vault.yourdomain.com
        method: jwt
        role: github-actions
        jwtPayload: ${{ steps.setup-vault.outputs.jwt }}
        secrets: |
          secret/data/autocert/deployment kubeconfig | KUBECONFIG

    - name: Deploy to Kubernetes
      run: |
        helm upgrade autocert ./helm/autocert \
          --namespace autocert-${{ github.ref == 'refs/heads/main' && 'prod' || 'staging' }} \
          --values helm/values-${{ github.ref == 'refs/heads/main' && 'prod' || 'staging' }}.yaml \
          --set image.tag=${{ github.sha }} \
          --wait
```

### Azure DevOps Pipeline with Vault

```yaml
trigger:
  - main
  - staging

pool:
  vmImage: 'ubuntu-latest'

variables:
  dockerRegistryServiceConnection: 'myregistryConnection'
  imageRepository: 'autocert'
  containerRegistry: 'myregistry.azurecr.io'
  dockerfilePath: '$(Build.SourcesDirectory)/src/AutoCert.API/Dockerfile'
  tag: '$(Build.BuildId)'

stages:
- stage: Build
  displayName: Build Docker Image
  jobs:
  - job: BuildImage
    displayName: Build and Push
    steps:
    - task: Docker@2
      displayName: Build and push image
      inputs:
        command: buildAndPush
        repository: $(imageRepository)/api
        dockerfile: $(dockerfilePath)
        containerRegistry: $(dockerRegistryServiceConnection)
        tags: |
          $(tag)
          latest

- stage: Deploy
  displayName: Deploy to Kubernetes
  dependsOn: Build
  condition: succeeded()
  jobs:
  - job: DeployK8s
    displayName: Deploy with Helm
    steps:
    - task: HelmDeploy@0
      inputs:
        command: 'upgrade'
        connectionType: 'Kubernetes'
        kubernetesServiceConnection: 'autocert-k8s-connection'
        namespace: 'autocert-$(System.CollectionUri)'
        chartPath: '$(Build.SourcesDirectory)/helm/autocert'
        chartVersion: 'auto'
        overrideValues: 'image.tag=$(tag)'
        valueFile: 'helm/values-$(System.CollectionUri).yaml'
        waitForExecution: true
        arguments: '--wait'
```

---

**Document Version:** 1.0  
**Last Updated:** February 9, 2026  
**Status:** Production Ready
