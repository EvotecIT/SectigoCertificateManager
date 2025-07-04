namespace SectigoCertificateManager.Models;

/// <summary>
/// Represents auto-renewal options for a certificate.
/// </summary>
public sealed class AutoRenewDetails
{
    /// <summary>Gets or sets the auto-renewal state.</summary>
    public string? State { get; set; }

    /// <summary>Gets or sets days before expiration to start auto-renewal.</summary>
    public int? DaysBeforeExpiration { get; set; }
}
