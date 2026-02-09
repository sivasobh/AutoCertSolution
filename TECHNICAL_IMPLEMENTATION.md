# AutoCert - Technical Implementation Guide
## .NET 10 Core Components

---

## Table of Contents
1. [Project Structure](#project-structure)
2. [Core Domain Models](#core-domain-models)
3. [Database Context Setup](#database-context-setup)
4. [Service Layer Architecture](#service-layer-architecture)
5. [Certificate Monitoring Service](#certificate-monitoring-service)
6. [Renewal Automation](#renewal-automation)
7. [Email Service](#email-service)
8. [Background Jobs](#background-jobs)

---

## Project Structure

```
AutocertSolution/
├── src/
│   ├── AutoCert.Core/
│   │   ├── Domain/
│   │   │   ├── Entities/
│   │   │   │   ├── User.cs
│   │   │   │   ├── Tenant.cs
│   │   │   │   ├── ApiRecord.cs
│   │   │   │   ├── Certificate.cs
│   │   │   │   ├── AlertRule.cs
│   │   │   │   ├── CertificateHistory.cs
│   │   │   │   └── RenewalEvent.cs
│   │   │   ├── Enums/
│   │   │   │   ├── CertificateStatus.cs
│   │   │   │   ├── UserRole.cs
│   │   │   │   ├── RenewalMethod.cs
│   │   │   │   └── RenewalStatus.cs
│   │   │   └── ValueObjects/
│   │   │       ├── Email.cs
│   │   │       └── Threshold.cs
│   │   ├── Interfaces/
│   │   │   ├── IRepository.cs
│   │   │   ├── IUnitOfWork.cs
│   │   │   ├── ICertificateService.cs
│   │   │   ├── IEmailService.cs
│   │   │   └── IRenewalService.cs
│   │   ├── Services/
│   │   │   ├── DomainServices/
│   │   │   │   └── CertificateValidationService.cs
│   │   │   └── ApplicationServices/
│   │   │       ├── CertificateMonitorService.cs
│   │   │       ├── RenewalService.cs
│   │   │       └── EmailNotificationService.cs
│   │   └── AutoCert.Core.csproj
│   │
│   ├── AutoCert.Infrastructure/
│   │   ├── Data/
│   │   │   ├── Repositories/
│   │   │   │   ├── GenericRepository.cs
│   │   │   │   ├── CertificateRepository.cs
│   │   │   │   ├── UserRepository.cs
│   │   │   │   └── UnitOfWork.cs
│   │   │   ├── Mappings/
│   │   │   │   └── MappingProfile.cs
│   │   │   ├── Migrations/
│   │   │   │   ├── 001_InitialCreate.cs
│   │   │   │   └── 002_AddAuditColumns.cs
│   │   │   ├── ApplicationDbContext.cs
│   │   │   └── DbContextConfiguration.cs
│   │   ├── Services/
│   │   │   ├── CertificateReaderService.cs
│   │   │   ├── EmailServiceImpl.cs
│   │   │   ├── RenewalServiceImpl.cs
│   │   │   └── TerraformExecutionService.cs
│   │   ├── BackgroundJobs/
│   │   │   ├── CertificateCheckJob.cs
│   │   │   ├── RenewalJob.cs
│   │   │   └── HealthCheckJob.cs
│   │   ├── Logging/
│   │   │   └── SerilogConfiguration.cs
│   │   └── AutoCert.Infrastructure.csproj
│   │
│   ├── AutoCert.API/
│   │   ├── Controllers/
│   │   │   ├── AuthController.cs
│   │   │   ├── TenantsController.cs
│   │   │   ├── ApisController.cs
│   │   │   ├── CertificatesController.cs
│   │   │   ├── AlertsController.cs
│   │   │   ├── RenewalsController.cs
│   │   │   └── DashboardController.cs
│   │   ├── Middleware/
│   │   │   ├── ExceptionHandlingMiddleware.cs
│   │   │   ├── TenantMiddleware.cs
│   │   │   ├── AuditLoggingMiddleware.cs
│   │   │   └── JwtAuthenticationMiddleware.cs
│   │   ├── Filters/
│   │   │   ├── AuthorizationFilter.cs
│   │   │   └── ValidationFilter.cs
│   │   ├── DTOs/
│   │   │   ├── Auth/
│   │   │   ├── Tenant/
│   │   │   ├── Api/
│   │   │   ├── Certificate/
│   │   │   └── Alert/
│   │   ├── Validators/
│   │   │   └── [DTOs Validators]
│   │   ├── Program.cs
│   │   ├── Dockerfile
│   │   ├── appsettings.json
│   │   ├── appsettings.Development.json
│   │   └── AutoCert.API.csproj
│   │
│   ├── AutoCert.WebApp/
│   │   ├── Pages/
│   │   │   ├── Index.razor
│   │   │   ├── Login.razor
│   │   │   ├── Dashboard.razor
│   │   │   ├── APIs/
│   │   │   ├── Certificates/
│   │   │   └── Alerts/
│   │   ├── Components/
│   │   │   ├── Layout/
│   │   │   ├── Dashboard/
│   │   │   └── Shared/
│   │   ├── Services/
│   │   │   ├── AuthService.cs
│   │   │   ├── ApiService.cs
│   │   │   └── CertificateService.cs
│   │   ├── Program.cs
│   │   ├── Dockerfile
│   │   ├── appsettings.json
│   │   ├── wwwroot/
│   │   └── AutoCert.WebApp.csproj
│   │
│   └── AutoCert.AutomationService/
│       ├── Workers/
│       │   ├── CertificateMonitorWorker.cs
│       │   ├── RenewalWorker.cs
│       │   └── HealthCheckWorker.cs
│       ├── Services/
│       │   ├── CertificateCheckService.cs
│       │   ├── TerraformService.cs
│       │   └── PythonRenewalService.cs
│       ├── Program.cs
│       ├── Dockerfile
│       ├── appsettings.json
│       └── AutoCert.AutomationService.csproj
│
├── tests/
│   ├── AutoCert.Tests/
│   │   ├── Unit/
│   │   │   ├── Services/
│   │   │   ├── Domain/
│   │   │   └── Validators/
│   │   ├── Integration/
│   │   │   ├── RepositoryTests.cs
│   │   │   └── ServiceTests.cs
│   │   └── AutoCert.Tests.csproj
│   └── AutoCert.IntegrationTests/
│       └── AutoCert.IntegrationTests.csproj
│
├── docs/
│   ├── API/
│   └── Architecture/
│
├── scripts/
│   ├── init-db.sql
│   ├── seed-data.sql
│   └── backup-db.sh
│
├── docker-compose.yml
├── docker-compose.prod.yml
├── Dockerfile
├── BUILD_INSTRUCTIONS.md
├── CONTAINERIZATION_GUIDE.md
├── DEVELOPMENT_PLAN.md
└── README.md
```

---

## Core Domain Models

### User Entity
```csharp
public class User : BaseEntity
{
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    
    // SSO Support
    public string? SsoProvider { get; set; }
    public string? SsoId { get; set; }
    
    // Account Status
    public bool IsActive { get; set; }
    public DateTime? LastLogin { get; set; }
    
    // Relationships
    public ICollection<TenantUser> TenantUsers { get; set; }
    public ICollection<AuditLog> AuditLogs { get; set; }
}

public class BaseEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
}
```

### Tenant Entity
```csharp
public class Tenant : BaseEntity
{
    public string Name { get; set; }
    public string Description { get; set; }
    public Guid OwnerId { get; set; }
    public bool IsActive { get; set; }
    
    // Relationships
    public User Owner { get; set; }
    public ICollection<TenantUser> TenantUsers { get; set; }
    public ICollection<ApiRecord> Apis { get; set; }
    public ICollection<AlertRule> AlertRules { get; set; }
}

public class TenantUser
{
    public Guid TenantUserId { get; set; }
    public Guid TenantId { get; set; }
    public Guid UserId { get; set; }
    public UserRole Role { get; set; }
    
    // Relationships
    public Tenant Tenant { get; set; }
    public User User { get; set; }
    public DateTime CreatedAt { get; set; }
}

public enum UserRole
{
    Admin,
    Manager,
    Viewer
}
```

### API Record Entity
```csharp
public class ApiRecord : BaseEntity
{
    public Guid TenantId { get; set; }
    public string Name { get; set; }
    public string Url { get; set; }
    public int? Port { get; set; }
    public string Protocol { get; set; } // HTTP, HTTPS
    public string Description { get; set; }
    public bool IsMonitored { get; set; }
    public int CheckIntervalMinutes { get; set; } // Default: 1440 (1 day)
    
    // Relationships
    public Tenant Tenant { get; set; }
    public ICollection<Certificate> Certificates { get; set; }
    public ICollection<AlertRule> AlertRules { get; set; }
}
```

### Certificate Entity
```csharp
public class Certificate : BaseEntity
{
    public Guid ApiRecordId { get; set; }
    public Guid TenantId { get; set; }
    
    // Certificate Info
    public string Thumbprint { get; set; }
    public string Subject { get; set; }
    public string Issuer { get; set; }
    public DateTime IssuedDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public string CertificateData { get; set; } // PEM format
    
    // Status (Green, Yellow, Red)
    public CertificateStatus Status { get; set; }
    
    // Monitoring
    public DateTime? LastChecked { get; set; }
    public DateTime? RenewalAttemptedAt { get; set; }
    public RenewalStatus? RenewalStatus { get; set; }
    
    // Relationships
    public ApiRecord ApiRecord { get; set; }
    public Tenant Tenant { get; set; }
    public ICollection<CertificateHistory> History { get; set; }
    public ICollection<RenewalEvent> RenewalEvents { get; set; }
    
    // Computed Properties
    public int DaysUntilExpiry => (ExpiryDate - DateTime.UtcNow).Days;
    public bool IsExpired => DateTime.UtcNow > ExpiryDate;
}

public enum CertificateStatus
{
    Green,    // Valid (>30 days)
    Yellow,   // Expiring (1-30 days)
    Red       // Expired or <1 day
}

public enum RenewalStatus
{
    Pending,
    Success,
    Failed
}
```

### Alert Rule Entity
```csharp
public class AlertRule : BaseEntity
{
    public Guid TenantId { get; set; }
    public Guid? ApiRecordId { get; set; } // Null = applies to all APIs
    public string Name { get; set; }
    public int ThresholdDays { get; set; } // Trigger when X days until expiry
    public string EmailRecipients { get; set; } // Comma-separated
    public bool IsActive { get; set; }
    
    // Relationships
    public Tenant Tenant { get; set; }
    public ApiRecord ApiRecord { get; set; }
}

public class CertificateHistory : BaseEntity
{
    public Guid CertificateId { get; set; }
    public string EventType { get; set; } // CHECKED, RENEWED, EXPIRED
    public CertificateStatus? OldStatus { get; set; }
    public CertificateStatus? NewStatus { get; set; }
    public DateTime? OldExpiryDate { get; set; }
    public DateTime? NewExpiryDate { get; set; }
    
    // Relationships
    public Certificate Certificate { get; set; }
}

public class RenewalEvent
{
    public Guid EventId { get; set; }
    public Guid CertificateId { get; set; }
    public OracleDateTime EventDate { get; set; }
    public RenewalStatus Status { get; set; }
    public RenewalMethod RenewalMethod { get; set; }
    public string ErrorMessage { get; set; }
    public Guid? ExecutedBy { get; set; }
    
    // Relationships
    public Certificate Certificate { get; set; }
}

public enum RenewalMethod
{
    Manual,
    Terraform,
    Python,
    Automated
}
```

---

## Database Context Setup

### ApplicationDbContext.cs
```csharp
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // DbSets
    public DbSet<User> Users { get; set; }
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<TenantUser> TenantUsers { get; set; }
    public DbSet<ApiRecord> ApiRecords { get; set; }
    public DbSet<Certificate> Certificates { get; set; }
    public DbSet<AlertRule> AlertRules { get; set; }
    public DbSet<CertificateHistory> CertificateHistory { get; set; }
    public DbSet<RenewalEvent> RenewalEvents { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User Configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.HasIndex(e => e.Email).IsUnique();
        });

        // Tenant Configuration
        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.HasOne(e => e.Owner)
                .WithMany()
                .HasForeignKey(e => e.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // TenantUser Configuration (Many-to-Many)
        modelBuilder.Entity<TenantUser>(entity =>
        {
            entity.HasKey(e => e.TenantUserId);
            entity.HasOne(e => e.Tenant)
                .WithMany(t => t.TenantUsers)
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.User)
                .WithMany(u => u.TenantUsers)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.TenantId, e.UserId }).IsUnique();
        });

        // ApiRecord Configuration
        modelBuilder.Entity<ApiRecord>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.Url).IsRequired().HasMaxLength(2048);
            entity.HasOne(e => e.Tenant)
                .WithMany(t => t.Apis)
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Certificate Configuration
        modelBuilder.Entity<Certificate>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Thumbprint).HasMaxLength(255);
            entity.HasIndex(e => e.Thumbprint).IsUnique();
            entity.HasIndex(e => new { e.TenantId, e.Status });
            entity.HasIndex(e => e.ExpiryDate);
            entity.HasOne(e => e.ApiRecord)
                .WithMany(a => a.Certificates)
                .HasForeignKey(e => e.ApiRecordId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Tenant)
                .WithMany()
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // AlertRule Configuration
        modelBuilder.Entity<AlertRule>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Tenant)
                .WithMany(t => t.AlertRules)
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.ApiRecord)
                .WithMany(a => a.AlertRules)
                .HasForeignKey(e => e.ApiRecordId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditFields();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateAuditFields()
    {
        var entries = ChangeTracker.Entries<BaseEntity>();
        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }
    }
}
```

### Program.cs Configuration
```csharp
var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptions => npgsqlOptions.MigrationsAssembly("AutoCert.Infrastructure")));

// Add services
builder.Services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ICertificateService, CertificateService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IRenewalService, RenewalService>();

// Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"])),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"]
        };
    });

// AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// API Documentation
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AutoCert API",
        Version = "v1",
        Description = "Certificate Monitoring and Renewal Platform"
    });
});

// Background Jobs
builder.Services.AddQuartz(q =>
{
    q.AddJob<CertificateCheckJob>(opts => opts.WithIdentity("CertificateCheckJob"));
    q.AddTrigger(opts => opts
        .ForJob("CertificateCheckJob")
        .WithIdentity("CertificateCheckTrigger")
        .WithCronSchedule("0 0 * * * ?") // Daily
    );
});

builder.Services.AddQuartzHostedService(opts =>
    opts.WaitForJobsToComplete = true);

var app = builder.Build();

// Middleware
app.UseSwagger();
app.UseSwaggerUI();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<AuditLoggingMiddleware>();
app.UseMiddleware<TenantMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Database Migration
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.MigrateAsync();
}

app.Run();
```

---

## Service Layer Architecture

### Certificate Service
```csharp
public interface ICertificateService
{
    Task<CertificateDto> GetCertificateAsync(Guid certificateId, Guid tenantId);
    Task<IEnumerable<CertificateDto>> GetTenantCertificatesAsync(Guid tenantId);
    Task<CertificateDto> CheckCertificateAsync(Guid apiRecordId, Guid tenantId);
    Task UpdateCertificateStatusAsync(Guid certificateId, CertificateStatus status);
    Task TriggerRenewalAsync(Guid certificateId, Guid tenantId, Guid userId);
}

public class CertificateService : ICertificateService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CertificateService> _logger;
    private readonly IEmailService _emailService;

    public CertificateService(
        IUnitOfWork unitOfWork,
        ILogger<CertificateService> logger,
        IEmailService emailService)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _emailService = emailService;
    }

    public async Task<CertificateDto> CheckCertificateAsync(
        Guid apiRecordId,
        Guid tenantId)
    {
        try
        {
            var apiRecord = await _unitOfWork.ApiRecords
                .GetByIdAsync(apiRecordId);

            if (apiRecord == null || apiRecord.TenantId != tenantId)
                throw new UnauthorizedAccessException();

            // Extract certificate from URL/API
            var certificate = ExtractCertificate(apiRecord.Url, apiRecord.Port);

            // Determine status
            var status = DetermineCertificateStatus(certificate.ExpiryDate);

            // Update database
            var existingCert = await _unitOfWork.Certificates
                .FirstOrDefaultAsync(c =>
                    c.ApiRecordId == apiRecordId &&
                    c.Thumbprint == certificate.Thumbprint);

            if (existingCert != null)
            {
                var oldStatus = existingCert.Status;
                existingCert.Status = status;
                existingCert.LastChecked = DateTime.UtcNow;

                await _unitOfWork.Certificates.UpdateAsync(existingCert);

                // Log history
                if (oldStatus != status)
                {
                    await LogCertificateHistoryAsync(
                        existingCert.Id,
                        "CHECKED",
                        oldStatus,
                        status);

                    // Trigger alerts if needed
                    await TriggerAlertsAsync(apiRecord.TenantId, apiRecordId, status);
                }
            }
            else
            {
                var newCert = new Certificate
                {
                    ApiRecordId = apiRecordId,
                    TenantId = tenantId,
                    Thumbprint = certificate.Thumbprint,
                    Subject = certificate.Subject,
                    Issuer = certificate.Issuer,
                    IssuedDate = certificate.IssuedDate,
                    ExpiryDate = certificate.ExpiryDate,
                    CertificateData = certificate.PemData,
                    Status = status,
                    LastChecked = DateTime.UtcNow
                };

                await _unitOfWork.Certificates.AddAsync(newCert);
            }

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Certificate checked for API {ApiId} in tenant {TenantId}",
                apiRecordId, tenantId);

            return MapToDto(existingCert);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error checking certificate for API {ApiId}",
                apiRecordId);
            throw;
        }
    }

    private CertificateStatus DetermineCertificateStatus(DateTime expiryDate)
    {
        var daysUntilExpiry = (expiryDate - DateTime.UtcNow).Days;

        return daysUntilExpiry switch
        {
            < 1 => CertificateStatus.Red,      // Expired or expiring today
            <= 30 => CertificateStatus.Yellow, // Expiring within 30 days
            _ => CertificateStatus.Green       // More than 30 days
        };
    }

    private async Task TriggerAlertsAsync(
        Guid tenantId,
        Guid apiRecordId,
        CertificateStatus status)
    {
        var rules = await _unitOfWork.AlertRules
            .Where(r =>
                r.TenantId == tenantId &&
                (r.ApiRecordId == apiRecordId || r.ApiRecordId == null) &&
                r.IsActive)
            .ToListAsync();

        foreach (var rule in rules)
        {
            if (ShouldTriggerAlert(rule, status))
            {
                await _emailService.SendAlertAsync(rule, apiRecordId, status);
            }
        }
    }

    private bool ShouldTriggerAlert(AlertRule rule, CertificateStatus status)
    {
        return (status == CertificateStatus.Red ||
                status == CertificateStatus.Yellow);
    }

    private ExternalCertificate ExtractCertificate(
        string url,
        int? port)
    {
        // Implementation to extract SSL/TLS certificate from URL
        // Using System.Net.Security
        var host = new Uri(url).Host;
        var portNumber = port ?? 443;

        using (var tcpClient = new TcpClient())
        {
            tcpClient.Connect(host, portNumber);

            using (var sslStream = new SslStream(tcpClient.GetStream(), false))
            {
                sslStream.AuthenticateAsClient(host);
                var cert = new X509Certificate2(sslStream.RemoteCertificate);

                return new ExternalCertificate
                {
                    Thumbprint = cert.Thumbprint,
                    Subject = cert.Subject,
                    Issuer = cert.Issuer,
                    IssuedDate = DateTime.Parse(cert.GetEffectiveDateString()),
                    ExpiryDate = DateTime.Parse(cert.GetExpirationDateString()),
                    PemData = cert.ExportToPem() // .NET 5+
                };
            }
        }
    }
}
```

---

## Certificate Monitoring Service

### Scheduled Certificate Check Job
```csharp
[DisallowConcurrentExecution]
public class CertificateCheckJob : IJob
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICertificateService _certificateService;
    private readonly ILogger<CertificateCheckJob> _logger;

    public CertificateCheckJob(
        IUnitOfWork unitOfWork,
        ICertificateService certificateService,
        ILogger<CertificateCheckJob> logger)
    {
        _unitOfWork = unitOfWork;
        _certificateService = certificateService;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Starting certificate check job at {0}", DateTime.UtcNow);

        try
        {
            // Get all active monitored APIs
            var apisToCheck = await _unitOfWork.ApiRecords
                .Where(a => a.IsMonitored)
                .ToListAsync();

            foreach (var api in apisToCheck)
            {
                try
                {
                    await _certificateService.CheckCertificateAsync(
                        api.Id,
                        api.TenantId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Error checking certificate for API {ApiId}",
                        api.Id);
                }
            }

            _logger.LogInformation(
                "Certificate check job completed. Checked {Count} APIs",
                apisToCheck.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Certificate check job failed");
            throw;
        }
    }
}
```

---

## Renewal Automation

### Terraform-based Renewal
```csharp
public interface IRenewalService
{
    Task<RenewalEvent> RenewWithTerraformAsync(
        Guid certificateId,
        Guid tenantId,
        Guid userId);
    Task<RenewalEvent> RenewWithPythonAsync(
        Guid certificateId,
        Guid tenantId,
        Guid userId);
    Task<RenewalEvent> ManualRenewAsync(
        Guid certificateId,
        string newCertData,
        Guid userId);
}

public class RenewalService : IRenewalService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RenewalService> _logger;
    private readonly IConfiguration _configuration;

    public async Task<RenewalEvent> RenewWithTerraformAsync(
        Guid certificateId,
        Guid tenantId,
        Guid userId)
    {
        var @event = new RenewalEvent
        {
            EventId = Guid.NewGuid(),
            CertificateId = certificateId,
            EventDate = DateTime.UtcNow,
            RenewalMethod = RenewalMethod.Terraform,
            ExecutedBy = userId,
            Status = RenewalStatus.Pending
        };

        try
        {
            var certificate = await _unitOfWork.Certificates
                .GetByIdAsync(certificateId);

            if (certificate.TenantId != tenantId)
                throw new UnauthorizedAccessException();

            // Execute Terraform
            var terraformPath = _configuration["Terraform:WorkingDirectory"];
            var result = await ExecuteTerraformRenewal(
                terraformPath,
                certificate.ApiRecord.Url);

            if (result.Success)
            {
                // Update certificate with new data
                certificate.CertificateData = result.NewCertificateData;
                certificate.ExpiryDate = result.NewExpiryDate;
                certificate.RenewalAttemptedAt = DateTime.UtcNow;
                certificate.Status = CertificateStatus.Green;

                await _unitOfWork.Certificates.UpdateAsync(certificate);
                @event.Status = RenewalStatus.Success;

                _logger.LogInformation(
                    "Successfully renewed certificate {CertId} using Terraform",
                    certificateId);
            }
            else
            {
                @event.Status = RenewalStatus.Failed;
                @event.ErrorMessage = result.ErrorMessage;

                _logger.LogWarning(
                    "Terraform renewal failed for certificate {CertId}: {Error}",
                    certificateId, result.ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            @event.Status = RenewalStatus.Failed;
            @event.ErrorMessage = ex.Message;

            _logger.LogError(ex,
                "Error during Terraform renewal for certificate {CertId}",
                certificateId);
        }

        await _unitOfWork.RenewalEvents.AddAsync(@event);
        await _unitOfWork.SaveChangesAsync();

        return @event;
    }

    private async Task<TerraformResult> ExecuteTerraformRenewal(
        string workingDirectory,
        string apiUrl)
    {
        var processInfo = new ProcessStartInfo
        {
            FileName = "terraform",
            Arguments = "apply -auto-approve -var=\"api_url=" + apiUrl + "\"",
            WorkingDirectory = workingDirectory,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        using (var process = Process.Start(processInfo))
        {
            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            // Parse output and extract new certificate data
            return ParseTerraformOutput(output, error, process.ExitCode);
        }
    }
}
```

### Python-based Renewal
```csharp
public class PythonRenewalService
{
    private readonly ILogger<PythonRenewalService> _logger;
    private readonly IConfiguration _configuration;

    public async Task<RenewalEvent> RenewWithPythonAsync(
        Guid certificateId,
        Guid tenantId,
        Guid userId)
    {
        var @event = new RenewalEvent
        {
            EventId = Guid.NewGuid(),
            CertificateId = certificateId,
            EventDate = DateTime.UtcNow,
            RenewalMethod = RenewalMethod.Python,
            ExecutedBy = userId,
            Status = RenewalStatus.Pending
        };

        try
        {
            var pythonScript = _configuration["Python:RenewalScriptPath"];
            var pythonExecutable = _configuration["Python:Executable"];

            var processInfo = new ProcessStartInfo
            {
                FileName = pythonExecutable,
                Arguments = $"\"{pythonScript}\" --cert-id {certificateId}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using (var process = Process.Start(processInfo))
            {
                var output = await process.StandardOutput.ReadToEndAsync();
                var error = await process.StandardError.ReadToEndAsync();
                await process.WaitForExitAsync();

                if (process.ExitCode == 0)
                {
                    @event.Status = RenewalStatus.Success;
                    _logger.LogInformation(
                        "Python renewal successful for certificate {CertId}",
                        certificateId);
                }
                else
                {
                    @event.Status = RenewalStatus.Failed;
                    @event.ErrorMessage = error;

                    _logger.LogWarning(
                        "Python renewal failed: {Error}",
                        error);
                }
            }
        }
        catch (Exception ex)
        {
            @event.Status = RenewalStatus.Failed;
            @event.ErrorMessage = ex.Message;

            _logger.LogError(ex,
                "Error during Python renewal");
        }

        return @event;
    }
}
```

---

## Email Service

```csharp
public interface IEmailService
{
    Task SendAlertAsync(AlertRule rule, Guid apiRecordId, CertificateStatus status);
    Task SendRenewalNotificationAsync(RenewalEvent @event);
}

public class EmailService : IEmailService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<EmailService> _logger;
    private readonly SmtpClient _smtpClient;
    private readonly IConfiguration _configuration;

    public EmailService(
        IUnitOfWork unitOfWork,
        ILogger<EmailService> logger,
        IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _configuration = configuration;

        // Initialize SMTP
        _smtpClient = new SmtpClient(configuration["Email:SmtpServer"])
        {
            Port = int.Parse(configuration["Email:SmtpPort"]),
            Credentials = new NetworkCredential(
                configuration["Email:Username"],
                configuration["Email:Password"]),
            EnableSsl = true
        };
    }

    public async Task SendAlertAsync(
        AlertRule rule,
        Guid apiRecordId,
        CertificateStatus status)
    {
        try
        {
            var apiRecord = await _unitOfWork.ApiRecords
                .GetByIdAsync(apiRecordId);

            var certificate = await _unitOfWork.Certificates
                .Where(c => c.ApiRecordId == apiRecordId)
                .OrderByDescending(c => c.CreatedAt)
                .FirstOrDefaultAsync();

            var recipients = rule.EmailRecipients.Split(',');

            foreach (var recipient in recipients)
            {
                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_configuration["Email:FromAddress"]),
                    Subject = $"Certificate Alert: {apiRecord.Name}",
                    Body = BuildAlertEmailBody(apiRecord, certificate, status),
                    IsBodyHtml = true
                };

                mailMessage.To.Add(recipient.Trim());

                await _smtpClient.SendMailAsync(mailMessage);

                // Log email
                var emailLog = new EmailLog
                {
                    LogId = Guid.NewGuid(),
                    ApiRecordId = apiRecordId,
                    CertificateId = certificate?.Id,
                    RecipientEmail = recipient.Trim(),
                    Subject = mailMessage.Subject,
                    Body = mailMessage.Body,
                    Status = "SENT",
                    SentAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.EmailLogs.AddAsync(emailLog);

                mailMessage.Dispose();
            }

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Alert email sent to {Recipients} for API {ApiId}",
                string.Join(",", recipients), apiRecordId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error sending alert email for API {ApiId}",
                apiRecordId);
            throw;
        }
    }

    private string BuildAlertEmailBody(
        ApiRecord apiRecord,
        Certificate certificate,
        CertificateStatus status)
    {
        var statusColor = status switch
        {
            CertificateStatus.Red => "red",
            CertificateStatus.Yellow => "orange",
            _ => "green"
        };

        var statusText = status switch
        {
            CertificateStatus.Red => "EXPIRED or EXPIRING SOON",
            CertificateStatus.Yellow => "EXPIRING SOON",
            _ => "VALID"
        };

        return $@"
<html>
<body>
    <h2>Certificate Alert - {apiRecord.Name}</h2>
    <p>API: {apiRecord.Url}</p>
    <p>Status: <span style='color: {statusColor}; font-weight: bold;'>{statusText}</span></p>
    <p>Expires: {certificate?.ExpiryDate:yyyy-MM-dd}</p>
    <p>Days Until Expiry: <strong>{certificate?.DaysUntilExpiry}</strong></p>
    <p><a href='https://yourapp.com/certificates/{certificate?.Id}'>View Details</a></p>
</body>
</html>";
    }
}
```

---

## Configuration

### appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "User ID=autocert;Password=changeme;Host=postgres;Port=5432;Database=autocert_db;Pooling=true;"
  },
  "Jwt": {
    "SecretKey": "your-secret-key-min-32-characters",
    "Issuer": "autocert-api",
    "Audience": "autocert-app",
    "ExpirationMinutes": 60
  },
  "Email": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "FromAddress": "noreply@autocert.com",
    "Username": "your-email@gmail.com",
    "Password": "your-app-password"
  },
  "Terraform": {
    "WorkingDirectory": "/app/terraform",
    "Executable": "terraform"
  },
  "Python": {
    "Executable": "python3",
    "RenewalScriptPath": "/app/scripts/renew_certificate.py"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  },
  "Serilog": {
    "MinimumLevel": "Information",
    "WriteTo": [
      {
        "Name": "Console"
      },
      {
        "Name": "File",
        "Args": {
          "path": "/app/logs/autocert-.txt",
          "rollingInterval": "Day"
        }
      }
    ]
  }
}
```

---

This guide provides the foundation for implementing the AutoCert platform. Each component can be developed incrementally following the development roadmap.
