# AutoCert - Kubernetes & Helm Deployment Guide
## Complete K8s Infrastructure and Secret Management

---

## Table of Contents
1. [Kubernetes Architecture](#kubernetes-architecture)
2. [Helm Chart Structure](#helm-chart-structure)
3. [Kubernetes Manifests](#kubernetes-manifests)
4. [HashiCorp Vault Integration](#hashicorp-vault-integration)
5. [Configuration Management](#configuration-management)
6. [Deployment Instructions](#deployment-instructions)
7. [Monitoring & Observability](#monitoring--observability)
8. [Troubleshooting](#troubleshooting)

---

## Kubernetes Architecture

### High-Level K8s Deployment Architecture

```
┌─────────────────────────────────────────────────────────────────────┐
│                    Kubernetes Cluster (AKS/EKS/GKE)                 │
├─────────────────────────────────────────────────────────────────────┤
│                                                                      │
│  ┌──────────────────────────────────────────────────────────────┐   │
│  │                   Ingress Controller (Nginx)                 │   │
│  │              (Manages external traffic routing)              │   │
│  └────────────────────┬─────────────────────────────────────────┘   │
│                       │                                              │
│      ┌────────────────┼────────────────┐                             │
│      │                │                │                             │
│  ┌───▼───┐      ┌────▼──┐      ┌─────▼──┐      ┌────────────┐      │
│  │ Service│      │Service│      │Service │      │ Service    │      │
│  │: Web  │      │: API │      │: Auto  │      │: Vault    │      │
│  │        │      │      │      │(Worker)│      │ (Optional) │      │
│  └───┬───┘      └───┬──┘      └────┬───┘      └────────────┘      │
│      │              │              │                              │
│  ┌───▼──────────┐  ┌▼──────────┐┌──▼──────────┐                 │
│  │ Deployment   │  │Deployment││ Deployment  │                 │
│  │ webapp (3)   │  │api (3)    ││ automation  │                 │
│  │ replicas     │  │replicas   ││ (1) replica │                 │
│  └──────────────┘  └───────────┘└─────────────┘                 │
│                                                                    │
│  ┌──────────────────────────────────────────────────────────────┐  │
│  │                    StatefulSet / Databases                   │  │
│  ├──────────────────────────────────────────────────────────────┤  │
│  │  ┌────────────────────────┐  ┌────────────────────────┐      │  │
│  │  │  PostgreSQL StatefulSet│  │  Redis StatefulSet    │      │  │
│  │  │  (Primary + Replicas)  │  │  (Master + Replicas)  │      │  │
│  │  │  with Persistent Vol   │  │  with Persistent Vol  │      │  │
│  │  └────────────────────────┘  └────────────────────────┘      │  │
│  └──────────────────────────────────────────────────────────────┘  │
│                                                                    │
│  ┌──────────────────────────────────────────────────────────────┐  │
│  │              ConfigMaps & Secrets Management                 │  │
│  ├──────────────────────────────────────────────────────────────┤  │
│  │  ┌─────────────────┐  ┌─────────────────┐  ┌────────────┐   │  │
│  │  │ ConfigMap       │  │ Secret          │  │ Vault Init │   │  │
│  │  │ (appsettings)   │  │ (Sealed/Vault)  │  │ Container  │   │  │
│  │  └─────────────────┘  └─────────────────┘  └────────────┘   │  │
│  └──────────────────────────────────────────────────────────────┘  │
│                                                                    │
│  ┌──────────────────────────────────────────────────────────────┐  │
│  │            Persistent Storage & Volumes                      │  │
│  ├──────────────────────────────────────────────────────────────┤  │
│  │  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐       │  │
│  │  │ PVC: DB Data │  │ PVC: Redis   │  │ PVC: Logs   │       │  │
│  │  │ (100Gi)      │  │ (50Gi)       │  │ (50Gi)      │       │  │
│  │  └──────────────┘  └──────────────┘  └──────────────┘       │  │
│  └──────────────────────────────────────────────────────────────┘  │
│                                                                    │
│  ┌──────────────────────────────────────────────────────────────┐  │
│  │           Monitoring & Logging                               │  │
│  ├──────────────────────────────────────────────────────────────┤  │
│  │  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐       │  │
│  │  │ Prometheus   │  │ Grafana      │  │ ELK Stack /  │       │  │
│  │  │ (Metrics)    │  │ (Dashboards) │  │ Loki (Logs)  │       │  │
│  │  └──────────────┘  └──────────────┘  └──────────────┘       │  │
│  └──────────────────────────────────────────────────────────────┘  │
│                                                                    │
└────────────────────────────────────────────────────────────────────┘
```

### Namespace Strategy

```
kubernetes-cluster/
├── default
│   └── System services only
│
├── autocert-prod (Production)
│   ├── Deployments (webapp, api, automation)
│   ├── StatefulSets (PostgreSQL, Redis)
│   ├── Services
│   ├── ConfigMaps
│   ├── Secrets (sealed)
│   └── PersistentVolumeClaims
│
├── autocert-staging (Staging)
│   ├── Same structure as prod
│   └── Lower resource limits
│
├── autocert-dev (Development)
│   ├── Single replica deployments
│   └── Minimal resources
│
├── vault (Secret Management)
│   ├── Vault StatefulSet
│   ├── Vault ConfigMap (policies)
│   └── Vault Secrets
│
└── monitoring (Observability)
    ├── Prometheus
    ├── Grafana
    ├── Loki / ELK
    └── AlertManager
```

---

## Helm Chart Structure

### Directory Structure

```
autocert-helm-chart/
├── Chart.yaml
├── values.yaml
├── values-prod.yaml
├── values-staging.yaml
├── values-dev.yaml
├── charts/
│   ├── postgresql/
│   ├── redis/
│   └── rabbitmq/
├── templates/
│   ├── namespace.yaml
│   ├── configmap.yaml
│   ├── secret.yaml
│   ├── vault-init.yaml
│   │
│   ├── deployments/
│   │   ├── webapp-deployment.yaml
│   │   ├── api-deployment.yaml
│   │   ├── automation-deployment.yaml
│   │   └── vault-agent-configmap.yaml
│   │
│   ├── statefulsets/
│   │   ├── postgresql-statefulset.yaml
│   │   ├── redis-statefulset.yaml
│   │   └── rabbitmq-statefulset.yaml
│   │
│   ├── services/
│   │   ├── webapp-service.yaml
│   │   ├── api-service.yaml
│   │   ├── postgres-service.yaml
│   │   ├── redis-service.yaml
│   │   └── vault-service.yaml
│   │
│   ├── ingress/
│   │   ├── ingress.yaml
│   │   └── tls-secret.yaml
│   │
│   ├── pvc/
│   │   ├── postgres-pvc.yaml
│   │   ├── redis-pvc.yaml
│   │   └── logs-pvc.yaml
│   │
│   ├── hpa/
│   │   ├── webapp-hpa.yaml
│   │   └── api-hpa.yaml
│   │
│   ├── rbac/
│   │   ├── service-account.yaml
│   │   ├── role.yaml
│   │   └── role-binding.yaml
│   │
│   ├── monitoring/
│   │   ├── servicemonitor.yaml
│   │   └── prometheusrule.yaml
│   │
│   └── _helpers.tpl
│
└── README.md
```

### Chart.yaml

```yaml
apiVersion: v2
name: autocert
description: A Helm chart for AutoCert - Certificate Monitoring & Renewal Platform
type: application
version: 1.0.0
appVersion: 1.0.0

# Chart metadata
home: https://github.com/yourusername/autocert
sources:
  - https://github.com/yourusername/autocert
maintainers:
  - name: Your Name
    email: your.email@example.com

# Chart dependencies
dependencies:
  - name: postgresql
    version: "12.x.x"
    repository: "https://charts.bitnami.com/bitnami"
    condition: postgresql.enabled
    
  - name: redis
    version: "17.x.x"
    repository: "https://charts.bitnami.com/bitnami"
    condition: redis.enabled
    
  - name: rabbitmq
    version: "12.x.x"
    repository: "https://charts.bitnami.com/bitnami"
    condition: rabbitmq.enabled

keywords:
  - certificate
  - monitoring
  - renewal
  - automated

# Chart constraints
kubeVersion: ">=1.24.0"
```

### values.yaml (Default Config)

```yaml
# Global settings
global:
  environment: development
  namespace: autocert-dev
  domain: autocert.local
  imagePullPolicy: IfNotPresent

# Image Registry
image:
  registry: myregistry.azurecr.io
  projectName: autocert

# Replica Configurations
replicaCount:
  webapp: 1
  api: 1
  automation: 1

# Web Application Configuration
webapp:
  image:
    repository: myregistry.azurecr.io/autocert/webapp
    tag: latest
    pullPolicy: IfNotPresent
  
  replicaCount: 1
  
  service:
    type: ClusterIP
    port: 80
    targetPort: 80
    annotations: {}
  
  ingress:
    enabled: true
    className: nginx
    annotations:
      cert-manager.io/cluster-issuer: "letsencrypt-prod"
    hosts:
      - host: autocert.yourdomain.com
        paths:
          - path: /
            pathType: Prefix
    tls:
      - secretName: autocert-tls
        hosts:
          - autocert.yourdomain.com
  
  resources:
    requests:
      cpu: 200m
      memory: 256Mi
    limits:
      cpu: 500m
      memory: 512Mi
  
  autoscaling:
    enabled: true
    minReplicas: 1
    maxReplicas: 5
    targetCPUUtilizationPercentage: 70
    targetMemoryUtilizationPercentage: 80

# API Configuration
api:
  image:
    repository: myregistry.azurecr.io/autocert/api
    tag: latest
    pullPolicy: IfNotPresent
  
  replicaCount: 1
  
  service:
    type: ClusterIP
    port: 5000
    targetPort: 5000
    annotations: {}
  
  resources:
    requests:
      cpu: 250m
      memory: 512Mi
    limits:
      cpu: 750m
      memory: 1024Mi
  
  autoscaling:
    enabled: true
    minReplicas: 1
    maxReplicas: 5
    targetCPUUtilizationPercentage: 70

# Automation Service Configuration
automation:
  image:
    repository: myregistry.azurecr.io/autocert/automation
    tag: latest
    pullPolicy: IfNotPresent
  
  replicaCount: 1
  
  resources:
    requests:
      cpu: 200m
      memory: 512Mi
    limits:
      cpu: 500m
      memory: 1024Mi

# PostgreSQL Configuration
postgresql:
  enabled: true
  auth:
    username: autocert
    password: changeme
    database: autocert_db
  
  primary:
    persistence:
      enabled: true
      size: 100Gi
      storageClassName: "default"
    
    resources:
      requests:
        cpu: 500m
        memory: 1Gi
      limits:
        cpu: 1000m
        memory: 2Gi
  
  metrics:
    enabled: true
    serviceMonitor:
      enabled: true

# Redis Configuration
redis:
  enabled: true
  auth:
    enabled: true
    password: redispass
  
  master:
    persistence:
      enabled: true
      size: 50Gi
  
  replica:
    replicaCount: 2
    persistence:
      enabled: true
      size: 50Gi
  
  resources:
    requests:
      cpu: 100m
      memory: 256Mi
    limits:
      cpu: 300m
      memory: 512Mi

# RabbitMQ Configuration
rabbitmq:
  enabled: true
  auth:
    username: rabbitmq
    password: changeme
  
  persistence:
    enabled: true
    size: 30Gi
  
  resources:
    requests:
      cpu: 250m
      memory: 512Mi
    limits:
      cpu: 500m
      memory: 1Gi

# Configuration Management
config:
  # Application Settings (stored in ConfigMap)
  appsettingsEnvironment: Development
  logLevel: Information
  certificateCheckInterval: 1440  # minutes
  renewalBuffer: 30  # days

# Vault Configuration for Secrets
vault:
  enabled: true
  address: "http://vault.vault.svc.cluster.local:8200"
  role: "autocert-role"
  secretPath: "secret/data/autocert"
  authMethod: "kubernetes"
  serviceAccount: "autocert"
  
  # Vault Agent Injector
  injector:
    enabled: true
    agentImage: vault:latest

# Database Credentials (Vault-managed)
database:
  host: postgresql
  port: 5432
  name: autocert_db
  username: autocert
  # password: managed by Vault
  
# Email Configuration (Vault-managed)
email:
  smtpServer: smtp.gmail.com
  smtpPort: 587
  fromAddress: noreply@autocert.com
  # username: managed by Vault
  # password: managed by Vault

# JWT Configuration (Vault-managed)
jwt:
  issuer: "autocert-api"
  audience: "autocert-app"
  expirationMinutes: 60
  # secretKey: managed by Vault

# Storage Configuration
storage:
  class: "default"
  databases:
    postgres:
      size: 100Gi
    redis:
      size: 50Gi
    logs:
      size: 50Gi

# RBAC Configuration
rbac:
  create: true
  serviceAccountName: autocert

# Security Context
securityContext:
  runAsNonRoot: true
  runAsUser: 1000
  fsGroup: 1000

# Network Policy
networkPolicy:
  enabled: false  # Set to true for production
  ingress:
    - from:
      - namespaceSelector:
          matchLabels:
            name: autocert-prod

# Monitoring
monitoring:
  prometheus:
    enabled: true
    interval: 30s
  
  alerts:
    enabled: true
    
  grafana:
    enabled: true
    adminPassword: admin

# Logging
logging:
  serilog:
    minLevel: Information
  
  centralized:
    enabled: true
    type: "loki"  # or "elasticsearch"
```

### values-prod.yaml (Production Overrides)

```yaml
global:
  environment: production
  namespace: autocert-prod

replicaCount:
  webapp: 3
  api: 3
  automation: 2

webapp:
  replicaCount: 3
  resources:
    requests:
      cpu: 500m
      memory: 512Mi
    limits:
      cpu: 1000m
      memory: 1Gi
  autoscaling:
    enabled: true
    minReplicas: 3
    maxReplicas: 10
    targetCPUUtilizationPercentage: 70

api:
  replicaCount: 3
  resources:
    requests:
      cpu: 500m
      memory: 1Gi
    limits:
      cpu: 1500m
      memory: 2Gi
  autoscaling:
    enabled: true
    minReplicas: 3
    maxReplicas: 10

automation:
  replicaCount: 2
  resources:
    requests:
      cpu: 500m
      memory: 1Gi
    limits:
      cpu: 1000m
      memory: 2Gi

postgresql:
  primary:
    persistence:
      size: 200Gi
      storageClassName: "fast-ssd"
    resources:
      requests:
        cpu: 2000m
        memory: 4Gi
      limits:
        cpu: 4000m
        memory: 8Gi
  
  metrics:
    enabled: true
    serviceMonitor:
      enabled: true

redis:
  master:
    persistence:
      size: 100Gi
      storageClassName: "fast-ssd"
  replica:
    replicaCount: 3
    persistence:
      size: 100Gi

vault:
  enabled: true
  address: "https://vault.yourdomain.com"
  role: "autocert-prod-role"

monitoring:
  prometheus:
    enabled: true
  alerts:
    enabled: true
  grafana:
    enabled: true

networkPolicy:
  enabled: true

securityContext:
  runAsNonRoot: true
  readOnlyRootFilesystem: true
```

---

## Kubernetes Manifests

### 1. Namespace

**templates/namespace.yaml**
```yaml
apiVersion: v1
kind: Namespace
metadata:
  name: {{ .Values.global.namespace }}
  labels:
    app: autocert
    environment: {{ .Values.global.environment }}
```

### 2. ConfigMap for Application Settings

**templates/configmap.yaml**
```yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: autocert-config
  namespace: {{ .Values.global.namespace }}
  labels:
    app: autocert
data:
  appsettings.json: |
    {
      "Logging": {
        "LogLevel": {
          "Default": "{{ .Values.config.logLevel }}"
        }
      },
      "Serilog": {
        "MinimumLevel": "{{ .Values.config.logLevel }}",
        "WriteTo": [
          {
            "Name": "Console"
          },
          {
            "Name": "File",
            "Args": {
              "path": "/var/log/autocert/autocert-.txt",
              "rollingInterval": "Day"
            }
          }
        ]
      },
      "ConnectionStrings": {
        "DefaultConnection": "User ID=autocert;Password=@DB_PASSWORD;Host=postgresql;Port=5432;Database=autocert_db;Pooling=true;"
      },
      "CertificateMonitoring": {
        "CheckIntervalMinutes": {{ .Values.config.certificateCheckInterval }},
        "RenewalBufferDays": {{ .Values.config.renewalBuffer }}
      },
      "Jwt": {
        "Issuer": "{{ .Values.jwt.issuer }}",
        "Audience": "{{ .Values.jwt.audience }}",
        "ExpirationMinutes": {{ .Values.jwt.expirationMinutes }}
      }
    }

  appsettings.Production.json: |
    {
      "Logging": {
        "LogLevel": {
          "Default": "Warning"
        }
      }
    }
```

### 3. Service Accounts & RBAC

**templates/rbac/service-account.yaml**
```yaml
apiVersion: v1
kind: ServiceAccount
metadata:
  name: {{ .Values.rbac.serviceAccountName }}
  namespace: {{ .Values.global.namespace }}
  labels:
    app: autocert
```

**templates/rbac/role.yaml**
```yaml
apiVersion: rbac.authorization.k8s.io/v1
kind: Role
metadata:
  name: {{ .Values.rbac.serviceAccountName }}-role
  namespace: {{ .Values.global.namespace }}
rules:
  - apiGroups: [""]
    resources: ["configmaps", "secrets"]
    verbs: ["get", "list", "watch"]
  - apiGroups: [""]
    resources: ["pods"]
    verbs: ["get", "list"]
  - apiGroups: [""]
    resources: ["pods/log"]
    verbs: ["get"]
```

**templates/rbac/role-binding.yaml**
```yaml
apiVersion: rbac.authorization.k8s.io/v1
kind: RoleBinding
metadata:
  name: {{ .Values.rbac.serviceAccountName }}-binding
  namespace: {{ .Values.global.namespace }}
roleRef:
  apiGroup: rbac.authorization.k8s.io
  kind: Role
  name: {{ .Values.rbac.serviceAccountName }}-role
subjects:
  - kind: ServiceAccount
    name: {{ .Values.rbac.serviceAccountName }}
    namespace: {{ .Values.global.namespace }}
```

### 4. Web Application Deployment

**templates/deployments/webapp-deployment.yaml**
```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: autocert-webapp
  namespace: {{ .Values.global.namespace }}
  labels:
    app: autocert
    component: webapp
spec:
  replicas: {{ .Values.webapp.replicaCount }}
  selector:
    matchLabels:
      app: autocert
      component: webapp
  template:
    metadata:
      labels:
        app: autocert
        component: webapp
      annotations:
        prometheus.io/scrape: "true"
        prometheus.io/port: "9090"
        prometheus.io/path: "/metrics"
        {{- if .Values.vault.injector.enabled }}
        vault.hashicorp.com/agent-inject: "true"
        vault.hashicorp.com/agent-inject-secret-db: "{{ .Values.vault.secretPath }}/database"
        vault.hashicorp.com/agent-inject-secret-email: "{{ .Values.vault.secretPath }}/email"
        vault.hashicorp.com/agent-inject-secret-jwt: "{{ .Values.vault.secretPath }}/jwt"
        vault.hashicorp.com/agent-inject-template-db: |
          {{`{{- with secret "secret/data/autocert/database" -}}
          export DB_PASSWORD={{ .Data.data.password }}
          {{- end }}`}}
        {{- end }}
    spec:
      serviceAccountName: {{ .Values.rbac.serviceAccountName }}
      securityContext:
        runAsNonRoot: {{ .Values.securityContext.runAsNonRoot }}
        runAsUser: {{ .Values.securityContext.runAsUser }}
        fsGroup: {{ .Values.securityContext.fsGroup }}
      
      containers:
      - name: webapp
        image: "{{ .Values.image.registry }}/{{ .Values.image.projectName }}/webapp:{{ .Values.webapp.image.tag }}"
        imagePullPolicy: {{ .Values.webapp.image.pullPolicy }}
        
        ports:
        - name: http
          containerPort: 80
          protocol: TCP
        
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: "{{ .Values.global.environment }}"
        - name: ASPNETCORE_URLS
          value: "http://+:80"
        - name: TZ
          value: "UTC"
        
        # Database connection from Vault
        {{- if .Values.vault.enabled }}
        - name: ConnectionStrings__DefaultConnection
          value: "User ID=autocert;Password=$(DB_PASSWORD);Host=postgresql;Port=5432;Database=autocert_db;Pooling=true;"
        - name: DB_PASSWORD
          valueFrom:
            secretKeyRef:
              name: vault-db-secret
              key: password
        {{- end }}
        
        # Redis connection
        - name: Redis__ConnectionString
          valueFrom:
            secretKeyRef:
              name: redis-secret
              key: connection-string
        
        # API base URL
        - name: ApiBaseUrl
          value: "http://autocert-api:{{ .Values.api.service.port }}"
        
        livenessProbe:
          httpGet:
            path: /health
            port: 80
          initialDelaySeconds: 30
          periodSeconds: 10
          timeoutSeconds: 5
          failureThreshold: 3
        
        readinessProbe:
          httpGet:
            path: /ready
            port: 80
          initialDelaySeconds: 10
          periodSeconds: 5
          timeoutSeconds: 3
          failureThreshold: 3
        
        resources:
          requests:
            cpu: {{ .Values.webapp.resources.requests.cpu }}
            memory: {{ .Values.webapp.resources.requests.memory }}
          limits:
            cpu: {{ .Values.webapp.resources.limits.cpu }}
            memory: {{ .Values.webapp.resources.limits.memory }}
        
        volumeMounts:
        - name: config
          mountPath: /app/appsettings.json
          subPath: appsettings.json
          readOnly: true
        - name: logs
          mountPath: /var/log/autocert
      
      volumes:
      - name: config
        configMap:
          name: autocert-config
      - name: logs
        emptyDir: {}
      
      affinity:
        podAntiAffinity:
          preferredDuringSchedulingIgnoredDuringExecution:
          - weight: 100
            podAffinityTerm:
              labelSelector:
                matchExpressions:
                - key: component
                  operator: In
                  values:
                  - webapp
              topologyKey: kubernetes.io/hostname
```

### 5. API Deployment

**templates/deployments/api-deployment.yaml**
```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: autocert-api
  namespace: {{ .Values.global.namespace }}
  labels:
    app: autocert
    component: api
spec:
  replicas: {{ .Values.api.replicaCount }}
  selector:
    matchLabels:
      app: autocert
      component: api
  template:
    metadata:
      labels:
        app: autocert
        component: api
      annotations:
        prometheus.io/scrape: "true"
        prometheus.io/port: "9090"
        {{- if .Values.vault.injector.enabled }}
        vault.hashicorp.com/agent-inject: "true"
        vault.hashicorp.com/agent-inject-secret-db: "{{ .Values.vault.secretPath }}/database"
        vault.hashicorp.com/agent-inject-secret-email: "{{ .Values.vault.secretPath }}/email"
        vault.hashicorp.com/agent-inject-secret-jwt: "{{ .Values.vault.secretPath }}/jwt"
        {{- end }}
    spec:
      serviceAccountName: {{ .Values.rbac.serviceAccountName }}
      securityContext:
        runAsNonRoot: {{ .Values.securityContext.runAsNonRoot }}
        runAsUser: {{ .Values.securityContext.runAsUser }}
        fsGroup: {{ .Values.securityContext.fsGroup }}
      
      containers:
      - name: api
        image: "{{ .Values.image.registry }}/{{ .Values.image.projectName }}/api:{{ .Values.api.image.tag }}"
        imagePullPolicy: {{ .Values.api.image.pullPolicy }}
        
        ports:
        - name: http
          containerPort: 5000
          protocol: TCP
        - name: https
          containerPort: 5001
          protocol: TCP
        
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: "{{ .Values.global.environment }}"
        - name: ASPNETCORE_URLS
          value: "http://+:5000"
        
        {{- if .Values.vault.enabled }}
        - name: ConnectionStrings__DefaultConnection
          value: "User ID=autocert;Password=$(DB_PASSWORD);Host=postgresql;Port=5432;Database=autocert_db;Pooling=true;"
        - name: DB_PASSWORD
          valueFrom:
            secretKeyRef:
              name: vault-db-secret
              key: password
        {{- end }}
        
        - name: Redis__ConnectionString
          valueFrom:
            secretKeyRef:
              name: redis-secret
              key: connection-string
        
        livenessProbe:
          httpGet:
            path: /api/health
            port: 5000
          initialDelaySeconds: 30
          periodSeconds: 10
          timeoutSeconds: 5
          failureThreshold: 3
        
        readinessProbe:
          httpGet:
            path: /api/ready
            port: 5000
          initialDelaySeconds: 10
          periodSeconds: 5
          timeoutSeconds: 3
          failureThreshold: 3
        
        resources:
          requests:
            cpu: {{ .Values.api.resources.requests.cpu }}
            memory: {{ .Values.api.resources.requests.memory }}
          limits:
            cpu: {{ .Values.api.resources.limits.cpu }}
            memory: {{ .Values.api.resources.limits.memory }}
        
        volumeMounts:
        - name: config
          mountPath: /app/appsettings.json
          subPath: appsettings.json
          readOnly: true
        - name: logs
          mountPath: /var/log/autocert
      
      volumes:
      - name: config
        configMap:
          name: autocert-config
      - name: logs
        emptyDir: {}
      
      affinity:
        podAntiAffinity:
          preferredDuringSchedulingIgnoredDuringExecution:
          - weight: 100
            podAffinityTerm:
              labelSelector:
                matchExpressions:
                - key: component
                  operator: In
                  values:
                  - api
              topologyKey: kubernetes.io/hostname
```

### 6. Automation Worker Deployment

**templates/deployments/automation-deployment.yaml**
```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: autocert-automation
  namespace: {{ .Values.global.namespace }}
  labels:
    app: autocert
    component: automation
spec:
  replicas: {{ .Values.automation.replicaCount }}
  selector:
    matchLabels:
      app: autocert
      component: automation
  template:
    metadata:
      labels:
        app: autocert
        component: automation
      annotations:
        {{- if .Values.vault.injector.enabled }}
        vault.hashicorp.com/agent-inject: "true"
        vault.hashicorp.com/agent-inject-secret-db: "{{ .Values.vault.secretPath }}/database"
        vault.hashicorp.com/agent-inject-secret-terraform: "{{ .Values.vault.secretPath }}/terraform"
        {{- end }}
    spec:
      serviceAccountName: {{ .Values.rbac.serviceAccountName }}
      securityContext:
        runAsNonRoot: {{ .Values.securityContext.runAsNonRoot }}
        runAsUser: {{ .Values.securityContext.runAsUser }}
        fsGroup: {{ .Values.securityContext.fsGroup }}
      
      containers:
      - name: automation
        image: "{{ .Values.image.registry }}/{{ .Values.image.projectName }}/automation:{{ .Values.automation.image.tag }}"
        imagePullPolicy: {{ .Values.automation.image.pullPolicy }}
        
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: "{{ .Values.global.environment }}"
        
        {{- if .Values.vault.enabled }}
        - name: ConnectionStrings__DefaultConnection
          value: "User ID=autocert;Password=$(DB_PASSWORD);Host=postgresql;Port=5432;Database=autocert_db;Pooling=true;"
        - name: DB_PASSWORD
          valueFrom:
            secretKeyRef:
              name: vault-db-secret
              key: password
        {{- end }}
        
        - name: CertificateCheckInterval
          value: "{{ .Values.config.certificateCheckInterval }}"
        - name: RenewalBuffer
          value: "{{ .Values.config.renewalBuffer }}"
        
        resources:
          requests:
            cpu: {{ .Values.automation.resources.requests.cpu }}
            memory: {{ .Values.automation.resources.requests.memory }}
          limits:
            cpu: {{ .Values.automation.resources.limits.cpu }}
            memory: {{ .Values.automation.resources.limits.memory }}
        
        volumeMounts:
        - name: config
          mountPath: /app/appsettings.json
          subPath: appsettings.json
          readOnly: true
        - name: logs
          mountPath: /var/log/autocert
        - name: terraform
          mountPath: /app/terraform
      
      volumes:
      - name: config
        configMap:
          name: autocert-config
      - name: logs
        emptyDir: {}
      - name: terraform
        configMap:
          name: terraform-configs
```

### 7. Services

**templates/services/webapp-service.yaml**
```yaml
apiVersion: v1
kind: Service
metadata:
  name: autocert-webapp
  namespace: {{ .Values.global.namespace }}
  labels:
    app: autocert
    component: webapp
spec:
  type: {{ .Values.webapp.service.type }}
  ports:
  - port: {{ .Values.webapp.service.port }}
    targetPort: http
    protocol: TCP
    name: http
  selector:
    app: autocert
    component: webapp
```

**templates/services/api-service.yaml**
```yaml
apiVersion: v1
kind: Service
metadata:
  name: autocert-api
  namespace: {{ .Values.global.namespace }}
  labels:
    app: autocert
    component: api
spec:
  type: {{ .Values.api.service.type }}
  ports:
  - port: {{ .Values.api.service.port }}
    targetPort: http
    protocol: TCP
    name: http
  selector:
    app: autocert
    component: api
```

### 8. Ingress

**templates/ingress/ingress.yaml**
```yaml
{{- if .Values.webapp.ingress.enabled }}
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: autocert-ingress
  namespace: {{ .Values.global.namespace }}
  labels:
    app: autocert
  {{- with .Values.webapp.ingress.annotations }}
  annotations:
    {{- toYaml . | nindent 4 }}
  {{- end }}
spec:
  {{- if .Values.webapp.ingress.className }}
  ingressClassName: {{ .Values.webapp.ingress.className }}
  {{- end }}
  {{- if .Values.webapp.ingress.tls }}
  tls:
    {{- range .Values.webapp.ingress.tls }}
    - hosts:
        {{- range .hosts }}
        - {{ . | quote }}
        {{- end }}
      secretName: {{ .secretName }}
    {{- end }}
  {{- end }}
  rules:
    {{- range .Values.webapp.ingress.hosts }}
    - host: {{ .host | quote }}
      http:
        paths:
          {{- range .paths }}
          - path: {{ .path }}
            pathType: {{ .pathType }}
            backend:
              service:
                name: autocert-webapp
                port:
                  number: {{ $.Values.webapp.service.port }}
          {{- end }}
    {{- end }}
{{- end }}
```

### 9. Persistent Volume Claims

**templates/pvc/postgres-pvc.yaml**
```yaml
apiVersion: v1
kind: PersistentVolumeClaim
metadata:
  name: postgres-pvc
  namespace: {{ .Values.global.namespace }}
  labels:
    app: autocert
spec:
  accessModes:
    - ReadWriteOnce
  storageClassName: {{ .Values.postgresql.primary.persistence.storageClassName }}
  resources:
    requests:
      storage: {{ .Values.postgresql.primary.persistence.size }}
```

**templates/pvc/redis-pvc.yaml**
```yaml
apiVersion: v1
kind: PersistentVolumeClaim
metadata:
  name: redis-pvc
  namespace: {{ .Values.global.namespace }}
  labels:
    app: autocert
spec:
  accessModes:
    - ReadWriteOnce
  storageClassName: {{ .Values.redis.master.persistence.storageClassName }}
  resources:
    requests:
      storage: {{ .Values.redis.master.persistence.size }}
```

### 10. Horizontal Pod Autoscaler

**templates/hpa/webapp-hpa.yaml**
```yaml
{{- if .Values.webapp.autoscaling.enabled }}
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: autocert-webapp-hpa
  namespace: {{ .Values.global.namespace }}
  labels:
    app: autocert
    component: webapp
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: autocert-webapp
  minReplicas: {{ .Values.webapp.autoscaling.minReplicas }}
  maxReplicas: {{ .Values.webapp.autoscaling.maxReplicas }}
  metrics:
  - type: Resource
    resource:
      name: cpu
      target:
        type: Utilization
        averageUtilization: {{ .Values.webapp.autoscaling.targetCPUUtilizationPercentage }}
  - type: Resource
    resource:
      name: memory
      target:
        type: Utilization
        averageUtilization: {{ .Values.webapp.autoscaling.targetMemoryUtilizationPercentage }}
{{- end }}
```

### 11. Network Policy

**templates/network-policy.yaml**
```yaml
{{- if .Values.networkPolicy.enabled }}
apiVersion: networking.k8s.io/v1
kind: NetworkPolicy
metadata:
  name: autocert-network-policy
  namespace: {{ .Values.global.namespace }}
spec:
  podSelector:
    matchLabels:
      app: autocert
  policyTypes:
  - Ingress
  - Egress
  ingress:
  - from:
    - namespaceSelector:
        matchLabels:
          name: {{ .Values.global.namespace }}
    - podSelector:
        matchLabels:
          app: nginx-ingress
  egress:
  - to:
    - namespaceSelector: {}
    ports:
    - protocol: TCP
      port: 5432  # PostgreSQL
    - protocol: TCP
      port: 6379  # Redis
    - protocol: TCP
      port: 5672  # RabbitMQ
    - protocol: TCP
      port: 53    # DNS
    - protocol: TCP
      port: 443   # HTTPS
{{- end }}
```

---

## HashiCorp Vault Integration

### Vault Setup & Configuration

**Vault Authentication with Kubernetes**

```hcl
# Enable Kubernetes authentication
vault auth enable kubernetes

# Configure Kubernetes auth method
vault write auth/kubernetes/config \
  kubernetes_host="https://$KUBERNETES_HOST:$KUBERNETES_PORT" \
  kubernetes_ca_cert=@/var/run/secrets/kubernetes.io/serviceaccount/ca.crt \
  token_reviewer_jwt=@/var/run/secrets/kubernetes.io/serviceaccount/token

# Create policy
vault policy write autocert - <<EOF
path "secret/data/autocert/database" {
  capabilities = ["read", "list"]
}
path "secret/data/autocert/email" {
  capabilities = ["read", "list"]
}
path "secret/data/autocert/jwt" {
  capabilities = ["read", "list"]
}
path "secret/data/autocert/terraform" {
  capabilities = ["read", "list"]
}
EOF

# Create role
vault write auth/kubernetes/role/autocert-role \
  bound_service_account_names=autocert \
  bound_service_account_namespaces=autocert-prod \
  policies=autocert \
  ttl=24h

# Store secrets
vault kv put secret/autocert/database \
  username=autocert \
  password=YOUR_STRONG_PASSWORD \
  host=postgresql \
  port=5432 \
  database=autocert_db

vault kv put secret/autocert/email \
  smtp_server=smtp.gmail.com \
  smtp_port=587 \
  from_address=noreply@autocert.com \
  username=your-email@gmail.com \
  password=YOUR_APP_PASSWORD

vault kv put secret/autocert/jwt \
  secret_key=YOUR_JWT_SECRET_KEY_MIN_32_CHARS \
  issuer=autocert-api \
  audience=autocert-app

vault kv put secret/autocert/terraform \
  api_key=YOUR_TERRAFORM_API_KEY \
  workspace=autocert-prod
```

### Vault Agent ConfigMap

**templates/vault-agent-configmap.yaml**
```yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: vault-agent-config
  namespace: {{ .Values.global.namespace }}
data:
  vault-agent-config.hcl: |
    exit_after_auth = true
    
    auto_auth {
      method "kubernetes" {
        mount_path = "auth/kubernetes"
        config = {
          role = "{{ .Values.vault.role }}"
        }
      }
      
      sink "file" {
        config = {
          path = "/vault/secrets/.vault-token"
        }
      }
    }
    
    template {
      source = "/vault/config/db-secret.tpl"
      destination = "/vault/secrets/db-secret.json"
    }
    
    template {
      source = "/vault/config/email-secret.tpl"
      destination = "/vault/secrets/email-secret.json"
    }

  db-secret.tpl: |
    {{`{{- with secret "secret/data/autocert/database" }}
    {
      "username": "{{ .Data.data.username }}",
      "password": "{{ .Data.data.password }}",
      "host": "{{ .Data.data.host }}",
      "port": {{ .Data.data.port }},
      "database": "{{ .Data.data.database }}"
    }
    {{- end }}`}}

  email-secret.tpl: |
    {{`{{- with secret "secret/data/autocert/email" }}
    {
      "smtpServer": "{{ .Data.data.smtp_server }}",
      "smtpPort": {{ .Data.data.smtp_port }},
      "fromAddress": "{{ .Data.data.from_address }}",
      "username": "{{ .Data.data.username }}",
      "password": "{{ .Data.data.password }}"
    }
    {{- end }}`}}
```

### Vault Injector Annotations

In deployment manifests, add these annotations to enable Vault Agent Injector:

```yaml
metadata:
  annotations:
    vault.hashicorp.com/agent-inject: "true"
    vault.hashicorp.com/role: "autocert-role"
    
    # Inject database secret
    vault.hashicorp.com/agent-inject-secret-database: "secret/data/autocert/database"
    vault.hashicorp.com/agent-inject-template-database: |
      {{`{{- with secret "secret/data/autocert/database" -}}
      export DB_USER={{ .Data.data.username }}
      export DB_PASSWORD={{ .Data.data.password }}
      export DB_HOST={{ .Data.data.host }}
      export DB_PORT={{ .Data.data.port }}
      {{- end }}`}}
    
    # Inject email secret
    vault.hashicorp.com/agent-inject-secret-email: "secret/data/autocert/email"
    vault.hashicorp.com/agent-inject-template-email: |
      {{`{{- with secret "secret/data/autocert/email" -}}
      export EMAIL_SMTP_SERVER={{ .Data.data.smtp_server }}
      export EMAIL_USERNAME={{ .Data.data.username }}
      export EMAIL_PASSWORD={{ .Data.data.password }}
      {{- end }}`}}
    
    # Inject JWT secret
    vault.hashicorp.com/agent-inject-secret-jwt: "secret/data/autocert/jwt"
    vault.hashicorp.com/agent-inject-template-jwt: |
      {{`{{- with secret "secret/data/autocert/jwt" -}}
      export JWT_SECRET_KEY={{ .Data.data.secret_key }}
      {{- end }}`}}
```

---

## Configuration Management

### Environment-Specific Configurations

```
configurations/
├── dev/
│   ├── appsettings.development.json
│   ├── vault-config.dev.hcl
│   └── kustomization.yaml
│
├── staging/
│   ├── appsettings.staging.json
│   ├── vault-config.staging.hcl
│   └── kustomization.yaml
│
└── prod/
    ├── appsettings.production.json
    ├── vault-config.prod.hcl
    └── kustomization.yaml
```

### appsettings.Production.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  },
  "Serilog": {
    "MinimumLevel": "Warning",
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
          "path": "/var/log/autocert/autocert-.txt",
          "rollingInterval": "Day",
          "rollingIntervalSize": 52428800,
          "maxRollingFiles": 14,
          "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
        }
      },
      {
        "Name": "Elasticsearch",
        "Args": {
          "nodeUriProvider": "http://elasticsearch:9200",
          "autoRegisterTemplate": true
        }
      }
    ]
  },
  "ConnectionStrings": {
    "DefaultConnection": "User ID=autocert;Password=@DB_PASSWORD;Host=postgresql;Port=5432;Database=autocert_db;Pooling=true;Connection Lifetime=300;Connection Idle Lifetime=60;"
  },
  "CertificateMonitoring": {
    "CheckIntervalMinutes": 1440,
    "RenewalBufferDays": 30,
    "TimeoutSeconds": 30,
    "RetryAttempts": 3
  },
  "EmailNotification": {
    "SmtpServer": "@EMAIL_SMTP_SERVER",
    "SmtpPort": 587,
    "FromAddress": "@EMAIL_FROM_ADDRESS",
    "EnableSsl": true,
    "TimeoutSeconds": 10
  },
  "Jwt": {
    "Issuer": "autocert-api",
    "Audience": "autocert-app",
    "ExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7,
    "SecretKey": "@JWT_SECRET_KEY"
  },
  "Redis": {
    "ConnectionString": "redis:6379,password=@REDIS_PASSWORD",
    "Database": 0,
    "DefaultCacheDuration": 3600
  },
  "Cors": {
    "AllowedOrigins": [
      "https://autocert.yourdomain.com"
    ],
    "AllowedMethods": ["GET", "POST", "PUT", "DELETE"],
    "AllowedHeaders": ["*"],
    "AllowCredentials": true
  }
}
```

---

## Deployment Instructions

### Prerequisites

```bash
# 1. Create namespaces
kubectl create namespace autocert-prod
kubectl create namespace autocert-staging
kubectl create namespace vault
kubectl create namespace monitoring

# 2. Label namespaces for network policies
kubectl label namespace autocert-prod name=autocert-prod
kubectl label namespace vault name=vault

# 3. Install required operators
helm repo add jetstack https://charts.jetstack.io
helm repo add bitnami https://charts.bitnami.com/bitnami
helm repo update

# Install cert-manager
helm install cert-manager jetstack/cert-manager \
  --namespace cert-manager \
  --create-namespace \
  --set installCRDs=true
```

### Deploy Vault

```bash
# Add Vault Helm repository
helm repo add hashicorp https://helm.releases.hashicorp.com
helm repo update

# Install Vault
helm install vault hashicorp/vault \
  --namespace vault \
  --values vault-values.yaml

# Initialize Vault
kubectl exec -it vault-0 -n vault -- vault operator init \
  -key-shares=5 \
  -key-threshold=3

# Unseal Vault (use 3 of 5 keys)
kubectl exec -it vault-0 -n vault -- vault operator unseal [KEY1]
kubectl exec -it vault-0 -n vault -- vault operator unseal [KEY2]
kubectl exec -it vault-0 -n vault -- vault operator unseal [KEY3]

# Store root token securely (this is critical!)
# Save to a secure location, do not commit to git
```

### Deploy AutoCert Application

```bash
# 1. Create container registry secret (for pulling images)
kubectl create secret docker-registry regcred \
  --docker-server=myregistry.azurecr.io \
  --docker-username=<username> \
  --docker-password=<password> \
  -n autocert-prod

# 2. Add AutoCert Helm repository
helm repo add autocert https://charts.autocert.io
helm repo update

# 3. Deploy to production
helm install autocert autocert/autocert \
  --namespace autocert-prod \
  --values values-prod.yaml \
  --set image.registry=myregistry.azurecr.io \
  --set vault.address=https://vault.yourdomain.com

# 4. Verify deployment
kubectl get pods -n autocert-prod
kubectl get services -n autocert-prod
kubectl get ingress -n autocert-prod

# 5. Check logs
kubectl logs -f deployment/autocert-webapp -n autocert-prod
kubectl logs -f deployment/autocert-api -n autocert-prod
```

### Upgrade Application

```bash
# Update Helm chart
helm repo update

# Upgrade release
helm upgrade autocert autocert/autocert \
  --namespace autocert-prod \
  --values values-prod.yaml

# Rollback if needed
helm rollback autocert 1 -n autocert-prod
```

### Post-Deployment Verification

```bash
# Check all resources
kubectl get all -n autocert-prod

# Verify database connectivity
kubectl port-forward svc/postgresql 5432:5432 -n autocert-prod
psql -h localhost -U autocert -d autocert_db

# Verify Vault access from pod
kubectl exec -it deployment/autocert-webapp -n autocert-prod -- \
  curl -H "X-Vault-Token: $VAULT_TOKEN" \
  http://vault.vault.svc.cluster.local:8200/v1/secret/config

# Verify application health
kubectl port-forward svc/autocert-webapp 8080:80 -n autocert-prod
curl http://localhost:8080/health
```

---

## Monitoring & Observability

### ServiceMonitor for Prometheus

**templates/monitoring/servicemonitor.yaml**
```yaml
apiVersion: monitoring.coreos.com/v1
kind: ServiceMonitor
metadata:
  name: autocert-monitor
  namespace: {{ .Values.global.namespace }}
spec:
  selector:
    matchLabels:
      app: autocert
  endpoints:
  - port: metrics
    interval: {{ .Values.monitoring.prometheus.interval }}
    path: /metrics
```

### PrometheusRule for Alerts

**templates/monitoring/prometheusrule.yaml**
```yaml
apiVersion: monitoring.coreos.com/v1
kind: PrometheusRule
metadata:
  name: autocert-alerts
  namespace: {{ .Values.global.namespace }}
spec:
  groups:
  - name: autocert
    interval: 30s
    rules:
    - alert: HighCertificateExpiryRate
      expr: rate(certificates_expiring_soon[5m]) > 10
      for: 5m
      annotations:
        summary: "High certificate expiry rate"
        description: "More than 10 certificates expiring soon"
    
    - alert: CertificateMonitoringDown
      expr: up{job="autocert-monitoring"} == 0
      for: 2m
      annotations:
        summary: "Certificate monitoring service is down"
    
    - alert: APIHighErrorRate
      expr: rate(api_requests_total{status=~"5.."}[5m]) > 0.05
      for: 5m
      annotations:
        summary: "API high error rate (>5%)"
    
    - alert: DatabaseConnectionPoolExhausted
      expr: db_connection_pool_available == 0
      for: 2m
      annotations:
        summary: "Database connection pool exhausted"
```

---

## Troubleshooting

### Common Issues & Solutions

**Issue: Pods not starting due to Vault authentication failure**
```bash
# Check Vault status
kubectl get pods -n vault
kubectl logs vault-0 -n vault

# Verify service account
kubectl get serviceaccount autocert -n autocert-prod
kubectl describe sa autocert -n autocert-prod

# Check RBAC
kubectl get role -n autocert-prod
kubectl get rolebinding -n autocert-prod
```

**Issue: Database connection errors**
```bash
# Verify PostgreSQL is running
kubectl get statefulset postgresql -n autocert-prod
kubectl logs postgresql-0 -n autocert-prod

# Test connection
kubectl run -it --rm debug --image=postgres:15 --restart=Never -- \
  psql -h postgresql.autocert-prod.svc.cluster.local -U autocert -d autocert_db

# Check PVC
kubectl get pvc -n autocert-prod
kubectl describe pvc -n autocert-prod
```

**Issue: ConfigMap or Secret not mounting**
```bash
# Verify ConfigMap exists
kubectl get configmap autocert-config -n autocert-prod
kubectl describe configmap autocert-config -n autocert-prod

# Check volume mounts in pod
kubectl describe pod <pod-name> -n autocert-prod

# Verify the mount path
kubectl exec <pod-name> -n autocert-prod -- ls -la /app/
```

**Issue: Vault Agent not injecting secrets**
```bash
# Check Vault Agent Injector logs
kubectl logs -f daemonset/vault-agent-injector -n vault

# Verify annotations in deployment
kubectl get deployment autocert-api -n autocert-prod -o yaml | grep vault

# Check if pod has init container
kubectl describe pod <pod-name> -n autocert-prod | grep -A 10 "Init Containers"
```

---

**Document Version:** 1.0  
**Last Updated:** February 9, 2026  
**Status:** Production Ready
