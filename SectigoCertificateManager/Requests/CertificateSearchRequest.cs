namespace SectigoCertificateManager.Requests;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Request used to search for certificates.
/// </summary>
public sealed class CertificateSearchRequest {
    [Range(1, int.MaxValue)]
    public int? Size { get; set; }

    [Range(1, int.MaxValue)]
    public int? Position { get; set; }

    public string? CommonName { get; set; }

    public string? SubjectAlternativeName { get; set; }

    public CertificateStatus? Status { get; set; }

    [Range(1, int.MaxValue)]
    public int? SslTypeId { get; set; }

    public string? DiscoveryStatus { get; set; }

    public string? Vendor { get; set; }

    [Range(1, int.MaxValue)]
    public int? OrgId { get; set; }

    public string? InstallStatus { get; set; }

    public string? RenewalStatus { get; set; }

    public string? Issuer { get; set; }

    public string? SerialNumber { get; set; }

    public string? Requester { get; set; }

    public string? ExternalRequester { get; set; }

    public string? SignatureAlgorithm { get; set; }

    public string? KeyAlgorithm { get; set; }

    [Range(1, int.MaxValue)]
    public int? KeySize { get; set; }

    public string? KeyParam { get; set; }

    public string? Sha1Hash { get; set; }

    public string? Md5Hash { get; set; }

    public string? KeyUsage { get; set; }

    public string? ExtendedKeyUsage { get; set; }

    public string? RequestedVia { get; set; }
}