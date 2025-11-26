using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using SectigoCertificateManager;
using System.Management.Automation;

namespace SectigoCertificateManager.PowerShell;

/// <summary>Lists certificates using the Enroll/Enterprise endpoint (<c>/api/v1/certificates</c>).</summary>
/// <para>Use this when your tenant exposes the Enroll API shown in the portal (e.g., https://yourtenant.enroll.enterprise.sectigo.com/api/v1/certificates).</para>
[Cmdlet(VerbsCommon.Get, "SectigoEnrollCertificates")]
[CmdletBinding()]
public sealed class GetSectigoEnrollCertificatesCommand : AsyncPSCmdlet {
    /// <summary>Base URL, e.g., https://company.enroll.enterprise.sectigo.com.</summary>
    [Parameter(Mandatory = true)]
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>User name for authentication.</summary>
    [Parameter(Mandatory = true)]
    public string Username { get; set; } = string.Empty;

    /// <summary>Password for authentication.</summary>
    [Parameter(Mandatory = true)]
    public string Password { get; set; } = string.Empty;

    /// <summary>Customer URI.</summary>
    [Parameter(Mandatory = true)]
    public string CustomerUri { get; set; } = string.Empty;

    /// <summary>Maximum records to fetch.</summary>
    [Parameter]
    public int Size { get; set; } = 30;

    /// <summary>Paging offset.</summary>
    [Parameter]
    public int Position { get; set; } = 0;

    /// <summary>Optional cancellation token.</summary>
    [Parameter]
    public CancellationToken CancellationToken { get; set; }

    /// <summary>Calls the Enroll list endpoint and writes JSON results.</summary>
    protected override async Task ProcessRecordAsync() {
        using var client = new HttpClient();
        var url = $"{BaseUrl.TrimEnd('/')}/api/v1/certificates?size={Size}&position={Position}";
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("login", Username);
        request.Headers.Add("password", Password);
        request.Headers.Add("customerUri", CustomerUri);

        using var response = await client.SendAsync(request, CancellationToken).ConfigureAwait(false);
        var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        using var doc = JsonDocument.Parse(body);
        WriteObject(doc.RootElement, true);
    }
}
