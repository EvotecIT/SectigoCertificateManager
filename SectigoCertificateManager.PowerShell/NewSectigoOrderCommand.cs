using SectigoCertificateManager;
using SectigoCertificateManager.Clients;
using SectigoCertificateManager.Requests;
using System.Management.Automation;
using System.Threading;

namespace SectigoCertificateManager.PowerShell;

/// <summary>Creates a new certificate order.</summary>
/// <para>Builds an API client and submits an <see cref="IssueCertificateRequest"/>.</para>
/// <list type="alertSet">
///   <item>
///     <term>Charges</term>
///     <description>Issuing a certificate may incur costs on your Sectigo account.</description>
///   </item>
/// </list>
/// <example>
///   <summary>Issue a certificate</summary>
///   <prefix>PS&gt; </prefix>
///   <code>New-SectigoOrder -BaseUrl "https://api.example.com" -Username "user" -Password "pass" -CustomerUri "example" -CommonName "www.example.com" -ProfileId 1</code>
///   <para>Creates a one-year certificate for the specified domain.</para>
/// </example>
/// <example>
///   <summary>Include subject alternative names</summary>
///   <prefix>PS&gt; </prefix>
///   <code>New-SectigoOrder -BaseUrl "https://api.example.com" -Username "user" -Password "pass" -CustomerUri "example" -CommonName "www.example.com" -ProfileId 1 -SubjectAlternativeNames "api.example.com","mail.example.com"</code>
///   <para>Issues a certificate that covers multiple host names.</para>
/// </example>
/// <seealso href="https://learn.microsoft.com/powershell/scripting/developer/cmdlet/writing-a-cmdlet"/>
/// <seealso href="https://github.com/SectigoCertificateManager/SectigoCertificateManager"/>
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
    public ApiVersion ApiVersion { get; set; } = ApiVersion.V25_6;

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

    /// <summary>Optional cancellation token.</summary>
    [Parameter]
    public CancellationToken CancellationToken { get; set; }

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
        ISectigoClient? client = null;
        try {
            client = TestHooks.ClientFactory?.Invoke(config) ?? new SectigoClient(config);
            TestHooks.CreatedClient = client;
            var certificates = new CertificatesClient(client);
            var request = new IssueCertificateRequest {
                CommonName = CommonName,
                ProfileId = ProfileId,
                Term = Term,
                SubjectAlternativeNames = SubjectAlternativeNames
            };
            var certificate = certificates.IssueAsync(request, CancellationToken)
                .GetAwaiter()
                .GetResult();
            WriteObject(certificate);
        } finally {
            if (client is IDisposable disposable) {
                disposable.Dispose();
            }
        }
    }
}