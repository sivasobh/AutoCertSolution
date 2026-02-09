namespace AutoCert.WebhookHandler;

/// <summary>
/// Handler for processing webhook events from certificate providers.
/// </summary>
public interface IWebhookProcessor
{
    /// <summary>
    /// Processes incoming webhook events.
    /// </summary>
    Task ProcessWebhookAsync(object webhookData, CancellationToken cancellationToken = default);
}
