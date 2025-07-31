using SectigoCertificateManager;
using SectigoCertificateManager.Clients;
using System;
using System.Management.Automation;
using System.Threading;

namespace SectigoCertificateManager.PowerShell;

/// <summary>Cancels an order.</summary>
/// <para>Creates an API client and calls the cancel endpoint.</para>
[Cmdlet(VerbsLifecycle.Stop, "SectigoOrder", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
[CmdletBinding()]
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
    public ApiVersion ApiVersion { get; set; } = ApiVersion.V25_6;

    /// <summary>The identifier of the order to cancel.</summary>
    [Parameter(Mandatory = true, Position = 0)]
    public int OrderId { get; set; }

    /// <summary>Optional cancellation token.</summary>
    [Parameter]
    public CancellationToken CancellationToken { get; set; }

    /// <summary>Cancels an order.</summary>
    /// <para>Builds an API client and calls the cancel endpoint.</para>
    protected override void ProcessRecord() {
        if (OrderId <= 0) {
            var ex = new ArgumentOutOfRangeException(nameof(OrderId));
            var record = new ErrorRecord(ex, "InvalidOrderId", ErrorCategory.InvalidArgument, OrderId);
            ThrowTerminatingError(record);
        }

        if (!ShouldProcess($"Order {OrderId}", "Cancel")) {
            return;
        }

        var config = new ApiConfig(BaseUrl, Username, Password, CustomerUri, ApiVersion);
        ISectigoClient? client = null;
        try {
            client = TestHooks.ClientFactory?.Invoke(config) ?? new SectigoClient(config);
            TestHooks.CreatedClient = client;
            var orders = new OrdersClient(client);
            orders.CancelAsync(OrderId, CancellationToken)
                .GetAwaiter()
                .GetResult();
        } finally {
            if (client is IDisposable disposable) {
                disposable.Dispose();
            }
        }
    }
}