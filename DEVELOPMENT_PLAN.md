# AutoCert - Certificate Monitoring & Renewal Platform
## Complete Development Plan

**Project Overview:** A comprehensive, multi-tenant certificate monitoring and renewal platform using .NET Core 10 with automated certificate renewal capabilities.

---

## Table of Contents
1. [System Architecture](#system-architecture)
2. [Technology Stack](#technology-stack)
3. [Database Design](#database-design)
4. [API Specifications](#api-specifications)
5. [Frontend Structure](#frontend-structure)
6. [Authentication & Authorization](#authentication--authorization)
7. [Feature Breakdown](#feature-breakdown)
8. [Deployment Strategy](#deployment-strategy)
9. [Development Roadmap](#development-roadmap)

---

## System Architecture

### High-Level Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                         Internet                                 │
└─────────────────┬───────────────────────────────────────────────┘
                  │
          ┌───────▼────────┐
          │  Load Balancer │
          └────────┬────────┘
                   │
    ┌──────────────┼──────────────┐
    │              │              │
┌───▼───┐      ┌──▼──┐       ┌──▼──┐
│ Web   │      │Web  │       │Web  │
│ App   │      │App  │       │App  │
│ (Replica 1)  │(Replica 2)  │(Replica N)
└───┬───┘      └──┬──┘       └──┬──┘
    │             │             │
    └─────────────┼─────────────┘
                  │
    ┌─────────────┼──────────────────┐
    │             │                  │
┌──▼──┐      ┌──▼──┐            ┌──▼──┐
│API  │      │API  │            │API  │
│Replica 1   │Replica 2         │Replica N
└──┬──┘      └──┬──┘            └──┬──┘
    │           │                  │
    └───────────┼──────────────────┘
                │
    ┌───────────┼────────────────┐
    │           │                │
┌───▼─────┐ ┌──▼──┐ ┌──────┐ ┌──▼──────┐
│PostgreSQL│ │Redis│ │RabbitMQ│ │Vault│
│(Multi-   │ │Cache│ │(Message)│ │(Secrets)
│tenant)   │ │     │ │Queue    │ │
└──────────┘ └─────┘ └────────┘ └────────┘

    ┌────────────────────────────────┐
    │ Background Services            │
    ├┬───────────────────────────────┤
    ││ Certificate Monitor Service    │
    ││ - Checks certificate expiry    │
    ││ - Updates status (G/Y/R)       │
    │├────────────────────────────────┤
    ││ Email Service                  │
    ││ - Sends notifications          │
    ││ - Template management          │
    │├────────────────────────────────┤
    ││ Certificate Renewal Service    │
    ││ - Executes renewals            │
    ││ - Logs renewal events          │
    │└─────────────────────────────────┘
```

### Service Components

```
WebApp (Presentation Layer)
├── Landing Page
├── Authentication (OAuth2/SSO/Username-Password)
├── Dashboard
│   ├── Tenant Selection
│   ├── API Certificate Status Overview
│   └── Statistics & Analytics
├── API Management
│   ├── Add/Edit/Delete APIs
│   ├── Configure Monitoring
│   └── Set Alert Thresholds
├── Certificate Management
│   ├── View Certificate Details
│   ├── Renewal History
│   └── Manual Renewal Trigger
└── Settings
    ├── Email Configuration
    ├── Alert Rules
    └── User Management

API (Business Logic Layer)
├── Authentication Service
├── Tenant Service
├── API Management Service
├── Certificate Service
├── Monitoring Service
├── Email Service
├── Renewal Service
└── Analytics Service

Background Services
├── Certificate Monitor (Scheduled)
├── Email Service (Queue-based)
└── Renewal Automation (Scheduled/Event-based)

Database (PostgreSQL)
├── Users & Authentication
├── Tenants & Access Control
├── APIs & Endpoints
├── Certificates & History
├── Alert Rules & Thresholds
└── Audit Logs
```

---

## Technology Stack

### Backend
```
Framework:          .NET Core 10
Language:           C#
ORM:                Entity Framework Core 10
API:                ASP.NET Core REST API
Background Jobs:    Hangfire / Quartz.NET
Message Queue:      RabbitMQ
Caching:            Redis
Email:              SendGrid / SMTP
Secret Management:  Azure Key Vault / Vault
Logging:            Serilog + ELK Stack
Testing:            xUnit + Moq
```

### Frontend
```
Framework:          Blazor / Angular / React
Language:           C# / TypeScript / JavaScript
Authentication:     OIDC / OAuth2
UI Components:      Bootstrap / Material UI
State Management:   Redux / NgRx / Blazor State
HTTP Client:        HttpClient / Axios
Charts:             Chart.js / ApexCharts
```

### Infrastructure
```
Containerization:   Docker & Docker Compose
Orchestration:      Kubernetes / Docker Swarm
IaC:                Terraform / Pulumi
CI/CD:              GitHub Actions / Azure DevOps
Cloud:              Azure / AWS / On-Premises
Monitoring:         Prometheus + Grafana
Logging:            ELK Stack (Elasticsearch, Logstash, Kibana)
```

### Open-Source Database Stack
```
Primary:            PostgreSQL 16+ (RDBMS)
Cache:              Redis 7+ (In-memory data store)
Message Queue:      RabbitMQ 3.12+ (Async processing)
Search (Optional):  Elasticsearch 8+ (Audit logs)
```

---

## Database Design

### Entity-Relationship Diagram

```
┌─────────────────┐     ┌──────────────┐     ┌──────────────┐
│     Users       │────▶│   Tenants    │◀────│  ApiRecords  │
│                 │     │              │     │              │
│ - UserId (PK)   │     │ - TenantId   │     │ - ApiRecordId│
│ - Email         │     │ - Name       │     │ - TenantId   │
│ - PasswordHash  │     │ - CreatedAt  │     │ - Url        │
│ - SSO Provider  │     └──────┬───────┘     │ - Port       │
│ - CreatedAt     │            │             │ - Protocol   │
└────────┬────────┘            │             └──────┬───────┘
         │                     │                    │
         │            ┌────────▼─────────┐          │
         │            │ TenantUsers      │          │
         │            │ (Many-to-Many)   │          │
         │            │                  │          │
         │            │ - UserId (FK)    │          │
         │            │ - TenantId (FK)  │          │
         │            │ - Role           │          │
         │            │ - Access Level   │          │
         │            └──────────────────┘          │
         │                                           │
         └─────────────────────────────────────────┐ │
                                                   │ │
                    ┌──────────────────────────────┘ │
                    │                                │
         ┌─────────▼─────────┐          ┌───────────▼─────┐
         │   AlertRules      │          │   Certificates  │
         │                   │          │                 │
         │ - RuleId (PK)     │          │ - CertId (PK)   │
         │ - TenantId (FK)   │          │ - ApiRecordId   │
         │ - ApiRecordId     │          │ - Thumbprint    │
         │ - ThresholdDays   │          │ - IssuedDate    │
         │ - AlertType       │          │ - ExpiryDate    │
         │ - EmailRecipients │          │ - Status (G/Y/R)│
         │ - IsActive        │          │ - CertificateData
         │                   │          │ - LastChecked   │
         │                   │          │ - RenewalAttempt│
         └───────────────────┘          └─────────────────┘
                                               ▲
                                               │
                                        ┌──────┴─────────┐
                                        │                │
                            ┌───────────▼────────┐ ┌─────▼──────────┐
                            │ CertificateHistory │ │ RenewalEvents  │
                            │                    │ │                │
                            │ - HistoryId (PK)   │ │ - EventId      │
                            │ - CertId (FK)      │ │ - CertId       │
                            │ - EventType        │ │ - EventDate    │
                            │ - OldStatus        │ │ - Status       │
                            │ - NewStatus        │ │ (PENDING/      │
                            │ - ChangedAt        │ │  SUCCESS/FAILED)
                            │ - ChangedBy        │ │ - ErrorMessage │
                            │                    │ │ - RenewalMethod│
                            └────────────────────┘ │ (MANUAL/AUTO)  │
                                                   │ - NewExpiry    │
                                                   └────────────────┘

         ┌──────────────────┐          ┌────────────────────┐
         │  EmailLogs       │          │  AuditLogs         │
         │                  │          │                    │
         │ - LogId (PK)     │          │ - LogId (PK)       │
         │ - RecipientEmail │          │ - UserId (FK)      │
         │ - Subject        │          │ - TenantId (FK)    │
         │ - Body           │          │ - Action           │
         │ - SentAt         │          │ - EntityType       │
         │ - Status         │          │ - EntityId         │
         │ - ErrorMessage   │          │ - OldValues        │
         │ - ApiRecordId    │          │ - NewValues        │
         │ - CertId (FK)    │          │ - Timestamp        │
         │                  │          │ - IpAddress        │
         │                  │          │                    │
         └──────────────────┘          └────────────────────┘
```

### Core Tables

```sql
-- Users Table
CREATE TABLE users (
    user_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    email VARCHAR(255) UNIQUE NOT NULL,
    password_hash VARCHAR(255),
    first_name VARCHAR(100),
    last_name VARCHAR(100),
    sso_provider VARCHAR(50),
    sso_id VARCHAR(255),
    is_active BOOLEAN DEFAULT true,
    last_login TIMESTAMP,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Tenants Table (Multi-tenant support)
CREATE TABLE tenants (
    tenant_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(255) NOT NULL,
    description TEXT,
    owner_id UUID REFERENCES users(user_id),
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Tenant Users (Access Control)
CREATE TABLE tenant_users (
    tenant_user_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID REFERENCES tenants(tenant_id) ON DELETE CASCADE,
    user_id UUID REFERENCES users(user_id) ON DELETE CASCADE,
    role VARCHAR(50), -- ADMIN, MANAGER, VIEWER
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UNIQUE(tenant_id, user_id)
);

-- API Records Table
CREATE TABLE api_records (
    api_record_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID REFERENCES tenants(tenant_id) ON DELETE CASCADE,
    name VARCHAR(255) NOT NULL,
    url VARCHAR(2048) NOT NULL,
    port INTEGER,
    protocol VARCHAR(10), -- HTTP, HTTPS
    description TEXT,
    is_monitored BOOLEAN DEFAULT true,
    check_interval_minutes INTEGER DEFAULT 1440,
    created_by UUID REFERENCES users(user_id),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Certificates Table
CREATE TABLE certificates (
    cert_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    api_record_id UUID REFERENCES api_records(api_record_id) ON DELETE CASCADE,
    tenant_id UUID REFERENCES tenants(tenant_id) ON DELETE CASCADE,
    thumbprint VARCHAR(255) UNIQUE,
    subject VARCHAR(255),
    issuer VARCHAR(255),
    issued_date DATE,
    expiry_date DATE NOT NULL,
    certificate_data TEXT, -- PEM format
    status VARCHAR(10), -- GREEN, YELLOW, RED
    last_checked TIMESTAMP,
    renewal_attempted_at TIMESTAMP,
    renewal_status VARCHAR(50),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_expiry_date (expiry_date),
    INDEX idx_status (status),
    INDEX idx_tenant_id (tenant_id)
);

-- Alert Rules Table
CREATE TABLE alert_rules (
    rule_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID REFERENCES tenants(tenant_id) ON DELETE CASCADE,
    api_record_id UUID REFERENCES api_records(api_record_id) ON DELETE CASCADE,
    name VARCHAR(255),
    threshold_days INTEGER, -- Days before expiry to trigger alert
    alert_type VARCHAR(50), -- GREEN, YELLOW, RED
    email_recipients VARCHAR(1000),
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Certificate History
CREATE TABLE certificate_history (
    history_id BIGSERIAL PRIMARY KEY,
    cert_id UUID REFERENCES certificates(cert_id) ON DELETE CASCADE,
    event_type VARCHAR(50), -- CREATED, CHECKED, RENEWED, EXPIRED
    old_status VARCHAR(10),
    new_status VARCHAR(10),
    old_expiry_date DATE,
    new_expiry_date DATE,
    changed_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    changed_by UUID REFERENCES users(user_id)
);

-- Renewal Events
CREATE TABLE renewal_events (
    event_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    cert_id UUID REFERENCES certificates(cert_id) ON DELETE CASCADE,
    event_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    status VARCHAR(50), -- PENDING, SUCCESS, FAILED
    renewal_method VARCHAR(50), -- MANUAL, AUTO, TERRAFORM, PYTHON
    error_message TEXT,
    new_cert_data TEXT,
    executed_by UUID REFERENCES users(user_id),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Email Logs
CREATE TABLE email_logs (
    log_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    api_record_id UUID REFERENCES api_records(api_record_id),
    cert_id UUID REFERENCES certificates(cert_id),
    recipient_email VARCHAR(255),
    subject VARCHAR(255),
    body TEXT,
    status VARCHAR(50), -- SENT, FAILED, BOUNCED
    error_message TEXT,
    sent_at TIMESTAMP,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Audit Logs
CREATE TABLE audit_logs (
    log_id BIGSERIAL PRIMARY KEY,
    user_id UUID REFERENCES users(user_id),
    tenant_id UUID REFERENCES tenants(tenant_id),
    action VARCHAR(255), -- CREATE, UPDATE, DELETE, LOGIN, etc
    entity_type VARCHAR(100),
    entity_id VARCHAR(255),
    old_values JSONB,
    new_values JSONB,
    timestamp TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    ip_address VARCHAR(45)
);

-- Create Indexes for Performance
CREATE INDEX idx_api_records_tenant ON api_records(tenant_id);
CREATE INDEX idx_tenant_users_user ON tenant_users(user_id);
CREATE INDEX idx_certificates_api ON certificates(api_record_id);
CREATE INDEX idx_alert_rules_tenant ON alert_rules(tenant_id);
CREATE INDEX idx_renewal_events_cert ON renewal_events(cert_id);
CREATE INDEX idx_email_logs_api ON email_logs(api_record_id);
CREATE INDEX idx_audit_logs_tenant ON audit_logs(tenant_id);
CREATE INDEX idx_audit_logs_timestamp ON audit_logs(timestamp);
```

---

## API Specifications

### RESTful API Endpoints

#### Authentication Endpoints
```
POST   /api/v1/auth/register          - Register new user
POST   /api/v1/auth/login             - Login with credentials
POST   /api/v1/auth/refresh           - Refresh JWT token
POST   /api/v1/auth/logout            - Logout user
POST   /api/v1/auth/sso-callback      - OAuth2/SSO callback
GET    /api/v1/auth/profile           - Get current user profile
PUT    /api/v1/auth/profile           - Update user profile
```

#### Tenant Management
```
GET    /api/v1/tenants                - List user's tenants
POST   /api/v1/tenants                - Create new tenant
GET    /api/v1/tenants/:tenantId      - Get tenant details
PUT    /api/v1/tenants/:tenantId      - Update tenant
DELETE /api/v1/tenants/:tenantId      - Delete tenant
GET    /api/v1/tenants/:tenantId/users - List tenant users
POST   /api/v1/tenants/:tenantId/users - Add user to tenant
PUT    /api/v1/tenants/:tenantId/users/:userId - Update user role
DELETE /api/v1/tenants/:tenantId/users/:userId - Remove user from tenant
```

#### API Records Management
```
GET    /api/v1/tenants/:tenantId/apis         - List APIs
POST   /api/v1/tenants/:tenantId/apis         - Create API record
GET    /api/v1/tenants/:tenantId/apis/:apiId  - Get API details
PUT    /api/v1/tenants/:tenantId/apis/:apiId  - Update API
DELETE /api/v1/tenants/:tenantId/apis/:apiId  - Delete API
POST   /api/v1/tenants/:tenantId/apis/:apiId/check - Manual certificate check
```

#### Certificate Management
```
GET    /api/v1/tenants/:tenantId/certificates     - List certificates
GET    /api/v1/tenants/:tenantId/apis/:apiId/cert - Get API's certificate
GET    /api/v1/tenants/:tenantId/certs/:certId    - Get certificate details
POST   /api/v1/tenants/:tenantId/certs/:certId/renew - Trigger renewal
GET    /api/v1/tenants/:tenantId/certs/:certId/history - Certificate history
```

#### Alert Rules
```
GET    /api/v1/tenants/:tenantId/alerts          - List alert rules
POST   /api/v1/tenants/:tenantId/alerts          - Create alert rule
GET    /api/v1/tenants/:tenantId/alerts/:ruleId  - Get alert rule
PUT    /api/v1/tenants/:tenantId/alerts/:ruleId  - Update alert rule
DELETE /api/v1/tenants/:tenantId/alerts/:ruleId  - Delete alert rule
```

#### Renewal Operations
```
POST   /api/v1/tenants/:tenantId/certs/:certId/renew-terraform - Terraform renewal
POST   /api/v1/tenants/:tenantId/certs/:certId/renew-python    - Python renewal
GET    /api/v1/tenants/:tenantId/renewal-events  - List renewal events
GET    /api/v1/tenants/:tenantId/renewal-events/:eventId - Get renewal event
```

#### Dashboard & Analytics
```
GET    /api/v1/tenants/:tenantId/dashboard       - Dashboard overview
GET    /api/v1/tenants/:tenantId/statistics      - Certificate statistics
GET    /api/v1/tenants/:tenantId/cert-status     - Certificate status summary
GET    /api/v1/tenants/:tenantId/alerts-summary  - Alerts summary
```

#### Audit & Logs
```
GET    /api/v1/tenants/:tenantId/audit-logs      - Audit logs
GET    /api/v1/tenants/:tenantId/email-logs      - Email logs
```

### Response Format

```json
{
  "success": true,
  "statusCode": 200,
  "message": "Operation successful",
  "data": {
    // Paginated response
    "items": [],
    "pageNumber": 1,
    "pageSize": 10,
    "totalRecords": 0,
    "totalPages": 0
  },
  "timestamp": "2026-02-09T10:30:00Z"
}
```

### Error Format

```json
{
  "success": false,
  "statusCode": 400,
  "message": "Validation failed",
  "errors": [
    {
      "field": "email",
      "message": "Invalid email format"
    }
  ],
  "timestamp": "2026-02-09T10:30:00Z"
}
```

---

## Frontend Structure

### Web Application Architecture

```
webapp/
├── src/
│   ├── pages/
│   │   ├── Index.razor (Landing Page)
│   │   ├── Login.razor
│   │   ├── Register.razor
│   │   ├── Dashboard.razor
│   │   ├── Tenants/
│   │   │   ├── List.razor
│   │   │   ├── Create.razor
│   │   │   └── Edit.razor
│   │   ├── APIs/
│   │   │   ├── List.razor
│   │   │   ├── Detail.razor
│   │   │   ├── Create.razor
│   │   │   └── Edit.razor
│   │   ├── Certificates/
│   │   │   ├── List.razor
│   │   │   ├── Detail.razor
│   │   │   └── RenewalsHistory.razor
│   │   ├── Alerts/
│   │   │   ├── List.razor
│   │   │   └── Create.razor
│   │   ├── Settings/
│   │   │   ├── EmailConfig.razor
│   │   │   ├── UserManagement.razor
│   │   │   └── TenantSettings.razor
│   │   └── Profile/
│   │       └── MyProfile.razor
│   │
│   ├── components/
│   │   ├── Layout/
│   │   │   ├── MainLayout.razor
│   │   │   ├── NavMenu.razor
│   │   │   └── Footer.razor
│   │   ├── Shared/
│   │   │   ├── AlertBox.razor
│   │   │   ├── Modal.razor
│   │   │   ├── LoadingSpinner.razor
│   │   │   └── Pagination.razor
│   │   ├── Dashboard/
│   │   │   ├── CertificateStatusCard.razor
│   │   │   ├── StatusChart.razor
│   │   │   └── AlertsSummary.razor
│   │   └── Certificate/
│   │       ├── CertificateStatusBadge.razor (Green/Yellow/Red)
│   │       ├── CertificateCard.razor
│   │       └── ExpiryTimeline.razor
│   │
│   ├── services/
│   │   ├── AuthService.cs
│   │   ├── TenantService.cs
│   │   ├── ApiRecordService.cs
│   │   ├── CertificateService.cs
│   │   ├── AlertService.cs
│   │   ├── HttpClientFactory.cs
│   │   └── StorageService.cs (Local/Session storage)
│   │
│   ├── models/
│   │   ├── Auth/
│   │   ├── Tenant/
│   │   ├── Api/
│   │   ├── Certificate/
│   │   ├── Alert/
│   │   └── Dashboard/
│   │
│   ├── App.razor
│   ├── Program.cs
│   └── appsettings.json
│
└── wwwroot/
    ├── css/
    │   ├── bootstrap.css
    │   └── app.css
    ├── js/
    │   └── app.js
    └── images/
```

### Landing Page Components
```
Landing Page
├── Hero Section
│   ├── Main Title: "AutoCert - Certificate Management Made Simple"
│   ├── Subtitle: "Monitor, Alert, and Renew Your Certificates Automatically"
│   └── CTA Buttons: [Login] [Sign Up]
│
├── Features Section
│   ├── Real-time Monitoring
│   │   └── Icon + Description
│   ├── Automatic Renewal
│   │   └── Icon + Description
│   ├── Multi-tenant Support
│   │   └── Icon + Description
│   ├── Email Alerts
│   │   └── Icon + Description
│   └── REST API
│       └── Icon + Description
│
├── Status Indicator Demo
│   ├── Green Badge (Valid)
│   ├── Yellow Badge (Expiring)
│   └── Red Badge (Expired)
│
├── Authentication Options
│   ├── Email/Password Form
│   ├── SSO Options (Google, Azure AD, GitHub)
│   └── Sign Up Link
│
├── Benefits Section
│   ├── "Never Miss Certificate Expiry Again"
│   ├── "Automated Renewal Saves Time"
│   ├── "Easy Multi-tenant Management"
│   └── "Detailed Audit Trail"
│
└── Footer
    ├── Links
    ├── Contact
    └── Legal
```

### Dashboard Components
```
Dashboard
├── Header
│   ├── Tenant Selector (Dropdown)
│   ├── User Menu
│   └── Notifications Badge
│
├── Main Content
│   ├── Overview Cards
│   │   ├── Total APIs: [Count]
│   │   ├── Valid Certificates: [Count] (Green)
│   │   ├── Expiring Soon: [Count] (Yellow)
│   │   └── Expired: [Count] (Red)
│   │
│   ├── Certificate Status Chart
│   │   └── Pie/Donut chart showing G/Y/R distribution
│   │
│   ├── Expiry Timeline
│   │   ├── Upcoming Expirations (next 30 days)
│   │   └── Sorted by expiry date
│   │
│   ├── Recent Alerts
│   │   ├── Alert Type: [Type]
│   │   ├── API: [Name]
│   │   ├── Days to Expiry: [X]
│   │   └── Action: View | Renew
│   │
│   └── Quick Actions
│       ├── Add New API
│       ├── Check All Certificates
│       ├── Configure Alerts
│       └── View Reports
│
└── Sidebar
    ├── Dashboard
    ├── APIs
    ├── Certificates
    ├── Alerts
    ├── Renewals
    ├── Reports
    └── Settings
```

---

## Authentication & Authorization

### User Authentication Flows

#### Username/Password Flow
```
User
  │
  ├─ Enter Email/Password
  │
  ▼
API /auth/login
  │
  ├─ Validate credentials
  ├─ Check user active status
  ├─ Verify MFA (if enabled)
  │
  ▼
Response
  │
  ├─ Valid: Return JWT Token + Refresh Token
  │           Set HttpOnly Cookies
  │           Redirect to Dashboard
  │
  └─ Invalid: Return 401 Unauthorized
              Show error message
```

#### SSO (OAuth2) Flow
```
User clicks "Login with [Google/Azure AD/GitHub]"
  │
  ▼
Redirect to OAuth Provider
  │
  ▼
User Authenticates with Provider
  │
  ▼
Provider Redirects to /auth/sso-callback
  │
  ├─ Validate callback
  ├─ Create or update user
  ├─ Create JWT tokens
  │
  ▼
Redirect to Dashboard
```

### Authorization Model

```
Role-Based Access Control (RBAC)
├── Admin
│   ├── Create/Delete Tenants
│   ├── Manage All Users
│   ├── Configure System Settings
│   ├── Access All APIs
│   ├── Configure Renewal Methods
│   └── View All Audit Logs
│
├── Manager
│   ├── Create/Edit APIs
│   ├── Configure Alerts
│   ├── Trigger Manual Renewals
│   ├── Manage Team Members
│   ├── View Reports
│   └── View Audit Logs
│
└── Viewer
    ├── View APIs (Read-only)
    ├── View Certificates (Read-only)
    ├── View Alerts (Read-only)
    └── View Reports (Read-only)
```

### JWT Token Structure
```
Header
{
  "alg": "HS256",
  "typ": "JWT"
}

Payload
{
  "sub": "user-id",
  "email": "user@example.com",
  "tenants": ["tenant-id-1", "tenant-id-2"],
  "roles": ["Admin"],
  "iat": 1644364200,
  "exp": 1644367800,
  "iss": "autocert-api",
  "aud": "autocert-app"
}

Signature
HMACSHA256(base64UrlEncode(header) + "." + base64UrlEncode(payload), secret)
```

---

## Feature Breakdown

### Phase 1: Core Functionality (MVP)

#### Week 1-2: Project Setup & Authentication
- [x] Project scaffolding (.NET 10)
- [x] Docker & Compose setup
- [x] Database schema & migrations
- [x] User registration & login (email/password)
- [x] JWT authentication
- [x] Role-based access control
- [x] Landing page

#### Week 3-4: Multi-tenant Infrastructure
- [x] Tenant management (CRUD)
- [x] Tenant-user relationships
- [x] Tenant isolation (data filtering)
- [x] Audit logging

#### Week 5-6: API Certificate Management
- [ ] API record management (CRUD)
- [ ] SSL/TLS certificate extraction
- [ ] Certificate status determination (G/Y/R)
- [ ] Certificate storage

#### Week 7-8: Monitoring & Alerts
- [ ] Scheduled certificate checking (Background service)
- [ ] Alert rule creation & management
- [ ] Email notification service
- [ ] Alert dashboard

#### Week 9-10: Web Dashboard
- [ ] Responsive dashboard UI
- [ ] Certificate status visualization (G/Y/R badges)
- [ ] API list with status indicator
- [ ] Quick actions
- [ ] Responsive design

### Phase 2: Automation & Renewal

#### Week 11-12: Manual Renewal (Demo)
- [ ] Manual renewal trigger UI
- [ ] Renewal event logging
- [ ] Renewal history tracking
- [ ] Success/failure notifications

#### Week 13-14: Terraform Integration
- [ ] Terraform module for certificate renewal
- [ ] Terraform state management
- [ ] Automated Terraform execution
- [ ] Output parsing & certificate update

#### Week 15-16: Python Integration
- [ ] Python script for certificate renewal
- [ ] Python dependency management
- [ ] Automated script execution
- [ ] Error handling & logging

### Phase 3: Advanced Features

#### Week 17-18: SSO Integration
- [ ] OAuth2 setup (Google, Azure AD, GitHub)
- [ ] SSO callback handling
- [ ] Auto user provisioning

#### Week 19-20: Advanced Analytics
- [ ] Certificate expiry trends
- [ ] Renewal success rate
- [ ] Alert frequency analysis
- [ ] Reports generation (PDF/Excel)

#### Week 21-22: API Enhancements
- [ ] API documentation (Swagger/OpenAPI)
- [ ] API versioning
- [ ] Rate limiting
- [ ] Webhooks for cert renewal events

#### Week 23-24: Kubernetes Orchestration
- [ ] Helm charts
- [ ] Auto-scaling configuration
- [ ] Health checks
- [ ] Resource quotas

### Phase 4: Production Ready

#### Week 25-26: Security Hardening
- [ ] Secret management (Vault)
- [ ] SSL/TLS endpoint encryption
- [ ] Input validation & sanitization
- [ ] OWASP vulnerability scanning
- [ ] Penetration testing

#### Week 27-28: Performance Optimization
- [ ] Database query optimization
- [ ] Caching strategy (Redis)
- [ ] API response compression
- [ ] Load testing

#### Week 29-30: Monitoring & Observability
- [ ] Prometheus metrics
- [ ] Grafana dashboards
- [ ] ELK stack (logs)
- [ ] Distributed tracing
- [ ] Alert monitoring

#### Week 31-32: Documentation & Deployment
- [ ] API documentation
- [ ] Deployment guides
- [ ] Admin manual
- [ ] User guides
- [ ] Production deployment

---

## Deployment Strategy

### Local Development
```bash
# Using Docker Compose
docker compose up -d

# Services available:
# - Web app: http://localhost:80
# - API: http://localhost:5000
# - PostgreSQL: localhost:5432
# - Redis: localhost:6379
# - RabbitMQ: localhost:5672 (Admin: http://localhost:15672)
```

### Staging Environment
```
Platform: Azure Container Instances / App Service
- Replicated production setup
- Load balancer
- Separate database
- Monitoring enabled
```

### Production Environment
```
Platform: Azure AKS (Kubernetes) / Docker Swarm
Architecture:
├── API Gateway / Load Balancer
├── Multiple instances (Auto-scaling)
├── PostgreSQL (Managed)
├── Redis (Managed Cache)
├── RabbitMQ (Message Broker)
└── Monitoring Stack

Deployment via:
- GitHub Actions / Azure DevOps
- Terraform for infrastructure
- Helm for Kubernetes
- Blue-Green deployment strategy
```

### Infrastructure as Code (Terraform)

```hcl
# Example: Azure Deployment
terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.0"
    }
  }
}

provider "azurerm" {
  features {}
}

resource "azurerm_resource_group" "rg" {
  name     = "autocert-rg"
  location = "East US"
}

resource "azurerm_container_registry" "acr" {
  name                = "autocertregistry"
  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location
  sku                 = "Standard"
  admin_enabled       = true
}

resource "azurerm_postgresql_flexible_server" "postgres" {
  name                = "autocert-postgres"
  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location
  version             = "15"
  
  administrator_login    = var.db_admin_user
  administrator_password = var.db_admin_password
  
  sku_name = "B_Standard_B2s"
}

resource "azurerm_redis_cache" "redis" {
  name                = "autocert-redis"
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
  capacity            = 2
  family              = "C"
  sku_name            = "Standard"
  enable_non_ssl_port = false
}

resource "azurerm_service_bus_namespace" "servicebus" {
  name                = "autocert-servicebus"
  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location
  sku                 = "Standard"
}

# Kubernetes cluster
resource "azurerm_kubernetes_cluster" "aks" {
  name                = "autocert-aks"
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
  dns_prefix          = "autocert"
  
  default_node_pool {
    name       = "default"
    node_count = 3
    vm_size    = "Standard_D2_v2"
  }
}
```

---

## Development Roadmap

### Timeline
```
Phase 1 (MVP):          Weeks 1-10
Phase 2 (Automation):   Weeks 11-16
Phase 3 (Advanced):     Weeks 17-24
Phase 4 (Production):   Weeks 25-32

Total: 8 months for full production-ready system
```

### Sprint Planning
```
Sprint 1 (Week 1-2):     Setup & Auth
Sprint 2 (Week 3-4):     Multi-tenancy
Sprint 3 (Week 5-6):     Certificate Mgmt
Sprint 4 (Week 7-8):     Monitoring & Alerts
Sprint 5 (Week 9-10):    Dashboard UI
Sprint 6 (Week 11-12):   Manual Renewal
Sprint 7 (Week 13-14):   Terraform Integration
Sprint 8 (Week 15-16):   Python Integration
Sprint 9 (Week 17-18):   SSO
Sprint 10 (Week 19-20):  Analytics
Sprint 11 (Week 21-22):  API Enhancement
Sprint 12 (Week 23-24):  K8s Orchestration
Sprint 13 (Week 25-26):  Security
Sprint 14 (Week 27-28):  Performance
Sprint 15 (Week 29-30):  Observability
Sprint 16 (Week 31-32):  Documentation & Release
```

### Success Metrics
```
✓ Zero unplanned certificate expirations
✓ 99.9% platform uptime
✓ <500ms API response time (p95)
✓ <5min certificate check cycle
✓ 100% successful automatic renewals
✓ <1min email notification delivery
✓ Support for 100+ tenants
✓ Support for 1000+ APIs per tenant
```

---

## Quick Links & Resources

- [.NET 10 Documentation](https://learn.microsoft.com/dotnet/)
- [Entity Framework Core](https://learn.microsoft.com/ef/core/)
- [PostgreSQL Documentation](https://www.postgresql.org/docs/)
- [Docker Documentation](https://docs.docker.com/)
- [Terraform Documentation](https://www.terraform.io/docs)
- [OAuth2 / OIDC](https://tools.ietf.org/html/rfc6749)
- [REST API Best Practices](https://restfulapi.net/)
- [Certificate Renewal Tools](#)

---

**Document Version:** 1.0
**Last Updated:** February 9, 2026
**Status:** Draft - Ready for Development
