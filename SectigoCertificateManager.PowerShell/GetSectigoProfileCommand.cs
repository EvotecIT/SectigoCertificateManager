using SectigoCertificateManager;
using SectigoCertificateManager.Clients;
using System.Management.Automation;

namespace SectigoCertificateManager.PowerShell;

/// <summary>Retrieves a profile.</summary>
/// <para>Creates an API client and returns profile details using the identifier.</para>
[Cmdlet(VerbsCommon.Get, "SectigoProfile")]
[CmdletBinding()]
[OutputType(typeof(Models.Profile))]
public sealed class GetSectigoProfileCommand : PSCmdlet {
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
    public ApiVersion ApiVersion { get; set; } = ApiVersion.V25_4;

    /// <summary>The profile identifier.</summary>
    [Parameter(Mandatory = true, Position = 0)]
    public int ProfileId { get; set; }

    /// <summary>Executes the cmdlet.</summary>
    /// <para>Creates an API client and retrieves the profile.</para>
    protected override void ProcessRecord() {
        var config = new ApiConfig(BaseUrl, Username, Password, CustomerUri, ApiVersion);
        var client = new SectigoClient(config);
        var profiles = new ProfilesClient(client);
        var profile = profiles.GetAsync(ProfileId).GetAwaiter().GetResult();
        WriteObject(profile);
    }
}