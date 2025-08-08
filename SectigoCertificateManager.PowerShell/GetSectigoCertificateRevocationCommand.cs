using SectigoCertificateManager;
using SectigoCertificateManager.Clients;
using System.Management.Automation;
using System.Threading;

namespace SectigoCertificateManager.PowerShell;

/// <summary>Retrieves certificate revocation information.</summary>
/// <para>Creates an API client and returns revocation details for the specified certificate.</para>
/// <list type="alertSet">
///   <item>
///     <term>Network</term>
///     <description>Requests revocation data from the Sectigo API.</description>
///   </item>
/// </list>
/// <example>
///   <summary>Check revocation status</summary>
///   <prefix>PS&gt; </prefix>
///   <code>Get-SectigoCertificateRevocation -BaseUrl "https://api.example.com" -Username "user" -Password "pass" -CustomerUri "example" -CertificateId 10</code>
///   <para>Retrieves revocation details for certificate 10.</para>
/// </example>
/// <example>
///   <summary>Use another API version</summary>
///   <prefix>PS&gt; </prefix>
///   <code>Get-SectigoCertificateRevocation -BaseUrl "https://api.example.com" -Username "user" -Password "pass" -CustomerUri "example" -CertificateId 10 -ApiVersion V25_5</code>
///   <para>Queries revocation information using API version 25.5.</para>
/// </example>
/// <seealso href="https://learn.microsoft.com/powershell/scripting/developer/cmdlet/writing-a-cmdlet"/>
/// <seealso href="https://github.com/SectigoCertificateManager/SectigoCertificateManager"/>
[Cmdlet(VerbsCommon.Get, "SectigoCertificateRevocation")]
[CmdletBinding()]
[OutputType(typeof(Models.CertificateRevocation))]
public sealed class GetSectigoCertificateRevocationCommand : PSCmdlet {
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
    /// <para>Creates an API client and retrieves certificate revocation details.</para>
    protected override void ProcessRecord() {
        ISectigoClient? client = null;
        try {
            var config = new ApiConfig(BaseUrl, Username, Password, CustomerUri, ApiVersion);
            client = TestHooks.ClientFactory?.Invoke(config) ?? new SectigoClient(config);
            TestHooks.CreatedClient = client;
            var certificates = new CertificatesClient(client);
            var revocation = certificates.GetRevocationAsync(CertificateId, CancellationToken)
                .GetAwaiter()
                .GetResult();
            WriteObject(revocation);
        } finally {
            if (client is IDisposable disposable) {
                disposable.Dispose();
            }
        }
    }
}