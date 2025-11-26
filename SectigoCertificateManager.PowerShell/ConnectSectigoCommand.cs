namespace SectigoCertificateManager.PowerShell;

using SectigoCertificateManager;
using System.Management.Automation;

/// <summary>Creates shared defaults for Sectigo cmdlets.</summary>
/// <para>Stores connection parameters in <c>PSDefaultParameterValues</c> so other Sectigo cmdlets can be called without repeating BaseUrl, Username, Password, CustomerUri, or ApiVersion.</para>
/// <example>
///   <summary>Connect once, reuse across cmdlets</summary>
///   <prefix>PS&gt; </prefix>
///   <code>Connect-Sectigo -BaseUrl "https://cert-manager.com/ssl" -Username "user" -Password "pass" -CustomerUri "tenant" -ApiVersion V25_6</code>
///   <para>Subsequent <c>Get-SectigoOrders</c> or <c>Get-SectigoCertificate</c> calls will inherit these values automatically.</para>
/// </example>
[Cmdlet(VerbsCommunications.Connect, "Sectigo")]
[CmdletBinding()]
public sealed class ConnectSectigoCommand : PSCmdlet {
    private const string DefaultBaseUrl = "https://cert-manager.com/api";

    /// <summary>The API base URL (e.g., https://cert-manager.com/ssl).</summary>
    [Parameter]
    public string BaseUrl { get; set; } = DefaultBaseUrl;

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
    public ApiVersion ApiVersion { get; set; } = ApiVersion.V25_5;

    /// <summary>Sets default parameter values for all Sectigo cmdlets.</summary>
    protected override void ProcessRecord() {
        var trimmedBaseUrl = BaseUrl.TrimEnd('/');
        DefaultParameterHelper.SetDefaults(SessionState, trimmedBaseUrl, Username, Password, CustomerUri, ApiVersion);
        WriteObject(new {
            BaseUrl = trimmedBaseUrl,
            Username,
            CustomerUri,
            ApiVersion
        });
    }
}
