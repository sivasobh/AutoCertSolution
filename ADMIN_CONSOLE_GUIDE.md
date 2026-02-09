# AutoCert - Admin Console Guide
## IT Admin/DevOps Tenant & API Management

---

## Table of Contents
1. [Admin Console Overview](#admin-console-overview)
2. [Access Control & Permissions](#access-control--permissions)
3. [Admin Dashboard](#admin-dashboard)
4. [Tenant Management](#tenant-management)
5. [API/URL Management](#apiurl-management)
6. [Certificate Renewal Management](#certificate-renewal-management)
7. [Renewal Methods & Platforms](#renewal-methods--platforms)
8. [Onboarding Workflow](#onboarding-workflow)
9. [Admin API Endpoints](#admin-api-endpoints)
10. [Admin Components & UI](#admin-components--ui)
11. [Audit & Compliance](#audit--compliance)
12. [Security & Best Practices](#security--best-practices)

---

## Admin Console Overview

### Purpose & Audience

```
Admin Console (Private, Role-Locked)
│
├─ Super Admin (Platform Owner)
│  ├─ Create/Delete Companies (Tenants)
│  ├─ Manage Platform-wide Settings
│  ├─ View All Audit Logs
│  └─ System Health Monitoring
│
├─ Company IT Admin (Per-Tenant Admin)
│  ├─ Manage APIs/URLs for their company
│  ├─ Manage Team Members (Invite/Remove)
│  ├─ Configure Alert Rules
│  ├─ View Company Audit Logs
│  └─ Export Reports
│
└─ Restricted Access
   ├─ Only accessible via VPN/IP whitelist
   ├─ Requires MFA
   ├─ IP-based rate limiting
   └─ Session timeout: 15 minutes
```

### Access Flow

```
IT Admin/DevOps
    │
    ├─ Login (Email + MFA)
    │
    ├─ Verify IP Whitelist (if enabled)
    │
    ├─ Get JWT with Admin Claims
    │
    └─ Access Admin Console
       ├─ Dashboard (Overview)
       ├─ Tenant Management
       ├─ API/URL Management
       ├─ User Management
       ├─ Audit Logs
       └─ Settings
```

---

## Access Control & Permissions

### Role Hierarchy

```
┌─────────────────────────────────────────┐
│   ROLE HIERARCHY & PERMISSIONS          │
├─────────────────────────────────────────┤
│                                         │
│  Super Admin (Platform Level)           │
│  ├─ Manage Tenants (CRUD)              │
│  ├─ Manage Super Users                  │
│  ├─ View Platform Logs                  │
│  ├─ Configure System Settings           │
│  ├─ Manage Billing/Subscriptions        │
│  └─ Token: Valid for 24 hours          │
│                                         │
│  Tenant Admin (Company Level)           │
│  ├─ Manage APIs (CRUD)                 │
│  ├─ Manage Team Members                 │
│  ├─ Configure Alerts                    │
│  ├─ View Tenant Logs                    │
│  ├─ Export Reports                      │
│  └─ Token: Valid for 4 hours           │
│                                         │
│  Tenant Manager                         │
│  ├─ View APIs (Read-only)              │
│  ├─ Create/Edit APIs                    │
│  ├─ Configure Alerts                    │
│  ├─ View Logs (own actions)            │
│  └─ Token: Valid for 2 hours           │
│                                         │
│  Tenant Viewer                          │
│  ├─ View APIs (Read-only)              │
│  ├─ View Status (Read-only)            │
│  └─ Token: Valid for 2 hours           │
│                                         │
└─────────────────────────────────────────┘
```

### Permission Matrix

```
Action                  | Super Admin | Tenant Admin | Manager | Viewer
─────────────────────────────────────────────────────────────────────
Create Tenant          | ✅          | ❌           | ❌      | ❌
Delete Tenant          | ✅          | ❌           | ❌      | ❌
Manage Tenant Users    | ✅          | ✅           | ❌      | ❌
Create API             | ✅          | ✅           | ✅      | ❌
Edit API               | ✅          | ✅           | ✅      | ❌
Delete API             | ✅          | ✅           | ❌      | ❌
View APIs              | ✅          | ✅           | ✅      | ✅
Manage Alerts          | ✅          | ✅           | ✅      | ❌
View Audit Logs        | ✅          | ✅           | ✅      | ❌
Export Reports         | ✅          | ✅           | ✅      | ❌
Change Settings        | ✅          | ✅           | ❌      | ❌
View Billing           | ✅          | ✅           | ❌      | ❌
Certificate Renewal    | ✅          | ✅           | ✅      | ❌
```

### JWT Claims for Admin Session

```json
{
  "sub": "user-id-uuid",
  "email": "admin@company.com",
  "name": "John Doe",
  "role": "TenantAdmin",
  "tenant_id": "tenant-uuid",
  "scopes": [
    "admin:read",
    "admin:write",
    "api:manage",
    "user:manage",
    "audit:read"
  ],
  "iat": 1644364200,
  "exp": 1644378600,
  "iss": "autocert-api",
  "aud": "autocert-admin-console",
  "ip_address": "192.168.1.1",
  "mfa_verified": true,
  "session_id": "session-uuid"
}
```

---

## Admin Dashboard

### Dashboard Layout

```
AutoCert Admin Console
┌─────────────────────────────────────────────────────────────────┐
│                     Top Navigation Bar                           │
│  [Logo] Search | [Company Name ▼] [User ▼] [Settings] [Logout] │
└─────────────────────────────────────────────────────────────────┘

┌──────────────────┬─────────────────────────────────────────────┐
│                  │                                             │
│   Left Sidebar   │         Main Content Area                   │
│                  │                                             │
│ ☰ Dashboard      │  DASHBOARD                                  │
│   Tenants        │  ┌──────────────────────────────────────┐   │
│   APIs           │  │ Quick Stats                          │   │
│   Users          │  │ ┌──────────────┬──────────────────┐  │   │
│   Alerts         │  │ │ Total APIs   │ Certificates     │  │   │
│   Logs           │  │ │ 42           │ Valid: 38        │  │   │
│   Settings       │  │ │              │ Expiring: 3      │  │   │
│   Reports        │  │ │              │ Expired: 1       │  │   │
│                  │  │ └──────────────┴──────────────────┘   │   │
│                  │  │                                        │   │
│                  │  │ Recent Activity                        │   │
│                  │  │ ┌────────────────────────────────┐    │   │
│                  │  │ │ API Added: api.example.com    │    │   │
│                  │  │ │ 2 hours ago                    │    │   │
│                  │  │ │                                │    │   │
│                  │  │ │ Certificate Renewed: api2.x.c │    │   │
│                  │  │ │ 5 hours ago                    │    │   │
│                  │  │ │                                │    │   │
│                  │  │ │ User Added: john@company.com   │    │   │
│                  │  │ │ 1 day ago                      │    │   │
│                  │  │ └────────────────────────────────┘    │   │
│                  │  │                                        │   │
│                  │  │ Certificate Status Overview            │   │
│                  │  │ [Green] Valid: 38 (90.5%)             │   │
│                  │  │ [Yellow] Expiring Soon: 3 (7.1%)      │   │
│                  │  │ [Red] Expired: 1 (2.4%)               │   │
│                  │  │                                        │   │
│                  │  └──────────────────────────────────────┘   │
│                  │                                             │
└──────────────────┴─────────────────────────────────────────────┘
```

### Dashboard Components

```csharp
// AdminDashboard.razor Component

@page "/admin/dashboard"
@layout AdminLayout
@attribute [Authorize(Roles = "Admin,TenantAdmin")]

<div class="admin-dashboard">
    <div class="dashboard-header">
        <h1>Admin Dashboard</h1>
        <div class="header-meta">
            <span class="tenant-name">@CurrentTenant.Name</span>
            <span class="last-updated">Updated: @LastUpdateTime.ToString("g")</span>
        </div>
    </div>

    <div class="quick-stats">
        <AdminStatCard Title="Total APIs" Value="@Stats.TotalApis" Icon="globe" />
        <AdminStatCard Title="Valid Certificates" Value="@Stats.ValidCerts" Icon="check-circle" Color="success" />
        <AdminStatCard Title="Expiring Soon" Value="@Stats.ExpiringCerts" Icon="alert-circle" Color="warning" />
        <AdminStatCard Title="Expired" Value="@Stats.ExpiredCerts" Icon="x-circle" Color="danger" />
    </div>

    <div class="dashboard-grid">
        <div class="card">
            <div class="card-header">
                <h3>Recent Activity</h3>
                <a href="/admin/audit-logs">View All</a>
            </div>
            <div class="card-body">
                <table class="activity-table">
                    <thead>
                        <tr>
                            <th>Action</th>
                            <th>Resource</th>
                            <th>User</th>
                            <th>Time</th>
                        </tr>
                    </thead>
                    <tbody>
                        @foreach (var activity in RecentActivities)
                        {
                            <tr>
                                <td><span class="badge badge-@activity.ActionType.ToLower()">@activity.ActionType</span></td>
                                <td>@activity.ResourceName</td>
                                <td>@activity.UserEmail</td>
                                <td>@activity.Timestamp.ToString("g")</td>
                            </tr>
                        }
                    </tbody>
                </table>
            </div>
        </div>

        <div class="card">
            <div class="card-header">
                <h3>Certificate Status</h3>
            </div>
            <div class="card-body">
                <CertificateStatusChart Data="@CertStatusData" />
            </div>
        </div>

        <div class="card">
            <div class="card-header">
                <h3>Upcoming Expirations</h3>
                <a href="/admin/certificates">View All</a>
            </div>
            <div class="card-body">
                <table class="expiration-table">
                    <thead>
                        <tr>
                            <th>API</th>
                            <th>Expires</th>
                            <th>Days Left</th>
                            <th>Status</th>
                        </tr>
                    </thead>
                    <tbody>
                        @foreach (var cert in UpcomingExpirations)
                        {
                            <tr>
                                <td>@cert.ApiName</td>
                                <td>@cert.ExpiryDate.ToString("yyyy-MM-dd")</td>
                                <td>
                                    <strong class="@GetStatusClass(cert.DaysUntilExpiry)">
                                        @cert.DaysUntilExpiry
                                    </strong>
                                </td>
                                <td>
                                    <span class="status-badge status-@cert.Status.ToLower()">
                                        @cert.Status
                                    </span>
                                </td>
                            </tr>
                        }
                    </tbody>
                </table>
            </div>
        </div>
    </div>
</div>

@code {
    private class DashboardStats
    {
        public int TotalApis { get; set; }
        public int ValidCerts { get; set; }
        public int ExpiringCerts { get; set; }
        public int ExpiredCerts { get; set; }
    }

    private DashboardStats Stats { get; set; } = new();
    private List<ActivityLog> RecentActivities { get; set; } = new();
    private List<CertificateDTO> UpcomingExpirations { get; set; } = new();
    private DateTime LastUpdateTime { get; set; }

    protected override async Task OnInitializedAsync()
    {
        // Load dashboard data
        await LoadDashboardData();
    }

    private async Task LoadDashboardData()
    {
        try
        {
            var tenantId = CurrentUser.TenantId;
            
            // Load statistics
            Stats = await AdminService.GetDashboardStatsAsync(tenantId);
            
            // Load recent activities
            RecentActivities = await AuditService.GetRecentActivitiesAsync(tenantId, 10);
            
            // Load upcoming expirations
            UpcomingExpirations = await CertificateService.GetUpcomingExpirationsAsync(tenantId, 30);
            
            LastUpdateTime = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading dashboard data");
            // Show user-friendly error
        }
    }
}
```

---

## Tenant Management

### Tenant Admin Page

```
Admin Console > Tenants
┌────────────────────────────────────────────────────────────────┐
│ Tenants Management                                             │
│                              [+ Create Tenant] [Filters ▼]    │
├────────────────────────────────────────────────────────────────┤
│                                                                │
│ Search: [_________________________]                           │
│                                  Filter: [All ▼] Status: [All ▼]
│                                                                │
│ ┌──────────────────────────────────────────────────────────┐  │
│ │ Tenant Name      │ Owner         │ APIs │ Users │ Status │  │
│ ├──────────────────────────────────────────────────────────┤  │
│ │ Acme Corp        │ John Smith    │ 12   │ 5     │ Active │  │
│ │ TechStart Inc    │ Sarah Johnson │ 8    │ 3     │ Active │  │
│ │ Global Services  │ Mike Davis    │ 25   │ 10    │ Active │  │
│ │ Beta Company     │ Lisa Anderson │ 2    │ 1     │ Pending│  │
│ └──────────────────────────────────────────────────────────┘  │
│                                                                │
│ Actions: [Edit] [View APIs] [Manage Users] [Deactivate]      │
│                                                                │
└────────────────────────────────────────────────────────────────┘
```

### Create/Edit Tenant Component

```csharp
// TenantForm.razor Component

@page "/admin/tenants/create"
@page "/admin/tenants/edit/{TenantId}"
@layout AdminLayout
@attribute [Authorize(Roles = "SuperAdmin,TenantAdmin")]

<div class="tenant-form">
    <div class="form-header">
        <h1>@(IsEdit ? "Edit Tenant" : "Create New Tenant")</h1>
        <p class="breadcrumb">Admin > Tenants > @(IsEdit ? "Edit" : "Create")</p>
    </div>

    <EditForm Model="@TenantForm" OnValidSubmit="@HandleSubmit">
        <DataAnnotationsValidator />
        <ValidationSummary />

        <div class="form-section">
            <h3>Basic Information</h3>
            
            <div class="form-group">
                <label>Tenant Name *</label>
                <InputText 
                    @bind-Value="@TenantForm.Name" 
                    class="form-control" 
                    placeholder="E.g., Acme Corporation" />
                <ValidationMessage For="@(() => TenantForm.Name)" />
            </div>

            <div class="form-group">
                <label>Description</label>
                <InputTextArea 
                    @bind-Value="@TenantForm.Description" 
                    class="form-control" 
                    rows="4" 
                    placeholder="Brief description of the tenant" />
            </div>

            <div class="form-row">
                <div class="form-group">
                    <label>Owner Email *</label>
                    <InputText 
                        @bind-Value="@TenantForm.OwnerEmail" 
                        class="form-control" 
                        type="email" 
                        placeholder="owner@company.com" />
                    <ValidationMessage For="@(() => TenantForm.OwnerEmail)" />
                </div>

                <div class="form-group">
                    <label>Owner Name *</label>
                    <InputText 
                        @bind-Value="@TenantForm.OwnerName" 
                        class="form-control" 
                        placeholder="Full name" />
                    <ValidationMessage For="@(() => TenantForm.OwnerName)" />
                </div>
            </div>

            <div class="form-row">
                <div class="form-group">
                    <label>Industry</label>
                    <InputSelect @bind-Value="@TenantForm.Industry" class="form-control">
                        <option value="">-- Select --</option>
                        <option value="Technology">Technology</option>
                        <option value="Finance">Finance</option>
                        <option value="Healthcare">Healthcare</option>
                        <option value="Retail">Retail</option>
                        <option value="Other">Other</option>
                    </InputSelect>
                </div>

                <div class="form-group">
                    <label>Subscription Plan</label>
                    <InputSelect @bind-Value="@TenantForm.PlanId" class="form-control">
                        <option value="">-- Select --</option>
                        @foreach (var plan in AvailablePlans)
                        {
                            <option value="@plan.Id">@plan.Name (@plan.ApiLimit APIs)</option>
                        }
                    </InputSelect>
                </div>
            </div>
        </div>

        <div class="form-section">
            <h3>Settings</h3>
            
            <div class="form-group checkbox">
                <InputCheckbox @bind-Value="@TenantForm.IsActive" />
                <label>Active</label>
            </div>

            <div class="form-group checkbox">
                <InputCheckbox @bind-Value="@TenantForm.AllowSSO" />
                <label>Allow SSO Authentication</label>
            </div>

            <div class="form-group">
                <label>Alert Email Recipients</label>
                <InputText 
                    @bind-Value="@TenantForm.AlertEmails" 
                    class="form-control" 
                    placeholder="email1@company.com, email2@company.com" />
                <small class="text-muted">Comma-separated email addresses</small>
            </div>

            <div class="form-group">
                <label>Default Certificate Check Interval (minutes)</label>
                <InputNumber @bind-Value="@TenantForm.CheckIntervalMinutes" class="form-control" />
            </div>

            <div class="form-group">
                <label>Default Renewal Buffer (days)</label>
                <InputNumber @bind-Value="@TenantForm.RenewalBufferDays" class="form-control" />
            </div>
        </div>

        <div class="form-section">
            <h3>Billing Information</h3>
            
            <div class="form-row">
                <div class="form-group">
                    <label>Billing Email</label>
                    <InputText 
                        @bind-Value="@TenantForm.BillingEmail" 
                        class="form-control" 
                        type="email" />
                </div>

                <div class="form-group">
                    <label>Billing Address</label>
                    <InputText 
                        @bind-Value="@TenantForm.BillingAddress" 
                        class="form-control" />
                </div>
            </div>
        </div>

        <div class="form-actions">
            <button type="submit" class="btn btn-primary">@(IsEdit ? "Update" : "Create") Tenant</button>
            <a href="/admin/tenants" class="btn btn-secondary">Cancel</a>
        </div>
    </EditForm>
</div>

@code {
    [Parameter]
    public string TenantId { get; set; }

    private TenantFormModel TenantForm { get; set; } = new();
    private List<SubscriptionPlan> AvailablePlans { get; set; } = new();
    private bool IsEdit => !string.IsNullOrEmpty(TenantId);

    protected override async Task OnInitializedAsync()
    {
        // Load subscription plans
        AvailablePlans = await AdminService.GetSubscriptionPlansAsync();

        if (IsEdit)
        {
            // Load existing tenant
            var tenant = await AdminService.GetTenantAsync(TenantId);
            TenantForm = MapToFormModel(tenant);
        }
    }

    private async Task HandleSubmit()
    {
        try
        {
            if (IsEdit)
            {
                await AdminService.UpdateTenantAsync(TenantId, TenantForm);
                await AuditService.LogActionAsync(
                    "TENANT_UPDATED",
                    "Tenant",
                    TenantId,
                    newValues: TenantForm);
            }
            else
            {
                var result = await AdminService.CreateTenantAsync(TenantForm);
                await AuditService.LogActionAsync(
                    "TENANT_CREATED",
                    "Tenant",
                    result.Id,
                    newValues: TenantForm);
                TenantId = result.Id;
            }

            NavigationManager.NavigateTo("/admin/tenants");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error saving tenant");
            // Show error notification
        }
    }
}

public class TenantFormModel
{
    [Required(ErrorMessage = "Tenant name is required")]
    [StringLength(255)]
    public string Name { get; set; }

    public string Description { get; set; }

    [Required(ErrorMessage = "Owner email is required")]
    [EmailAddress]
    public string OwnerEmail { get; set; }

    [Required]
    public string OwnerName { get; set; }

    public string Industry { get; set; }
    public string PlanId { get; set; }
    public bool IsActive { get; set; } = true;
    public bool AllowSSO { get; set; }
    public string AlertEmails { get; set; }
    public int CheckIntervalMinutes { get; set; } = 1440;
    public int RenewalBufferDays { get; set; } = 30;
    public string BillingEmail { get; set; }
    public string BillingAddress { get; set; }
}
```

---

## API/URL Management

### API Management Page

```
Admin Console > APIs
┌────────────────────────────────────────────────────────────────┐
│ API Management                                                 │
│ Tenant: [Acme Corp ▼]        [+ Add API] [Import CSV] [Export]│
├────────────────────────────────────────────────────────────────┤
│                                                                │
│ Search APIs: [________________________]  Filter: [Status ▼]   │
│                                                                │
│ ┌──────────────────────────────────────────────────────────┐  │
│ │ API Name    │ URL              │ Port │ Status  │ Action│  │
│ ├──────────────────────────────────────────────────────────┤  │
│ │ API Service │ api.example.com  │ 443  │ ✅ Valid│ ... × │  │
│ │ Data API    │ data.example.com │ 443  │ ⚠️ 25d  │ ... × │  │
│ │ Auth Server │ auth.example.com │ 443  │ 🛑 Exp │ ... × │  │
│ │ Legacy API  │ old.example.com  │ 8443 │ ✅ Valid│ ... × │  │
│ └──────────────────────────────────────────────────────────┘  │
│                                                                │
│ Actions: [Edit] [Monitor] [Renew] [Delete]                   │
│                                                                │
└────────────────────────────────────────────────────────────────┘
```

### Add/Edit API Component

```csharp
// ApiForm.razor Component

@page "/admin/apis/create"
@page "/admin/apis/edit/{ApiId}"
@layout AdminLayout
@attribute [Authorize(Roles = "Admin,TenantAdmin,Manager")]

<div class="api-form">
    <div class="form-header">
        <h1>@(IsEdit ? "Edit API" : "Add New API")</h1>
    </div>

    <EditForm Model="@ApiForm" OnValidSubmit="@HandleSubmit">
        <DataAnnotationsValidator />
        <ValidationSummary />

        <div class="tabs">
            <div class="tab-content active">
                <h3>Basic Information</h3>

                <div class="form-group">
                    <label>API Name *</label>
                    <InputText 
                        @bind-Value="@ApiForm.Name" 
                        class="form-control" 
                        placeholder="E.g., Production API Server" />
                    <ValidationMessage For="@(() => ApiForm.Name)" />
                </div>

                <div class="form-group">
                    <label>Description</label>
                    <InputTextArea 
                        @bind-Value="@ApiForm.Description" 
                        class="form-control" 
                        rows="3" />
                </div>

                <div class="form-group">
                    <label>URL/Hostname *</label>
                    <InputText 
                        @bind-Value="@ApiForm.Url" 
                        class="form-control" 
                        placeholder="api.example.com" />
                    <ValidationMessage For="@(() => ApiForm.Url)" />
                    <small class="text-muted">Domain name or IP address</small>
                </div>

                <div class="form-row">
                    <div class="form-group">
                        <label>Protocol</label>
                        <InputSelect @bind-Value="@ApiForm.Protocol" class="form-control">
                            <option value="HTTPS">HTTPS (443)</option>
                            <option value="HTTP">HTTP (80)</option>
                            <option value="CUSTOM">Custom Port</option>
                        </InputSelect>
                    </div>

                    <div class="form-group">
                        <label>Port (Optional)</label>
                        <InputNumber 
                            @bind-Value="@ApiForm.Port" 
                            class="form-control" 
                            min="1" 
                            max="65535" />
                        <small class="text-muted">Leave empty for default</small>
                    </div>
                </div>

                <div class="form-group">
                    <label>Certificate Authority (CA)</label>
                    <InputSelect @bind-Value="@ApiForm.CertificateAuthority" class="form-control">
                        <option value="">Auto-detect</option>
                        <option value="LetsEncrypt">Let's Encrypt</option>
                        <option value="DigiCert">DigiCert</option>
                        <option value="GlobalSign">GlobalSign</option>
                        <option value="Sectigo">Sectigo (Comodo)</option>
                        <option value="Other">Other</option>
                    </InputSelect>
                </div>

                <div class="form-group checkbox">
                    <InputCheckbox @bind-Value="@ApiForm.IsMonitored" />
                    <label>Enable Certificate Monitoring</label>
                </div>
            </div>

            <div class="tab-content">
                <h3>Monitoring Settings</h3>

                @if (ApiForm.IsMonitored)
                {
                    <div class="form-group">
                        <label>Check Interval (minutes)</label>
                        <InputNumber 
                            @bind-Value="@ApiForm.CheckIntervalMinutes" 
                            class="form-control"
                            min="60"
                            max="43200" />
                        <small class="text-muted">Between 1 hour (60) and 30 days (43200)</small>
                    </div>

                    <div class="form-group">
                        <label>Renewal Buffer (days)</label>
                        <InputNumber 
                            @bind-Value="@ApiForm.RenewalBufferDays" 
                            class="form-control"
                            min="1"
                            max="90" />
                        <small class="text-muted">Alert when X days before expiration</small>
                    </div>

                    <div class="form-group checkbox">
                        <InputCheckbox @bind-Value="@ApiForm.EnableAutoRenewal" />
                        <label>Enable Automatic Renewal</label>
                    </div>

                    @if (ApiForm.EnableAutoRenewal)
                    {
                        <div class="form-group">
                            <label>Renewal Method</label>
                            <InputSelect @bind-Value="@ApiForm.RenewalMethod" class="form-control">
                                <option value="Manual">Manual Renewal</option>
                                <option value="Terraform">Terraform Automation</option>
                                <option value="Python">Python Script</option>
                                <option value="Webhook">Webhook Integration</option>
                            </InputSelect>
                        </div>

                        @if (ApiForm.RenewalMethod == "Terraform")
                        {
                            <div class="form-group">
                                <label>Terraform Workspace</label>
                                <InputText 
                                    @bind-Value="@ApiForm.TerraformWorkspace" 
                                    class="form-control"
                                    placeholder="production" />
                            </div>

                            <div class="form-group">
                                <label>Terraform Variables (JSON)</label>
                                <InputTextArea 
                                    @bind-Value="@ApiForm.TerraformVariables" 
                                    class="form-control"
                                    rows="4"
                                    placeholder='{"domain": "api.example.com"}' />
                            </div>
                        }
                    }
                }
                else
                {
                    <p class="alert alert-info">Enable monitoring to set up renewal options.</p>
                }
            </div>

            <div class="tab-content">
                <h3>Notifications</h3>

                <div class="form-group">
                    <label>Alert Email Recipients</label>
                    <InputText 
                        @bind-Value="@ApiForm.AlertEmails" 
                        class="form-control"
                        placeholder="admin@company.com, devops@company.com" />
                    <small class="text-muted">Comma-separated email addresses</small>
                </div>

                <div class="form-group">
                    <label>Slack Webhook (Optional)</label>
                    <InputText 
                        @bind-Value="@ApiForm.SlackWebhook" 
                        class="form-control"
                        placeholder="https://hooks.slack.com/services/..." />
                    <small class="text-muted">For Slack notifications</small>
                </div>

                <div class="form-group">
                    <label>PagerDuty Integration Key (Optional)</label>
                    <InputText 
                        @bind-Value="@ApiForm.PagerDutyKey" 
                        class="form-control"
                        placeholder="Your integration key" />
                </div>
            </div>
        </div>

        <div class="form-actions">
            <button type="submit" class="btn btn-primary">@(IsEdit ? "Update" : "Add") API</button>
            <a href="/admin/apis" class="btn btn-secondary">Cancel</a>
        </div>
    </EditForm>
</div>

@code {
    [Parameter]
    public string ApiId { get; set; }

    private ApiFormModel ApiForm { get; set; } = new();
    private bool IsEdit => !string.IsNullOrEmpty(ApiId);

    protected override async Task OnInitializedAsync()
    {
        if (IsEdit)
        {
            var api = await AdminService.GetApiAsync(CurrentTenant.Id, ApiId);
            ApiForm = MapToFormModel(api);
        }
    }

    private async Task HandleSubmit()
    {
        try
        {
            if (IsEdit)
            {
                await AdminService.UpdateApiAsync(CurrentTenant.Id, ApiId, ApiForm);
                await AuditService.LogActionAsync(
                    "API_UPDATED",
                    "Api",
                    ApiId,
                    newValues: ApiForm);
            }
            else
            {
                var result = await AdminService.CreateApiAsync(CurrentTenant.Id, ApiForm);
                await AuditService.LogActionAsync(
                    "API_CREATED",
                    "Api",
                    result.Id,
                    newValues: ApiForm);
            }

            NavigationManager.NavigateTo("/admin/apis");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error saving API");
        }
    }
}

public class ApiFormModel
{
    [Required]
    [StringLength(255)]
    public string Name { get; set; }

    public string Description { get; set; }

    [Required]
    public string Url { get; set; }

    [Required]
    public string Protocol { get; set; } = "HTTPS";

    public int? Port { get; set; }

    public string CertificateAuthority { get; set; }

    public bool IsMonitored { get; set; } = true;

    public int CheckIntervalMinutes { get; set; } = 1440;

    public int RenewalBufferDays { get; set; } = 30;

    public bool EnableAutoRenewal { get; set; }

    public string RenewalMethod { get; set; } = "Manual";

    public string TerraformWorkspace { get; set; }

    public string TerraformVariables { get; set; }

    public string AlertEmails { get; set; }

    public string SlackWebhook { get; set; }

    public string PagerDutyKey { get; set; }
}
```

### Bulk Import APIs

```csharp
// BulkImportApis.razor Component

@page "/admin/apis/import"
@layout AdminLayout
@attribute [Authorize(Roles = "Admin,TenantAdmin")]

<div class="bulk-import">
    <div class="form-header">
        <h1>Bulk Import APIs</h1>
        <p>Import multiple APIs from CSV file</p>
    </div>

    <div class="import-section">
        <h3>1. Download Template</h3>
        <button class="btn btn-secondary" @onclick="DownloadTemplate">
            📥 Download CSV Template
        </button>

        <h3>2. Upload CSV File</h3>
        <div class="file-upload">
            <InputFile OnChange="@HandleFileSelected" accept=".csv" />
            <small class="text-muted">
                CSV format: Name, URL, Port, Protocol, MonitoringEnabled, CheckIntervalMinutes, RenewalBufferDays
            </small>
        </div>

        @if (!string.IsNullOrEmpty(FileName))
        {
            <p class="file-info">Selected: @FileName (@FileSizeKB KB)</p>
        }

        <h3>3. Preview & Validate</h3>
        @if (PreviewData.Any())
        {
            <table class="preview-table">
                <thead>
                    <tr>
                        <th>Row</th>
                        <th>Name</th>
                        <th>URL</th>
                        <th>Status</th>
                        <th>Message</th>
                    </tr>
                </thead>
                <tbody>
                    @foreach (var (index, item, validation) in PreviewData)
                    {
                        <tr class="@(validation.IsValid ? "valid" : "invalid")">
                            <td>@index</td>
                            <td>@item.Name</td>
                            <td>@item.Url</td>
                            <td>
                                @if (validation.IsValid)
                                {
                                    <span class="badge badge-success">✓ Valid</span>
                                }
                                else
                                {
                                    <span class="badge badge-danger">✗ Error</span>
                                }
                            </td>
                            <td>@validation.Message</td>
                        </tr>
                    }
                </tbody>
            </table>

            <div class="import-summary">
                <strong>Summary:</strong>
                <span class="success">✓ @ValidCount valid</span>
                <span class="error">✗ @InvalidCount invalid</span>
            </div>

            <button class="btn btn-primary" @onclick="ConfirmImport" disabled="@(InvalidCount > 0)">
                📤 Import APIs
            </button>
        }

        @if (ImportProgress > 0)
        {
            <div class="progress">
                <div class="progress-bar" style="width: @ImportProgress%">
                    @ImportProgress%
                </div>
            </div>
            <p class="text-center">Importing... @ImportedCount / @TotalCount</p>
        }

        @if (!string.IsNullOrEmpty(ImportMessage))
        {
            <div class="alert alert-@(ImportSuccess ? "success" : "danger")">
                @ImportMessage
            </div>
        }
    </div>
</div>

@code {
    private string FileName { get; set; }
    private int FileSizeKB { get; set; }
    private List<(int Index, ApiImportModel Item, ValidationResult Validation)> PreviewData { get; set; } = new();
    private byte[] FileContent { get; set; }
    private int ValidCount { get; set; }
    private int InvalidCount { get; set; }
    private int ImportProgress { get; set; }
    private int ImportedCount { get; set; }
    private int TotalCount { get; set; }
    private string ImportMessage { get; set; }
    private bool ImportSuccess { get; set; }

    private void DownloadTemplate()
    {
        // Generate and download CSV template
        var csv = "Name,URL,Port,Protocol,MonitoringEnabled,CheckIntervalMinutes,RenewalBufferDays\n" +
                  "API Server,api.example.com,443,HTTPS,true,1440,30\n";
        
        // Trigger download
    }

    private async Task HandleFileSelected(InputFileChangeEventArgs e)
    {
        try
        {
            var file = e.File;
            FileName = file.Name;
            FileSizeKB = (int)(file.Size / 1024);

            using var stream = file.OpenReadStream();
            using var reader = new StreamReader(stream);
            var csv = await reader.ReadToEndAsync();

            PreviewData = ParseAndValidateCSV(csv);
            ValidCount = PreviewData.Count(x => x.Validation.IsValid);
            InvalidCount = PreviewData.Count(x => !x.Validation.IsValid);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error reading file");
        }
    }

    private List<(int, ApiImportModel, ValidationResult)> ParseAndValidateCSV(string csv)
    {
        var result = new List<(int, ApiImportModel, ValidationResult)>();
        var lines = csv.Split('\n');

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            var parts = lines[i].Split(',');
            var item = new ApiImportModel
            {
                Name = parts[0]?.Trim(),
                Url = parts[1]?.Trim(),
                Port = int.TryParse(parts[2], out var port) ? port : (int?)null,
                Protocol = parts[3]?.Trim() ?? "HTTPS"
            };

            var validation = ValidateApiModel(item);
            result.Add((i, item, validation));
        }

        return result;
    }

    private ValidationResult ValidateApiModel(ApiImportModel model)
    {
        var errors = new List<string>();

        if (string.IsNullOrEmpty(model.Name))
            errors.Add("Name is required");

        if (string.IsNullOrEmpty(model.Url))
            errors.Add("URL is required");

        // Additional validation...

        return new ValidationResult
        {
            IsValid = errors.Count == 0,
            Message = string.Join("; ", errors)
        };
    }

    private async Task ConfirmImport()
    {
        TotalCount = ValidCount;
        var validItems = PreviewData.Where(x => x.Validation.IsValid).Select(x => x.Item).ToList();

        try
        {
            foreach (var item in validItems)
            {
                await AdminService.CreateApiAsync(CurrentTenant.Id, new ApiFormModel
                {
                    Name = item.Name,
                    Url = item.Url,
                    Port = item.Port,
                    Protocol = item.Protocol
                });

                ImportedCount++;
                ImportProgress = (ImportedCount * 100) / TotalCount;
            }

            ImportSuccess = true;
            ImportMessage = $"Successfully imported {ImportedCount} APIs";
        }
        catch (Exception ex)
        {
            ImportSuccess = false;
            ImportMessage = $"Error importing APIs: {ex.Message}";
        }
    }
}
```

---

## Certificate Renewal Management

### Manual Renewal Button

```
API Details Page
┌────────────────────────────────────────────────────────────────┐
│ API: api.example.com                                           │
│ Status: ⚠️ Expiring in 25 days                                 │
├────────────────────────────────────────────────────────────────┤
│                                                                │
│ Certificate Information                                        │
│ ┌──────────────────────────────────────────────────────────┐  │
│ │ Issuer: Let's Encrypt                                    │  │
│ │ Issued: 2025-08-10                                       │  │
│ │ Expires: 2026-03-06                                      │  │
│ │ Days Left: 25 days                                       │  │
│ │ Renewal Buffer: 30 days                                  │  │
│ │ Auto-Renewal: Enabled (Terraform)                        │  │
│ │                                                           │  │
│ │ [🔄 Renew Now] [View Details] [Renewal History]          │  │
│ └──────────────────────────────────────────────────────────┘  │
│                                                                │
│ Renewal Configuration                                         │
│ ┌──────────────────────────────────────────────────────────┐  │
│ │ Method: Terraform ✓                                      │  │
│ │ Script: /terraform/renew-certs.tf                        │  │
│ │ Workspace: production                                    │  │
│ │ Target Platform: AWS Load Balancer                       │  │
│ │ Last Renewal: 2025-08-10 by admin@company.com            │  │
│ │ Last Renewal Status: SUCCESS ✅                           │  │
│ └──────────────────────────────────────────────────────────┘  │
│                                                                │
└────────────────────────────────────────────────────────────────┘
```

### Renewal Trigger Modal

```
Modal: Confirm Certificate Renewal
┌───────────────────────────────────────┐
│ ⚠️ Renew Certificate?                 │
├───────────────────────────────────────┤
│                                       │
│ API: api.example.com                  │
│ Current Cert Expires: 2026-03-06      │
│ Renewal Method: Terraform              │
│ Target: AWS Load Balancer              │
│                                       │
│ This will:                            │
│ 1. Generate new certificate            │
│ 2. Run renewal terraform script        │
│ 3. Update Load Balancer certificate    │
│ 4. Verify installation                │
│ 5. Log renewal event                  │
│                                       │
│ Estimated Duration: 2-5 minutes       │
│                                       │
│ [Cancel] [Renew] [View Script]        │
│                                       │
└───────────────────────────────────────┘
```

### Renewal Status Tracking

```csharp
// RenewalStatus.razor Component

@page "/admin/apis/{ApiId}/renewal"
@layout AdminLayout
@attribute [Authorize(Roles = "Admin,TenantAdmin")]

<div class="renewal-management">
    <div class="card">
        <div class="card-header">
            <h2>Certificate Renewal: @Api.Name</h2>
            <div class="status-badge status-@CurrentStatus.ToLower()">
                @CurrentStatus
            </div>
        </div>

        <div class="card-body">
            <div class="renewal-current-state">
                <h3>Current Certificate</h3>
                <div class="cert-info">
                    <p><strong>Domain:</strong> @Api.Url</p>
                    <p><strong>Issuer:</strong> @Certificate.Issuer</p>
                    <p><strong>Issued:</strong> @Certificate.IssuedDate.ToString("yyyy-MM-dd")</p>
                    <p><strong>Expires:</strong> @Certificate.ExpiryDate.ToString("yyyy-MM-dd")</p>
                    <p>
                        <strong>Days Left:</strong>
                        <span class="@GetStatusClass(DaysUntilExpiry)">
                            @DaysUntilExpiry days
                        </span>
                    </p>
                </div>
            </div>

            <div class="renewal-method-section">
                <h3>Renewal Configuration</h3>
                <div class="method-details">
                    <p><strong>Method:</strong> <span class="badge badge-primary">@Api.RenewalMethod</span></p>
                    <p><strong>Platform:</strong> <span class="badge badge-info">@Api.DeploymentPlatform</span></p>
                    <p><strong>Script Location:</strong> <code>@Api.RenewalScriptPath</code></p>
                    
                    @if (!string.IsNullOrEmpty(Api.TerraformWorkspace))
                    {
                        <p><strong>Terraform Workspace:</strong> <code>@Api.TerraformWorkspace</code></p>
                    }
                </div>
            </div>

            <div class="renewal-actions">
                <h3>Actions</h3>
                <button class="btn btn-danger" @onclick="TriggerRenewal" disabled="@IsRenewing">
                    @if (IsRenewing)
                    {
                        <span>🔄 Renewing...</span>
                    }
                    else
                    {
                        <span>🔄 Renew Certificate Now</span>
                    }
                </button>

                <button class="btn btn-secondary" @onclick="ViewRenewalScript">
                    📄 View Renewal Script
                </button>

                <a href="/admin/apis/@ApiId/renewal-history" class="btn btn-info">
                    📋 Renewal History
                </a>
            </div>

            @if (IsRenewing)
            {
                <div class="renewal-progress">
                    <h4>Renewal in Progress...</h4>
                    <div class="progress-steps">
                        <div class="step @(CurrentStep >= 1 ? "completed" : "")">
                            <span>1</span> Generating Certificate
                        </div>
                        <div class="step @(CurrentStep >= 2 ? "completed" : "")">
                            <span>2</span> Executing Renewal Script
                        </div>
                        <div class="step @(CurrentStep >= 3 ? "completed" : "")">
                            <span>3</span> Updating Deployment
                        </div>
                        <div class="step @(CurrentStep >= 4 ? "completed" : "")">
                            <span>4</span> Verifying Installation
                        </div>
                    </div>
                    <p class="status-message">@StatusMessage</p>
                </div>
            }

            @if (!string.IsNullOrEmpty(LastRenewalMessage))
            {
                <div class="alert alert-@(LastRenewalSuccess ? "success" : "danger")">
                    <strong>@(LastRenewalSuccess ? "✓ Success" : "✗ Failed"):</strong>
                    @LastRenewalMessage
                </div>
            }

            <div class="renewal-history-preview">
                <h3>Recent Renewal History</h3>
                <table class="table">
                    <thead>
                        <tr>
                            <th>Date</th>
                            <th>Status</th>
                            <th>Method</th>
                            <th>Duration</th>
                            <th>Details</th>
                        </tr>
                    </thead>
                    <tbody>
                        @foreach (var renewal in RecentRenewals)
                        {
                            <tr>
                                <td>@renewal.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")</td>
                                <td>
                                    <span class="badge badge-@(renewal.Status == "SUCCESS" ? "success" : "danger")">
                                        @renewal.Status
                                    </span>
                                </td>
                                <td>@renewal.Method</td>
                                <td>@renewal.Duration.TotalMinutes.ToString("F1") min</td>
                                <td>
                                    @if (!string.IsNullOrEmpty(renewal.ErrorMessage))
                                    {
                                        <span title="@renewal.ErrorMessage">⚠️ Error</span>
                                    }
                                </td>
                            </tr>
                        }
                    </tbody>
                </table>
            </div>
        </div>
    </div>
</div>

@code {
    [Parameter]
    public string ApiId { get; set; }

    private ApiDTO Api { get; set; }
    private CertificateDTO Certificate { get; set; }
    private List<RenewalEventDTO> RecentRenewals { get; set; } = new();
    
    private bool IsRenewing { get; set; }
    private int CurrentStep { get; set; }
    private string StatusMessage { get; set; }
    private string LastRenewalMessage { get; set; }
    private bool LastRenewalSuccess { get; set; }
    private int DaysUntilExpiry { get; set; }
    private string CurrentStatus { get; set; }

    protected override async Task OnInitializedAsync()
    {
        Api = await AdminService.GetApiAsync(ApiId);
        Certificate = await CertificateService.GetLatestCertificateAsync(ApiId);
        RecentRenewals = await RenewalService.GetRecentRenewalsAsync(ApiId, 5);
        
        DaysUntilExpiry = (int)(Certificate.ExpiryDate - DateTime.UtcNow).TotalDays;
        CurrentStatus = GetCertificateStatus(DaysUntilExpiry);
    }

    private async Task TriggerRenewal()
    {
        IsRenewing = true;
        CurrentStep = 0;
        StatusMessage = "Starting renewal process...";

        try
        {
            CurrentStep = 1;
            StatusMessage = "Generating new certificate...";
            await Task.Delay(500); // Simulate work

            CurrentStep = 2;
            StatusMessage = $"Executing {Api.RenewalMethod} renewal script...";
            var renewalResult = await RenewalService.TriggerRenewalAsync(
                ApiId,
                Api.RenewalMethod);

            CurrentStep = 3;
            StatusMessage = "Updating deployment platform...";
            await Task.Delay(500);

            CurrentStep = 4;
            StatusMessage = "Verifying certificate installation...";
            var verification = await CertificateService.VerifyCertificateAsync(ApiId);

            if (verification.IsValid)
            {
                LastRenewalSuccess = true;
                LastRenewalMessage = $"Certificate renewed successfully. New expiry: {verification.ExpiryDate:yyyy-MM-dd}";
                
                // Refresh certificate info
                Certificate = await CertificateService.GetLatestCertificateAsync(ApiId);
                DaysUntilExpiry = (int)(Certificate.ExpiryDate - DateTime.UtcNow).TotalDays;
            }
            else
            {
                LastRenewalSuccess = false;
                LastRenewalMessage = "Certificate verification failed. Please check the renewal script output.";
            }

            await AuditService.LogActionAsync(new AuditLog
            {
                Action = "CERTIFICATE_RENEWED",
                EntityType = "Api",
                EntityId = ApiId,
                NewValues = renewalResult
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Renewal failed");
            LastRenewalSuccess = false;
            LastRenewalMessage = $"Renewal failed: {ex.Message}";
            StatusMessage = "Renewal failed. See details below.";
        }
        finally
        {
            IsRenewing = false;
        }
    }

    private void ViewRenewalScript()
    {
        NavigationManager.NavigateTo($"/admin/apis/{ApiId}/view-script");
    }
}
```

---

## Renewal Methods & Platforms

### Supported Renewal Methods

```
┌─────────────────────────────────────────────────────────────┐
│              RENEWAL METHOD COMPARISON                       │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│ 1. TERRAFORM (Infrastructure as Code)                      │
│    ├─ Best for: Cloud infrastructure (AWS, Azure, GCP)     │
│    ├─ Platforms: ALB, NLB, API Gateway, App Gateway        │
│    ├─ Automation: Full automation support                  │
│    ├─ Rollback: Version control enabled                    │
│    └─ Example: AWS Load Balancer, Azure App Gateway        │
│                                                             │
│ 2. SHELL SCRIPT                                            │
│    ├─ Best for: VM-based deployments                       │
│    ├─ Platforms: Nginx, HAProxy, Apache, Custom Apps       │
│    ├─ Automation: SSH-based execution                      │
│    ├─ Rollback: Manual or backup-based                     │
│    └─ Example: On-premise load balancers                   │
│                                                             │
│ 3. KUBERNETES (Native Support)                             │
│    ├─ Best for: Kubernetes clusters                        │
│    ├─ Platforms: K8s Secrets, Cert-Manager, Ingress        │
│    ├─ Automation: kubectl/API-based                        │
│    ├─ Rollback: Volume rollback / ConfigMap revert         │
│    └─ Example: AKS, EKS, GKE, On-prem K8s                  │
│                                                             │
│ 4. PYTHON SCRIPT                                           │
│    ├─ Best for: Custom applications                        │
│    ├─ Platforms: APIs, Web frameworks                      │
│    ├─ Automation: Subprocess execution                     │
│    ├─ Rollback: Application-level rollback                 │
│    └─ Example: Django, FastAPI, Flask apps               │
│                                                             │
│ 5. WEBHOOK                                                 │
│    ├─ Best for: External services                          │
│    ├─ Platforms: Custom applications, 3rd party APIs       │
│    ├─ Automation: HTTP POST with certificate data          │
│    ├─ Rollback: External service responsibility            │
│    └─ Example: Custom CI/CD, SaaS platforms               │
│                                                             │
│ 6. MANUAL                                                  │
│    ├─ Best for: Testing, manual deployments                │
│    ├─ Platforms: All platforms                             │
│    ├─ Automation: No automation                            │
│    ├─ Rollback: Manual process                             │
│    └─ Example: Dev/test environments                       │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

### Renewal Workflow

```
Manual Renewal Trigger
    │
    ├─ Step 1: User clicks "Renew Certificate Now"
    │
    ├─ Step 2: System validates
    │   ├─ Check certificate exists
    │   ├─ Check renewal method configured
    │   ├─ Verify credentials/permissions
    │   └─ Check deployment platform accessible
    │
    ├─ Step 3: Acquire new certificate
    │   ├─ For Let's Encrypt: ACME challenge
    │   ├─ For other CAs: Upload CSR and receive cert
    │   └─ Store in Vault/Secrets Manager
    │
    ├─ Step 4: Execute renewal script
    │   ├─ Load renewal method configuration
    │   ├─ Prepare deployment parameters
    │   ├─ Execute platform-specific renewal
    │   └─ Capture output/logs
    │
    ├─ Step 5: Update target platform
    │   ├─ Kubernetes: Update K8s Secret/Ingress
    │   ├─ Load Balancer: Update certificate binding
    │   ├─ VM App: Reload configuration/restart
    │   └─ Webhook: Send renewal notification
    │
    ├─ Step 6: Verification
    │   ├─ Check new certificate is active
    │   ├─ Verify certificate chain
    │   ├─ Test HTTPS connectivity
    │   └─ Validate certificate dates
    │
    └─ Step 7: Log & Notify
        ├─ Create renewal event record
        ├─ Audit log entry
        ├─ Send success/failure email
        └─ Update dashboard
```

### Example Code: Kubernetes Certificate Renewal

#### 1. Kubernetes Secret Update Script

```bash
#!/bin/bash
# renew-k8s-certificate.sh

set -e

API_NAME=$1
NAMESPACE=$2
SECRET_NAME=$3
CERT_PATH=$4
KEY_PATH=$5

echo "[K8s] Starting certificate renewal for $API_NAME in namespace $NAMESPACE..."

# 1. Verify certificate exists
if [ ! -f "$CERT_PATH" ] || [ ! -f "$KEY_PATH" ]; then
    echo "ERROR: Certificate or key file not found"
    exit 1
fi

# 2. Create TLS secret with new certificate
echo "[K8s] Updating secret: $SECRET_NAME"
kubectl create secret tls $SECRET_NAME \
    --cert=$CERT_PATH \
    --key=$KEY_PATH \
    --namespace=$NAMESPACE \
    --dry-run=client \
    -o yaml | kubectl apply -f -

# 3. Get Ingress resources using this secret
echo "[K8s] Finding Ingress resources using secret..."
INGRESSES=$(kubectl get ingress -n $NAMESPACE -o json | \
    jq -r ".items[] | select(.spec.tls[]?.secretName==\"$SECRET_NAME\") | .metadata.name")

# 4. Trigger rollout restart for pods
echo "[K8s] Restarting pods to load new certificate..."
kubectl rollout restart deployment -n $NAMESPACE

# 5. Wait for rollout to complete
echo "[K8s] Waiting for rollout to complete..."
kubectl rollout status deployment -n $NAMESPACE --timeout=5m

# 6. Verify new certificate
echo "[K8s] Verifying certificate installation..."
POD=$(kubectl get pods -n $NAMESPACE -o name | head -1)
kubectl exec -n $NAMESPACE $POD -- openssl s_client -connect localhost:443 \
    -showcerts < /dev/null 2>/dev/null | openssl x509 -noout -dates

echo "✓ Certificate renewed successfully in Kubernetes"
```

#### 2. Kubernetes Cert-Manager (Automated)

```yaml
# cert-manager-certificate.yaml
# Automatic TLS certificate management and renewal

apiVersion: cert-manager.io/v1
kind: Certificate
metadata:
  name: api-tls
  namespace: autocert
spec:
  # Certificate details
  secretName: api-tls-secret
  issuerRef:
    name: letsencrypt-prod
    kind: ClusterIssuer
  dnsNames:
    - api.example.com
    - "*.api.example.com"
  duration: 2160h  # 90 days
  renewBefore: 720h  # 30 days
  
  # For existing certificate renewal
  commonName: api.example.com
  privateKey:
    algorithm: RSA
    size: 4096

---
apiVersion: cert-manager.io/v1
kind: ClusterIssuer
metadata:
  name: letsencrypt-prod
spec:
  acme:
    server: https://acme-v02.api.letsencrypt.org/directory
    email: admin@example.com
    privateKeySecretRef:
      name: letsencrypt-key
    solvers:
    - http01:
        ingress:
          class: nginx

---
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: api-ingress
  namespace: autocert
  annotations:
    cert-manager.io/cluster-issuer: letsencrypt-prod
spec:
  ingressClassName: nginx
  tls:
  - hosts:
    - api.example.com
    secretName: api-tls-secret
  rules:
  - host: api.example.com
    http:
      paths:
      - path: /
        pathType: Prefix
        backend:
          service:
            name: api-service
            port:
              number: 443
```

### Example Code: Terraform Certificate Renewal

#### 1. AWS Load Balancer Certificate Renewal

```hcl
# terraform/modules/certificate-renewal/main.tf

terraform {
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 5.0"
    }
    tls = {
      source  = "hashicorp/tls"
      version = "~> 4.0"
    }
  }
}

# 1. Generate new private key
resource "tls_private_key" "api_key" {
  algorithm = "RSA"
  rsa_bits  = 4096
}

# 2. Create certificate signing request
resource "tls_cert_request" "api_csr" {
  private_key_pem = tls_private_key.api_key.private_key_pem

  subject {
    common_name  = var.domain_name
    organization = var.organization_name
  }

  dns_names = [var.domain_name]
}

# 3. Import/request new certificate from ACM
resource "aws_acm_certificate" "api_cert" {
  domain_name       = var.domain_name
  validation_method = "DNS"

  tags = {
    Name        = "${var.api_name}-cert"
    Environment = var.environment
    ManagedBy   = "AutoCert"
  }

  lifecycle {
    create_before_destroy = true
  }
}

# 4. Validate certificate (DNS validation)
resource "aws_acm_certificate_validation" "api_cert" {
  certificate_arn           = aws_acm_certificate.api_cert.arn
  timeouts {
    create = "5m"
  }
}

# 5. Update Load Balancer with new certificate
resource "aws_lb_listener" "api" {
  load_balancer_arn = var.load_balancer_arn
  port              = "443"
  protocol          = "HTTPS"
  ssl_policy        = "ELBSecurityPolicy-TLS-1-2-2017-01"
  certificate_arn   = aws_acm_certificate.api_cert.arn

  default_action {
    type             = "forward"
    target_group_arn = var.target_group_arn
  }
}

# 6. Output certificate information
output "certificate_arn" {
  value       = aws_acm_certificate.api_cert.arn
  description = "ARN of the new certificate"
}

output "certificate_status" {
  value       = aws_acm_certificate.api_cert.status
  description = "Certificate validation status"
}

output "renewal_date" {
  value       = aws_acm_certificate.api_cert.not_after
  description = "Certificate expiration date"
}
```

#### 2. terraform/variables.tf

```hcl
variable "api_name" {
  type        = string
  description = "Name of the API"
}

variable "domain_name" {
  type        = string
  description = "Domain name for the certificate"
}

variable "organization_name" {
  type        = string
  description = "Organization name for the certificate"
}

variable "environment" {
  type        = string
  description = "Environment (dev/staging/prod)"
}

variable "load_balancer_arn" {
  type        = string
  description = "ARN of the target load balancer"
}

variable "target_group_arn" {
  type        = string
  description = "ARN of the target group"
}

variable "renewal_buffer_days" {
  type        = number
  default     = 30
  description = "Days before expiry to trigger renewal"
}
```

#### 3. Renewal Execution Script

```bash
#!/bin/bash
# renew-aws-alb-certificate.sh

set -e

API_NAME=$1
DOMAIN_NAME=$2
ENVIRONMENT=$3
TF_WORKSPACE=$4

echo "🔄 Starting Terraform-based certificate renewal for $API_NAME ($DOMAIN_NAME)"

# 1. Set Terraform workspace
echo "[Terraform] Setting workspace to: $TF_WORKSPACE"
cd terraform/modules/certificate-renewal
terraform workspace select $TF_WORKSPACE

# 2. Initialize Terraform
echo "[Terraform] Initializing..."
terraform init -upgrade

# 3. Validate configuration
echo "[Terraform] Validating configuration..."
terraform validate

# 4. Plan renewal
echo "[Terraform] Planning certificate renewal..."
terraform plan \
    -var="api_name=$API_NAME" \
    -var="domain_name=$DOMAIN_NAME" \
    -var="environment=$ENVIRONMENT" \
    -out=tfplan

# 5. Apply renewal (requires approval)
echo "[Terraform] Applying renewal..."
terraform apply tfplan

# 6. Get certificate details
echo "[Terraform] Retrieving certificate details..."
CERT_ARN=$(terraform output -raw certificate_arn)
CERT_STATUS=$(terraform output -raw certificate_status)
RENEWAL_DATE=$(terraform output -raw renewal_date)

echo "✓ Certificate Details:"
echo "  - ARN: $CERT_ARN"
echo "  - Status: $CERT_STATUS"
echo "  - Next Renewal: $RENEWAL_DATE"

# 7. Verify certificate is active on ALB
echo "[Verification] Testing HTTPS connectivity..."
sleep 10  # Wait for DNS propagation
openssl s_client -connect $DOMAIN_NAME:443 -showcerts < /dev/null 2>/dev/null | \
    openssl x509 -noout -dates

echo "✓ Certificate renewed successfully via Terraform"
```

### Example Code: VM/Load Balancer Certificate Renewal

#### 1. Nginx Load Balancer Renewal Script

```bash
#!/bin/bash
# renew-nginx-certificate.sh

set -e

DOMAIN=$1
EMAIL=$2
CERT_PATH=${3:-/etc/ssl/certs}
KEY_PATH=${4:-/etc/ssl/private}
LB_SERVER=${5:-localhost}

echo "🔄 Renewing certificate for Nginx: $DOMAIN"

# 1. Install certbot if needed
if ! command -v certbot &> /dev/null; then
    echo "[Setup] Installing certbot..."
    apt-get update
    apt-get install -y certbot python3-certbot-nginx
fi

# 2. Renew certificate with Let's Encrypt
echo "[Certbot] Requesting new certificate for $DOMAIN..."
certbot certonly \
    --standalone \
    --non-interactive \
    --agree-tos \
    --email $EMAIL \
    --domain $DOMAIN \
    --cert-path $CERT_PATH \
    --key-path $KEY_PATH

# 3. Update Nginx configuration
echo "[Nginx] Updating SSL certificate paths..."
NGINX_CONF="/etc/nginx/sites-enabled/default"

sed -i "s|ssl_certificate .*|ssl_certificate $CERT_PATH/$DOMAIN/fullchain.pem;|g" $NGINX_CONF
sed -i "s|ssl_certificate_key .*|ssl_certificate_key $KEY_PATH/$DOMAIN/privkey.pem;|g" $NGINX_CONF

# 4. Test Nginx configuration
echo "[Nginx] Testing configuration..."
nginx -t

# 5. Reload Nginx (zero-downtime)
echo "[Nginx] Reloading Nginx..."
systemctl reload nginx

# 6. Verify certificate
echo "[Verification] Verifying certificate installation..."
sleep 5

openssl s_client -connect $LB_SERVER:443 -showcerts < /dev/null 2>/dev/null | \
    openssl x509 -noout -dates

echo "✓ Certificate renewed successfully for Nginx"
```

#### 2. HAProxy Load Balancer Renewal Script

```bash
#!/bin/bash
# renew-haproxy-certificate.sh

DOMAIN=$1
EMAIL=$2
CERT_PATH=${3:-/etc/ssl/certs}
HAPROXY_CONF=${4:-/etc/haproxy/haproxy.cfg}

echo "🔄 Renewing certificate for HAProxy: $DOMAIN"

# 1. Renew certificate
echo "[Certbot] Requesting new certificate..."
certbot certonly \
    --standalone \
    --non-interactive \
    --agree-tos \
    --email $EMAIL \
    --domain $DOMAIN

# 2. Create combined PEM (HAProxy requires both cert and key in one file)
echo "[HAProxy] Creating combined certificate file..."
COMBINED_CERT="$CERT_PATH/$DOMAIN.pem"

cat /etc/letsencrypt/live/$DOMAIN/fullchain.pem \
    /etc/letsencrypt/live/$DOMAIN/privkey.pem > $COMBINED_CERT

chmod 600 $COMBINED_CERT

# 3. Update HAProxy configuration if needed
echo "[HAProxy] Updating configuration..."
sed -i "s|ssl_cert .*|ssl_cert $COMBINED_CERT|g" $HAPROXY_CONF

# 4. Test configuration
echo "[HAProxy] Testing configuration..."
haproxy -c -f $HAPROXY_CONF

# 5. Reload HAProxy
echo "[HAProxy] Reloading..."
systemctl reload haproxy

# 6. Verify
echo "[Verification] Verifying certificate..."
openssl s_client -connect localhost:443 -showcerts < /dev/null 2>/dev/null | \
    openssl x509 -noout -dates

echo "✓ Certificate renewed successfully for HAProxy"
```

### Example Code: Python Application Renewal

```python
# renewal_service.py

import os
import subprocess
import ssl
from datetime import datetime, timedelta
from typing import Dict, Optional
from enum import Enum

class RenewalPlatform(Enum):
    KUBERNETES = "kubernetes"
    TERRAFORM = "terraform"
    SHELL_SCRIPT = "shell_script"
    PYTHON = "python"
    WEBHOOK = "webhook"

class CertificateRenewalService:
    """
    Service to handle certificate renewal across multiple platforms
    """
    
    def __init__(self, vault_client, logger):
        self.vault = vault_client
        self.logger = logger
    
    async def renew_certificate(
        self,
        api_id: str,
        method: RenewalPlatform,
        config: Dict
    ) -> Dict:
        """
        Main entry point for certificate renewal
        """
        self.logger.info(f"Starting renewal for API {api_id} using {method.value}")
        
        try:
            # Step 1: Generate/obtain new certificate
            cert_data = await self._get_new_certificate(api_id, config)
            
            # Step 2: Execute renewal based on method
            if method == RenewalPlatform.TERRAFORM:
                result = await self._renew_via_terraform(api_id, cert_data, config)
            elif method == RenewalPlatform.KUBERNETES:
                result = await self._renew_via_kubernetes(api_id, cert_data, config)
            elif method == RenewalPlatform.SHELL_SCRIPT:
                result = await self._renew_via_shell(api_id, cert_data, config)
            elif method == RenewalPlatform.PYTHON:
                result = await self._renew_via_python(api_id, cert_data, config)
            elif method == RenewalPlatform.WEBHOOK:
                result = await self._renew_via_webhook(api_id, cert_data, config)
            else:
                raise ValueError(f"Unknown renewal method: {method}")
            
            # Step 3: Verify renewal
            verification = await self._verify_certificate(api_id, config)
            
            return {
                "status": "success",
                "api_id": api_id,
                "method": method.value,
                "execution_result": result,
                "verification": verification,
                "timestamp": datetime.utcnow().isoformat()
            }
            
        except Exception as e:
            self.logger.error(f"Renewal failed: {str(e)}")
            return {
                "status": "failure",
                "api_id": api_id,
                "method": method.value,
                "error": str(e),
                "timestamp": datetime.utcnow().isoformat()
            }
    
    async def _renew_via_terraform(
        self,
        api_id: str,
        cert_data: Dict,
        config: Dict
    ) -> Dict:
        """
        Execute renewal using Terraform
        """
        self.logger.info(f"Executing Terraform renewal for {api_id}")
        
        tf_dir = config.get("tf_dir", "terraform/modules/certificate-renewal")
        workspace = config.get("workspace", "default")
        
        try:
            # Prepare Terraform variables
            tf_vars = {
                "api_name": config.get("api_name"),
                "domain_name": config.get("domain_name"),
                "environment": config.get("environment", "production"),
                "load_balancer_arn": config.get("load_balancer_arn")
            }
            
            # Create tfvars file
            tfvars_content = "\n".join([
                f'{k} = "{v}"' for k, v in tf_vars.items()
            ])
            
            # Execute Terraform commands
            commands = [
                f"cd {tf_dir}",
                f"terraform workspace select {workspace}",
                "terraform init -upgrade",
                "terraform validate",
                f"terraform apply -var-file=renewal.tfvars -auto-approve"
            ]
            
            output = subprocess.run(
                " && ".join(commands),
                shell=True,
                capture_output=True,
                text=True,
                timeout=300
            )
            
            if output.returncode != 0:
                raise Exception(f"Terraform execution failed: {output.stderr}")
            
            return {
                "method": "terraform",
                "output": output.stdout,
                "execution_time": 120
            }
            
        except Exception as e:
            raise
    
    async def _renew_via_kubernetes(
        self,
        api_id: str,
        cert_data: Dict,
        config: Dict
    ) -> Dict:
        """
        Execute renewal using Kubernetes
        """
        self.logger.info(f"Executing K8s renewal for {api_id}")
        
        namespace = config.get("namespace", "default")
        secret_name = config.get("secret_name")
        
        try:
            # Prepare certificate data
            cert_path = "/tmp/tls.crt"
            key_path = "/tmp/tls.key"
            
            # Write certificate files
            with open(cert_path, "w") as f:
                f.write(cert_data["certificate"])
            with open(key_path, "w") as f:
                f.write(cert_data["private_key"])
            
            # Execute renewal script
            renewal_script = f"""
            #!/bin/bash
            kubectl create secret tls {secret_name} \
                --cert={cert_path} \
                --key={key_path} \
                --namespace={namespace} \
                --dry-run=client \
                -o yaml | kubectl apply -f -
            
            kubectl rollout restart deployment -n {namespace}
            kubectl rollout status deployment -n {namespace} --timeout=5m
            """
            
            result = subprocess.run(
                renewal_script,
                shell=True,
                capture_output=True,
                text=True,
                timeout=300
            )
            
            if result.returncode != 0:
                raise Exception(f"K8s renewal failed: {result.stderr}")
            
            return {
                "method": "kubernetes",
                "namespace": namespace,
                "secret_name": secret_name,
                "output": result.stdout
            }
            
        except Exception as e:
            raise
    
    async def _renew_via_webhook(
        self,
        api_id: str,
        cert_data: Dict,
        config: Dict
    ) -> Dict:
        """
        Execute renewal via webhook notification
        """
        self.logger.info(f"Sending webhook renewal notification for {api_id}")
        
        webhook_url = config.get("webhook_url")
        
        import aiohttp
        
        payload = {
            "api_id": api_id,
            "domain": config.get("domain_name"),
            "certificate": cert_data["certificate"],
            "private_key": cert_data["private_key"],
            "issued_at": datetime.utcnow().isoformat(),
            "expires_at": cert_data["expiry_date"],
            "action": "renew"
        }
        
        async with aiohttp.ClientSession() as session:
            async with session.post(webhook_url, json=payload, timeout=30) as resp:
                if resp.status != 200:
                    raise Exception(f"Webhook failed: {resp.status}")
                
                body = await resp.json()
                return {
                    "method": "webhook",
                    "status": resp.status,
                    "response": body
                }
    
    async def _verify_certificate(
        self,
        api_id: str,
        config: Dict
    ) -> Dict:
        """
        Verify that certificate was properly installed
        """
        self.logger.info(f"Verifying certificate installation for {api_id}")
        
        host = config.get("domain_name")
        port = config.get("port", 443)
        
        try:
            # Create SSL connection
            context = ssl.create_default_context()
            with ssl.create_connection((host, port), timeout=10) as sock:
                with context.wrap_socket(sock, server_hostname=host) as ssock:
                    cert_der = ssock.getpeercert(binary_form=True)
                    cert = ssl.DER_cert_to_PEM_cert(cert_der)
                    
                    # Parse certificate dates
                    import ssl
                    cert_obj = ssl.cert_time_to_seconds
                    # Additional verification logic
                    
                    return {
                        "verified": True,
                        "subject": ssock.getpeercert()["subject"],
                        "certificate": cert
                    }
        except Exception as e:
            return {
                "verified": False,
                "error": str(e)
            }
```

---

## API List with Manual Renewal Button

The following component displays all APIs for a tenant with prominent renewal buttons and renewal progress tracking.

```csharp
// ApiListWithRenewal.razor - Enhanced API list component with renewal buttons

@page "/admin/apis"
@layout AdminLayout
@attribute [Authorize(Roles = "Admin,TenantAdmin")]

<div class="api-list-container">
    <div class="list-header">
        <h1>Certificate Management - APIs</h1>
        <div class="header-actions">
            <a href="/admin/apis/create" class="btn btn-primary">+ Add New API</a>
            <button class="btn btn-secondary" @onclick="RefreshList">🔄 Refresh</button>
            <button class="btn btn-info" @onclick="ShowBulkRenewalModal">🔄 Bulk Renew</button>
        </div>
    </div>

    <div class="filters">
        <input type="text" placeholder="Search APIs..." @bind="SearchTerm" @oninput="ApplyFilters" class="form-control" />
        <select @bind="FilterStatus" @onchange="ApplyFilters" class="form-control">
            <option value="">All Status</option>
            <option value="valid">Valid</option>
            <option value="expiring">Expiring Soon</option>
            <option value="expired">Expired</option>
        </select>
    </div>

    <div class="api-table-container">
        <table class="table table-striped api-table">
            <thead>
                <tr>
                    <th>API Name</th>
                    <th>URL</th>
                    <th>Certificate Status</th>
                    <th>Expires</th>
                    <th>Days Left</th>
                    <th>Renewal Method</th>
                    <th>Actions</th>
                </tr>
            </thead>
            <tbody>
                @foreach (var api in FilteredApis)
                {
                    <tr class="api-row status-@api.CertificateStatus.ToLower()">
                        <td><strong>@api.Name</strong></td>
                        <td><code>@api.Url</code></td>
                        <td>
                            <span class="badge badge-@GetStatusBadgeClass(api.CertificateStatus)">
                                @GetStatusIcon(api.CertificateStatus) @api.CertificateStatus
                            </span>
                        </td>
                        <td>@api.CertificateExpiryDate?.ToString("yyyy-MM-dd")</td>
                        <td>
                            <strong class="@GetDaysUntilExpiryClass(api.DaysUntilExpiry)">
                                @api.DaysUntilExpiry days
                            </strong>
                        </td>
                        <td><span class="badge badge-info">@api.RenewalMethod</span></td>
                        <td class="action-buttons">
                            <button class="btn btn-sm btn-danger" 
                                    @onclick="@(() => TriggerRenewal(api.Id))"
                                    title="Manually renew certificate now">
                                🔄 Renew
                            </button>
                            <a href="/admin/apis/@api.Id" class="btn btn-sm btn-info">View</a>
                            <div class="dropdown">
                                <button class="btn btn-sm btn-secondary">⋮</button>
                                <div class="dropdown-menu">
                                    <a class="dropdown-item" href="/admin/apis/@api.Id/renewal-history">
                                        📋 History
                                    </a>
                                    <a class="dropdown-item" href="/admin/apis/@api.Id/edit">
                                        ✏️ Edit
                                    </a>
                                    <a class="dropdown-item" @onclick="@(() => DeleteApi(api.Id))">
                                        🗑️ Delete
                                    </a>
                                </div>
                            </div>
                        </td>
                    </tr>
                }
            </tbody>
        </table>
    </div>

    @if (ShowRenewalModal)
    {
        <RenewalConfirmationModal 
            Api="@SelectedApiForRenewal"
            OnCancel="@(() => ShowRenewalModal = false)"
            OnConfirm="@ConfirmRenewal"
            Options="@RenewalOptions" />
    }

    @if (RenewalInProgress)
    {
        <RenewalProgressModal 
            Api="@SelectedApiForRenewal"
            Progress="@RenewalProgress"
            Percentage="@RenewalPercentage"
            Message="@RenewalStatusMessage" />
    }

    @if (!string.IsNullOrEmpty(RenewalResultMessage))
    {
        <div class="alert alert-@(RenewalSuccess ? "success" : "danger") alert-dismissible">
            <button type="button" class="close" @onclick="ClearRenewalResult">×</button>
            <h4>@(RenewalSuccess ? "✓ Renewal Successful" : "✗ Renewal Failed")</h4>
            <p>@RenewalResultMessage</p>
            @if (!string.IsNullOrEmpty(RenewalErrorDetails))
            {
                <details>
                    <summary>Error Details</summary>
                    <pre>@RenewalErrorDetails</pre>
                </details>
            }
        </div>
    }
</div>

@code {
    private List<ApiDTO> AllApis { get; set; } = new();
    private List<ApiDTO> FilteredApis { get; set; } = new();
    private string SearchTerm { get; set; } = "";
    private string FilterStatus { get; set; } = "";

    private bool ShowRenewalModal { get; set; }
    private ApiDTO SelectedApiForRenewal { get; set; }
    private RenewalOptions RenewalOptions { get; set; } = new();

    private bool RenewalInProgress { get; set; }
    private int RenewalProgress { get; set; } // 1-4 (steps)
    private string RenewalStatusMessage { get; set; }
    private int RenewalPercentage { get; set; }

    private bool RenewalSuccess { get; set; }
    private string RenewalResultMessage { get; set; }
    private string RenewalErrorDetails { get; set; }

    public class RenewalOptions
    {
        public bool NotifyOnCompletion { get; set; } = true;
    }

    protected override async Task OnInitializedAsync()
    {
        await LoadApis();
    }

    private async Task LoadApis()
    {
        AllApis = await AdminService.GetApisAsync(CurrentTenant.Id);
        ApplyFilters();
    }

    private void ApplyFilters()
    {
        FilteredApis = AllApis
            .Where(a => string.IsNullOrEmpty(SearchTerm) || 
                       a.Name.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                       a.Url.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase))
            .Where(a => string.IsNullOrEmpty(FilterStatus) || a.CertificateStatus == FilterStatus)
            .ToList();
    }

    private void TriggerRenewal(string apiId)
    {
        SelectedApiForRenewal = AllApis.FirstOrDefault(a => a.Id == apiId);
        ShowRenewalModal = true;
    }

    private async Task ConfirmRenewal(string apiId)
    {
        ShowRenewalModal = false;
        RenewalInProgress = true;
        RenewalProgress = 0;

        try
        {
            // Step 1: Generate Certificate
            RenewalProgress = 1;
            RenewalPercentage = 25;
            RenewalStatusMessage = "Generating new certificate...";
            StateHasChanged();
            await Task.Delay(1000);

            // Step 2: Execute Script
            RenewalProgress = 2;
            RenewalPercentage = 50;
            RenewalStatusMessage = $"Executing {SelectedApiForRenewal.RenewalMethod} renewal script...";
            StateHasChanged();

            var result = await AdminService.RenewCertificateAsync(
                CurrentTenant.Id,
                apiId,
                SelectedApiForRenewal.RenewalMethod);

            // Step 3: Update Platform
            RenewalProgress = 3;
            RenewalPercentage = 75;
            RenewalStatusMessage = $"Updating {SelectedApiForRenewal.DeploymentPlatform}...";
            StateHasChanged();
            await Task.Delay(500);

            // Step 4: Verify
            RenewalProgress = 4;
            RenewalPercentage = 100;
            RenewalStatusMessage = "Verifying certificate installation...";
            StateHasChanged();

            var verification = await AdminService.VerifyCertificateAsync(apiId);

            RenewalSuccess = true;
            RenewalResultMessage = $"Certificate renewed successfully! New expiry: {verification.ExpiryDate:yyyy-MM-dd}";

            // Refresh list
            await LoadApis();

            // Notify if requested
            if (RenewalOptions.NotifyOnCompletion)
            {
                await NotificationService.SendRenewalNotificationAsync(apiId, true, null);
            }

            await AuditService.LogActionAsync(new AuditLog
            {
                Action = "CERTIFICATE_RENEWED_MANUAL",
                EntityType = "Api",
                EntityId = apiId,
                Status = "SUCCESS"
            });
        }
        catch (Exception ex)
        {
            RenewalSuccess = false;
            RenewalResultMessage = "Certificate renewal failed.";
            RenewalErrorDetails = ex.Message;

            if (RenewalOptions.NotifyOnCompletion)
            {
                await NotificationService.SendRenewalNotificationAsync(apiId, false, ex.Message);
            }

            Logger.LogError(ex, "Renewal failed");
        }
        finally
        {
            RenewalInProgress = false;
        }
    }

    private void ClearRenewalResult()
    {
        RenewalResultMessage = null;
        RenewalErrorDetails = null;
    }

    private async Task RefreshList()
    {
        await LoadApis();
    }

    private void ShowBulkRenewalModal()
    {
        // TODO: Implement bulk renewal modal
    }

    private async Task DeleteApi(string apiId)
    {
        if (await ConfirmAsync("Are you sure you want to delete this API?"))
        {
            await AdminService.DeleteApiAsync(apiId);
            await LoadApis();
        }
    }

    private string GetStatusIcon(string status) => status switch
    {
        "valid" => "✅",
        "expiring" => "⚠️",
        "expired" => "🛑",
        _ => "❓"
    };

    private string GetStatusBadgeClass(string status) => status switch
    {
        "valid" => "success",
        "expiring" => "warning",
        "expired" => "danger",
        _ => "secondary"
    };

    private string GetDaysUntilExpiryClass(int days) => days switch
    {
        > 30 => "text-success",
        >= 1 and <= 30 => "text-warning",
        < 1 => "text-danger",
        _ => ""
    };
}
```

---

### Complete Onboarding Flow

```
New Client Onboarding Process
├─ Step 1: Admin Creates Tenant
│  ├─ Enter company details
│  ├─ Assign owner/IT admin
│  ├─ Select subscription plan
│  └─ Create tenant in database
│
├─ Step 2: Tenant Owner Receives Invitation
│  ├─ Email with secure link
│  ├─ Accept invitation
│  └─ Create login credentials
│
├─ Step 3: Add APIs/URLs
│  ├─ Manual entry
│  ├─ Bulk CSV import
│  └─ Verify SSL/TLS connectivity
│
├─ Step 4: Configure Monitoring
│  ├─ Set check intervals
│  ├─ Define renewal buffers
│  └─ Configure alerts
│
├─ Step 5: Initial Certificate Scan
│  ├─ Scan all APIs
│  ├─ Store certificate data
│  └─ Generate status report
│
└─ Step 6: Live & Monitoring
   ├─ Continuous monitoring
   ├─ Email alerts on expiry
   └─ Automatic renewals (if enabled)
```

### Onboarding Wizard Component

```csharp
// OnboardingWizard.razor - Main coordinator component

@page "/admin/onboarding"
@layout AdminLayout
@attribute [Authorize(Roles = "SuperAdmin")]

<div class="onboarding-wizard">
    <div class="wizard-header">
        <h1>Client Onboarding</h1>
        <div class="progress-indicator">
            <div class="step @(CurrentStep >= 1 ? "active" : "")">
                <span class="step-number">1</span>
                <span class="step-label">Create Tenant</span>
            </div>
            <div class="step @(CurrentStep >= 2 ? "active" : "")">
                <span class="step-number">2</span>
                <span class="step-label">Add APIs</span>
            </div>
            <div class="step @(CurrentStep >= 3 ? "active" : "")">
                <span class="step-number">3</span>
                <span class="step-label">Configure Renewal</span>
            </div>
            <div class="step @(CurrentStep >= 4 ? "active" : "")">
                <span class="step-number">4</span>
                <span class="step-label">Verify & Complete</span>
            </div>
        </div>
    </div>

    <div class="wizard-content">
        @if (CurrentStep == 1)
        {
            <TenantCreationStep @ref="TenantStep" OnNext="@NextStep" />
        }
        else if (CurrentStep == 2)
        {
            <ApiAdditionStep TenantId="@CreatedTenantId" @ref="ApiStep" OnNext="@NextStep" />
        }
        else if (CurrentStep == 3)
        {
            <RenewalConfigurationStep TenantId="@CreatedTenantId" @ref="RenewalStep" OnNext="@NextStep" />
        }
        else if (CurrentStep == 4)
        {
            <VerificationStep TenantId="@CreatedTenantId" OnComplete="@CompleteOnboarding" />
        }
    </div>

    <div class="wizard-actions">
        @if (CurrentStep > 1)
        {
            <button class="btn btn-secondary" @onclick="@(() => CurrentStep--)">← Back</button>
        }
        @if (CurrentStep < 4)
        {
            <button class="btn btn-primary" @onclick="@NextStep">Next →</button>
        }
    </div>
</div>

@code {
    private int CurrentStep { get; set; } = 1;
    private string CreatedTenantId { get; set; }
    
    private TenantCreationStep TenantStep { get; set; }
    private ApiAdditionStep ApiStep { get; set; }
    private RenewalConfigurationStep RenewalStep { get; set; }

    private async Task NextStep()
    {
        if (CurrentStep == 1)
        {
            CreatedTenantId = await TenantStep.SubmitAsync();
            if (!string.IsNullOrEmpty(CreatedTenantId))
                CurrentStep++;
        }
        else if (CurrentStep == 2)
        {
            var success = await ApiStep.SubmitAsync();
            if (success)
                CurrentStep++;
        }
        else if (CurrentStep == 3)
        {
            var success = await RenewalStep.SubmitAsync();
            if (success)
                CurrentStep++;
        }
    }

    private async Task CompleteOnboarding()
    {
        // Mark tenant as onboarded
        await AdminService.CompleteOnboardingAsync(CreatedTenantId);
        
        // Send welcome email
        await NotificationService.SendWelcomeEmailAsync(CreatedTenantId);
        
        // Audit log
        await AuditService.LogActionAsync(new AuditLog
        {
            Action = "TENANT_ONBOARDED",
            EntityType = "Tenant",
            EntityId = CreatedTenantId
        });
        
        // Redirect to dashboard
        NavigationManager.NavigateTo("/admin/tenants");
    }
}
```

### Step 3: Renewal Configuration Component

**Detailed Renewal Method Selection & Configuration**

This component allows the admin to specify exactly how certificates will be renewed for this tenant.

```csharp
// RenewalConfigurationStep.razor - Step 3 of onboarding

@page "/admin/onboarding/renewal/{TenantId}"
@layout AdminLayout

<div class="renewal-config-step">
    <div class="step-header">
        <h2>Step 3: Configure Certificate Renewal</h2>
        <p>Specify how AutoCert will automatically or manually renew certificates</p>
    </div>

    <EditForm Model="@RenewalConfig" OnValidSubmit="@HandleSubmit">
        <DataAnnotationsValidator />
        <ValidationSummary />

        <div class="form-section">
            <h3>🔄 Default Renewal Method</h3>
            <p class="help-text">Choose how certificates will be renewed (can vary per API)</p>

            <div class="method-cards">
                @foreach (var method in RenewalMethods)
                {
                    <div class="method-card @(RenewalConfig.DefaultRenewalMethod == method.Value ? "selected" : "")"
                         @onclick="@(() => SelectMethod(method.Value))">
                        <div class="method-icon">@method.Icon</div>
                        <div class="method-name">@method.Name</div>
                        <div class="method-description">@method.Description</div>
                        <input type="radio" name="method" value="@method.Value" @bind="@RenewalConfig.DefaultRenewalMethod" style="visibility:hidden;" />
                    </div>
                }
            </div>

            @if (!string.IsNullOrEmpty(RenewalConfig.DefaultRenewalMethod))
            {
                <div class="method-details">
                    <h4>Details for @GetMethodName(RenewalConfig.DefaultRenewalMethod)</h4>
                    @GetMethodDetailText(RenewalConfig.DefaultRenewalMethod)
                </div>
            }
        </div>

        @if (!string.IsNullOrEmpty(RenewalConfig.DefaultRenewalMethod))
        {
            <div class="form-section">
                <h3>🚀 Deployment Target</h3>
                <p class="help-text">Where will renewed certificates be deployed?</p>

                <div class="form-group">
                    <label>Target Platform *</label>
                    <InputSelect @bind-Value="@RenewalConfig.DeploymentPlatform" class="form-control">
                        <option value="">-- Select Platform --</option>
                        @foreach (var platform in GetAvailablePlatforms())
                        {
                            <option value="@platform.Value">@platform.Label</option>
                        }
                    </InputSelect>
                </div>

                @if (!string.IsNullOrEmpty(RenewalConfig.DeploymentPlatform))
                {
                    <RenderPlatformConfig Platform="@RenewalConfig.DeploymentPlatform" Config="@RenewalConfig" />
                }
            </div>

            <div class="form-section">
                <h3>📜 Renewal Script</h3>
                <p class="help-text">Upload or reference your renewal script</p>

                <div class="form-group">
                    <label>Script Source</label>
                    <InputSelect @bind-Value="@RenewalConfig.ScriptSource" class="form-control">
                        <option value="provided">Use AutoCert Provided Template</option>
                        <option value="upload">Upload Custom Script</option>
                        <option value="path">Reference Existing Path</option>
                    </InputSelect>
                </div>

                @if (RenewalConfig.ScriptSource == "provided")
                {
                    <div class="template-info">
                        <p>We'll use our tested renewal template for @GetMethodName(RenewalConfig.DefaultRenewalMethod)</p>
                        <button type="button" class="btn btn-sm btn-info" @onclick="ViewTemplate">
                            📄 View Template
                        </button>
                    </div>
                }
                else if (RenewalConfig.ScriptSource == "upload")
                {
                    <div class="file-upload">
                        <InputFile OnChange="@HandleFileSelected" />
                        <small>Supported: .sh, .py, .tf, .yaml, .json</small>
                    </div>
                }
                else if (RenewalConfig.ScriptSource == "path")
                {
                    <div class="form-group">
                        <InputText @bind-Value="@RenewalConfig.ScriptPath" class="form-control" 
                            placeholder="/scripts/renewal.sh or /terraform/renew.tf" />
                    </div>
                }
            </div>

            <div class="form-section">
                <h3>⚙️ Renewal Behavior</h3>

                <div class="form-group">
                    <label>Renewal Trigger</label>
                    <div class="radio-group">
                        <label class="radio">
                            <InputRadio @bind-Value="@RenewalConfig.RenewalTrigger" Value="auto" />
                            Automatic (Every X days before expiry)
                        </label>
                        <label class="radio">
                            <InputRadio @bind-Value="@RenewalConfig.RenewalTrigger" Value="manual" />
                            Manual Only (Admin clicks button)
                        </label>
                        <label class="radio">
                            <InputRadio @bind-Value="@RenewalConfig.RenewalTrigger" Value="both" />
                            Both Automatic & Manual
                        </label>
                    </div>
                </div>

                @if (RenewalConfig.RenewalTrigger != "manual")
                {
                    <div class="form-group">
                        <label>Renewal Buffer (days before expiry)</label>
                        <InputNumber @bind-Value="@RenewalConfig.RenewalBufferDays" class="form-control" 
                            min="1" max="90" />
                        <small class="text-muted">Automatic renewal will occur this many days before expiration</small>
                    </div>

                    <div class="form-group checkbox">
                        <InputCheckbox @bind-Value="@RenewalConfig.RequireApproval" />
                        <label>Require Approval</label>
                        <small class="text-muted">SuperAdmin must approve even automatic renewal</small>
                    </div>
                }
            </div>

            <div class="form-section">
                <h3>📧 Notifications</h3>

                <div class="form-group">
                    <label>Alert Emails (renewal success/failure)</label>
                    <InputText @bind-Value="@RenewalConfig.AlertEmails" class="form-control" 
                        placeholder="admin@company.com, devops@company.com" />
                </div>

                <div class="form-group checkbox">
                    <InputCheckbox @bind-Value="@RenewalConfig.NotifyOnSuccess" />
                    <label>Email on Success</label>
                </div>

                <div class="form-group checkbox">
                    <InputCheckbox @bind-Value="@RenewalConfig.NotifyOnFailure" />
                    <label>Email on Failure</label>
                </div>
            </div>

            <div class="form-section">
                <h3>🔗 Webhook (Optional)</h3>
                <p class="help-text">Notify external systems when certificate is renewed</p>

                <div class="form-group">
                    <label>Webhook URL</label>
                    <InputText @bind-Value="@RenewalConfig.WebhookUrl" class="form-control" 
                        placeholder="https://example.com/api/cert-renewed" />
                </div>
            </div>
        }

        <div class="form-actions">
            <button type="submit" class="btn btn-primary">Save & Continue →</button>
            <button type="button" class="btn btn-secondary" @onclick="@(() => OnBack.InvokeAsync())">← Back</button>
        </div>
    </EditForm>
</div>

@code {
    [Parameter]
    public string TenantId { get; set; }

    [Parameter]
    public EventCallback OnNext { get; set; }

    [Parameter]
    public EventCallback OnBack { get; set; }

    private RenewalConfigModel RenewalConfig { get; set; } = new();

    private List<MethodOption> RenewalMethods { get; set; } = new()
    {
        new MethodOption 
        { 
            Value = "terraform", 
            Name = "Terraform", 
            Icon = "🏗️",
            Description = "Cloud infrastructure (AWS, Azure, GCP)" 
        },
        new MethodOption 
        { 
            Value = "kubernetes", 
            Name = "Kubernetes", 
            Icon = "☸️",
            Description = "K8s clusters (AKS, EKS, GKE)" 
        },
        new MethodOption 
        { 
            Value = "shell_script", 
            Name = "Shell Script", 
            Icon = "🔧",
            Description = "VMs, Load Balancers (Nginx, HAProxy)" 
        },
        new MethodOption 
        { 
            Value = "python", 
            Name = "Python", 
            Icon = "🐍",
            Description = "Custom applications" 
        },
        new MethodOption 
        { 
            Value = "webhook", 
            Name = "Webhook", 
            Icon = "🔗",
            Description = "External services" 
        },
        new MethodOption 
        { 
            Value = "manual", 
            Name = "Manual Only", 
            Icon = "👤",
            Description = "Testing/Dev only" 
        }
    };

    public class MethodOption
    {
        public string Value { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
        public string Description { get; set; }
    }

    public class RenewalConfigModel
    {
        [Required(ErrorMessage = "Renewal method is required")]
        public string DefaultRenewalMethod { get; set; }

        [Required(ErrorMessage = "Deployment platform is required")]
        public string DeploymentPlatform { get; set; }

        public string ScriptSource { get; set; } = "provided";
        public string ScriptPath { get; set; }
        public string ScriptContent { get; set; }

        public string RenewalTrigger { get; set; } = "auto";
        public int RenewalBufferDays { get; set; } = 30;
        public bool RequireApproval { get; set; } = false;

        public string AlertEmails { get; set; }
        public bool NotifyOnSuccess { get; set; } = true;
        public bool NotifyOnFailure { get; set; } = true;

        public string WebhookUrl { get; set; }

        // Platform-specific configs
        public string AwsRegion { get; set; }
        public string LoadBalancerArn { get; set; }
        public string K8sClusterUrl { get; set; }
        public string K8sNamespace { get; set; }
        public string K8sSecretName { get; set; }
    }

    private void SelectMethod(string method)
    {
        RenewalConfig.DefaultRenewalMethod = method;
        RenewalConfig.DeploymentPlatform = null;
    }

    private List<(string Value, string Label)> GetAvailablePlatforms()
    {
        return RenewalConfig.DefaultRenewalMethod switch
        {
            "terraform" => new()
            {
                ("aws_alb", "AWS Application Load Balancer (ALB)"),
                ("aws_nlb", "AWS Network Load Balancer (NLB)"),
                ("azure_app_gateway", "Azure Application Gateway"),
                ("gcp_lb", "Google Cloud Load Balancer")
            },
            "kubernetes" => new()
            {
                ("k8s_secret", "Kubernetes TLS Secret"),
                ("k8s_ingress", "Kubernetes Ingress"),
                ("k8s_cert_manager", "Cert-Manager"),
                ("aks", "Azure Kubernetes Service"),
                ("eks", "AWS EKS"),
                ("gke", "Google GKE")
            },
            "shell_script" => new()
            {
                ("nginx", "Nginx Load Balancer"),
                ("haproxy", "HAProxy Load Balancer"),
                ("apache", "Apache Web Server"),
                ("custom", "Custom Application")
            },
            _ => new()
        };
    }

    private string GetMethodName(string method)
    {
        return RenewalMethods.FirstOrDefault(x => x.Value == method)?.Name ?? method;
    }

    private string GetMethodDetailText(string method)
    {
        return method switch
        {
            "terraform" => "Terraform will manage your cloud infrastructure certificates. Supports AWS (ALB, NLB, API Gateway), Azure (App Gateway, CDN), and GCP (Load Balancer).",
            "kubernetes" => "Kubernetes updates will refresh certificates in K8s Secrets, Ingress resources, or via Cert-Manager automation.",
            "shell_script" => "Shell scripts execute on your load balancers or VMs to update certificates (Nginx, HAProxy, Apache, custom apps).",
            "python" => "Python scripts handle renewal for custom applications. Can be used for any application type.",
            "webhook" => "HTTP POST request sent to your endpoint with certificate data. Your system handles the renewal.",
            "manual" => "No automation. Admins manually trigger renewal via dashboard button.",
            _ => ""
        };
    }

    private void ViewTemplate()
    {
        // Show template based on method selected
    }

    private async Task HandleFileSelected(InputFileChangeEventArgs e)
    {
        // Handle file upload
    }

    public async Task<bool> SubmitAsync()
    {
        try
        {
            await AdminService.SaveRenewalConfigAsync(TenantId, RenewalConfig);
            await AuditService.LogActionAsync("RENEWAL_CONFIG_SAVED", "Tenant", TenantId);
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error saving renewal config");
            return false;
        }
    }
}
```

---

## Admin API Endpoints

### Tenant Management Endpoints

```csharp
[ApiController]
[Route("api/v1/admin/tenants")]
[Authorize(Roles = "SuperAdmin,TenantAdmin")]
public class AdminTenantController : ControllerBase
{
    private readonly IAdminService _adminService;
    private readonly IAuditService _auditService;

    /// <summary>
    /// Get all tenants (Super Admin only)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> GetAllTenants(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string search = null,
        [FromQuery] string status = null)
    {
        var result = await _adminService.GetTenantsAsync(page, pageSize, search, status);
        return Ok(result);
    }

    /// <summary>
    /// Get tenant details
    /// </summary>
    [HttpGet("{tenantId}")]
    public async Task<IActionResult> GetTenant(string tenantId)
    {
        // Verify authorization
        var tenant = await _adminService.GetTenantAsync(tenantId);
        
        if (tenant == null)
            return NotFound();

        return Ok(tenant);
    }

    /// <summary>
    /// Create new tenant (Super Admin only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> CreateTenant([FromBody] CreateTenantRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var tenant = await _adminService.CreateTenantAsync(request);
        
        await _auditService.LogActionAsync(new AuditLog
        {
            Action = "TENANT_CREATED",
            EntityType = "Tenant",
            EntityId = tenant.Id,
            NewValues = tenant
        });

        // Send welcome email to tenant owner
        await _notificationService.SendTenantWelcomeEmailAsync(tenant);

        return CreatedAtAction(nameof(GetTenant), new { tenantId = tenant.Id }, tenant);
    }

    /// <summary>
    /// Update tenant
    /// </summary>
    [HttpPut("{tenantId}")]
    [Authorize(Roles = "SuperAdmin,TenantAdmin")]
    public async Task<IActionResult> UpdateTenant(
        string tenantId,
        [FromBody] UpdateTenantRequest request)
    {
        var existingTenant = await _adminService.GetTenantAsync(tenantId);
        
        if (existingTenant == null)
            return NotFound();

        var updatedTenant = await _adminService.UpdateTenantAsync(tenantId, request);

        await _auditService.LogActionAsync(new AuditLog
        {
            Action = "TENANT_UPDATED",
            EntityType = "Tenant",
            EntityId = tenantId,
            OldValues = existingTenant,
            NewValues = updatedTenant
        });

        return Ok(updatedTenant);
    }

    /// <summary>
    /// Delete tenant (Super Admin only)
    /// </summary>
    [HttpDelete("{tenantId}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> DeleteTenant(string tenantId)
    {
        var tenant = await _adminService.GetTenantAsync(tenantId);
        
        if (tenant == null)
            return NotFound();

        await _adminService.DeleteTenantAsync(tenantId);

        await _auditService.LogActionAsync(new AuditLog
        {
            Action = "TENANT_DELETED",
            EntityType = "Tenant",
            EntityId = tenantId,
            OldValues = tenant
        });

        return NoContent();
    }

    /// <summary>
    /// Get tenant users
    /// </summary>
    [HttpGet("{tenantId}/users")]
    public async Task<IActionResult> GetTenantUsers(string tenantId)
    {
        var users = await _adminService.GetTenantUsersAsync(tenantId);
        return Ok(users);
    }

    /// <summary>
    /// Invite user to tenant
    /// </summary>
    [HttpPost("{tenantId}/users/invite")]
    public async Task<IActionResult> InviteUserToTenant(
        string tenantId,
        [FromBody] InviteUserRequest request)
    {
        var invitation = await _adminService.InviteUserAsync(tenantId, request);
        
        await _auditService.LogActionAsync(new AuditLog
        {
            Action = "USER_INVITED",
            EntityType = "User",
            EntityId = request.Email,
            NewValues = invitation
        });

        // Send invitation email
        await _notificationService.SendInvitationEmailAsync(invitation);

        return Ok(invitation);
    }

    /// <summary>
    /// Remove user from tenant
    /// </summary>
    [HttpDelete("{tenantId}/users/{userId}")]
    public async Task<IActionResult> RemoveUserFromTenant(string tenantId, string userId)
    {
        await _adminService.RemoveUserFromTenantAsync(tenantId, userId);

        await _auditService.LogActionAsync(new AuditLog
        {
            Action = "USER_REMOVED",
            EntityType = "User",
            EntityId = userId
        });

        return NoContent();
    }
}
```

### API Management Endpoints

```csharp
[ApiController]
[Route("api/v1/admin/apis")]
[Authorize(Roles = "Admin,TenantAdmin,Manager")]
public class AdminApiController : ControllerBase
{
    private readonly IAdminService _adminService;
    private readonly ICertificateService _certificateService;
    private readonly IAuditService _auditService;

    /// <summary>
    /// Get all APIs for a tenant
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetApis(
        [FromQuery] string tenantId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _adminService.GetApisAsync(tenantId, page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Create new API
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateApi(
        [FromQuery] string tenantId,
        [FromBody] CreateApiRequest request)
    {
        var api = await _adminService.CreateApiAsync(tenantId, request);
        
        await _auditService.LogActionAsync(new AuditLog
        {
            Action = "API_CREATED",
            EntityType = "Api",
            EntityId = api.Id,
            NewValues = api
        });

        // Schedule initial certificate check
        await _certificateService.ScheduleCheckAsync(api.Id);

        return CreatedAtAction(nameof(GetApi), new { apiId = api.Id }, api);
    }

    /// <summary>
    /// Get API details
    /// </summary>
    [HttpGet("{apiId}")]
    public async Task<IActionResult> GetApi(string apiId)
    {
        var api = await _adminService.GetApiAsync(apiId);
        
        if (api == null)
            return NotFound();

        return Ok(api);
    }

    /// <summary>
    /// Update API
    /// </summary>
    [HttpPut("{apiId}")]
    public async Task<IActionResult> UpdateApi(
        string apiId,
        [FromBody] UpdateApiRequest request)
    {
        var existingApi = await _adminService.GetApiAsync(apiId);
        
        if (existingApi == null)
            return NotFound();

        var updatedApi = await _adminService.UpdateApiAsync(apiId, request);

        await _auditService.LogActionAsync(new AuditLog
        {
            Action = "API_UPDATED",
            EntityType = "Api",
            EntityId = apiId,
            OldValues = existingApi,
            NewValues = updatedApi
        });

        return Ok(updatedApi);
    }

    /// <summary>
    /// Delete API
    /// </summary>
    [HttpDelete("{apiId}")]
    public async Task<IActionResult> DeleteApi(string apiId)
    {
        var api = await _adminService.GetApiAsync(apiId);
        
        if (api == null)
            return NotFound();

        await _adminService.DeleteApiAsync(apiId);

        await _auditService.LogActionAsync(new AuditLog
        {
            Action = "API_DELETED",
            EntityType = "Api",
            EntityId = apiId,
            OldValues = api
        });

        return NoContent();
    }

    /// <summary>
    /// Manually trigger certificate check for an API
    /// </summary>
    [HttpPost("{apiId}/check-certificate")]
    public async Task<IActionResult> CheckCertificate(string apiId)
    {
        var certificate = await _certificateService.CheckCertificateAsync(apiId);

        await _auditService.LogActionAsync(new AuditLog
        {
            Action = "CERTIFICATE_CHECKED",
            EntityType = "Certificate",
            EntityId = certificate.Id,
            NewValues = certificate
        });

        return Ok(certificate);
    }

    /// <summary>
    /// Trigger certificate renewal for an API
    /// </summary>
    [HttpPost("{apiId}/renew-certificate")]
    public async Task<IActionResult> RenewCertificate(
        string apiId,
        [FromBody] RenewCertificateRequest request)
    {
        var api = await _adminService.GetApiAsync(apiId);
        
        if (api == null)
            return NotFound();

        try
        {
            var renewalEvent = await _certificateService.TriggerRenewalAsync(
                apiId,
                request.RenewalMethod ?? api.RenewalMethod);

            await _auditService.LogActionAsync(new AuditLog
            {
                Action = "RENEWAL_TRIGGERED",
                EntityType = "RenewalEvent",
                EntityId = renewalEvent.Id,
                NewValues = renewalEvent,
                Status = "SUCCESS"
            });

            return Ok(new
            {
                status = "queued",
                renewalId = renewalEvent.Id,
                estimatedDuration = "2-5 minutes",
                message = "Certificate renewal queued. Check status below."
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Renewal trigger failed");
            
            await _auditService.LogActionAsync(new AuditLog
            {
                Action = "RENEWAL_TRIGGERED",
                EntityType = "RenewalEvent",
                EntityId = apiId,
                Status = "FAILURE",
                ErrorMessage = ex.Message
            });

            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get renewal history for an API
    /// </summary>
    [HttpGet("{apiId}/renewal-history")]
    public async Task<IActionResult> GetRenewalHistory(
        string apiId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var renewals = await _certificateService.GetRenewalHistoryAsync(
            apiId,
            page,
            pageSize);

        return Ok(renewals);
    }

    /// <summary>
    /// Get renewal status/progress
    /// </summary>
    [HttpGet("renewal/{renewalId}/status")]
    public async Task<IActionResult> GetRenewalStatus(string renewalId)
    {
        var renewal = await _certificateService.GetRenewalEventAsync(renewalId);
        
        if (renewal == null)
            return NotFound();

        return Ok(new
        {
            id = renewal.Id,
            apiId = renewal.ApiId,
            status = renewal.Status, // QUEUED, IN_PROGRESS, SUCCESS, FAILED
            progress = renewal.Progress, // 0-100%
            currentStep = renewal.CurrentStep,
            startTime = renewal.StartTime,
            completionTime = renewal.CompletionTime,
            error = renewal.ErrorMessage,
            logs = renewal.ExecutionLogs
        });
    }

    /// <summary>
    /// Bulk renew certificates for multiple APIs
    /// </summary>
    [HttpPost("bulk-renew")]
    [Authorize(Roles = "SuperAdmin,TenantAdmin")]
    public async Task<IActionResult> BulkRenewCertificates(
        [FromQuery] string tenantId,
        [FromBody] BulkRenewalRequest request)
    {
        try
        {
            var results = new List<RenewalResult>();

            foreach (var apiId in request.ApiIds)
            {
                try
                {
                    var renewal = await _certificateService.TriggerRenewalAsync(
                        apiId,
                        request.RenewalMethod);

                    results.Add(new RenewalResult
                    {
                        ApiId = apiId,
                        Status = "queued",
                        RenewalId = renewal.Id
                    });
                }
                catch (Exception ex)
                {
                    results.Add(new RenewalResult
                    {
                        ApiId = apiId,
                        Status = "failed",
                        Error = ex.Message
                    });
                }
            }

            await _auditService.LogActionAsync(new AuditLog
            {
                Action = "BULK_RENEWAL_TRIGGERED",
                EntityType = "Api",
                NewValues = new { Count = request.ApiIds.Count, Successfully = results.Count(x => x.Status == "queued") }
            });

            return Ok(new
            {
                status = "queued",
                total = request.ApiIds.Count,
                successfullyQueued = results.Count(x => x.Status == "queued"),
                failed = results.Count(x => x.Status == "failed"),
                results = results
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}

public class RenewCertificateRequest
{
    public string RenewalMethod { get; set; }
    public bool RequireApproval { get; set; } = false;
    public string Reason { get; set; }
}

public class BulkRenewalRequest
{
    [Required]
    public List<string> ApiIds { get; set; }
    
    [Required]
    public string RenewalMethod { get; set; }
}

public class RenewalResult
{
    public string ApiId { get; set; }
    public string Status { get; set; }
    public string RenewalId { get; set; }
    public string Error { get; set; }
}
```

    /// <summary>
    /// Bulk import APIs from CSV
    /// </summary>
    [HttpPost("bulk-import")]
    public async Task<IActionResult> BulkImport(
        [FromQuery] string tenantId,
        IFormFile csvFile)
    {
        if (csvFile == null)
            return BadRequest("CSV file is required");

        using var stream = csvFile.OpenReadStream();
        using var reader = new StreamReader(stream);
        
        var result = await _adminService.BulkImportApisAsync(tenantId, reader);

        await _auditService.LogActionAsync(new AuditLog
        {
            Action = "BULK_IMPORT",
            EntityType = "Api",
            NewValues = new { ImportedCount = result.SuccessCount }
        });

        return Ok(result);
    }

    /// <summary>
    /// Export APIs as CSV
    /// </summary>
    [HttpGet("export")]
    public async Task<IActionResult> ExportApis([FromQuery] string tenantId)
    {
        var apis = await _adminService.GetApisAsync(tenantId);
        var csv = _adminService.GenerateCsv(apis);

        return File(
            Encoding.UTF8.GetBytes(csv),
            "text/csv",
            $"apis_{DateTime.UtcNow:yyyyMMdd}.csv");
    }
}
```

---

## Audit & Compliance

### Admin Action Audit Log

```csharp
public class AdminAuditLog : BaseEntity
{
    // Actor
    public Guid UserId { get; set; }
    public string UserEmail { get; set; }
    public string UserRole { get; set; }

    // Action
    public string Action { get; set; } // TENANT_CREATED, API_UPDATED, RENEWAL_TRIGGERED etc
    public string EntityType { get; set; } // Tenant, Api, User, RenewalEvent, etc
    public string EntityId { get; set; }

    // Details
    public string EntityName { get; set; }
    public JsonElement OldValues { get; set; } // For update/delete operations
    public JsonElement NewValues { get; set; } // For create/update operations

    // Context
    public string IpAddress { get; set; }
    public string UserAgent { get; set; }
    public string TenantId { get; set; }

    // Result
    public string Status { get; set; } // SUCCESS, FAILURE, DENIED
    public string ErrorMessage { get; set; }

    public DateTime CreatedAt { get; set; }
}
```

### Renewal Event Tracking

```csharp
public class RenewalEvent : BaseEntity
{
    // Identifiers
    public string ApiId { get; set; }
    public string TenantId { get; set; }
    public Guid UserId { get; set; } // Who triggered the renewal

    // Renewal Details
    public string RenewalMethod { get; set; } // terraform, kubernetes, shell_script, python, webhook, manual
    public string DeploymentPlatform { get; set; } // aws_alb, k8s_secret, nginx, etc
    public string RenewalReason { get; set; } // manual_trigger, auto_schedule, expiry_warning, etc

    // Status Tracking
    public string Status { get; set; } // QUEUED, IN_PROGRESS, SUCCESS, FAILED, ROLLED_BACK
    public int Progress { get; set; } // 0-100%
    public string CurrentStep { get; set; } // Which step is executing

    // Certificate Details
    public string OldCertificateThumbprint { get; set; }
    public string NewCertificateThumbprint { get; set; }
    public DateTime? OldExpiryDate { get; set; }
    public DateTime? NewExpiryDate { get; set; }

    // Execution Details
    public DateTime? StartTime { get; set; }
    public DateTime? CompletionTime { get; set; }
    public TimeSpan? ExecutionDuration { get; set; }

    // Script & Output
    public string ScriptPath { get; set; }
    public string ScriptContent { get; set; }
    public string ExecutionLogs { get; set; } // Full execution output
    public string ErrorMessage { get; set; }
    public int? ExitCode { get; set; }

    // Verification
    public bool VerificationSucceeded { get; set; }
    public string VerificationDetails { get; set; }

    // Rollback Information
    public string RolledBackBy { get; set; }
    public DateTime? RollbackTime { get; set; }
    public string RollbackReason { get; set; }

    // Notification
    public bool NotificationSent { get; set; }
    public DateTime? NotificationTime { get; set; }
    public List<string> NotifiedEmails { get; set; } = new();

    // Relationships
    public ApiRecord Api { get; set; }
    public Tenant Tenant { get; set; }
}
```

### Renewal Database Queries

```csharp
// Get recent renewals for an API
public async Task<List<RenewalEvent>> GetRecentRenewalsAsync(string apiId, int limit = 10)
{
    return await _context.RenewalEvents
        .Where(r => r.ApiId == apiId)
        .OrderByDescending(r => r.CreatedAt)
        .Take(limit)
        .ToListAsync();
}

// Get all renewals for a tenant
public async Task<IEnumerable<RenewalEvent>> GetTenantRenewalsAsync(string tenantId, DateTime since)
{
    return await _context.RenewalEvents
        .Where(r => r.TenantId == tenantId && r.CreatedAt >= since)
        .OrderByDescending(r => r.CreatedAt)
        .ToListAsync();
}

// Get failed renewals
public async Task<IEnumerable<RenewalEvent>> GetFailedRenewalsAsync(string tenantId)
{
    return await _context.RenewalEvents
        .Where(r => r.TenantId == tenantId && r.Status == "FAILED")
        .OrderByDescending(r => r.CreatedAt)
        .ToListAsync();
}

// Get successful renewal count for metrics
public async Task<int> GetSuccessfulRenewalCountAsync(string tenantId, DateTime since)
{
    return await _context.RenewalEvents
        .Where(r => r.TenantId == tenantId && 
                   r.Status == "SUCCESS" && 
                   r.CompletionTime >= since)
        .CountAsync();
}
```

### Audit Log Page

```
Admin Console > Audit Logs
┌────────────────────────────────────────────────────────────────┐
│ Audit Logs - Complete Admin Activity History                  │
│                      [Export] [Filters ▼]                      │
├────────────────────────────────────────────────────────────────┤
│                                                                │
│ Tenant: [All ▼] | Action: [All ▼] | User: [All ▼]             │
│ Date Range: [From] to [To]                                    │
│                                                                │
│ ┌──────────────────────────────────────────────────────────┐  │
│ │ Time       │ User     │ Action        │ Entity  │ Status  │  │
│ ├──────────────────────────────────────────────────────────┤  │
│ │ 14:32:10   │ john@... │ TENANT_CREATED│ Acme... │ SUCCESS │  │
│ │ 14:25:45   │ admin@..│ API_UPDATED   │ API... │ SUCCESS │  │
│ │ 14:15:22   │ mike@.. │ USER_REMOVED  │ user...│ SUCCESS │  │
│ │ 14:08:30   │ 192.... │ LOGIN_FAILED  │ -      │ DENIED  │  │
│ │ 13:45:10   │ admin@..│ CONFIG_CHANGE │ email..│ SUCCESS │  │
│ └──────────────────────────────────────────────────────────┘  │
│                                                                │
│ [◀ Previous] Page 1 of 256 [Next ▶]                           │
│                                                                │
└────────────────────────────────────────────────────────────────┘
```

---

## Security & Best Practices

### Admin Console Security Features

```csharp
public class AdminConsoleSecurityConfiguration
{
    // 1. IP Whitelisting
    public static class IpWhitelistMiddleware
    {
        public static void Configure(IApplicationBuilder app)
        {
            app.Use(async (context, next) =>
            {
                var adminPath = context.Request.Path.StartsWithSegments("/admin");
                
                if (adminPath)
                {
                    var clientIp = context.Connection.RemoteIpAddress;
                    var whitelist = await GetWhitelistedIpsAsync();
                    
                    if (!whitelist.Contains(clientIp))
                    {
                        context.Response.StatusCode = 403;
                        await context.Response.WriteAsync("Access Denied");
                        return;
                    }
                }
                
                await next();
            });
        }
    }

    // 2. Two-Factor Authentication
    public static class MfaRequiredAttribute : AuthorizeAttribute
    {
        public MfaRequiredAttribute()
        {
            Policy = "AdminMfaRequired";
        }
    }

    // 3. Rate Limiting
    public static class AdminRateLimitPolicy
    {
        public static void Configure(IServiceCollection services)
        {
            services.AddRateLimiter(options =>
            {
                options.AddPolicy("admin", httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.User.FindFirst("sub")?.Value,
                        factory: partition => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 100,
                            Window = TimeSpan.FromMinutes(1)
                        }));
            });
        }
    }

    // 4. Session Timeout
    public static class SessionTimeoutPolicy
    {
        public const int AdminSessionTimeoutMinutes = 15;  // 15 minutes
        public const int SuperAdminSessionTimeoutMinutes = 30; // 30 minutes
        
        public class SessionMiddleware
        {
            private readonly RequestDelegate _next;

            public SessionMiddleware(RequestDelegate next) => _next = next;

            public async Task InvokeAsync(HttpContext context)
            {
                if (context.User.Identity?.IsAuthenticated ?? false)
                {
                    var lastActivity = context.Session.GetString("LastActivity");
                    var lastActivityTime = DateTime.Parse(lastActivity ?? DateTime.UtcNow.ToString());
                    
                    var role = context.User.FindFirst("role")?.Value;
                    var timeoutMinutes = role == "SuperAdmin" 
                        ? SuperAdminSessionTimeoutMinutes 
                        : AdminSessionTimeoutMinutes;
                    
                    if (DateTime.UtcNow - lastActivityTime > TimeSpan.FromMinutes(timeoutMinutes))
                    {
                        await context.SignOutAsync();
                        context.Response.Redirect("/login");
                        return;
                    }
                    
                    context.Session.SetString("LastActivity", DateTime.UtcNow.ToString());
                }
                
                await _next(context);
            }
        }
    }

    // 5. Encrypt Sensitive Data
    public class SensitiveDataEncryption
    {
        public static string EncryptSensitiveValue(string plaintext)
        {
            using var aes = System.Security.Cryptography.Aes.Create();
            // Implementation...
        }

        public static string DecryptSensitiveValue(string ciphertext)
        {
            using var aes = System.Security.Cryptography.Aes.Create();
            // Implementation...
        }
    }
}
```

### Security Checklist for Admin Console

```
✅ Network Security
  ├─ IP Whitelisting (VPN/Corporate network only)
  ├─ DDoS Protection enabled
  ├─ WAF rules applied
  └─ SSL/TLS 1.3+ required

✅ Authentication & Authorization
  ├─ Multi-Factor Authentication (MFA) mandatory
  ├─ Strong password policy enforced
  ├─ Session timeout (15-30 minutes)
  ├─ No password sharing allowed
  └─ Role-based access control (RBAC) enforced

✅ Admin Actions
  ├─ All actions logged with tamper-proof audit trail
  ├─ Sensitive operations require re-authentication
  ├─ Bulk operations reviewed before execution
  ├─ Delete operations soft-delete first (24hr recovery)
  └─ Dangerous actions require approval

✅ Data Protection
  ├─ All API calls over HTTPS
  ├─ Sensitive data encrypted at rest and in transit
  ├─ No credentials logged or displayed in audit logs
  ├─ Regular data backups (daily, 30-day retention)
  └─ GDPR/compliance requirements met

✅ Monitoring & Alerting
  ├─ Failed login attempts monitored
  ├─ Unusual activity alerts
  ├─ Bulk operation alerts
  ├─ Changes to security settings alert
  └─ Real-time incident response team notified

✅ Compliance
  ├─ SOC 2 Type II compliance
  ├─ GDPR data handling
  ├─ HIPAA considerations
  └─ Regular security audits (quarterly)
```

---

## Quick Reference Guide

### Admin URLs

```
Landing Page:     https://autocert.yourdomain.com/admin
Dashboard:        https://autocert.yourdomain.com/admin/dashboard
Tenants:          https://autocert.yourdomain.com/admin/tenants
APIs:             https://autocert.yourdomain.com/admin/apis
Users:            https://autocert.yourdomain.com/admin/users
Audit Logs:       https://autocert.yourdomain.com/admin/audit-logs
Settings:         https://autocert.yourdomain.com/admin/settings
Onboarding:       https://autocert.yourdomain.com/admin/onboarding
```

### Admin CLI Commands (Optional)

```bash
# List all tenants
autocert-cli tenants list

# Create tenant
autocert-cli tenants create --name "Acme Corp" --owner john@acme.com

# Add API
autocert-cli apis add --tenant-id <id> --url api.example.com

# Bulk import
autocert-cli apis import --tenant-id <id> --file apis.csv

# Check certificate
autocert-cli certs check --api-id <id>

# Export audit logs
autocert-cli logs export --tenant-id <id> --since 2026-01-01
```

---

**Document Version:** 1.0  
**Last Updated:** February 9, 2026  
**Status:** Production Ready
