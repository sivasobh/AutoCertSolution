# AutoCert - Complete Deployment Checklist & Strategy
## From Development to Production

---

## Pre-Deployment Checklist

### 1. Infrastructure Preparation (Week 1)

- [ ] **Kubernetes Cluster Setup**
  - [ ] AKS/EKS/GKE cluster provisioned (3+ nodes for HA)
  - [ ] Cluster networking configured
  - [ ] Storage classes defined (fast-ssd, standard)
  - [ ] RBAC configured
  - [ ] Network policies defined

- [ ] **External Services**
  - [ ] Domain name registered and DNS configured
  - [ ] SSL/TLS certificates obtained (or cert-manager setup)
  - [ ] Load balancer/ingress controller configured
  - [ ] NAT/Egress routing configured

- [ ] **Monitoring & Logging Stack**
  - [ ] Prometheus installed and configured
  - [ ] Grafana deployed with dashboards
  - [ ] ELK or Loki deployed for centralized logging
  - [ ] AlertManager configured

- [ ] **Security & Secret Management**
  - [ ] HashiCorp Vault deployed and initialized
  - [ ] Vault Kubernetes auth method configured
  - [ ] Vault policies created
  - [ ] Sealed Secrets or External Secrets Operator installed
  - [ ] Pod Security Policies/Standards applied

### 2. Container Registry Setup

- [ ] **Azure Container Registry (ACR) / ECR / Harbor**
  - [ ] Registry created
  - [ ] Repository structure created (webapp, api, automation)
  - [ ] Image pull secrets configured in K8s
  - [ ] Registry cleanup policies configured
  - [ ] Image scanning enabled for vulnerabilities

### 3. Application Preparation

- [ ] **Code Quality**
  - [ ] SonarQube analysis passed (0 critical/major issues)
  - [ ] Unit test coverage > 80%
  - [ ] Integration tests passing
  - [ ] OWASP security scan completed
  - [ ] Dependency vulnerabilities checked (SNYK/Dependabot)

- [ ] **Build Artifacts**
  - [ ] Docker images built successfully
  - [ ] Images tagged with version and latest
  - [ ] Database migrations tested
  - [ ] Configuration files validated
  - [ ] Secrets redacted from images

- [ ] **Documentation**
  - [ ] Helm chart complete and tested
  - [ ] values-prod.yaml configured correctly
  - [ ] README with deployment instructions
  - [ ] Runbook for common operations
  - [ ] Disaster recovery procedures documented

### 4. Vault Configuration

- [ ] **Vault Setup**
  - [ ] Vault cluster initialized and unsealed
  - [ ] HA replication configured (if applicable)
  - [ ] Backups configured
  - [ ] Audit logging enabled

- [ ] **Secret Management**
  - [ ] Database credentials stored
  - [ ] JWT secrets stored
  - [ ] Email service credentials stored
  - [ ] API keys stored
  - [ ] Terraform/cloud provider credentials stored
  - [ ] Secret rotation policies defined

- [ ] **Access Control**
  - [ ] Kubernetes auth method enabled
  - [ ] Service accounts created
  - [ ] Policies bound to service accounts
  - [ ] ACLs configured
  - [ ] Audit logging for secret access enabled

### 5. Database Preparation

- [ ] **PostgreSQL Setup**
  - [ ] Database instances provisioned (primary + replicas)
  - [ ] Backups configured (automated daily)
  - [ ] Connection pooling configured
  - [ ] SSL/TLS configured
  - [ ] Monitoring and alerting configured
  - [ ] Parameter Groups optimized for workload
  - [ ] Extensions installed (if needed)

- [ ] **Database Updates**
  - [ ] Connection strings updated
  - [ ] Initial data loaded
  - [ ] Migration scripts validated
  - [ ] User accounts created
  - [ ] Permissions assigned

### 6. Network Configuration

- [ ] **Networking**
  - [ ] Ingress controller deployed (Nginx/Traefik)
  - [ ] TLS/SSL certificates configured
  - [ ] DNS records pointing to load balancer
  - [ ] Network policies applied
  - [ ] WAF rules configured (if applicable)
  - [ ] DDoS protection enabled (if applicable)

