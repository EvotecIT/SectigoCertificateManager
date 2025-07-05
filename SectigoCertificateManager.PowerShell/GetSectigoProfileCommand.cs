using SectigoCertificateManager;
using SectigoCertificateManager.Clients;
using System.Management.Automation;

namespace SectigoCertificateManager.PowerShell;

/// <summary>Retrieves a profile from Sectigo Certificate Manager.</summary>
[Cmdlet(VerbsCommon.Get, "SectigoProfile")]
[OutputType(typeof(Models.Profile))]
public sealed class GetSectigoProfileCommand : PSCmdlet {
    /// <para>Base address of the Sectigo API.</para>
    [Parameter(Mandatory = true)]
    public string BaseUrl { get; set; } = string.Empty;

    /// <para>API username.</para>
    [Parameter(Mandatory = true)]
    public string Username { get; set; } = string.Empty;

    /// <para>API password.</para>
    [Parameter(Mandatory = true)]
    public string Password { get; set; } = string.Empty;

    /// <para>Customer URI associated with the account.</para>
    [Parameter(Mandatory = true)]
    public string CustomerUri { get; set; } = string.Empty;

    /// <para>API version to use.</para>
    [Parameter]
    public ApiVersion ApiVersion { get; set; } = ApiVersion.V25_4;

    /// <para>Identifier of the profile to retrieve.</para>
    [Parameter(Mandatory = true, Position = 0)]
    public int ProfileId { get; set; }

    /// <summary>Executes the cmdlet.</summary>
    protected override void ProcessRecord() {
        var config = new ApiConfig(BaseUrl, Username, Password, CustomerUri, ApiVersion);
        var client = new SectigoClient(config);
        var profiles = new ProfilesClient(client);
        var profile = profiles.GetAsync(ProfileId).GetAwaiter().GetResult();
        WriteObject(profile);
    }
}