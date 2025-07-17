using SectigoCertificateManager;
using SectigoCertificateManager.Clients;
using System.Management.Automation;

namespace SectigoCertificateManager.PowerShell;

/// <summary>Retrieves certificate revocation information.</summary>
/// <para>Creates an API client and returns revocation details for the specified certificate.</para>
[Cmdlet(VerbsCommon.Get, "SectigoCertificateRevocation")]
[CmdletBinding()]
[OutputType(typeof(Models.CertificateRevocation))]
public sealed class GetSectigoCertificateRevocationCommand : PSCmdlet
{
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

    /// <summary>The certificate identifier.</summary>
    [Parameter(Mandatory = true, Position = 0)]
    public int CertificateId { get; set; }

    /// <summary>Executes the cmdlet.</summary>
    /// <para>Creates an API client and retrieves certificate revocation details.</para>
    protected override void ProcessRecord()
    {
        var config = new ApiConfig(BaseUrl, Username, Password, CustomerUri, ApiVersion);
        var client = new SectigoClient(config);
        var certificates = new CertificatesClient(client);
        var revocation = certificates.GetRevocationAsync(CertificateId).GetAwaiter().GetResult();
        WriteObject(revocation);
    }
}
