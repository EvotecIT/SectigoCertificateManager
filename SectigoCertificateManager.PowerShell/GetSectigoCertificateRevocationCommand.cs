using SectigoCertificateManager;
using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;

namespace SectigoCertificateManager.PowerShell;

/// <summary>Retrieves certificate revocation information.</summary>
/// <para>Resolves revocation details for the specified certificate using the active Sectigo connection.</para>
/// <example>
///   <summary>Retrieve revocation details for a certificate</summary>
///   <code>Get-SectigoCertificateRevocation -CertificateId 12345</code>
///   <para>Returns revocation information for the certificate through the active connection.</para>
/// </example>
/// <list type="alertSet">
///   <item>
///     <term>Network</term>
///     <description>Requests revocation data from the Sectigo API.</description>
///   </item>
/// </list>
/// <seealso href="https://learn.microsoft.com/powershell/scripting/developer/cmdlet/writing-a-cmdlet"/>
/// <seealso href="https://github.com/EvotecIT/SectigoCertificateManager"/>
[Cmdlet(VerbsCommon.Get, "SectigoCertificateRevocation")]
[CmdletBinding()]
[OutputType(typeof(Models.CertificateRevocation))]
public sealed class GetSectigoCertificateRevocationCommand : AsyncPSCmdlet {
    /// <summary>The API version to use.</summary>
    [Parameter]
    [PSDefaultValue(Value = ApiVersion.V25_6)]
    public ApiVersion ApiVersion { get; set; } = ApiVersion.V25_6;

    /// <summary>The certificate identifier.</summary>
    [Parameter(Mandatory = true, Position = 0)]
    public int CertificateId { get; set; }

    /// <summary>Optional cancellation token.</summary>
    [Parameter]
    public CancellationToken CancellationToken { get; set; }

    /// <summary>Executes the cmdlet.</summary>
    /// <para>Resolves certificate revocation details through <see cref="CertificateService"/>.</para>
    protected override async Task ProcessRecordAsync() {
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(CancelToken, CancellationToken);
        var effectiveToken = linked.Token;

        CertificateService? service = null;
        var usingAdmin = false;
        try {
            if (ConnectionHelper.TryGetAdminConfig(SessionState, out var adminConfig) && adminConfig is not null) {
                service = new CertificateService(adminConfig);
                usingAdmin = true;
            } else {
                var config = ConnectionHelper.GetLegacyConfig(SessionState);
                service = new CertificateService(config);
            }

            WriteVerbose(usingAdmin
                ? $"Retrieving revocation details for certificate Id={CertificateId} using the Admin API."
                : $"Retrieving revocation details for certificate Id={CertificateId} using the legacy API.");

            var revocation = await service
                .GetRevocationAsync(CertificateId, effectiveToken)
                .ConfigureAwait(false);
            WriteObject(revocation);
        } finally {
            service?.Dispose();
        }
    }
}
