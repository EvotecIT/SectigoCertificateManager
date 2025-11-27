using SectigoCertificateManager;
using SectigoCertificateManager.Clients;
using SectigoCertificateManager.Requests;
using System;
using System.Management.Automation;
using System.Threading;

namespace SectigoCertificateManager.PowerShell;

/// <summary>Creates a new organization.</summary>
/// <para>Submits a <see cref="CreateOrganizationRequest"/> using the active Sectigo connection.</para>
/// <list type="alertSet">
///   <item>
///     <term>Charges</term>
///     <description>Creating organizations may modify account configuration and incur fees.</description>
///   </item>
/// </list>
/// <example>
///   <summary>Create an organization</summary>
///   <prefix>PS&gt; </prefix>
///   <code>Connect-Sectigo -BaseUrl "https://cert-manager.com/api" -Username "user" -Password "pass" -CustomerUri "example"; New-SectigoOrganization -Name "Example Org"</code>
///   <para>Creates a new organization with the given name for the connected account.</para>
/// </example>
/// <seealso href="https://learn.microsoft.com/powershell/scripting/developer/cmdlet/writing-a-cmdlet"/>
/// <seealso href="https://github.com/SectigoCertificateManager/SectigoCertificateManager"/>
[Cmdlet(VerbsCommon.New, "SectigoOrganization")]
[CmdletBinding()]
[OutputType(typeof(int))]
public sealed class NewSectigoOrganizationCommand : PSCmdlet {
    /// <summary>The API version to use when calling the legacy API.</summary>
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

        var adminConfigObj = SessionState.PSVariable.GetValue("SectigoAdminApiConfig");
        if (adminConfigObj is not null) {
            throw new PSInvalidOperationException("New-SectigoOrganization is not yet supported with an Admin (OAuth2) connection. Connect with legacy credentials to use this cmdlet.");
        }

        var config = ConnectionHelper.GetLegacyConfig(SessionState);
        ISectigoClient? client = null;
        try {
            client = TestHooks.ClientFactory?.Invoke(config) ?? new SectigoClient(config);
            TestHooks.CreatedClient = client;
            var statePart = string.IsNullOrWhiteSpace(StateOrProvince) ? string.Empty : $", StateOrProvince='{StateOrProvince}'";
            WriteVerbose(
                $"Creating a new organization with Name='{Name}'{statePart} using the legacy API at '{config.BaseUrl}'.");
            var organizations = new OrganizationsClient(client);
            var request = new CreateOrganizationRequest {
                Name = Name,
                StateOrProvince = StateOrProvince
            };
            var id = organizations.CreateAsync(request, CancellationToken)
                .GetAwaiter()
                .GetResult();
            WriteObject(id);
        } finally {
            if (client is IDisposable disposable) {
                disposable.Dispose();
            }
        }
    }
}
