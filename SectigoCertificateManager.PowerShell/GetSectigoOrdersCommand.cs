using SectigoCertificateManager;
using SectigoCertificateManager.Clients;
using System.Management.Automation;

namespace SectigoCertificateManager.PowerShell;

/// <summary>Lists orders from Sectigo Certificate Manager.</summary>
[Cmdlet(VerbsCommon.Get, "SectigoOrders")]
[OutputType(typeof(Models.Order))]
public sealed class GetSectigoOrdersCommand : PSCmdlet {
    /// <para>Base address of the Sectigo API.</para>
    [Parameter(Mandatory = true)]
    public string BaseUrl { get; set; } = string.Empty;

    /// <para>API username.</para>
    [Parameter(Mandatory = true)]
    public string Username { get; set; } = string.Empty;

    /// <para>API password.</para>
    [Parameter(Mandatory = true)]
    public string Password { get; set; } = string.Empty;

    /// <para>Customer URI associated with the account.</para>
    [Parameter(Mandatory = true)]
    public string CustomerUri { get; set; } = string.Empty;

    /// <para>API version to use.</para>
    [Parameter]
    public ApiVersion ApiVersion { get; set; } = ApiVersion.V25_4;

    /// <summary>Executes the cmdlet.</summary>
    protected override void ProcessRecord() {
        var config = new ApiConfig(BaseUrl, Username, Password, CustomerUri, ApiVersion);
        var client = new SectigoClient(config);
        var orders = new OrdersClient(client);
        var result = orders.ListOrdersAsync().GetAwaiter().GetResult();
        if (result is not null) {
            foreach (var order in result) {
                WriteObject(order);
            }
        }
    }
}