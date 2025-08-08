using SectigoCertificateManager;
using SectigoCertificateManager.Clients;
using System.Management.Automation;
using System.Threading;

namespace SectigoCertificateManager.PowerShell;

/// <summary>Retrieves profiles.</summary>
/// <para>Creates an API client and lists all profiles for the account.</para>
/// <list type="alertSet">
///   <item>
///     <term>Network</term>
///     <description>Retrieves profile information from the Sectigo API.</description>
///   </item>
/// </list>
/// <example>
///   <summary>List all profiles</summary>
///   <prefix>PS&gt; </prefix>
///   <code>Get-SectigoProfiles -BaseUrl "https://api.example.com" -Username "user" -Password "pass" -CustomerUri "example"</code>
///   <para>Outputs every profile available to the account.</para>
/// </example>
/// <example>
///   <summary>Use a specific API version</summary>
///   <prefix>PS&gt; </prefix>
///   <code>Get-SectigoProfiles -BaseUrl "https://api.example.com" -Username "user" -Password "pass" -CustomerUri "example" -ApiVersion V25_5</code>
///   <para>Requests profiles using a different API version.</para>
/// </example>
/// <seealso href="https://learn.microsoft.com/powershell/scripting/developer/cmdlet/writing-a-cmdlet"/>
/// <seealso href="https://github.com/SectigoCertificateManager/SectigoCertificateManager"/>
[Cmdlet(VerbsCommon.Get, "SectigoProfiles")]
[CmdletBinding()]
[OutputType(typeof(Models.Profile))]
public sealed class GetSectigoProfilesCommand : PSCmdlet {
    /// <summary>The API base URL.</summary>
    [Parameter(Mandatory = true)]
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>The user name for authentication.</summary>
    [Parameter(Mandatory = true)]
    public string Username { get; set; } = string.Empty;

    /// <summary>The password for authentication.</summary>
    [Parameter(Mandatory = true)]
    public string Password { get; set; } = string.Empty;

    /// <summary>The customer URI assigned by Sectigo.</summary>
    [Parameter(Mandatory = true)]
    public string CustomerUri { get; set; } = string.Empty;

    /// <summary>The API version to use.</summary>
    [Parameter]
    public ApiVersion ApiVersion { get; set; } = ApiVersion.V25_6;

    /// <summary>Optional cancellation token.</summary>
    [Parameter]
    public CancellationToken CancellationToken { get; set; }

    /// <summary>Executes the cmdlet.</summary>
    /// <para>Creates an API client and outputs all profiles.</para>
    protected override void ProcessRecord() {
        ISectigoClient? client = null;
        try {
            var config = new ApiConfig(BaseUrl, Username, Password, CustomerUri, ApiVersion);
            client = TestHooks.ClientFactory?.Invoke(config) ?? new SectigoClient(config);
            TestHooks.CreatedClient = client;
            var profilesClient = new ProfilesClient(client);
            var list = profilesClient.ListProfilesAsync(CancellationToken)
                .GetAwaiter()
                .GetResult();
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