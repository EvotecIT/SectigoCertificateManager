using SectigoCertificateManager;
using SectigoCertificateManager.Clients;
using System.Management.Automation;
using System.Threading;

namespace SectigoCertificateManager.PowerShell;

/// <summary>Retrieves order history.</summary>
/// <para>Returns history entries for an order using the active Sectigo connection.</para>
/// <list type="alertSet">
///   <item>
///     <term>Network</term>
///     <description>Contacts the Sectigo API to fetch order history.</description>
///   </item>
/// </list>
/// <example>
///   <summary>Get history for an order</summary>
///   <prefix>PS&gt; </prefix>
///   <code>Connect-Sectigo -BaseUrl "https://cert-manager.com/api" -Username "user" -Password "pass" -CustomerUri "example"; Get-SectigoOrderHistory -OrderId 100</code>
///   <para>Shows all history entries for order 100 on the connected account.</para>
/// </example>
/// <seealso href="https://learn.microsoft.com/powershell/scripting/developer/cmdlet/writing-a-cmdlet"/>
/// <seealso href="https://github.com/SectigoCertificateManager/SectigoCertificateManager"/>
[Cmdlet(VerbsCommon.Get, "SectigoOrderHistory")]
[CmdletBinding()]
[OutputType(typeof(Models.OrderHistoryEntry))]
public sealed class GetSectigoOrderHistoryCommand : PSCmdlet {
    /// <summary>The API version to use when calling the legacy API.</summary>
    [Parameter]
    public ApiVersion ApiVersion { get; set; } = ApiVersion.V25_6;

    /// <summary>The identifier of the order.</summary>
    [Parameter(Mandatory = true, Position = 0)]
    public int OrderId { get; set; }

    /// <summary>Optional cancellation token.</summary>
    [Parameter]
    public CancellationToken CancellationToken { get; set; }

    /// <summary>Executes the cmdlet.</summary>
    /// <para>Retrieves the history for the specified order.</para>
    protected override void ProcessRecord() {
        if (OrderId <= 0) {
            var ex = new ArgumentOutOfRangeException(nameof(OrderId));
            var record = new ErrorRecord(ex, "InvalidOrderId", ErrorCategory.InvalidArgument, OrderId);
            ThrowTerminatingError(record);
        }

        var adminConfigObj = SessionState.PSVariable.GetValue("SectigoAdminApiConfig");
        if (adminConfigObj is not null) {
            throw new PSInvalidOperationException("Get-SectigoOrderHistory is not yet supported with an Admin (OAuth2) connection. Connect with legacy credentials to use this cmdlet.");
        }

        var config = ConnectionHelper.GetLegacyConfig(SessionState);
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
