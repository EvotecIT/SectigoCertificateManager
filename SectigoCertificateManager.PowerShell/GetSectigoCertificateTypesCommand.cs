using SectigoCertificateManager;
using SectigoCertificateManager.Clients;
using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;

namespace SectigoCertificateManager.PowerShell;

/// <summary>Retrieves available certificate types.</summary>
/// <para>Lists certificate types for the account using the active Sectigo connection.</para>
/// <list type="alertSet">
///   <item>
///     <term>Network</term>
///     <description>Retrieves type information from the Sectigo API.</description>
///   </item>
/// </list>
/// <example>
///   <summary>List certificate types</summary>
///   <prefix>PS&gt; </prefix>
///   <code>Connect-Sectigo -BaseUrl "https://cert-manager.com/api" -Username "user" -Password "pass" -CustomerUri "example"; Get-SectigoCertificateTypes</code>
///   <para>Outputs all available certificate types for the connected account.</para>
/// </example>
/// <example>
///   <summary>Filter by organization</summary>
///   <prefix>PS&gt; </prefix>
///   <code>Get-SectigoCertificateTypes -OrganizationId 42</code>
///   <para>Returns types available for the specified organization.</para>
/// </example>
/// <seealso href="https://learn.microsoft.com/powershell/scripting/developer/cmdlet/writing-a-cmdlet"/>
/// <seealso href="https://github.com/SectigoCertificateManager/SectigoCertificateManager"/>
[Cmdlet(VerbsCommon.Get, "SectigoCertificateTypes")]
[CmdletBinding()]
[OutputType(typeof(Models.CertificateType))]
public sealed class GetSectigoCertificateTypesCommand : AsyncPSCmdlet {
    /// <summary>The API version to use when calling the legacy API.</summary>
    [Parameter]
    public ApiVersion ApiVersion { get; set; } = ApiVersion.V25_6;

    /// <summary>Optional organization identifier used to filter results.</summary>
    [Parameter]
    public int? OrganizationId { get; set; }

    /// <summary>Optional cancellation token.</summary>
    [Parameter]
    public CancellationToken CancellationToken { get; set; }

    /// <summary>Executes the cmdlet.</summary>
    /// <para>Outputs all certificate types.</para>
    protected override async Task ProcessRecordAsync() {
        var adminConfigObj = SessionState.PSVariable.GetValue("SectigoAdminApiConfig");
        if (adminConfigObj is not null) {
            throw new PSInvalidOperationException("Get-SectigoCertificateTypes is not yet supported with an Admin (OAuth2) connection. Connect with legacy credentials to use this cmdlet.");
        }

        using var linked = CancellationTokenSource.CreateLinkedTokenSource(CancelToken, CancellationToken);
        var effectiveToken = linked.Token;

        ISectigoClient? client = null;
        try {
            var config = ConnectionHelper.GetLegacyConfig(SessionState);
            client = TestHooks.ClientFactory?.Invoke(config) ?? new SectigoClient(config);
            TestHooks.CreatedClient = client;
            var orgPart = OrganizationId.HasValue ? $" for OrganizationId={OrganizationId.Value}" : string.Empty;
            WriteVerbose(
                $"Listing certificate types{orgPart} using the legacy API at '{config.BaseUrl}'.");
            var types = new CertificateTypesClient(client);
            var list = await types.ListTypesAsync(OrganizationId, effectiveToken)
                .ConfigureAwait(false);
            foreach (var type in list) {
                WriteObject(type);
            }
        } finally {
            if (client is IDisposable disposable) {
                disposable.Dispose();
            }
        }
    }
}
