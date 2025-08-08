using SectigoCertificateManager;
using SectigoCertificateManager.Clients;
using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;

namespace SectigoCertificateManager.PowerShell;

/// <summary>Retrieves certificate orders.</summary>
/// <para>Creates an API client and lists all orders for the account.</para>
/// <list type="alertSet">
///   <item>
///     <term>Network</term>
///     <description>This cmdlet connects to the Sectigo API and may take time to return results.</description>
///   </item>
/// </list>
/// <example>
///   <summary>List all orders</summary>
///   <prefix>PS&gt; </prefix>
///   <code>Get-SectigoOrders -BaseUrl "https://api.example.com" -Username "user" -Password "pass" -CustomerUri "example"</code>
///   <para>Retrieves every order for the specified account.</para>
/// </example>
/// <example>
///   <summary>Use a specific API version</summary>
///   <prefix>PS&gt; </prefix>
///   <code>Get-SectigoOrders -BaseUrl "https://api.example.com" -Username "user" -Password "pass" -CustomerUri "example" -ApiVersion V25_5</code>
///   <para>Overrides the default API version.</para>
/// </example>
/// <seealso href="https://learn.microsoft.com/powershell/scripting/developer/cmdlet/writing-a-cmdlet"/>
/// <seealso href="https://github.com/SectigoCertificateManager/SectigoCertificateManager"/>
[Cmdlet(VerbsCommon.Get, "SectigoOrders")]
[CmdletBinding()]
[OutputType(typeof(Models.Order))]
public sealed class GetSectigoOrdersCommand : AsyncPSCmdlet {
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
    /// <para>Creates an API client and outputs all orders.</para>
    protected override async Task ProcessRecordAsync() {
        var config = new ApiConfig(BaseUrl, Username, Password, CustomerUri, ApiVersion);
        ISectigoClient? client = null;
        try {
            client = TestHooks.ClientFactory?.Invoke(config) ?? new SectigoClient(config);
            TestHooks.CreatedClient = client;
            var orders = new OrdersClient(client);

            using var linked = CancellationTokenSource.CreateLinkedTokenSource(CancelToken, CancellationToken);
            await foreach (var order in orders.EnumerateOrdersAsync(cancellationToken: linked.Token).ConfigureAwait(false)) {
                WriteObject(order);
            }
        } finally {
            if (client is IDisposable disposable) {
                disposable.Dispose();
            }
        }
    }
}