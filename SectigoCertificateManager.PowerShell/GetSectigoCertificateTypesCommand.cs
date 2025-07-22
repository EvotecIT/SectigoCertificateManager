using SectigoCertificateManager;
using SectigoCertificateManager.Clients;
using System.Management.Automation;
using System.Threading;

namespace SectigoCertificateManager.PowerShell;

/// <summary>Retrieves available certificate types.</summary>
/// <para>Creates an API client and lists certificate types for the account.</para>
[Cmdlet(VerbsCommon.Get, "SectigoCertificateTypes")]
[CmdletBinding()]
[OutputType(typeof(Models.CertificateType))]
public sealed class GetSectigoCertificateTypesCommand : PSCmdlet {
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

    /// <summary>Optional organization identifier used to filter results.</summary>
    [Parameter]
    public int? OrganizationId { get; set; }

    /// <summary>Optional cancellation token.</summary>
    [Parameter]
    public CancellationToken CancellationToken { get; set; }

    /// <summary>Executes the cmdlet.</summary>
    /// <para>Creates an API client and outputs all certificate types.</para>
    protected override void ProcessRecord() {
        var config = new ApiConfig(BaseUrl, Username, Password, CustomerUri, ApiVersion);
        var client = new SectigoClient(config);
        var types = new CertificateTypesClient(client);
        var list = types.ListTypesAsync(OrganizationId, CancellationToken)
            .GetAwaiter()
            .GetResult();
        foreach (var type in list) {
            WriteObject(type);
        }
    }
}