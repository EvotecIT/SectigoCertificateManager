using SectigoCertificateManager;
using System.Management.Automation;
using System.Threading;

namespace SectigoCertificateManager.PowerShell;

/// <summary>Retrieves certificate status.</summary>
/// <para>Resolves certificate status using the active Sectigo connection.</para>
/// <list type="alertSet">
///   <item>
///     <term>Network</term>
///     <description>Requests status information from the Sectigo API.</description>
///   </item>
/// </list>
/// <seealso href="https://learn.microsoft.com/powershell/scripting/developer/cmdlet/writing-a-cmdlet"/>
/// <seealso href="https://github.com/SectigoCertificateManager/SectigoCertificateManager"/>
[Cmdlet(VerbsCommon.Get, "SectigoCertificateStatus")]
[CmdletBinding()]
public sealed class GetSectigoCertificateStatusCommand : PSCmdlet {
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
    /// <para>Resolves certificate status through <see cref="CertificateService"/>.</para>
    protected override void ProcessRecord() {
        CertificateService? service = null;
        try {
            if (ConnectionHelper.TryGetAdminConfig(SessionState, out var adminConfig) && adminConfig is not null) {
                service = new CertificateService(adminConfig);
            } else {
                var config = ConnectionHelper.GetLegacyConfig(SessionState);
                service = new CertificateService(config);
            }

            var status = service
                .GetStatusAsync(CertificateId, CancellationToken)
                .GetAwaiter()
                .GetResult();
            WriteObject(status);
        } finally {
            service?.Dispose();
        }
    }
}
