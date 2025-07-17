using SectigoCertificateManager;
using SectigoCertificateManager.Clients;
using System.Management.Automation;
using System.Threading.Tasks;

namespace SectigoCertificateManager.PowerShell;

/// <summary>Retrieves certificate orders.</summary>
/// <para>Creates an API client and lists all orders for the account.</para>
[Cmdlet(VerbsCommon.Get, "SectigoOrders")]
[OutputType(typeof(Models.Order))]
public sealed class GetSectigoOrdersCommand : AsyncPSCmdlet {
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

    /// <summary>Executes the cmdlet.</summary>
    /// <para>Creates an API client and outputs all orders.</para>
    protected override async Task ProcessRecordAsync() {
        var config = new ApiConfig(BaseUrl, Username, Password, CustomerUri, ApiVersion);
        var client = new SectigoClient(config);
        var orders = new OrdersClient(client);

        await foreach (var order in orders.EnumerateOrdersAsync(cancellationToken: CancelToken).ConfigureAwait(false)) {
            WriteObject(order);
        }
    }
}