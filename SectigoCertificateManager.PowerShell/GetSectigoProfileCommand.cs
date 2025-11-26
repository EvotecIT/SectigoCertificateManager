using SectigoCertificateManager;
using SectigoCertificateManager.Clients;
using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;

namespace SectigoCertificateManager.PowerShell;

/// <summary>Retrieves a profile.</summary>
/// <para>Returns profile details using the identifier and the active Sectigo connection.</para>
/// <list type="alertSet">
///   <item>
///     <term>Network</term>
///     <description>Queries the Sectigo API for profile information.</description>
///   </item>
/// </list>
/// <example>
///   <summary>Get a profile</summary>
///   <prefix>PS&gt; </prefix>
///   <code>Connect-Sectigo -BaseUrl "https://cert-manager.com/api" -Username "user" -Password "pass" -CustomerUri "example"; Get-SectigoProfile -ProfileId 5</code>
///   <para>Retrieves details of profile 5 for the connected account.</para>
/// </example>
/// <seealso href="https://learn.microsoft.com/powershell/scripting/developer/cmdlet/writing-a-cmdlet"/>
/// <seealso href="https://github.com/SectigoCertificateManager/SectigoCertificateManager"/>
[Cmdlet(VerbsCommon.Get, "SectigoProfile")]
[CmdletBinding()]
[OutputType(typeof(Models.Profile))]
public sealed class GetSectigoProfileCommand : AsyncPSCmdlet {
    /// <summary>The API version to use when calling the legacy API.</summary>
    [Parameter]
    public ApiVersion ApiVersion { get; set; } = ApiVersion.V25_6;

    /// <summary>The profile identifier.</summary>
    [Parameter(Mandatory = true, Position = 0)]
    public int ProfileId { get; set; }

    /// <summary>Optional cancellation token.</summary>
    [Parameter]
    public CancellationToken CancellationToken { get; set; }

    /// <summary>Executes the cmdlet.</summary>
    /// <para>Retrieves the profile using the active connection.</para>
    protected override async Task ProcessRecordAsync() {
        var adminConfigObj = SessionState.PSVariable.GetValue("SectigoAdminApiConfig");
        if (adminConfigObj is not null) {
            throw new PSInvalidOperationException("Get-SectigoProfile is not yet supported with an Admin (OAuth2) connection. Connect with legacy credentials to use this cmdlet.");
        }

        using var linked = CancellationTokenSource.CreateLinkedTokenSource(CancelToken, CancellationToken);
        var effectiveToken = linked.Token;

        ISectigoClient? client = null;
        try {
            var config = ConnectionHelper.GetLegacyConfig(SessionState);
            client = TestHooks.ClientFactory?.Invoke(config) ?? new SectigoClient(config);
            TestHooks.CreatedClient = client;
            var profiles = new ProfilesClient(client);
            var profile = await profiles.GetAsync(ProfileId, effectiveToken)
                .ConfigureAwait(false);
            WriteObject(profile);
        } finally {
            if (client is IDisposable disposable) {
                disposable.Dispose();
            }
        }
    }
}
