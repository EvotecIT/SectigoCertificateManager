using SectigoCertificateManager;
using SectigoCertificateManager.Clients;
using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;

namespace SectigoCertificateManager.PowerShell;

/// <summary>Retrieves audit log entries.</summary>
/// <para>Creates an API client and lists audit log entries for the account.</para>
[Cmdlet(VerbsCommon.Get, "SectigoAuditLog")]
[CmdletBinding()]
[OutputType(typeof(Models.AuditLogEntry))]
public sealed class GetSectigoAuditLogCommand : AsyncPSCmdlet {
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

    /// <summary>Filters entries from this date.</summary>
    [Parameter]
    public DateTimeOffset? From { get; set; }

    /// <summary>Filters entries up to this date.</summary>
    [Parameter]
    public DateTimeOffset? To { get; set; }

    /// <summary>Number of entries to request per page.</summary>
    [Parameter]
    public int PageSize { get; set; } = 200;

    /// <summary>Optional cancellation token.</summary>
    [Parameter]
    public CancellationToken CancellationToken { get; set; }

    /// <summary>Executes the cmdlet.</summary>
    /// <para>Creates an API client and outputs audit log entries.</para>
    protected override async Task ProcessRecordAsync() {
        var config = new ApiConfig(BaseUrl, Username, Password, CustomerUri, ApiVersion);
        ISectigoClient? client = null;
        try {
            client = TestHooks.ClientFactory?.Invoke(config) ?? new SectigoClient(config);
            TestHooks.CreatedClient = client;
            var audits = new AuditClient(client);

            using var linked = CancellationTokenSource.CreateLinkedTokenSource(CancelToken, CancellationToken);
            await foreach (var entry in audits.EnumerateAsync(From, To, PageSize, linked.Token).ConfigureAwait(false)) {
                WriteObject(entry);
            }
        } finally {
            if (client is IDisposable disposable) {
                disposable.Dispose();
            }
        }
    }
}
