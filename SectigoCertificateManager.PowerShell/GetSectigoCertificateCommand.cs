using SectigoCertificateManager;
using SectigoCertificateManager.Clients;
using System.Management.Automation;
using System.Threading;

namespace SectigoCertificateManager.PowerShell;

/// <summary>Retrieves certificate details.</summary>
/// <para>Builds an API client and returns the certificate for the specified identifier.</para>
/// <list type="alertSet">
///   <item>
///     <term>Network</term>
///     <description>Queries the Sectigo API to fetch certificate data.</description>
///   </item>
/// </list>
/// <example>
///   <summary>Get a certificate</summary>
///   <prefix>PS&gt; </prefix>
///   <code>Get-SectigoCertificate -BaseUrl "https://api.example.com" -Username "user" -Password "pass" -CustomerUri "example" -CertificateId 10</code>
///   <para>Retrieves details for certificate identifier 10.</para>
/// </example>
/// <example>
///   <summary>Retrieve using a different API version</summary>
///   <prefix>PS&gt; </prefix>
///   <code>Get-SectigoCertificate -BaseUrl "https://api.example.com" -Username "user" -Password "pass" -CustomerUri "example" -CertificateId 10 -ApiVersion V25_5</code>
///   <para>Uses API version 25.5 to obtain the certificate.</para>
/// </example>
/// <seealso href="https://learn.microsoft.com/powershell/scripting/developer/cmdlet/writing-a-cmdlet"/>
/// <seealso href="https://github.com/SectigoCertificateManager/SectigoCertificateManager"/>
[Cmdlet(VerbsCommon.Get, "SectigoCertificate")]
[CmdletBinding()]
[OutputType(typeof(Models.Certificate))]
public sealed class GetSectigoCertificateCommand : PSCmdlet {
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

    /// <summary>The certificate identifier.</summary>
    [Parameter(Mandatory = true, Position = 0)]
    public int CertificateId { get; set; }

    /// <summary>Optional cancellation token.</summary>
    [Parameter]
    public CancellationToken CancellationToken { get; set; }

    /// <summary>Executes the cmdlet.</summary>
    /// <para>Creates an API client and retrieves the certificate.</para>
    protected override void ProcessRecord() {
        ISectigoClient? client = null;
        try {
            var config = new ApiConfig(BaseUrl, Username, Password, CustomerUri, ApiVersion);
            client = TestHooks.ClientFactory?.Invoke(config) ?? new SectigoClient(config);
            TestHooks.CreatedClient = client;
            var certificates = new CertificatesClient(client);
            var certificate = certificates.GetAsync(CertificateId, CancellationToken)
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