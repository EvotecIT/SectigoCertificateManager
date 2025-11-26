namespace SectigoCertificateManager.AdminApi;

/// <summary>
/// Represents the response returned by device enrollment and renewal endpoints.
/// </summary>
public sealed class AdminDeviceEnrollResponse {
    /// <summary>Gets or sets the device certificate identifier.</summary>
    public int DeviceCertId { get; set; }

    /// <summary>Gets or sets the certificate status.</summary>
    public string? Status { get; set; }
}

