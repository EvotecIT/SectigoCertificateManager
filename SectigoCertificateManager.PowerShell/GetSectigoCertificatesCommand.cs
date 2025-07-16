using SectigoCertificateManager;
using SectigoCertificateManager.Clients;
using SectigoCertificateManager.Requests;
using System.Management.Automation;

namespace SectigoCertificateManager.PowerShell;

/// <summary>Streams certificates matching provided filters.</summary>
/// <para>Creates an API client and outputs certificates found using search.</para>
[Cmdlet(VerbsCommon.Get, "SectigoCertificates")]
[OutputType(typeof(Models.Certificate))]
public sealed class GetSectigoCertificatesCommand : PSCmdlet {
    /// <summary>The API base URL.</summary>
    [Parameter(Mandatory = true)]
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>The user name for authentication.</summary>
    [Parameter(Mandatory = true)]
    public string Username { get; set; } = string.Empty;

    /// <summary>The password for authentication.</summary>
    [Parameter(Mandatory = true)]
    public string Password { get; set; } = string.Empty;

    /// <summary>The customer URI assigned by Sectigo.</summary>
    [Parameter(Mandatory = true)]
    public string CustomerUri { get; set; } = string.Empty;

    /// <summary>The API version to use.</summary>
    [Parameter]
    public ApiVersion ApiVersion { get; set; } = ApiVersion.V25_4;

    /// <summary>The number of results to return.</summary>
    [Parameter]
    public int? Size { get; set; }

    /// <summary>The result offset.</summary>
    [Parameter]
    public int? Position { get; set; }

    /// <summary>The certificate common name filter.</summary>
    [Parameter]
    public string? CommonName { get; set; }

    /// <summary>The subject alternative name filter.</summary>
    [Parameter]
    public string? SubjectAlternativeName { get; set; }

    /// <summary>The certificate status filter.</summary>
    [Parameter]
    public CertificateStatus? Status { get; set; }

    /// <summary>The SSL type identifier filter.</summary>
    [Parameter]
    public int? SslTypeId { get; set; }

    /// <summary>The discovery status filter.</summary>
    [Parameter]
    public string? DiscoveryStatus { get; set; }

    /// <summary>The certificate vendor filter.</summary>
    [Parameter]
    public string? Vendor { get; set; }

    /// <summary>The organization identifier filter.</summary>
    [Parameter]
    public int? OrgId { get; set; }

    /// <summary>The installation status filter.</summary>
    [Parameter]
    public string? InstallStatus { get; set; }

    /// <summary>The renewal status filter.</summary>
    [Parameter]
    public string? RenewalStatus { get; set; }

    /// <summary>The issuer name filter.</summary>
    [Parameter]
    public string? Issuer { get; set; }

    /// <summary>The certificate serial number filter.</summary>
    [Parameter]
    public string? SerialNumber { get; set; }

    /// <summary>The requester name filter.</summary>
    [Parameter]
    public string? Requester { get; set; }

    /// <summary>The external requester name filter.</summary>
    [Parameter]
    public string? ExternalRequester { get; set; }

    /// <summary>The signature algorithm filter.</summary>
    [Parameter]
    public string? SignatureAlgorithm { get; set; }

    /// <summary>The key algorithm filter.</summary>
    [Parameter]
    public string? KeyAlgorithm { get; set; }

    /// <summary>The key size filter.</summary>
    [Parameter]
    public int? KeySize { get; set; }

    /// <summary>The key parameter filter.</summary>
    [Parameter]
    public string? KeyParam { get; set; }

    /// <summary>The SHA1 hash filter.</summary>
    [Parameter]
    public string? Sha1Hash { get; set; }

    /// <summary>The MD5 hash filter.</summary>
    [Parameter]
    public string? Md5Hash { get; set; }

    /// <summary>The key usage filter.</summary>
    [Parameter]
    public string? KeyUsage { get; set; }

    /// <summary>The extended key usage filter.</summary>
    [Parameter]
    public string? ExtendedKeyUsage { get; set; }

    /// <summary>The interface used to request the certificate.</summary>
    [Parameter]
    public string? RequestedVia { get; set; }

    /// <summary>The starting date for the search.</summary>
    [Parameter]
    public DateTime? DateFrom { get; set; }

    /// <summary>The ending date for the search.</summary>
    [Parameter]
    public DateTime? DateTo { get; set; }

    /// <summary>Executes the cmdlet.</summary>
    /// <para>Creates an API client and streams certificates.</para>
    protected override void ProcessRecord() {
        var config = new ApiConfig(BaseUrl, Username, Password, CustomerUri, ApiVersion);
        var client = new SectigoClient(config);
        var certificates = new CertificatesClient(client);
        var request = new CertificateSearchRequest {
            Size = Size,
            Position = Position,
            CommonName = CommonName,
            SubjectAlternativeName = SubjectAlternativeName,
            Status = Status,
            SslTypeId = SslTypeId,
            DiscoveryStatus = DiscoveryStatus,
            Vendor = Vendor,
            OrgId = OrgId,
            InstallStatus = InstallStatus,
            RenewalStatus = RenewalStatus,
            Issuer = Issuer,
            SerialNumber = SerialNumber,
            Requester = Requester,
            ExternalRequester = ExternalRequester,
            SignatureAlgorithm = SignatureAlgorithm,
            KeyAlgorithm = KeyAlgorithm,
            KeySize = KeySize,
            KeyParam = KeyParam,
            Sha1Hash = Sha1Hash,
            Md5Hash = Md5Hash,
            KeyUsage = KeyUsage,
            ExtendedKeyUsage = ExtendedKeyUsage,
            RequestedVia = RequestedVia,
            DateFrom = DateFrom,
            DateTo = DateTo
        };
        var enumerator = certificates.EnumerateSearchAsync(request).GetAsyncEnumerator();

        try {
            while (enumerator.MoveNextAsync().GetAwaiter().GetResult()) {
                WriteObject(enumerator.Current);
            }
        } finally {
            enumerator.DisposeAsync().GetAwaiter().GetResult();
        }
    }
}
