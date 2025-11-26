using SectigoCertificateManager;
using SectigoCertificateManager.Clients;
using SectigoCertificateManager.Requests;
using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;

namespace SectigoCertificateManager.PowerShell;

/// <summary>Creates a new certificate order.</summary>
/// <para>Builds an API client and submits an <see cref="IssueCertificateRequest"/>.</para>
/// <list type="alertSet">
///   <item>
///     <term>Charges</term>
///     <description>Issuing a certificate may incur costs on your Sectigo account.</description>
///   </item>
/// </list>
/// <seealso href="https://learn.microsoft.com/powershell/scripting/developer/cmdlet/writing-a-cmdlet"/>
/// <seealso href="https://github.com/SectigoCertificateManager/SectigoCertificateManager"/>
[Cmdlet(VerbsCommon.New, "SectigoOrder")]
[CmdletBinding()]
[OutputType(typeof(Models.Certificate))]
public sealed class NewSectigoOrderCommand : AsyncPSCmdlet {
    /// <summary>The API version to use.</summary>
    [Parameter]
    public ApiVersion ApiVersion { get; set; } = ApiVersion.V25_6;

    /// <summary>The certificate common name.</summary>
    [Parameter(Mandatory = true)]
    public string CommonName { get; set; } = string.Empty;

    /// <summary>The profile identifier used for issuance.</summary>
    [Parameter(Mandatory = true)]
    public int ProfileId { get; set; }

    /// <summary>The certificate term in months.</summary>
    [Parameter]
    public int Term { get; set; } = 12;

    /// <summary>Optional subject alternative names.</summary>
    [Parameter]
    [Alias("SubjectAlternativeName", "San")]
    public string[] SubjectAlternativeNames { get; set; } = System.Array.Empty<string>();

    /// <summary>Optional cancellation token.</summary>
    [Parameter]
    public CancellationToken CancellationToken { get; set; }

    /// <summary>Issues a certificate using provided parameters.</summary>
    /// <para>Builds an API client and submits an <see cref="IssueCertificateRequest"/>.</para>
    protected override async Task ProcessRecordAsync() {
        foreach (var san in SubjectAlternativeNames) {
            if (string.IsNullOrWhiteSpace(san)) {
                var ex = new ArgumentException("Value cannot be empty.", nameof(SubjectAlternativeNames));
                var record = new ErrorRecord(ex, "InvalidSubjectAlternativeName", ErrorCategory.InvalidArgument, san);
                ThrowTerminatingError(record);
            }
        }

        if (string.IsNullOrWhiteSpace(CommonName)) {
            var ex = new ArgumentException("Value cannot be empty.", nameof(CommonName));
            var record = new ErrorRecord(ex, "InvalidCommonName", ErrorCategory.InvalidArgument, CommonName);
            ThrowTerminatingError(record);
        }

        var adminConfigObj = SessionState.PSVariable.GetValue("SectigoAdminApiConfig");
        if (adminConfigObj is not null) {
            throw new PSInvalidOperationException("New-SectigoOrder is not yet supported with an Admin (OAuth2) connection. Connect with legacy credentials to use this cmdlet.");
        }

        using var linked = CancellationTokenSource.CreateLinkedTokenSource(CancelToken, CancellationToken);
        var effectiveToken = linked.Token;

        var config = ConnectionHelper.GetLegacyConfig(SessionState);
        ISectigoClient? client = null;
        try {
            client = TestHooks.ClientFactory?.Invoke(config) ?? new SectigoClient(config);
            TestHooks.CreatedClient = client;
            var certificates = new CertificatesClient(client);
            var request = new IssueCertificateRequest {
                CommonName = CommonName,
                ProfileId = ProfileId,
                Term = Term,
                SubjectAlternativeNames = SubjectAlternativeNames
            };
            var certificate = await certificates.IssueAsync(request, effectiveToken)
                .ConfigureAwait(false);
            WriteObject(certificate);
        } finally {
            if (client is IDisposable disposable) {
                disposable.Dispose();
            }
        }
    }
}
