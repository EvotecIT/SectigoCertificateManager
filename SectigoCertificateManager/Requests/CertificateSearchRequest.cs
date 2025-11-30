namespace SectigoCertificateManager.Requests;

/// <summary>
/// Request used to search for certificates.
/// </summary>
public sealed class CertificateSearchRequest {
    /// <summary>Gets or sets the number of results to return.</summary>
    public int? Size { get; set; }

    /// <summary>Gets or sets the result offset.</summary>
    public int? Position { get; set; }

    /// <summary>Gets or sets the certificate common name filter.</summary>
    public string? CommonName { get; set; }

    /// <summary>Gets or sets the subject alternative name filter.</summary>
    public string? SubjectAlternativeName { get; set; }

    /// <summary>Gets or sets the certificate status filter.</summary>
    public CertificateStatus? Status { get; set; }

    /// <summary>Gets or sets the SSL type identifier filter.</summary>
    public int? SslTypeId { get; set; }

    /// <summary>Gets or sets the discovery status filter (deprecated in favour of RequestedVia).</summary>
    public DiscoveryStatus? DiscoveryStatus { get; set; }

    /// <summary>Gets or sets the certificate vendor filter.</summary>
    public string? Vendor { get; set; }

    /// <summary>Gets or sets the organization identifier filter.</summary>
    public int? OrgId { get; set; }

    /// <summary>Gets or sets the installation status filter.</summary>
    public InstallStatus? InstallStatus { get; set; }

    /// <summary>Gets or sets the renewal status filter.</summary>
    public RenewalStatus? RenewalStatus { get; set; }

    /// <summary>Gets or sets the issuer name filter.</summary>
    public string? Issuer { get; set; }

    /// <summary>Gets or sets the issuer distinguished name filter.</summary>
    public string? IssuerDn { get; set; }

    /// <summary>Gets or sets the certificate serial number filter.</summary>
    public string? SerialNumber { get; set; }

    /// <summary>Gets or sets the requester name filter.</summary>
    public string? Requester { get; set; }

    /// <summary>Gets or sets the external requester name filter.</summary>
    public string? ExternalRequester { get; set; }

    /// <summary>Gets or sets the signature algorithm filter.</summary>
    public string? SignatureAlgorithm { get; set; }

    /// <summary>Gets or sets the key algorithm filter.</summary>
    public string? KeyAlgorithm { get; set; }

    /// <summary>Gets or sets the key size filter.</summary>
    public int? KeySize { get; set; }

    /// <summary>Gets or sets the key parameter filter.</summary>
    public string? KeyParam { get; set; }

    /// <summary>Gets or sets the SHA1 hash filter.</summary>
    public string? Sha1Hash { get; set; }

    /// <summary>Gets or sets the MD5 hash filter.</summary>
    public string? Md5Hash { get; set; }

    /// <summary>Gets or sets the key usage filter.</summary>
    public string? KeyUsage { get; set; }

    /// <summary>Gets or sets the extended key usage filter.</summary>
    public string? ExtendedKeyUsage { get; set; }

    /// <summary>Gets or sets the interface used to request the certificate.</summary>
    public string? RequestedVia { get; set; }

    /// <summary>Gets or sets the starting date for the search.</summary>
    public DateTime? DateFrom { get; set; }

    /// <summary>Gets or sets the ending date for the search.</summary>
    public DateTime? DateTo { get; set; }
}
