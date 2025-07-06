using SectigoCertificateManager;
using SectigoCertificateManager.Clients;
using System.Management.Automation;

namespace SectigoCertificateManager.PowerShell;

[Cmdlet(VerbsCommon.Get, "SectigoOrders")]
[OutputType(typeof(Models.Order))]
public sealed class GetSectigoOrdersCommand : PSCmdlet {
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

    protected override void ProcessRecord() {
        var config = new ApiConfig(BaseUrl, Username, Password, CustomerUri, ApiVersion);
        var client = new SectigoClient(config);
        var orders = new OrdersClient(client);
        var enumerator = orders.EnumerateOrdersAsync().GetAsyncEnumerator();

        try {
            while (enumerator.MoveNextAsync().GetAwaiter().GetResult()) {
                WriteObject(enumerator.Current);
            }
        } finally {
            enumerator.DisposeAsync().GetAwaiter().GetResult();
        }
    }
}