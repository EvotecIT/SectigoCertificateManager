using SectigoCertificateManager;
using SectigoCertificateManager.Clients;
using SectigoCertificateManager.Requests;
using System;
using System.Management.Automation;
using System.Threading;

namespace SectigoCertificateManager.PowerShell;

/// <summary>Creates a new organization.</summary>
/// <para>Builds an API client and submits a <see cref="CreateOrganizationRequest"/>.</para>
[Cmdlet(VerbsCommon.New, "SectigoOrganization")]
[CmdletBinding()]
[OutputType(typeof(int))]
public sealed class NewSectigoOrganizationCommand : PSCmdlet {
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

    /// <summary>The organization name.</summary>
    [Parameter(Mandatory = true, Position = 0)]
    public string Name { get; set; } = string.Empty;

    /// <summary>The state or province.</summary>
    [Parameter]
    public string? StateOrProvince { get; set; }

    /// <summary>Optional cancellation token.</summary>
    [Parameter]
    public CancellationToken CancellationToken { get; set; }

    /// <summary>Executes the cmdlet.</summary>
    /// <para>Creates an organization and outputs its identifier.</para>
    protected override void ProcessRecord() {
        if (string.IsNullOrWhiteSpace(Name)) {
            var ex = new ArgumentException("Value cannot be empty.", nameof(Name));
            var record = new ErrorRecord(ex, "InvalidName", ErrorCategory.InvalidArgument, Name);
            ThrowTerminatingError(record);
        }

        var config = new ApiConfig(BaseUrl, Username, Password, CustomerUri, ApiVersion);
        var client = new SectigoClient(config);
        var organizations = new OrganizationsClient(client);
        var request = new CreateOrganizationRequest {
            Name = Name,
            StateOrProvince = StateOrProvince
        };
        var id = organizations.CreateAsync(request, CancellationToken)
            .GetAwaiter()
            .GetResult();
        WriteObject(id);
    }
}
