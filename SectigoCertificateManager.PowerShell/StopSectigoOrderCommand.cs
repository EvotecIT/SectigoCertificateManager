using SectigoCertificateManager;
using SectigoCertificateManager.Clients;
using System;
using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;

namespace SectigoCertificateManager.PowerShell;

/// <summary>Cancels an order.</summary>
/// <para>Calls the cancel endpoint using the active Sectigo connection.</para>
/// <list type="alertSet">
///   <item>
///     <term>Irreversible</term>
///     <description>Once an order is cancelled the action cannot be reversed.</description>
///   </item>
/// </list>
/// <example>
///   <summary>Cancel an order</summary>
///   <prefix>PS&gt; </prefix>
///   <code>Connect-Sectigo -BaseUrl "https://cert-manager.com/api" -Username "user" -Password "pass" -CustomerUri "example"; Stop-SectigoOrder -OrderId 100</code>
///   <para>Cancels the specified order immediately for the connected account.</para>
/// </example>
/// <seealso href="https://learn.microsoft.com/powershell/scripting/developer/cmdlet/confirmimpact-attribute"/>
/// <seealso href="https://github.com/SectigoCertificateManager/SectigoCertificateManager"/>
[Cmdlet(VerbsLifecycle.Stop, "SectigoOrder", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
[CmdletBinding()]
public sealed class StopSectigoOrderCommand : AsyncPSCmdlet {
    /// <summary>The API version to use when calling the legacy API.</summary>
    [Parameter]
    public ApiVersion ApiVersion { get; set; } = ApiVersion.V25_6;

    /// <summary>The identifier of the order to cancel.</summary>
    [Parameter(Mandatory = true, Position = 0)]
    public int OrderId { get; set; }

    /// <summary>Optional cancellation token.</summary>
    [Parameter]
    public CancellationToken CancellationToken { get; set; }

    /// <summary>Cancels an order.</summary>
    /// <para>Calls the cancel endpoint for the specified order.</para>
    protected override async Task ProcessRecordAsync() {
        if (OrderId <= 0) {
            var ex = new ArgumentOutOfRangeException(nameof(OrderId));
            var record = new ErrorRecord(ex, "InvalidOrderId", ErrorCategory.InvalidArgument, OrderId);
            ThrowTerminatingError(record);
        }

        if (!ShouldProcess($"Order {OrderId}", "Cancel")) {
            return;
        }

        var adminConfigObj = SessionState.PSVariable.GetValue("SectigoAdminApiConfig");
        if (adminConfigObj is not null) {
            throw new PSInvalidOperationException("Stop-SectigoOrder is not yet supported with an Admin (OAuth2) connection. Connect with legacy credentials to use this cmdlet.");
        }

        using var linked = CancellationTokenSource.CreateLinkedTokenSource(CancelToken, CancellationToken);
        var effectiveToken = linked.Token;

        var config = ConnectionHelper.GetLegacyConfig(SessionState);
        ISectigoClient? client = null;
        try {
            client = TestHooks.ClientFactory?.Invoke(config) ?? new SectigoClient(config);
            TestHooks.CreatedClient = client;
            var orders = new OrdersClient(client);
            await orders
                .CancelAsync(OrderId, effectiveToken)
                .ConfigureAwait(false);
        } finally {
            if (client is IDisposable disposable) {
                disposable.Dispose();
            }
        }
    }
}
