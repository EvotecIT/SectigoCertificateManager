using SectigoCertificateManager;
using SectigoCertificateManager.Clients;
using System.Management.Automation;
using System.Threading;

namespace SectigoCertificateManager.PowerShell;

/// <summary>Retrieves order history.</summary>
/// <para>Creates an API client and returns history entries for an order.</para>
/// <list type="alertSet">
///   <item>
///     <term>Network</term>
///     <description>Contacts the Sectigo API to fetch order history.</description>
///   </item>
/// </list>
/// <example>
///   <summary>Get history for an order</summary>
///   <prefix>PS&gt; </prefix>
///   <code>Get-SectigoOrderHistory -BaseUrl "https://api.example.com" -Username "user" -Password "pass" -CustomerUri "example" -OrderId 100</code>
///   <para>Shows all history entries for order 100.</para>
/// </example>
/// <example>
///   <summary>Specify a different API version</summary>
///   <prefix>PS&gt; </prefix>
///   <code>Get-SectigoOrderHistory -BaseUrl "https://api.example.com" -Username "user" -Password "pass" -CustomerUri "example" -OrderId 100 -ApiVersion V25_5</code>
///   <para>Requests history using API version 25.5.</para>
/// </example>
/// <seealso href="https://learn.microsoft.com/powershell/scripting/developer/cmdlet/writing-a-cmdlet"/>
/// <seealso href="https://github.com/SectigoCertificateManager/SectigoCertificateManager"/>
[Cmdlet(VerbsCommon.Get, "SectigoOrderHistory")]
[CmdletBinding()]
[OutputType(typeof(Models.OrderHistoryEntry))]
public sealed class GetSectigoOrderHistoryCommand : PSCmdlet {
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

    /// <summary>The identifier of the order.</summary>
    [Parameter(Mandatory = true, Position = 0)]
    public int OrderId { get; set; }

    /// <summary>Optional cancellation token.</summary>
    [Parameter]
    public CancellationToken CancellationToken { get; set; }

    /// <summary>Executes the cmdlet.</summary>
    /// <para>Creates an API client and retrieves the history for the specified order.</para>
    protected override void ProcessRecord() {
        if (OrderId <= 0) {
            var ex = new ArgumentOutOfRangeException(nameof(OrderId));
            var record = new ErrorRecord(ex, "InvalidOrderId", ErrorCategory.InvalidArgument, OrderId);
            ThrowTerminatingError(record);
        }

        var config = new ApiConfig(BaseUrl, Username, Password, CustomerUri, ApiVersion);
        ISectigoClient? client = null;
        try {
            client = TestHooks.ClientFactory?.Invoke(config) ?? new SectigoClient(config);
            TestHooks.CreatedClient = client;
            var orders = new OrdersClient(client);
            var history = orders.GetHistoryAsync(OrderId, CancellationToken)
                .GetAwaiter()
                .GetResult();
            WriteObject(history, true);
        } finally {
            if (client is IDisposable disposable) {
                disposable.Dispose();
            }
        }
    }
}