using SectigoCertificateManager;
using System;
using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;

namespace SectigoCertificateManager.PowerShell;

/// <summary>Deletes (or revokes) a certificate.</summary>
/// <para>Uses the active Sectigo connection to remove a certificate, revoking it when using the Admin API.</para>
/// <list type="alertSet">
///   <item>
///     <term>Irreversible</term>
///     <description>Deleting a certificate cannot be undone.</description>
///   </item>
/// </list>
/// <example>
///   <summary>Delete a certificate</summary>
///   <prefix>PS&gt; </prefix>
///   <code>Connect-Sectigo -BaseUrl "https://cert-manager.com/api" -Username "user" -Password "pass" -CustomerUri "example"; Remove-SectigoCertificate -CertificateId 10</code>
///   <para>Permanently removes certificate 10 for the connected account.</para>
/// </example>
/// <seealso href="https://learn.microsoft.com/powershell/scripting/developer/cmdlet/shouldprocess-attribute"/>
/// <seealso href="https://github.com/SectigoCertificateManager/SectigoCertificateManager"/>
[Cmdlet(VerbsCommon.Remove, "SectigoCertificate", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
[CmdletBinding()]
public sealed class RemoveSectigoCertificateCommand : AsyncPSCmdlet {
    /// <summary>The API version to use when calling the legacy API.</summary>
    [Parameter]
    public ApiVersion ApiVersion { get; set; } = ApiVersion.V25_6;

    /// <summary>The identifier of the certificate to delete.</summary>
    [Parameter(Mandatory = true, Position = 0)]
    public int CertificateId { get; set; }

    /// <summary>Optional cancellation token.</summary>
    [Parameter]
    public CancellationToken CancellationToken { get; set; }

    /// <summary>Deletes a certificate.</summary>
    /// <para>Removes the specified certificate using the active connection.</para>
    protected override async Task ProcessRecordAsync() {
        if (CertificateId <= 0) {
            var ex = new ArgumentOutOfRangeException(nameof(CertificateId));
            var record = new ErrorRecord(ex, "InvalidCertificateId", ErrorCategory.InvalidArgument, CertificateId);
            ThrowTerminatingError(record);
        }

        if (!ShouldProcess($"Certificate {CertificateId}", "Delete")) {
            return;
        }

        using var linked = CancellationTokenSource.CreateLinkedTokenSource(CancelToken, CancellationToken);
        var effectiveToken = linked.Token;

        CertificateService? service = null;
        try {
            if (ConnectionHelper.TryGetAdminConfig(SessionState, out var adminConfig) && adminConfig is not null) {
                service = new CertificateService(adminConfig);
            } else {
                var config = ConnectionHelper.GetLegacyConfig(SessionState);
                service = new CertificateService(config);
            }

            await service
                .RemoveAsync(CertificateId, cancellationToken: effectiveToken)
                .ConfigureAwait(false);
        } finally {
            service?.Dispose();
        }
    }
}
