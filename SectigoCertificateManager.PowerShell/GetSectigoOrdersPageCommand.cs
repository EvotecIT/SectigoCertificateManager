using SectigoCertificateManager;
using SectigoCertificateManager.Clients;
using SectigoCertificateManager.Requests;
using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;

namespace SectigoCertificateManager.PowerShell;

/// <summary>Retrieves a single page of orders.</summary>
/// <para>Lists orders using paging parameters for the active Sectigo connection.</para>
/// <list type="alertSet">
///   <item>
///     <term>Network</term>
///     <description>This cmdlet issues a paged request to the Sectigo API and may require multiple calls for all data.</description>
///   </item>
/// </list>
/// <example>
///   <summary>Fetch the first page</summary>
///   <prefix>PS&gt; </prefix>
///   <code>Connect-Sectigo -BaseUrl "https://cert-manager.com/api" -Username "user" -Password "pass" -CustomerUri "example"; Get-SectigoOrdersPage -Size 50</code>
///   <para>Retrieves up to fifty orders starting at the beginning of the list for the connected account.</para>
/// </example>
/// <example>
///   <summary>Continue from a specific position</summary>
///   <prefix>PS&gt; </prefix>
///   <code>Get-SectigoOrdersPage -Position 50 -Size 50</code>
///   <para>Retrieves the next fifty orders after position fifty.</para>
/// </example>
/// <seealso href="https://learn.microsoft.com/powershell/scripting/developer/cmdlet/writing-a-cmdlet"/>
/// <seealso href="https://github.com/SectigoCertificateManager/SectigoCertificateManager"/>
[Cmdlet(VerbsCommon.Get, "SectigoOrdersPage")]
[CmdletBinding()]
[OutputType(typeof(Models.Order))]
public sealed class GetSectigoOrdersPageCommand : AsyncPSCmdlet {
    /// <summary>The API version to use when calling the legacy API.</summary>
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
    /// <para>Uses the active Sectigo connection and outputs the orders from a single page.</para>
    protected override async Task ProcessRecordAsync() {
        var adminConfigObj = SessionState.PSVariable.GetValue("SectigoAdminApiConfig");
        if (adminConfigObj is not null) {
            throw new PSInvalidOperationException("Get-SectigoOrdersPage is not yet supported with an Admin (OAuth2) connection. Connect with legacy credentials to use this cmdlet.");
        }

        var config = ConnectionHelper.GetLegacyConfig(SessionState);
        ISectigoClient? client = null;
        try {
            client = TestHooks.ClientFactory?.Invoke(config) ?? new SectigoClient(config);
            TestHooks.CreatedClient = client;
            WriteVerbose(
                $"Requesting orders page from legacy API at '{config.BaseUrl}' with Position={Position?.ToString() ?? "<none>"} and Size={Size?.ToString() ?? "<none>"}.");
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
