using SectigoCertificateManager;
using SectigoCertificateManager.Clients;
using System.Management.Automation;

namespace SectigoCertificateManager.PowerShell;

[Cmdlet(VerbsLifecycle.Stop, "SectigoOrder")]
public sealed class StopSectigoOrderCommand : PSCmdlet {
    [Parameter(Mandatory = true)]
    public string BaseUrl { get; set; } = string.Empty;

    [Parameter(Mandatory = true)]
    public string Username { get; set; } = string.Empty;

    [Parameter(Mandatory = true)]
    public string Password { get; set; } = string.Empty;

    [Parameter(Mandatory = true)]
    public string CustomerUri { get; set; } = string.Empty;

    [Parameter]
    public ApiVersion ApiVersion { get; set; } = ApiVersion.V25_4;

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
