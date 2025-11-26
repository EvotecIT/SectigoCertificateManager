using SectigoCertificateManager;
using SectigoCertificateManager.Clients;
using SectigoCertificateManager.Requests;
using SectigoCertificateManager.Models;
using System.Globalization;
using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;

namespace SectigoCertificateManager.PowerShell;

/// <summary>Retrieves certificate inventory as exposed by the Sectigo SSL API.</summary>
/// <para>Wraps the inventory CSV endpoint and returns parsed <see cref="InventoryRecord"/> objects.</para>
/// <example>
///   <summary>Certificates expiring in the next 30 days</summary>
///   <prefix>PS&gt; </prefix>
///   <code>$to = (Get-Date).Date.AddDays(30); Get-SectigoInventory -DateTo $to</code>
///   <para>Filters inventory by an upper expiration bound.</para>
/// </example>
[Cmdlet(VerbsCommon.Get, "SectigoInventory")]
[CmdletBinding()]
[OutputType(typeof(InventoryRecord))]
public sealed class GetSectigoInventoryCommand : AsyncPSCmdlet {
    /// <summary>API version to use.</summary>
    [Parameter]
    public ApiVersion ApiVersion { get; set; } = ApiVersion.V25_6;

    /// <summary>Maximum number of records to return.</summary>
    [Parameter]
    public int? Size { get; set; } = 50;

    /// <summary>Position offset for paging.</summary>
    [Parameter]
    public int? Position { get; set; } = 0;

    /// <summary>Filter certificates issued or updated from this date (yyyy-MM-dd).</summary>
    [Parameter]
    public DateTime? DateFrom { get; set; }

    /// <summary>Filter certificates issued or updated up to this date (yyyy-MM-dd).</summary>
    [Parameter]
    public DateTime? DateTo { get; set; }

    /// <summary>Optional cancellation token.</summary>
    [Parameter]
    public CancellationToken CancellationToken { get; set; }

    /// <summary>Downloads inventory and writes <see cref="InventoryRecord"/> objects.</summary>
    protected override async Task ProcessRecordAsync() {
        var adminConfigObj = SessionState.PSVariable.GetValue("SectigoAdminApiConfig");
        if (adminConfigObj is not null) {
            throw new PSInvalidOperationException("Get-SectigoInventory is not available with an Admin (OAuth2) connection. Use Get-SectigoCertificates instead or connect with legacy credentials.");
        }

        var config = ConnectionHelper.GetLegacyConfig(SessionState);
        var baseUrl = config.BaseUrl.TrimEnd('/');
        var fromText = DateFrom?.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "<none>";
        var toText = DateTo?.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "<none>";
        var sizeText = Size?.ToString(CultureInfo.InvariantCulture) ?? "<none>";
        var positionText = Position?.ToString(CultureInfo.InvariantCulture) ?? "<none>";
        WriteVerbose(
            $"Requesting inventory from '{baseUrl}/v1/inventory.csv' with Size={sizeText}, Position={positionText}, DateFrom={fromText}, DateTo={toText}, ApiVersion={ApiVersion}.");

        ISectigoClient? client = null;
        try {
            client = TestHooks.ClientFactory?.Invoke(config) ?? new SectigoClient(config);
            TestHooks.CreatedClient = client;
            var inventory = new InventoryClient(client);
            var request = new InventoryCsvRequest {
                Size = Size,
                Position = Position,
                DateFrom = DateFrom?.Date,
                DateTo = DateTo?.Date
            };
            var records = await inventory.DownloadCsvAsync(request, CancellationToken).ConfigureAwait(false);
            WriteObject(records, true);
        } finally {
            if (client is IDisposable disposable) {
                disposable.Dispose();
            }
        }
    }
}
