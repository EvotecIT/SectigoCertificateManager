using SectigoCertificateManager;
using SectigoCertificateManager.Clients;
using System;
using System.Management.Automation;
using System.Threading;

namespace SectigoCertificateManager.PowerShell;

/// <summary>Deletes a certificate.</summary>
/// <para>Builds an API client and calls the delete endpoint.</para>
/// <list type="alertSet">
///   <item>
///     <term>Irreversible</term>
///     <description>Deleting a certificate cannot be undone.</description>
///   </item>
/// </list>
/// <example>
///   <summary>Delete a certificate</summary>
///   <prefix>PS&gt; </prefix>
///   <code>Remove-SectigoCertificate -BaseUrl "https://api.example.com" -Username "user" -Password "pass" -CustomerUri "example" -CertificateId 10</code>
///   <para>Permanently removes certificate 10.</para>
/// </example>
/// <example>
///   <summary>Use a different API version</summary>
///   <prefix>PS&gt; </prefix>
///   <code>Remove-SectigoCertificate -BaseUrl "https://api.example.com" -Username "user" -Password "pass" -CustomerUri "example" -CertificateId 10 -ApiVersion V25_5</code>
///   <para>Deletes the certificate using API version 25.5.</para>
/// </example>
/// <seealso href="https://learn.microsoft.com/powershell/scripting/developer/cmdlet/shouldprocess-attribute"/>
/// <seealso href="https://github.com/SectigoCertificateManager/SectigoCertificateManager"/>
[Cmdlet(VerbsCommon.Remove, "SectigoCertificate", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
[CmdletBinding()]
public sealed class RemoveSectigoCertificateCommand : PSCmdlet {
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

    /// <summary>The identifier of the certificate to delete.</summary>
    [Parameter(Mandatory = true, Position = 0)]
    public int CertificateId { get; set; }

    /// <summary>Optional cancellation token.</summary>
    [Parameter]
    public CancellationToken CancellationToken { get; set; }

    /// <summary>Deletes a certificate.</summary>
    /// <para>Builds an API client and calls the delete endpoint.</para>
    protected override void ProcessRecord() {
        if (CertificateId <= 0) {
            var ex = new ArgumentOutOfRangeException(nameof(CertificateId));
            var record = new ErrorRecord(ex, "InvalidCertificateId", ErrorCategory.InvalidArgument, CertificateId);
            ThrowTerminatingError(record);
        }

        if (!ShouldProcess($"Certificate {CertificateId}", "Delete")) {
            return;
        }

        var config = new ApiConfig(BaseUrl, Username, Password, CustomerUri, ApiVersion);
        ISectigoClient? client = null;
        try {
            client = TestHooks.ClientFactory?.Invoke(config) ?? new SectigoClient(config);
            TestHooks.CreatedClient = client;
            var certificates = new CertificatesClient(client);
            certificates.DeleteAsync(CertificateId, CancellationToken)
                .GetAwaiter()
                .GetResult();
        } finally {
            if (client is IDisposable disposable) {
                disposable.Dispose();
            }
        }
    }
}