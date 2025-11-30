namespace SectigoCertificateManager.Requests;

/// <summary>
/// Request payload used for manual SSL renewals in the Admin API.
/// </summary>
public sealed class AdminSslManualRenewRequest {
    /// <summary>
    /// Gets or sets the certificate identifier.
    /// When not set or less than or equal to zero, the identifier from the URL is used.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the order number to associate with the renewal.
    /// </summary>
    public string? OrderNumber { get; set; }

    /// <summary>
    /// Gets or sets the DCV mode for domain validation.
    /// </summary>
    public DcvMode DcvMode { get; set; }

    /// <summary>
    /// Gets or sets the DCV email address, when email validation is used.
    /// </summary>
    public string? DcvEmail { get; set; }
}
