using SectigoCertificateManager;
using SectigoCertificateManager.Clients;
using System.Management.Automation;

namespace SectigoCertificateManager.PowerShell;

/// <summary>Retrieves a certificate from Sectigo Certificate Manager.</summary>
[Cmdlet(VerbsCommon.Get, "SectigoCertificate")]
[OutputType(typeof(Models.Certificate))]
public sealed class GetSectigoCertificateCommand : PSCmdlet {
    /// <para>Base address of the Sectigo API.</para>
    [Parameter(Mandatory = true)]
    public string BaseUrl { get; set; } = string.Empty;

    /// <para>API username.</para>
    [Parameter(Mandatory = true)]
    public string Username { get; set; } = string.Empty;

    /// <para>API password.</para>
    [Parameter(Mandatory = true)]
    public string Password { get; set; } = string.Empty;

    /// <para>Customer URI associated with the account.</para>
    [Parameter(Mandatory = true)]
    public string CustomerUri { get; set; } = string.Empty;

    /// <para>API version to use.</para>
    [Parameter]
    public ApiVersion ApiVersion { get; set; } = ApiVersion.V25_4;

    /// <para>Identifier of the certificate to retrieve.</para>
    [Parameter(Mandatory = true, Position = 0)]
    public int CertificateId { get; set; }

    /// <summary>Executes the cmdlet.</summary>
    protected override void ProcessRecord() {
        var config = new ApiConfig(BaseUrl, Username, Password, CustomerUri, ApiVersion);
        var client = new SectigoClient(config);
        var certificates = new CertificatesClient(client);
        var certificate = certificates.GetAsync(CertificateId).GetAwaiter().GetResult();
        WriteObject(certificate);
    }
}