using SectigoCertificateManager;
using SectigoCertificateManager.Clients;
using System.Management.Automation;

namespace SectigoCertificateManager.PowerShell;

/// <summary>Retrieves order history.</summary>
/// <para>Creates an API client and returns history entries for an order.</para>
[Cmdlet(VerbsCommon.Get, "SectigoOrderHistory")]
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
    public ApiVersion ApiVersion { get; set; } = ApiVersion.V25_4;

    /// <summary>The identifier of the order.</summary>
    [Parameter(Mandatory = true, Position = 0)]
    public int OrderId { get; set; }

    /// <summary>Executes the cmdlet.</summary>
    /// <para>Creates an API client and retrieves the history for the specified order.</para>
    protected override void ProcessRecord() {
        if (OrderId <= 0) {
            var ex = new ArgumentOutOfRangeException(nameof(OrderId));
            var record = new ErrorRecord(ex, "InvalidOrderId", ErrorCategory.InvalidArgument, OrderId);
            ThrowTerminatingError(record);
        }

        var config = new ApiConfig(BaseUrl, Username, Password, CustomerUri, ApiVersion);
        var client = new SectigoClient(config);
        var orders = new OrdersClient(client);
        var history = orders.GetHistoryAsync(OrderId).GetAwaiter().GetResult();
        WriteObject(history, true);
    }
}
