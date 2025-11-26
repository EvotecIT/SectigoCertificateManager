using SectigoCertificateManager;
using System.Management.Automation;
using System.Threading;

namespace SectigoCertificateManager.PowerShell;

/// <summary>Retrieves certificate revocation information.</summary>
/// <para>Resolves revocation details for the specified certificate using the active Sectigo connection.</para>
/// <list type="alertSet">
///   <item>
///     <term>Network</term>
///     <description>Requests revocation data from the Sectigo API.</description>
///   </item>
/// </list>
/// <seealso href="https://learn.microsoft.com/powershell/scripting/developer/cmdlet/writing-a-cmdlet"/>
/// <seealso href="https://github.com/SectigoCertificateManager/SectigoCertificateManager"/>
[Cmdlet(VerbsCommon.Get, "SectigoCertificateRevocation")]
[CmdletBinding()]
[OutputType(typeof(Models.CertificateRevocation))]
public sealed class GetSectigoCertificateRevocationCommand : PSCmdlet {
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
    /// <para>Resolves certificate revocation details through <see cref="CertificateService"/>.</para>
    protected override void ProcessRecord() {
        CertificateService? service = null;
        try {
            if (ConnectionHelper.TryGetAdminConfig(SessionState, out var adminConfig)) {
                service = new CertificateService(adminConfig!);
            } else {
                var config = ConnectionHelper.GetLegacyConfig(SessionState);
                service = new CertificateService(config);
            }

            var revocation = service
                .GetRevocationAsync(CertificateId, CancellationToken)
                .GetAwaiter()
                .GetResult();
            WriteObject(revocation);
        } finally {
            service?.Dispose();
        }
    }
}