- [ ] **Connectivity**
  - [ ] Private endpoint for databases (if in cloud)
  - [ ] VPN or private connectivity for sensitive access
  - [ ] Firewall rules configured
  - [ ] Egress filtering configured

---

## Deployment Strategy: Blue-Green Deployment

### Blue-Green Architecture

```
Production Environment
├── BLUE (Current - v1.0.0)
│   ├── 3x Web App replicas
│   ├── 3x API replicas
│   ├── 1x Automation worker
│   ├── PostgreSQL (shared)
│   └── Redis (shared)
│
├── GREEN (New - v1.1.0)
│   ├── 3x Web App replicas
│   ├── 3x API replicas
│   ├── 1x Automation worker
│   └── (Same DB & Cache)
│
└── Load Balancer
    └── Routes 100% traffic to BLUE
        (Can switch to GREEN after validation)
```

### Blue-Green Deployment Steps

```bash
#!/bin/bash
# blue-green-deployment.sh

set -e

NAMESPACE="autocert-prod"
ENVIRONMENT="blue"       # Current
NEW_ENVIRONMENT="green"  # New

echo "=== Blue-Green Deployment ==="
echo "Current: $ENVIRONMENT"
echo "Deploying: $NEW_ENVIRONMENT"

# Step 1: Deploy new version
echo "1. Deploying $NEW_ENVIRONMENT environment..."
helm upgrade autocert-$NEW_ENVIRONMENT autocert/autocert \
  --namespace $NAMESPACE \
  --values helm/values-prod.yaml \
  --set image.tag=$NEW_VERSION \
  --create-namespace \
  --wait \
  --timeout 10m

# Step 2: Run smoke tests
echo "2. Running smoke tests against $NEW_ENVIRONMENT..."
curl -f http://autocert-$NEW_ENVIRONMENT:80/health || exit 1
curl -f http://autocert-$NEW_ENVIRONMENT:5000/api/health || exit 1

# Step 3: Validate database migrations
echo "3. Validating database..."
kubectl exec -it deployment/autocert-api-$NEW_ENVIRONMENT -n $NAMESPACE -- \
  dotnet AutoCert.API.dll migrate

# Step 4: Switch traffic
echo "4. Switching traffic from $ENVIRONMENT to $NEW_ENVIRONMENT..."
kubectl patch ingress autocert-ingress -n $NAMESPACE --type merge -p \
  '{"spec":{"rules":[{"http":{"paths":[{"backend":{"service":{"name":"autocert-'"$NEW_ENVIRONMENT"'","port":{"number":80}}}}]}}]}}'

# Step 5: Monitor
echo "5. Monitoring..."
sleep 30
ERROR_RATE=$(kubectl logs deployment/autocert-api-$NEW_ENVIRONMENT -n $NAMESPACE --tail=100 | grep -c "ERROR" || true)

if [ $ERROR_RATE -gt 5 ]; then
  echo "❌ High error rate detected. Rolling back..."
  kubectl patch ingress autocert-ingress -n $NAMESPACE --type merge -p \
    '{"spec":{"rules":[{"http":{"paths":[{"backend":{"service":{"name":"autocert-'"$ENVIRONMENT"'","port":{"number":80}}}}]}}]}}'
  exit 1
fi

# Step 6: Cleanup old environment
echo "6. Cleaning up $ENVIRONMENT..."
helm uninstall autocert-$ENVIRONMENT -n $NAMESPACE || true

echo "✅ Deployment successful!"
```

---

## Canary Deployment Strategy

### Gradual Traffic Shift

```
Istio/Flagger Configuration:

Timeline:
  T+0:    Blue:  100% traffic
          Green: 0% traffic

  T+10m:  Blue:  90% traffic
          Green: 10% traffic
  
  T+20m:  Blue:  75% traffic
          Green: 25% traffic
  
  T+30m:  Blue:  50% traffic
          Green: 50% traffic
  
  T+40m:  Blue:  25% traffic
          Green: 75% traffic

  T+50m:  Blue:  0% traffic
          Green: 100% traffic (Complete)
```

