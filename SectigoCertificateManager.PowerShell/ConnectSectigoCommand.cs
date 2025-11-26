namespace SectigoCertificateManager.PowerShell;

using SectigoCertificateManager;
using SectigoCertificateManager.AdminApi;
using System.Management.Automation;

/// <summary>Creates shared defaults for Sectigo cmdlets.</summary>
/// <para>
/// Stores connection parameters for either the legacy SCM API (username/password)
/// or the Admin Operations API (OAuth2 client credentials). Other Sectigo cmdlets
/// reuse the active connection without repeating authentication arguments.
/// </para>
/// <example>
///   <summary>Connect using legacy SCM credentials</summary>
///   <prefix>PS&gt; </prefix>
///   <code>Connect-Sectigo -BaseUrl "https://cert-manager.com/api" -Username "user" -Password "pass" -CustomerUri "tenant" -ApiVersion V25_6</code>
///   <para>Subsequent <c>Get-SectigoOrders</c> or <c>Get-SectigoCertificate</c> calls will use the legacy API configuration.</para>
/// </example>
/// <example>
///   <summary>Connect using Admin Operations API client credentials</summary>
///   <prefix>PS&gt; </prefix>
///   <code>Connect-Sectigo -ClientId "&lt;client id&gt;" -ClientSecret "&lt;client secret&gt;" -Instance "enterprise" -AdminBaseUrl "https://admin.enterprise.sectigo.com"</code>
///   <para>Subsequent certificate cmdlets such as <c>Get-SectigoCertificate</c> and <c>Export-SectigoCertificate</c> will route through the Admin API.</para>
/// </example>
[Cmdlet(VerbsCommunications.Connect, "Sectigo")]
[CmdletBinding(DefaultParameterSetName = LegacyParameterSet)]
public sealed class ConnectSectigoCommand : PSCmdlet {
    private const string DefaultBaseUrl = "https://cert-manager.com/api";
    private const string LegacyParameterSet = "Legacy";
    private const string AdminParameterSet = "Admin";
    private const string DefaultAdminBaseUrl = "https://admin.enterprise.sectigo.com";

    /// <summary>The API base URL (e.g., https://cert-manager.com/ssl).</summary>
    [Parameter(ParameterSetName = LegacyParameterSet)]
    public string BaseUrl { get; set; } = DefaultBaseUrl;

    /// <summary>The user name for authentication.</summary>
    [Parameter(Mandatory = true, ParameterSetName = LegacyParameterSet)]
    public string Username { get; set; } = string.Empty;

    /// <summary>The password for authentication.</summary>
    [Parameter(Mandatory = true, ParameterSetName = LegacyParameterSet)]
    public string Password { get; set; } = string.Empty;

    /// <summary>The customer URI assigned by Sectigo.</summary>
    [Parameter(Mandatory = true, ParameterSetName = LegacyParameterSet)]
    public string CustomerUri { get; set; } = string.Empty;

    /// <summary>The API version to use.</summary>
    [Parameter(ParameterSetName = LegacyParameterSet)]
    public ApiVersion ApiVersion { get; set; } = ApiVersion.V25_6;

    /// <summary>The OAuth2 client identifier for the Admin API.</summary>
    [Parameter(Mandatory = true, ParameterSetName = AdminParameterSet)]
    public string ClientId { get; set; } = string.Empty;

    /// <summary>The OAuth2 client secret for the Admin API.</summary>
    [Parameter(Mandatory = true, ParameterSetName = AdminParameterSet)]
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>The Sectigo instance name (for example, enterprise).</summary>
    [Parameter(ParameterSetName = AdminParameterSet)]
    public string Instance { get; set; } = "enterprise";

    /// <summary>Base URL of the Admin API.</summary>
    [Parameter(ParameterSetName = AdminParameterSet)]
    public string AdminBaseUrl { get; set; } = DefaultAdminBaseUrl;

    /// <summary>OAuth2 token endpoint URL for the Admin API.</summary>
    [Parameter(ParameterSetName = AdminParameterSet)]
    public string TokenUrl { get; set; } = "https://auth.sso.sectigo.com/auth/realms/apiclients/protocol/openid-connect/token";

    /// <summary>Sets default parameter values for all Sectigo cmdlets.</summary>
    protected override void ProcessRecord() {
        if (ParameterSetName == AdminParameterSet) {
            var trimmedAdminBase = AdminBaseUrl.TrimEnd('/');
            var config = new AdminApiConfig(trimmedAdminBase, TokenUrl, ClientId, ClientSecret);
            SessionState.PSVariable.Set("SectigoAdminApiConfig", config);
            WriteObject(new {
                Mode = "Admin",
                BaseUrl = trimmedAdminBase,
                Instance,
                TokenUrl,
                ClientId
            });
        } else {
            var trimmedBaseUrl = BaseUrl.TrimEnd('/');
            var config = new ApiConfig(trimmedBaseUrl, Username, Password, CustomerUri, ApiVersion);
            SessionState.PSVariable.Set("SectigoApiConfig", config);
            DefaultParameterHelper.SetDefaults(SessionState, trimmedBaseUrl, Username, Password, CustomerUri, ApiVersion);
            WriteObject(new {
                Mode = "Legacy",
                BaseUrl = trimmedBaseUrl,
                Username,
                CustomerUri,
                ApiVersion
            });
        }
    }
}
