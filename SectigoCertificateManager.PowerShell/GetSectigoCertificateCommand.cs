using System.Management.Automation;
using SectigoCertificateManager;
using SectigoCertificateManager.Clients;

namespace SectigoCertificateManager.PowerShell;

[Cmdlet(VerbsCommon.Get, "SectigoCertificate")]
[OutputType(typeof(Models.Certificate))]
public sealed class GetSectigoCertificateCommand : PSCmdlet
{
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

    protected override void ProcessRecord()
    {
        var config = new ApiConfig(BaseUrl, Username, Password, CustomerUri, ApiVersion);
        var client = new SectigoClient(config);
        var certificates = new CertificatesClient(client);
        var certificate = certificates.GetAsync(CertificateId).GetAwaiter().GetResult();
        WriteObject(certificate);
    }
}
