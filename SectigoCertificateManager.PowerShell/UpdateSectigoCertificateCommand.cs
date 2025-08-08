using SectigoCertificateManager;
using SectigoCertificateManager.Clients;
using SectigoCertificateManager.Requests;
using System.Management.Automation;
using System.Threading;

namespace SectigoCertificateManager.PowerShell;

/// <summary>Renews an existing certificate.</summary>
/// <para>Builds an API client and submits a <see cref="RenewCertificateRequest"/> to the renew endpoint.</para>
/// <list type="alertSet">
///   <item>
///     <term>Network</term>
///     <description>Contacts the Sectigo API and replaces the specified certificate.</description>
///   </item>
/// </list>
/// <example>
///   <summary>Renew a certificate</summary>
///   <prefix>PS&gt; </prefix>
///   <code>Update-SectigoCertificate -BaseUrl "https://api.example.com" -Username "user" -Password "pass" -CustomerUri "example" -CertificateId 10 -Csr "CSR" -DcvMode "Email"</code>
///   <para>Renews certificate 10 using the provided signing request.</para>
/// </example>
/// <example>
///   <summary>Specify a DCV email</summary>
///   <prefix>PS&gt; </prefix>
///   <code>Update-SectigoCertificate -BaseUrl "https://api.example.com" -Username "user" -Password "pass" -CustomerUri "example" -CertificateId 10 -Csr "CSR" -DcvMode "Email" -DcvEmail "admin@example.com"</code>
///   <para>Sends the domain control validation to a specific address.</para>
/// </example>
/// <seealso href="https://learn.microsoft.com/powershell/scripting/developer/cmdlet/writing-a-cmdlet"/>
/// <seealso href="https://github.com/SectigoCertificateManager/SectigoCertificateManager"/>
[Cmdlet(VerbsData.Update, "SectigoCertificate", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
[CmdletBinding()]
[OutputType(typeof(int))]
public sealed class UpdateSectigoCertificateCommand : PSCmdlet {
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

    /// <summary>The identifier of the certificate to renew.</summary>
    [Parameter(Mandatory = true, Position = 0)]
    public int CertificateId { get; set; }

    /// <summary>The certificate signing request.</summary>
    [Parameter(Mandatory = true)]
    public string Csr { get; set; } = string.Empty;

    /// <summary>The domain control validation mode.</summary>
    [Parameter(Mandatory = true)]
    public string DcvMode { get; set; } = string.Empty;

    /// <summary>The domain control validation email address.</summary>
    [Parameter]
    public string? DcvEmail { get; set; }

    /// <summary>Optional cancellation token.</summary>
    [Parameter]
    public CancellationToken CancellationToken { get; set; }

    /// <summary>Renews a certificate using provided parameters.</summary>
    /// <para>Builds an API client and submits a <see cref="RenewCertificateRequest"/>.</para>
    protected override void ProcessRecord() {
        if (!ShouldProcess($"Certificate {CertificateId}", "Update")) {
            return;
        }

        var config = new ApiConfig(BaseUrl, Username, Password, CustomerUri, ApiVersion);
        ISectigoClient? client = null;
        try {
            client = TestHooks.ClientFactory?.Invoke(config) ?? new SectigoClient(config);
            TestHooks.CreatedClient = client;
            var certificates = new CertificatesClient(client);
            var request = new RenewCertificateRequest {
                Csr = Csr,
                DcvMode = DcvMode,
                DcvEmail = DcvEmail
            };
            var newId = certificates.RenewAsync(CertificateId, request, CancellationToken)
                .GetAwaiter()
                .GetResult();
            WriteObject(newId);
        } finally {
            if (client is IDisposable disposable) {
                disposable.Dispose();
            }
        }
    }
}