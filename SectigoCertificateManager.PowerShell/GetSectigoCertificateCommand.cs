using SectigoCertificateManager;
using SectigoCertificateManager.Clients;
using SectigoCertificateManager.Responses;
using System.Management.Automation;
using System.Threading;

namespace SectigoCertificateManager.PowerShell;

/// <summary>Retrieves certificate details.</summary>
/// <para>
/// Uses <see cref="CertificateService"/> to retrieve certificate information
/// for the active Sectigo connection (legacy SCM or Admin Operations API).
/// </para>
/// <list type="alertSet">
///   <item>
///     <term>Network</term>
///     <description>Queries the Sectigo API to fetch certificate data.</description>
///   </item>
/// </list>
/// <example>
///   <summary>Get a single certificate</summary>
///   <prefix>PS&gt; </prefix>
///   <code>Connect-Sectigo -ClientId "&lt;client id&gt;" -ClientSecret "&lt;client secret&gt;"; Get-SectigoCertificate -CertificateId 12345</code>
///   <para>Connects using the Admin API and retrieves certificate 12345.</para>
/// </example>
/// <example>
///   <summary>List the latest certificates (legacy SCM)</summary>
///   <prefix>PS&gt; </prefix>
///   <code>Connect-Sectigo -BaseUrl "https://cert-manager.com/api" -Username "user" -Password "pass" -CustomerUri "tenant"; Get-SectigoCertificate -Size 30</code>
///   <para>Connects using legacy credentials and lists the latest 30 certificates.</para>
/// </example>
/// <seealso href="https://learn.microsoft.com/powershell/scripting/developer/cmdlet/writing-a-cmdlet"/>
/// <seealso href="https://github.com/SectigoCertificateManager/SectigoCertificateManager"/>
[Cmdlet(VerbsCommon.Get, "SectigoCertificate", DefaultParameterSetName = ListParameterSet)]
[Alias("Get-SectigoCertificates")]
[CmdletBinding()]
[OutputType(typeof(Models.Certificate), typeof(CertificateResponse))]
public sealed class GetSectigoCertificateCommand : AsyncPSCmdlet {
    private const string ByIdParameterSet = "ById";
    private const string ListParameterSet = "List";

    /// <summary>The API version to use when using the legacy API.</summary>
    [Parameter]
    public ApiVersion ApiVersion { get; set; } = ApiVersion.V25_6;

    /// <summary>The certificate identifier.</summary>
    [Parameter(Mandatory = true, Position = 0, ParameterSetName = ByIdParameterSet)]
    public int CertificateId { get; set; }

    /// <summary>Maximum number of certificates to retrieve.</summary>
    [Parameter(ParameterSetName = ListParameterSet)]
    public int Size { get; set; } = 30;

    /// <summary>Position offset for paging.</summary>
    [Parameter(ParameterSetName = ListParameterSet)]
    public int Position { get; set; }

    /// <summary>Optional cancellation token.</summary>
    [Parameter]
    public CancellationToken CancellationToken { get; set; }

    /// <summary>Executes the cmdlet.</summary>
    /// <para>Routes certificate retrieval through <see cref="CertificateService"/> using the active connection.</para>
    protected override async Task ProcessRecordAsync() {
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(CancelToken, CancellationToken);
        var effectiveToken = linked.Token;

        CertificateService? service = null;
        try {
            if (ConnectionHelper.TryGetAdminConfig(SessionState, out var adminConfig) && adminConfig is not null) {
                service = new CertificateService(adminConfig);
            } else {
                var config = ConnectionHelper.GetLegacyConfig(SessionState);
                service = new CertificateService(config);
            }

            if (ParameterSetName == ByIdParameterSet) {
                var certificate = await service
                    .GetAsync(CertificateId, effectiveToken)
                    .ConfigureAwait(false);
                WriteObject(certificate);
                return;
            }

            var certificates = await service
                .ListAsync(Size, Position, effectiveToken)
                .ConfigureAwait(false);
            var response = new CertificateResponse { Certificates = certificates };
            WriteObject(response);
        } finally {
            service?.Dispose();
        }
    }
}
