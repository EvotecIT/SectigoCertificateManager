namespace SectigoCertificateManager.AdminApi;

using System.Collections.Generic;

/// <summary>
/// Request payload for <c>/api/ssl/v2/enroll</c>.
/// </summary>
public sealed class AdminSslEnrollRequest {
    /// <summary>Gets or sets the organization identifier.</summary>
    public int OrgId { get; set; }

    /// <summary>Gets or sets the subject alternative names (comma separated).</summary>
    public string? SubjAltNames { get; set; }

    /// <summary>Gets or sets the certificate profile identifier.</summary>
    public int CertType { get; set; }

    /// <summary>Gets or sets the certificate validity period in days.</summary>
    public int Term { get; set; }

    /// <summary>Gets or sets comments for the request.</summary>
    public string? Comments { get; set; }

    /// <summary>Gets or sets the external requester description or e-mail.</summary>
    public string? ExternalRequester { get; set; }

    /// <summary>Gets or sets the DCV mode.</summary>
    public string? DcvMode { get; set; }

    /// <summary>Gets or sets the DCV e-mail address.</summary>
    public string? DcvEmail { get; set; }

    /// <summary>Gets or sets the certificate signing request.</summary>
    public string Csr { get; set; } = string.Empty;
}

/// <summary>
/// Request payload for <c>/api/ssl/v2/enroll-keygen</c>.
/// </summary>
public sealed class AdminSslEnrollKeyGenRequest {
    /// <summary>Gets or sets the organization identifier.</summary>
    public int OrgId { get; set; }

    /// <summary>Gets or sets the subject alternative names (comma separated).</summary>
    public string? SubjAltNames { get; set; }

    /// <summary>Gets or sets the certificate profile identifier.</summary>
    public int CertType { get; set; }

    /// <summary>Gets or sets the certificate validity period in days.</summary>
    public int Term { get; set; }

    /// <summary>Gets or sets comments for the request.</summary>
    public string? Comments { get; set; }

    /// <summary>Gets or sets the external requester description or e-mail.</summary>
    public string? ExternalRequester { get; set; }

    /// <summary>Gets or sets the DCV mode.</summary>
    public string? DcvMode { get; set; }

    /// <summary>Gets or sets the DCV e-mail address.</summary>
    public string? DcvEmail { get; set; }

    /// <summary>Gets or sets the certificate common name.</summary>
    public string? CommonName { get; set; }

    /// <summary>Gets or sets the pass phrase used to protect generated PKCS#12 keystores.</summary>
    public string? PassPhrase { get; set; }

    /// <summary>Gets or sets the key size (for RSA) or curve parameter (for EC).</summary>
    public int? KeySize { get; set; }

    /// <summary>Gets or sets the key parameter (size or curve name).</summary>
    public string? KeyParam { get; set; }

    /// <summary>Gets or sets the key algorithm.</summary>
    public string? Algorithm { get; set; }

    /// <summary>Gets or sets the key generation method.</summary>
    public string? KeyGenerationMethod { get; set; }

    /// <summary>Gets or sets a value indicating whether the key is reusable.</summary>
    public bool? ReuseKey { get; set; }

    /// <summary>Gets or sets a value indicating whether the key is exportable.</summary>
    public bool? ExportableKey { get; set; }

    /// <summary>Gets or sets a value indicating whether the key must be generated on HSM.</summary>
    public bool? HsmOnly { get; set; }
}

/// <summary>
/// Response payload returned by enrollment endpoints.
/// </summary>
public sealed class AdminSslEnrollResponse {
    /// <summary>Gets or sets the SSL identifier.</summary>
    public int SslId { get; set; }

    /// <summary>Gets or sets the renewal identifier.</summary>
    public string? RenewId { get; set; }
}

