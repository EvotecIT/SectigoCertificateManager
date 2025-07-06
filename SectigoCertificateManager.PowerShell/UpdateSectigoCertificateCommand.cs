using SectigoCertificateManager;
using SectigoCertificateManager.Clients;
using SectigoCertificateManager.Requests;
using System.Management.Automation;

namespace SectigoCertificateManager.PowerShell;

[Cmdlet(VerbsData.Update, "SectigoCertificate")]
[OutputType(typeof(int))]
public sealed class UpdateSectigoCertificateCommand : PSCmdlet {
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

    [Parameter(Mandatory = true, Position = 0)]
    public int CertificateId { get; set; }

    [Parameter(Mandatory = true)]
    public string Csr { get; set; } = string.Empty;

    [Parameter(Mandatory = true)]
    public string DcvMode { get; set; } = string.Empty;

    [Parameter]
    public string? DcvEmail { get; set; }

    /// <summary>Renews a certificate using provided parameters.</summary>
    /// <para>Builds an API client and submits a <see cref="RenewCertificateRequest"/>.</para>
    protected override void ProcessRecord() {
        var config = new ApiConfig(BaseUrl, Username, Password, CustomerUri, ApiVersion);
        var client = new SectigoClient(config);
        var certificates = new CertificatesClient(client);
        var request = new RenewCertificateRequest {
            Csr = Csr,
            DcvMode = DcvMode,
            DcvEmail = DcvEmail
        };
        var newId = certificates.RenewAsync(CertificateId, request).GetAwaiter().GetResult();
        WriteObject(newId);
    }
}
