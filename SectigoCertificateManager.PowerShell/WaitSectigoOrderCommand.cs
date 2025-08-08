using SectigoCertificateManager;
using SectigoCertificateManager.Clients;
using System.Management.Automation;
using System.Threading;

namespace SectigoCertificateManager.PowerShell;

/// <summary>Waits for an order to reach a terminal status.</summary>
/// <para>Creates an API client and polls the order status until it is completed or cancelled.</para>
/// <list type="alertSet">
///   <item>
///     <term>Delay</term>
///     <description>The cmdlet waits and repeatedly queries the API until the order finishes.</description>
///   </item>
/// </list>
/// <example>
///   <summary>Wait for an order</summary>
///   <prefix>PS&gt; </prefix>
///   <code>Wait-SectigoOrder -BaseUrl "https://api.example.com" -Username "user" -Password "pass" -CustomerUri "example" -OrderId 100</code>
///   <para>Blocks until order 100 completes or is cancelled.</para>
/// </example>
/// <example>
///   <summary>Specify API version</summary>
///   <prefix>PS&gt; </prefix>
///   <code>Wait-SectigoOrder -BaseUrl "https://api.example.com" -Username "user" -Password "pass" -CustomerUri "example" -OrderId 100 -ApiVersion V25_5</code>
///   <para>Uses a different API version while waiting for completion.</para>
/// </example>
/// <seealso href="https://learn.microsoft.com/powershell/scripting/developer/cmdlet/writing-a-cmdlet"/>
/// <seealso href="https://github.com/SectigoCertificateManager/SectigoCertificateManager"/>
[Cmdlet(VerbsLifecycle.Wait, "SectigoOrder")]
[CmdletBinding()]
[OutputType(typeof(OrderStatus))]
public sealed class WaitSectigoOrderCommand : PSCmdlet {
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

    /// <summary>The identifier of the order to wait on.</summary>
    [Parameter(Mandatory = true, Position = 0)]
    public int OrderId { get; set; }

    /// <summary>Delay between status checks.</summary>
    [Parameter]
    public TimeSpan PollInterval { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>Optional cancellation token.</summary>
    [Parameter]
    public CancellationToken CancellationToken { get; set; }

    /// <summary>Executes the cmdlet.</summary>
    /// <para>Polls the order status until it reaches a terminal value.</para>
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
            var statuses = new OrderStatusClient(client);
            var status = statuses
                .WatchAsync(OrderId, PollInterval, CancellationToken)
                .GetAwaiter()
                .GetResult();
            WriteObject(status);
        } finally {
            if (client is IDisposable disposable) {
                disposable.Dispose();
            }
        }
    }
}