namespace SectigoCertificateManager.AdminApi;

using System.Collections.Generic;

/// <summary>
/// Represents a device certificate entry returned by the Admin API device list endpoint.
/// </summary>
public sealed class AdminDeviceIdentity {
    /// <summary>Gets or sets the device certificate identifier.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the certificate status.</summary>
    public string? Status { get; set; }

    /// <summary>
    /// Gets or sets the backend certificate identifier. This identifier should not be used
    /// for certificate operations and is provided for migration scenarios only.
    /// </summary>
    public string? BackendCertId { get; set; }

    /// <summary>Gets or sets additional certificate fingerprint and subject details.</summary>
    public AdminDeviceCertificateDetails? CertificateDetails { get; set; }
}

