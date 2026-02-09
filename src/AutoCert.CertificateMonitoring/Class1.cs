namespace AutoCert.CertificateMonitoring;

/// <summary>
/// Service for monitoring certificate expiration and status.
/// </summary>
public interface ICertificateMonitoringService
{
    /// <summary>
    /// Monitors all certificates for expiration status.
    /// </summary>
    Task MonitorCertificatesAsync(CancellationToken cancellationToken = default);
}
