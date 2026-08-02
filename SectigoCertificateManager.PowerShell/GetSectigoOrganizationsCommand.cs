using SectigoCertificateManager;
using SectigoCertificateManager.Clients;
using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;

namespace SectigoCertificateManager.PowerShell;

/// <summary>Retrieves organizations.</summary>
/// <para>Builds an API client and lists all organizations for the account.</para>
/// <example>
///   <summary>List organizations using a legacy connection</summary>
///   <code>$credential = Get-Credential; Connect-Sectigo -BaseUrl 'https://cert-manager.com/api' -Username $credential.UserName -Password $credential.GetNetworkCredential().Password -CustomerUri 'tenant'; Get-SectigoOrganizations</code>
///   <para>Creates a legacy API connection and lists its organizations.</para>
/// </example>
/// <list type="alertSet">
///   <item>
///     <term>Legacy connection required</term>
///     <description>This cmdlet does not support an Admin (OAuth2) connection. Connect with legacy credentials before calling it.</description>
///   </item>
///   <item>
///     <term>Network</term>
///     <description>Requests organization data from the Sectigo API.</description>
///   </item>
/// </list>
/// <seealso href="https://learn.microsoft.com/powershell/scripting/developer/cmdlet/writing-a-cmdlet"/>
/// <seealso href="https://github.com/EvotecIT/SectigoCertificateManager"/>
[Cmdlet(VerbsCommon.Get, "SectigoOrganizations")]
[OutputType(typeof(Models.Organization))]
public sealed class GetSectigoOrganizationsCommand : AsyncPSCmdlet {
    /// <summary>The API version to use.</summary>
    [Parameter]
    [PSDefaultValue(Value = ApiVersion.V25_6)]
    public ApiVersion ApiVersion { get; set; } = ApiVersion.V25_6;

    /// <summary>Optional cancellation token.</summary>
    [Parameter]
    public CancellationToken CancellationToken { get; set; }

    /// <summary>Executes the cmdlet.</summary>
    /// <para>Creates an API client and outputs all organizations.</para>
    protected override async Task ProcessRecordAsync() {
        var adminConfigObj = SessionState.PSVariable.GetValue("SectigoAdminApiConfig");
        if (adminConfigObj is not null) {
            throw new PSInvalidOperationException("Get-SectigoOrganizations is not yet supported with an Admin (OAuth2) connection. Connect with legacy credentials to use this cmdlet.");
        }

        using var linked = CancellationTokenSource.CreateLinkedTokenSource(CancelToken, CancellationToken);
        var effectiveToken = linked.Token;

        ISectigoClient? client = null;
        try {
            var config = ConnectionHelper.GetLegacyConfig(SessionState);
            client = TestHooks.ClientFactory?.Invoke(config) ?? new SectigoClient(config);
            TestHooks.CreatedClient = client;
            WriteVerbose($"Listing organizations using the legacy API at '{config.BaseUrl}'.");
            var organizations = new OrganizationsClient(client);
            var list = await organizations.ListOrganizationsAsync(effectiveToken)
                .ConfigureAwait(false);
            foreach (var org in list) {
                WriteObject(org);
            }
        } finally {
            if (client is IDisposable disposable) {
                disposable.Dispose();
            }
        }
    }
}
