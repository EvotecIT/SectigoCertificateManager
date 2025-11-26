using SectigoCertificateManager;
using SectigoCertificateManager.Clients;
using SectigoCertificateManager.Responses;
using System.Management.Automation;
using System.Threading;

namespace SectigoCertificateManager.PowerShell;

/// <summary>Retrieves certificate details.</summary>
/// <para>Builds an API client and returns the certificate for the specified identifier.</para>
/// <list type="alertSet">
///   <item>
///     <term>Network</term>
///     <description>Queries the Sectigo API to fetch certificate data.</description>
///   </item>
/// </list>
/// <seealso href="https://learn.microsoft.com/powershell/scripting/developer/cmdlet/writing-a-cmdlet"/>
/// <seealso href="https://github.com/SectigoCertificateManager/SectigoCertificateManager"/>
[Cmdlet(VerbsCommon.Get, "SectigoCertificate", DefaultParameterSetName = ListParameterSet)]
[Alias("Get-SectigoCertificates")]
[CmdletBinding()]
[OutputType(typeof(Models.Certificate), typeof(CertificateResponse))]
public sealed class GetSectigoCertificateCommand : PSCmdlet {
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
    protected override void ProcessRecord() {
        var hasAdmin = ConnectionHelper.TryGetAdminConfig(SessionState, out var adminConfig);
        CertificateService? service = null;
        try {
            if (hasAdmin) {
                service = new CertificateService(adminConfig!);
            } else {
                var config = ConnectionHelper.GetLegacyConfig(SessionState);
                service = new CertificateService(config);
            }

            if (ParameterSetName == ByIdParameterSet) {
                var certificate = service
                    .GetAsync(CertificateId, CancellationToken)
                    .GetAwaiter()
                    .GetResult();
                WriteObject(certificate);
                return;
            }

            var certificates = service
                .ListAsync(Size, Position, CancellationToken)
                .GetAwaiter()
                .GetResult();
            var response = new CertificateResponse { Certificates = certificates };
            WriteObject(response);
        } finally {
            service?.Dispose();
        }
    }
}
