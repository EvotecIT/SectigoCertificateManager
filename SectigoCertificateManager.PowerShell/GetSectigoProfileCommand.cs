using System.Management.Automation;
using SectigoCertificateManager;
using SectigoCertificateManager.Clients;

namespace SectigoCertificateManager.PowerShell;

[Cmdlet(VerbsCommon.Get, "SectigoProfile")]
[OutputType(typeof(Models.Profile))]
public sealed class GetSectigoProfileCommand : PSCmdlet
{
    [Parameter(Mandatory = true)]
    public string BaseUrl { get; set; } = string.Empty;

    [Parameter(Mandatory = true)]
    public string Username { get; set; } = string.Empty;

    [Parameter(Mandatory = true)]
    public string Password { get; set; } = string.Empty;

    [Parameter(Mandatory = true)]
    public string CustomerUri { get; set; } = string.Empty;

    [Parameter]
    public ApiVersion ApiVersion { get; set; } = ApiVersion.V25_4;

    [Parameter(Mandatory = true, Position = 0)]
    public int ProfileId { get; set; }

    protected override void ProcessRecord()
    {
        var config = new ApiConfig(BaseUrl, Username, Password, CustomerUri, ApiVersion);
        var client = new SectigoClient(config);
        var profiles = new ProfilesClient(client);
        var profile = profiles.GetAsync(ProfileId).GetAwaiter().GetResult();
        WriteObject(profile);
    }
}
