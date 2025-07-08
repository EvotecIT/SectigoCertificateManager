using SectigoCertificateManager;
using SectigoCertificateManager.Clients;
using System.Management.Automation;

namespace SectigoCertificateManager.PowerShell;

/// <summary>Cancels an order.</summary>
/// <para>Creates an API client and calls the cancel endpoint.</para>
[Cmdlet(VerbsLifecycle.Stop, "SectigoOrder")]
[OutputType(typeof(void))]
public sealed class StopSectigoOrderCommand : PSCmdlet {
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

    /// <summary>The identifier of the order to cancel.</summary>
    [Parameter(Mandatory = true, Position = 0)]
    public int OrderId { get; set; }

    /// <summary>Cancels an order.</summary>
    /// <para>Builds an API client and calls the cancel endpoint.</para>
    protected override void ProcessRecord() {
        var config = new ApiConfig(BaseUrl, Username, Password, CustomerUri, ApiVersion);
        var client = new SectigoClient(config);
        var orders = new OrdersClient(client);
        orders.CancelAsync(OrderId).GetAwaiter().GetResult();
    }
}
