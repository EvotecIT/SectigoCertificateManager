using SectigoCertificateManager;
using SectigoCertificateManager.Requests;
using System.Management.Automation;
using System.Threading;

namespace SectigoCertificateManager.PowerShell;

/// <summary>Renews an existing certificate.</summary>
/// <para>Uses the active Sectigo connection and submits a <see cref="RenewCertificateRequest"/> to the appropriate renew endpoint.</para>
/// <list type="alertSet">
///   <item>
///     <term>Network</term>
///     <description>Contacts the Sectigo API and replaces the specified certificate.</description>
///   </item>
/// </list>
/// <seealso href="https://learn.microsoft.com/powershell/scripting/developer/cmdlet/writing-a-cmdlet"/>
/// <seealso href="https://github.com/SectigoCertificateManager/SectigoCertificateManager"/>
[Cmdlet(VerbsData.Update, "SectigoCertificate", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
[CmdletBinding()]
[OutputType(typeof(int))]
public sealed class UpdateSectigoCertificateCommand : PSCmdlet {
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
    /// <para>Submits a <see cref="RenewCertificateRequest"/> using the active connection.</para>
    protected override void ProcessRecord() {
        if (!ShouldProcess($"Certificate {CertificateId}", "Update")) {
            return;
        }

        CertificateService? service = null;
        try {
            if (ConnectionHelper.TryGetAdminConfig(SessionState, out var adminConfig) && adminConfig is not null) {
                service = new CertificateService(adminConfig);
            } else {
                var config = ConnectionHelper.GetLegacyConfig(SessionState);
                service = new CertificateService(config);
            }

            var request = new RenewCertificateRequest {
                Csr = Csr,
                DcvMode = DcvMode,
                DcvEmail = DcvEmail
            };

            var newId = service
                .RenewByIdAsync(CertificateId, request, CancellationToken)
                .GetAwaiter()
                .GetResult();
            WriteObject(newId);
        } finally {
            service?.Dispose();
        }
    }
}
