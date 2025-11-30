using SectigoCertificateManager;
using System;
using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;

namespace SectigoCertificateManager.PowerShell;

/// <summary>Retrieves a keystore download link for a certificate.</summary>
/// <para>
/// Uses the Admin Operations API via <see cref="CertificateService"/> to create a
/// keystore download link for the specified certificate. This cmdlet requires an
/// Admin (OAuth2) connection created with <c>Connect-Sectigo -ClientId/-ClientSecret</c>.
/// </para>
/// <list type="alertSet">
///   <item>
///     <term>Admin only</term>
///     <description>This cmdlet is not available when connected to the legacy SCM API.</description>
///   </item>
/// </list>
/// <example>
///   <summary>Get a PKCS#12 keystore link</summary>
///   <prefix>PS&gt; </prefix>
///   <code>Connect-Sectigo -ClientId "&lt;client id&gt;" -ClientSecret "&lt;client secret&gt;"; Get-SectigoCertificateKeystoreLink -CertificateId 12345 -FormatType p12</code>
///   <para>Returns a download link for a PKCS#12 keystore containing the specified certificate.</para>
/// </example>
[Cmdlet(VerbsCommon.Get, "SectigoCertificateKeystoreLink")]
[CmdletBinding()]
[OutputType(typeof(string))]
public sealed class GetSectigoCertificateKeystoreLinkCommand : AsyncPSCmdlet {
    /// <summary>The certificate identifier.</summary>
    [Parameter(Mandatory = true, Position = 0)]
    public int CertificateId { get; set; }

    /// <summary>Keystore format type.</summary>
    [Parameter(Mandatory = true)]
    public KeystoreFormatType FormatType { get; set; } = KeystoreFormatType.P12;

    /// <summary>Optional passphrase used to protect the keystore.</summary>
    [Parameter]
    public string? Passphrase { get; set; }

    /// <summary>Optional cancellation token.</summary>
    [Parameter]
    public CancellationToken CancellationToken { get; set; }

    /// <summary>Executes the cmdlet.</summary>
    /// <para>Creates a keystore download link using the Admin Operations API.</para>
    protected override async Task ProcessRecordAsync() {
        if (CertificateId <= 0) {
            var ex = new ArgumentOutOfRangeException(nameof(CertificateId));
            var record = new ErrorRecord(ex, "InvalidCertificateId", ErrorCategory.InvalidArgument, CertificateId);
            ThrowTerminatingError(record);
        }

        if (!ConnectionHelper.TryGetAdminConfig(SessionState, out var adminConfig) || adminConfig == null) {
            var ex = new PSInvalidOperationException("Get-SectigoCertificateKeystoreLink is only supported with an Admin (OAuth2) connection. Use Connect-Sectigo -ClientId / -ClientSecret first.");
            var record = new ErrorRecord(ex, "AdminConnectionRequired", ErrorCategory.InvalidOperation, null);
            ThrowTerminatingError(record);
        }

        using var linked = CancellationTokenSource.CreateLinkedTokenSource(CancelToken, CancellationToken);
        var effectiveToken = linked.Token;

        WriteVerbose($"Requesting keystore download link for certificate Id={CertificateId} with format '{FormatType}' using the Admin API.");
        var service = new CertificateService(adminConfig!);
        var link = await service
            .CreateKeystoreDownloadLinkAsync(CertificateId, FormatType, Passphrase, effectiveToken)
            .ConfigureAwait(false);
        WriteObject(link);
    }
}
