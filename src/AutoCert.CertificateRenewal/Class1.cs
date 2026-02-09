namespace AutoCert.CertificateRenewal;

/// <summary>
/// Service for managing certificate renewal operations.
/// </summary>
public interface ICertificateRenewalService
{
    /// <summary>
    /// Renews certificates that are approaching expiration.
    /// </summary>
    Task RenewCertificatesAsync(CancellationToken cancellationToken = default);
}
