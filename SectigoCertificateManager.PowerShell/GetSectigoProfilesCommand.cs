using SectigoCertificateManager;
using SectigoCertificateManager.Clients;
using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;

namespace SectigoCertificateManager.PowerShell;

/// <summary>Retrieves profiles.</summary>
/// <para>Lists all profiles for the account using the active Sectigo connection.</para>
/// <list type="alertSet">
///   <item>
///     <term>Network</term>
///     <description>Retrieves profile information from the Sectigo API.</description>
///   </item>
/// </list>
/// <example>
///   <summary>List all profiles</summary>
///   <prefix>PS&gt; </prefix>
///   <code>Connect-Sectigo -BaseUrl "https://cert-manager.com/api" -Username "user" -Password "pass" -CustomerUri "example"; Get-SectigoProfiles</code>
///   <para>Outputs every profile available to the connected account.</para>
/// </example>
/// <seealso href="https://learn.microsoft.com/powershell/scripting/developer/cmdlet/writing-a-cmdlet"/>
/// <seealso href="https://github.com/SectigoCertificateManager/SectigoCertificateManager"/>
[Cmdlet(VerbsCommon.Get, "SectigoProfiles")]
[CmdletBinding()]
[OutputType(typeof(Models.Profile))]
public sealed class GetSectigoProfilesCommand : AsyncPSCmdlet {
    /// <summary>The API version to use when calling the legacy API.</summary>
    [Parameter]
    public ApiVersion ApiVersion { get; set; } = ApiVersion.V25_6;

    /// <summary>Optional cancellation token.</summary>
    [Parameter]
    public CancellationToken CancellationToken { get; set; }

    /// <summary>Executes the cmdlet.</summary>
    /// <para>Outputs all profiles using the active connection.</para>
    protected override async Task ProcessRecordAsync() {
        var adminConfigObj = SessionState.PSVariable.GetValue("SectigoAdminApiConfig");
        if (adminConfigObj is not null) {
            throw new PSInvalidOperationException("Get-SectigoProfiles is not yet supported with an Admin (OAuth2) connection. Connect with legacy credentials to use this cmdlet.");
        }

        using var linked = CancellationTokenSource.CreateLinkedTokenSource(CancelToken, CancellationToken);
        var effectiveToken = linked.Token;

        ISectigoClient? client = null;
        try {
            var config = ConnectionHelper.GetLegacyConfig(SessionState);
            client = TestHooks.ClientFactory?.Invoke(config) ?? new SectigoClient(config);
            TestHooks.CreatedClient = client;
            var profilesClient = new ProfilesClient(client);
            var list = await profilesClient.ListProfilesAsync(effectiveToken)
                .ConfigureAwait(false);
            foreach (var profile in list) {
                WriteObject(profile);
            }
        } finally {
            if (client is IDisposable disposable) {
                disposable.Dispose();
            }
        }
    }
}