### Canary Deployment YAML

```yaml
apiVersion: flagger.app/v1beta1
kind: Canary
metadata:
  name: autocert-api
  namespace: autocert-prod
spec:
  targetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: autocert-api
  progressDeadlineSeconds: 300
  service:
    port: 5000
    targetPort: 5000
  analysis:
    interval: 1m
    threshold: 5
    maxWeight: 100
    stepWeight: 10
    metrics:
    - name: request-success-rate
      thresholdRange:
        min: 99
      interval: 1m
    - name: request-duration
      thresholdRange:
        max: 500
      interval: 1m
    webhooks:
    - name: smoke-tests
      url: http://flagger-loadtester/
      timeout: 30s
      metadata:
        type: smoke
        cmd: "curl -s http://autocert-api:5000/api/health"
    - name: load-tests
      url: http://flagger-loadtester/
      timeout: 60s
      metadata:
        type: load
        cmd: "hey -z 1m -q 10 -c 2 http://autocert-api:5000/api/health"
```

---

## Rolling Deployment Strategy

### Rolling Update Configuration

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: autocert-api
spec:
  replicas: 3
  strategy:
    type: RollingUpdate
    rollingUpdate:
      maxSurge: 1        # One additional pod during update
      maxUnavailable: 0  # Zero pods unavailable (no downtime)
  template:
    # ... pod template
```

### Deployment Timeline

```
Initial State (3 pods, v1.0.0):
  Pod1[v1.0.0] Pod2[v1.0.0] Pod3[v1.0.0]

Step 1 - Surge (4 pods, max 1 surge):
  Pod1[v1.0.0] Pod2[v1.0.0] Pod3[v1.0.0] Pod4[v1.1.0]

Step 2 - Terminate old:
  Pod1[v1.0.0] Pod2[v1.0.0] Pod3[v1.1.0] Pod4[v1.1.0]

Step 3 - Continue:
  Pod1[v1.0.0] Pod2[v1.1.0] Pod3[v1.1.0] Pod4[v1.1.0]

Final State (3 pods, v1.1.0):
  Pod1[v1.1.0] Pod2[v1.1.0] Pod3[v1.1.0]
  
Total time: ~3-5 minutes (depends on pod startup time and readiness probes)
```

---

## Complete Deployment Workflow

### Phase 1: Preparation (T-1 Day Before Deployment)

```bash
#!/bin/bash
# pre-deployment-validation.sh

echo "=== Pre-Deployment Validation ==="

# 1. Check cluster health
echo "1. Checking cluster health..."
kubectl get nodes -o wide
kubectl get pvc
kubectl get pv

# 2. Verify Vault connectivity
echo "2. Verifying Vault..."
vault status
vault auth list
vault policy list

# 3. Test database connectivity
echo "3. Testing database connections..."
kubectl exec -it postgresql-0 -n autocert-prod -- \
  psql -U autocert -d autocert_db -c "SELECT version();"

# 4. Verify secrets in Vault
echo "4. Verifying Vault secrets..."
vault kv list secret/autocert/
vault kv get secret/autocert/database
vault kv get secret/autocert/jwt

# 5. Build and push new images
echo "5. Building and pushing images..."
docker build -t myregistry.azurecr.io/autocert/api:v1.1.0 ./src/API
docker push myregistry.azurecr.io/autocert/api:v1.1.0

# 6. Validate Helm chart
echo "6. Validating Helm chart..."
helm lint ./helm/autocert
helm template ./helm/autocert --values helm/values-prod.yaml | kubectl apply --dry-run=client -f -

# 7. Create backup
echo "7. Creating database backup..."
kubectl exec postgresql-0 -n autocert-prod -- \
  pg_dump -U autocert autocert_db | \
  gzip > /backups/autocert_db_$(date +%Y%m%d_%H%M%S).sql.gz

echo "✅ All checks passed. Ready for deployment."
```

### Phase 2: Deployment (T-0 Actual Deployment)

```bash
#!/bin/bash
# deploy-production.sh

set -e

