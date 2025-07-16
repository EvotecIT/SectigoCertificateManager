using SectigoCertificateManager;
using SectigoCertificateManager.Clients;
using System.Management.Automation;

namespace SectigoCertificateManager.PowerShell;

/// <summary>Retrieves organizations.</summary>
/// <para>Builds an API client and lists all organizations for the account.</para>
[Cmdlet(VerbsCommon.Get, "SectigoOrganizations")]
[OutputType(typeof(Models.Organization))]
public sealed class GetSectigoOrganizationsCommand : PSCmdlet {
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

    /// <summary>Executes the cmdlet.</summary>
    /// <para>Creates an API client and outputs all organizations.</para>
    protected override void ProcessRecord() {
        var config = new ApiConfig(BaseUrl, Username, Password, CustomerUri, ApiVersion);
        var client = new SectigoClient(config);
        var organizations = new OrganizationsClient(client);
        var list = organizations.ListOrganizationsAsync().GetAwaiter().GetResult();
        foreach (var org in list) {
            WriteObject(org);
        }
    }
}