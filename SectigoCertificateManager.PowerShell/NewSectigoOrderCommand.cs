using SectigoCertificateManager;
using SectigoCertificateManager.Clients;
using SectigoCertificateManager.Requests;
using System.Management.Automation;

namespace SectigoCertificateManager.PowerShell;

/// <summary>Creates a new certificate order.</summary>
/// <para>Builds an API client and submits an <see cref="IssueCertificateRequest"/>.</para>
[Cmdlet(VerbsCommon.New, "SectigoOrder")]
[CmdletBinding()]
[OutputType(typeof(Models.Certificate))]
public sealed class NewSectigoOrderCommand : PSCmdlet {
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

    /// <summary>The certificate common name.</summary>
    [Parameter(Mandatory = true)]
    public string CommonName { get; set; } = string.Empty;

    /// <summary>The profile identifier used for issuance.</summary>
    [Parameter(Mandatory = true)]
    public int ProfileId { get; set; }

    /// <summary>The certificate term in months.</summary>
    [Parameter]
    public int Term { get; set; } = 12;

    /// <summary>Optional subject alternative names.</summary>
    [Parameter]
    [Alias("SubjectAlternativeName", "San")]
    public string[] SubjectAlternativeNames { get; set; } = System.Array.Empty<string>();

    /// <summary>Issues a certificate using provided parameters.</summary>
    /// <para>Builds an API client and submits an <see cref="IssueCertificateRequest"/>.</para>
    protected override void ProcessRecord() {
        foreach (var san in SubjectAlternativeNames) {
            if (string.IsNullOrWhiteSpace(san)) {
                var ex = new ArgumentException("Value cannot be empty.", nameof(SubjectAlternativeNames));
                var record = new ErrorRecord(ex, "InvalidSubjectAlternativeName", ErrorCategory.InvalidArgument, san);
                ThrowTerminatingError(record);
            }
        }

        if (string.IsNullOrWhiteSpace(CommonName)) {
            var ex = new ArgumentException("Value cannot be empty.", nameof(CommonName));
            var record = new ErrorRecord(ex, "InvalidCommonName", ErrorCategory.InvalidArgument, CommonName);
            ThrowTerminatingError(record);
        }

        var config = new ApiConfig(BaseUrl, Username, Password, CustomerUri, ApiVersion);
        var client = new SectigoClient(config);
        var certificates = new CertificatesClient(client);
        var request = new IssueCertificateRequest {
            CommonName = CommonName,
            ProfileId = ProfileId,
            Term = Term,
            SubjectAlternativeNames = SubjectAlternativeNames
        };
        var certificate = certificates.IssueAsync(request).GetAwaiter().GetResult();
        WriteObject(certificate);
    }
}