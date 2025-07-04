using SectigoCertificateManager;
using SectigoCertificateManager.Clients;
using SectigoCertificateManager.Requests;
using System.Management.Automation;

namespace SectigoCertificateManager.PowerShell;

[Cmdlet(VerbsCommon.New, "SectigoOrder")]
[OutputType(typeof(Models.Certificate))]
public sealed class NewSectigoOrderCommand : PSCmdlet {
    [Parameter(Mandatory = true)]
    public string BaseUrl { get; set; } = string.Empty;

    [Parameter(Mandatory = true)]
    public string Username { get; set; } = string.Empty;

    [Parameter(Mandatory = true)]
    public string Password { get; set; } = string.Empty;

    [Parameter(Mandatory = true)]
    public string CustomerUri { get; set; } = string.Empty;

    [Parameter]
    public ApiVersion ApiVersion { get; set; } = ApiVersion.V25_4;

    [Parameter(Mandatory = true)]
    public string CommonName { get; set; } = string.Empty;

    [Parameter(Mandatory = true)]
    public int ProfileId { get; set; }

    [Parameter]
    public int Term { get; set; } = 12;

    [Parameter]
    public string[] SubjectAlternativeName { get; set; } = System.Array.Empty<string>();

    /// <summary>Issues a certificate using provided parameters.</summary>
    /// <para>Builds an API client and submits an <see cref="IssueCertificateRequest"/>.</para>
    protected override void ProcessRecord() {
        var config = new ApiConfig(BaseUrl, Username, Password, CustomerUri, ApiVersion);
        var client = new SectigoClient(config);
        var certificates = new CertificatesClient(client);
        var request = new IssueCertificateRequest {
            CommonName = CommonName,
            ProfileId = ProfileId,
            Term = Term,
            SubjectAlternativeNames = SubjectAlternativeName
        };
        var certificate = certificates.IssueAsync(request).GetAwaiter().GetResult();
        WriteObject(certificate);
    }
}