NAMESPACE="autocert-prod"
VERSION="v1.1.0"
DEPLOYMENT_ID=$(date +%Y%m%d_%H%M%S)

echo "=== AutoCert Production Deployment ==="
echo "Version: $VERSION"
echo "Timestamp: $DEPLOYMENT_ID"
echo "Namespace: $NAMESPACE"

# Step 1: Create deployment record
echo "Creating deployment record..."
cat > deployment_$DEPLOYMENT_ID.log << EOF
Deployment ID: $DEPLOYMENT_ID
Version: $VERSION
Started: $(date -u +%Y-%m-%dT%H:%M:%SZ)
Namespace: $NAMESPACE
EOF

# Step 2: Deploy to production
echo "Deploying to production..."
helm upgrade autocert autocert/autocert \
  --namespace $NAMESPACE \
  --values helm/values-prod.yaml \
  --values helm/values-prod-secrets.yaml \
  --set image.webapp.tag=$VERSION \
  --set image.api.tag=$VERSION \
  --set image.automation.tag=$VERSION \
  --wait \
  --timeout 15m \
  --atomic

# Step 3: Wait for rollout
echo "Waiting for rollout..."
kubectl rollout status deployment/autocert-api -n $NAMESPACE --timeout=5m
kubectl rollout status deployment/autocert-webapp -n $NAMESPACE --timeout=5m
kubectl rollout status deployment/autocert-automation -n $NAMESPACE --timeout=5m

# Step 4: Verify deployment
echo "Verifying deployment..."
kubectl get deployments -n $NAMESPACE -o wide
kubectl get pods -n $NAMESPACE
kubectl get services -n $NAMESPACE

# Step 5: Run smoke tests
echo "Running smoke tests..."
WEBAPP_POD=$(kubectl get pod -n $NAMESPACE -l component=webapp -o jsonpath='{.items[0].metadata.name}')
API_POD=$(kubectl get pod -n $NAMESPACE -l component=api -o jsonpath='{.items[0].metadata.name}')

kubectl exec $WEBAPP_POD -n $NAMESPACE -- curl -f http://localhost/health
kubectl exec $API_POD -n $NAMESPACE -- curl -f http://localhost:5000/api/health

# Step 6: Check logs for errors
echo "Checking logs for errors..."
ERROR_COUNT=$(kubectl logs deployment/autocert-api -n $NAMESPACE --tail=100 | grep -c "ERROR" || echo "0")
if [ $ERROR_COUNT -gt 10 ]; then
  echo "⚠️  WARNING: High error count detected: $ERROR_COUNT"
fi

# Step 7: Verify database migrations
echo "Verifying database..."
kubectl exec $API_POD -n $NAMESPACE -- \
  dotnet AutoCert.API.dll database:migrate || true

# Step 8: Update deployment record
echo "Deployment completed successfully!"
cat >> deployment_$DEPLOYMENT_ID.log << EOF
Completed: $(date -u +%Y-%m-%dT%H:%M:%SZ)
Status: SUCCESS
EOF

echo "✅ Deployment successful!"
echo "📋 Deployment log: deployment_$DEPLOYMENT_ID.log"
```

### Phase 3: Post-Deployment (T+0 After Deployment)

```bash
#!/bin/bash
# post-deployment-validation.sh

NAMESPACE="autocert-prod"
DURATION=3600  # Monitor for 1 hour

echo "=== Post-Deployment Monitoring ==="
echo "Monitoring duration: $DURATION seconds"

START_TIME=$(date +%s)
END_TIME=$((START_TIME + DURATION))

while [ $(date +%s) -lt $END_TIME ]; do
  echo "--- Status Check at $(date) ---"
  
  # Check pod health
  echo "Pod Status:"
  kubectl get pods -n $NAMESPACE --no-headers | awk '{print $1, $3, $4, $5}'
  
  # Check error rates
  echo "Error Rates (last 5 min):"
  kubectl logs deployment/autocert-api -n $NAMESPACE --tail=500 2>/dev/null | \
    grep -c "ERROR" || echo "0"
  
  # Check resource usage
  echo "Resource Usage:"
  kubectl top pods -n $NAMESPACE --no-headers || echo "Metrics not available"
  
  # Check database connection
  echo "Database Status:"
  kubectl exec -it postgresql-0 -n $NAMESPACE -- \
    pg_isready -U autocert || echo "DB check failed"
  
  # Wait 5 minutes before next check
  echo "Waiting 5 minutes..."
  sleep 300
