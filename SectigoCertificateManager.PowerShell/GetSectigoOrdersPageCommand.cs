using SectigoCertificateManager;
using SectigoCertificateManager.Clients;
using SectigoCertificateManager.Requests;
using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;

namespace SectigoCertificateManager.PowerShell;

/// <summary>Retrieves a single page of orders.</summary>
/// <para>Creates an API client and lists orders using paging parameters.</para>
/// <list type="alertSet">
///   <item>
///     <term>Network</term>
///     <description>This cmdlet issues a paged request to the Sectigo API and may require multiple calls for all data.</description>
///   </item>
/// </list>
/// <example>
///   <summary>Fetch the first page</summary>
///   <prefix>PS&gt; </prefix>
///   <code>Get-SectigoOrdersPage -BaseUrl "https://api.example.com" -Username "user" -Password "pass" -CustomerUri "example" -Size 50</code>
///   <para>Retrieves up to fifty orders starting at the beginning of the list.</para>
/// </example>
/// <example>
///   <summary>Continue from a specific position</summary>
///   <prefix>PS&gt; </prefix>
///   <code>Get-SectigoOrdersPage -BaseUrl "https://api.example.com" -Username "user" -Password "pass" -CustomerUri "example" -Position 50 -Size 50</code>
///   <para>Retrieves the next fifty orders after position fifty.</para>
/// </example>
/// <seealso href="https://learn.microsoft.com/powershell/scripting/developer/cmdlet/writing-a-cmdlet"/>
/// <seealso href="https://github.com/SectigoCertificateManager/SectigoCertificateManager"/>
[Cmdlet(VerbsCommon.Get, "SectigoOrdersPage")]
[CmdletBinding()]
[OutputType(typeof(Models.Order))]
public sealed class GetSectigoOrdersPageCommand : AsyncPSCmdlet {
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

    /// <summary>The result offset.</summary>
    [Parameter]
    public int? Position { get; set; }

    /// <summary>Number of results to return.</summary>
    [Parameter]
    public int? Size { get; set; }

    /// <summary>Optional cancellation token.</summary>
    [Parameter]
    public CancellationToken CancellationToken { get; set; }

    /// <summary>Executes the cmdlet.</summary>
    /// <para>Creates an API client and outputs the orders from a single page.</para>
    protected override async Task ProcessRecordAsync() {
        var config = new ApiConfig(BaseUrl, Username, Password, CustomerUri, ApiVersion);
        ISectigoClient? client = null;
        try {
            client = TestHooks.ClientFactory?.Invoke(config) ?? new SectigoClient(config);
            TestHooks.CreatedClient = client;
            var orders = new OrdersClient(client);
            var request = new OrderSearchRequest { Size = Size, Position = Position };

            using var linked = CancellationTokenSource.CreateLinkedTokenSource(CancelToken, CancellationToken);
            var result = await orders.SearchAsync(request, linked.Token).ConfigureAwait(false);
            if (result is null) {
                return;
            }

            foreach (var order in result.Orders) {
                WriteObject(order);
            }
        } finally {
            if (client is IDisposable disposable) {
                disposable.Dispose();
            }
        }
    }
}