using SectigoCertificateManager;
using SectigoCertificateManager.Clients;
using SectigoCertificateManager.Requests;
using SectigoCertificateManager.Responses;
using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;

namespace SectigoCertificateManager.PowerShell;

/// <summary>Retrieves recent SSL certificates.</summary>
/// <para>Calls the SSL certificate search endpoint and returns the requested number of most recent certificates.</para>
/// <example>
///   <summary>Get the latest 30 certificates</summary>
///   <prefix>PS&gt; </prefix>
///   <code>Get-SectigoCertificates -BaseUrl "https://cert-manager.com/api/ssl" -Username "user" -Password "pass" -CustomerUri "tenant" -Size 30</code>
///   <para>Returns up to 30 certificates as provided by the API.</para>
/// </example>
[Cmdlet(VerbsCommon.Get, "SectigoCertificates")]
[CmdletBinding()]
[OutputType(typeof(CertificateResponse))]
public sealed class GetSectigoCertificatesCommand : AsyncPSCmdlet {
    /// <summary>The API base URL (e.g., https://cert-manager.com/api/ssl).</summary>
    [Parameter(Mandatory = true)]
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>User name for authentication.</summary>
    [Parameter(Mandatory = true)]
    public string Username { get; set; } = string.Empty;

    /// <summary>Password for authentication.</summary>
    [Parameter(Mandatory = true)]
    public string Password { get; set; } = string.Empty;

    /// <summary>Customer URI assigned by Sectigo.</summary>
    [Parameter(Mandatory = true)]
    public string CustomerUri { get; set; } = string.Empty;

    /// <summary>API version to use.</summary>
    [Parameter]
    public ApiVersion ApiVersion { get; set; } = ApiVersion.V25_5;

    /// <summary>Maximum number of certificates to retrieve.</summary>
    [Parameter]
    public int Size { get; set; } = 30;

    /// <summary>Optional cancellation token.</summary>
    [Parameter]
    public CancellationToken CancellationToken { get; set; }

    /// <summary>Executes the cmdlet and writes certificate search results.</summary>
    /// <para>Builds a client, calls the certificate search endpoint, and writes the response object to the pipeline.</para>
    protected override async Task ProcessRecordAsync() {
        var config = new ApiConfig(BaseUrl, Username, Password, CustomerUri, ApiVersion);
        ISectigoClient? client = null;
        try {
            client = TestHooks.ClientFactory?.Invoke(config) ?? new SectigoClient(config);
            TestHooks.CreatedClient = client;
            var certificates = new CertificatesClient(client);
            var request = new CertificateSearchRequest { Size = Size };
            var result = await certificates.SearchAsync(request, CancellationToken).ConfigureAwait(false);
            WriteObject(result, true);
        } finally {
            if (client is IDisposable disposable) {
                disposable.Dispose();
            }
        }
    }
}
