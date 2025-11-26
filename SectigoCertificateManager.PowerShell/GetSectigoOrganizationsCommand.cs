using SectigoCertificateManager;
using SectigoCertificateManager.Clients;
using System.Management.Automation;
using System.Threading;

namespace SectigoCertificateManager.PowerShell;

/// <summary>Retrieves organizations.</summary>
/// <para>Builds an API client and lists all organizations for the account.</para>
/// <list type="alertSet">
///   <item>
///     <term>Network</term>
///     <description>Requests organization data from the Sectigo API.</description>
///   </item>
/// </list>
/// <seealso href="https://learn.microsoft.com/powershell/scripting/developer/cmdlet/writing-a-cmdlet"/>
/// <seealso href="https://github.com/SectigoCertificateManager/SectigoCertificateManager"/>
[Cmdlet(VerbsCommon.Get, "SectigoOrganizations")]
[OutputType(typeof(Models.Organization))]
public sealed class GetSectigoOrganizationsCommand : PSCmdlet {
    /// <summary>The API version to use.</summary>
    [Parameter]
    public ApiVersion ApiVersion { get; set; } = ApiVersion.V25_6;

    /// <summary>Optional cancellation token.</summary>
    [Parameter]
    public CancellationToken CancellationToken { get; set; }

    /// <summary>Executes the cmdlet.</summary>
    /// <para>Creates an API client and outputs all organizations.</para>
    protected override void ProcessRecord() {
        var adminConfigObj = SessionState.PSVariable.GetValue("SectigoAdminApiConfig");
        if (adminConfigObj is not null) {
            throw new PSInvalidOperationException("Get-SectigoOrganizations is not yet supported with an Admin (OAuth2) connection. Connect with legacy credentials to use this cmdlet.");
        }

        ISectigoClient? client = null;
        try {
            var config = ConnectionHelper.GetLegacyConfig(SessionState);
            client = TestHooks.ClientFactory?.Invoke(config) ?? new SectigoClient(config);
            TestHooks.CreatedClient = client;
            var organizations = new OrganizationsClient(client);
            var list = organizations.ListOrganizationsAsync(CancellationToken)
                .GetAwaiter()
                .GetResult();
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
