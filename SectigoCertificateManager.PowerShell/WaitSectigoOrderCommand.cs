using SectigoCertificateManager;
using SectigoCertificateManager.Clients;
using System.Management.Automation;
using System.Threading;

namespace SectigoCertificateManager.PowerShell;

/// <summary>Waits for an order to reach a terminal status.</summary>
/// <para>Polls the order status using the active Sectigo connection until it is completed or cancelled.</para>
/// <list type="alertSet">
///   <item>
///     <term>Delay</term>
///     <description>The cmdlet waits and repeatedly queries the API until the order finishes.</description>
///   </item>
/// </list>
/// <example>
///   <summary>Wait for an order</summary>
///   <prefix>PS&gt; </prefix>
///   <code>Connect-Sectigo -BaseUrl "https://cert-manager.com/api" -Username "user" -Password "pass" -CustomerUri "example"; Wait-SectigoOrder -OrderId 100</code>
///   <para>Blocks until order 100 completes or is cancelled for the connected account.</para>
/// </example>
/// <seealso href="https://learn.microsoft.com/powershell/scripting/developer/cmdlet/writing-a-cmdlet"/>
/// <seealso href="https://github.com/SectigoCertificateManager/SectigoCertificateManager"/>
[Cmdlet(VerbsLifecycle.Wait, "SectigoOrder")]
[CmdletBinding()]
[OutputType(typeof(OrderStatus))]
public sealed class WaitSectigoOrderCommand : PSCmdlet {
    /// <summary>The API version to use when calling the legacy API.</summary>
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

        var adminConfigObj = SessionState.PSVariable.GetValue("SectigoAdminApiConfig");
        if (adminConfigObj is not null) {
            throw new PSInvalidOperationException("Wait-SectigoOrder is not yet supported with an Admin (OAuth2) connection. Connect with legacy credentials to use this cmdlet.");
        }

        var config = ConnectionHelper.GetLegacyConfig(SessionState);
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