done

echo "✅ Monitoring completed. Deployment stable."
```

---

## Rollback Procedures

### Quick Rollback (Last Known Good)

```bash
#!/bin/bash
# rollback.sh

NAMESPACE="autocert-prod"
RELEASE_NAME="autocert"

echo "=== Emergency Rollback ==="

# Get previous revision
PREVIOUS_REVISION=$(helm history $RELEASE_NAME -n $NAMESPACE | grep "deployed" | tail -1 | awk '{print $1}')

echo "Rolling back to revision: $PREVIOUS_REVISION"

# Perform rollback
helm rollback $RELEASE_NAME $PREVIOUS_REVISION -n $NAMESPACE --wait

# Verify rollback
kubectl get pods -n $NAMESPACE
helm list -n $NAMESPACE

echo "✅ Rollback completed"
```

### Database Rollback

```bash
#!/bin/bash
# db-rollback.sh

BACKUP_FILE="/backups/autocert_db_20260209_150000.sql.gz"

echo "=== Database Rollback ==="
echo "Restoring from: $BACKUP_FILE"

# Create restore job
kubectl exec -it postgresql-0 -n autocert-prod << EOF
  gunzip < $BACKUP_FILE | psql -U autocert -d autocert_db
EOF

echo "✅ Database restored"
```

---

## Monitoring Dashboard Checks

### Key Metrics to Monitor Post-Deployment

```
Application Metrics:
├── API Response Time (p50, p95, p99)
├── Error Rate (5xx, 4xx)
├── Request Throughput
├── Database Query Time
└── Cache Hit Ratio

Infrastructure Metrics:
├── CPU Usage per Pod
├── Memory Usage per Pod
├── Network I/O
├── Disk I/O
└── Pod Restart Count

Business Metrics:
├── Certificate Checks Completed
├── Renewal Attempts vs Success
├── Alert Notifications Sent
├── User Logins
└── API Calls per Tenant
```

### Example Prometheus Queries

```promql
# API Error Rate
rate(http_requests_total{status=~"5.."}[5m])

# P95 Response Time
histogram_quantile(0.95, http_request_duration_seconds)

# Pod CPU Usage
container_cpu_usage_seconds_total

# Memory Usage Percentage
(container_memory_usage_bytes / container_spec_memory_limit_bytes) * 100

# Certificate Monitoring Job Duration
job_certificate_check_duration_seconds

# Database Connection Pool Usage
pg_connection_pool_available
```

---

## Communication Plan

### Deployment Notification

```
Subject: AutoCert v1.1.0 Production Deployment

Stakeholders: [DevOps, SRE, Product, Support]

Timeline:
- T-1 day: Notification of scheduled deployment
- T-0: Deployment starts (13:00 UTC)
- T+15min: Deployment complete, smoke tests running
- T+1hr: Monitoring stable, full rollout verification
- T+EOD: Final reports and metrics

Status Page: https://status.autocert.com
Slack Channel: #autocert-deployments
On-call Engineer: [Name]

Rollback Plan: If critical issues detected, will rollback to v1.0.0
Estimated rollback time: 5-10 minutes
```

---

## Success Criteria

Deployment is considered successful when:

- ✅ All pods are in Running state
- ✅ All readiness/liveness probes passing
- ✅ Error rate < 0.5% over 5 minutes
- ✅ API response time p95 < 500ms
- ✅ Database connectivity confirmed
- ✅ Zero critical/major alerts
- ✅ Smoke tests passing
- ✅ No increase in pod restart count
- ✅ All environment variables properly set
- ✅ Vault secrets accessible from pods

---

**Document Version:** 1.0  
**Last Updated:** February 9, 2026  
**Status:** Production Ready
