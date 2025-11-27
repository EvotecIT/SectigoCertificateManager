using SectigoCertificateManager;
using SectigoCertificateManager.Clients;
using SectigoCertificateManager.Responses;
using System;
using System.Management.Automation;
using System.Net.Http;
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
/// <example>
///   <summary>Filter Admin certificates by status, requester and expiration</summary>
///   <prefix>PS&gt; </prefix>
///   <code>Connect-Sectigo -ClientId "&lt;client id&gt;" -ClientSecret "&lt;client secret&gt;"; Get-SectigoCertificate -Size 50 -Status Issued -Requester "user@example.com" -ExpiresBefore (Get-Date).AddDays(30) -Detailed</code>
///   <para>Connects using the Admin API and lists detailed certificates that are issued, requested by the specified user and expiring within the next 30 days.</para>
/// </example>
/// <seealso href="https://learn.microsoft.com/powershell/scripting/developer/cmdlet/writing-a-cmdlet"/>
/// <seealso href="https://github.com/SectigoCertificateManager/SectigoCertificateManager"/>
[Cmdlet(VerbsCommon.Get, "SectigoCertificate", DefaultParameterSetName = ListParameterSet)]
[Alias("Get-SectigoCertificates")]
[CmdletBinding()]
[OutputType(typeof(Models.Certificate))]
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

    /// <summary>
    /// Optional certificate status filter (for example, Issued, Expired). When specified, applies only to Admin connections.
    /// </summary>
    [Parameter(ParameterSetName = ListParameterSet)]
    [ValidateNotNullOrEmpty]
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Optional organization identifier filter. When specified, applies only to Admin connections.
    /// </summary>
    [Parameter(ParameterSetName = ListParameterSet)]
    public int OrgId { get; set; }

    /// <summary>
    /// Optional requester filter. When specified, applies only to Admin connections.
    /// </summary>
    [Parameter(ParameterSetName = ListParameterSet)]
    [ValidateNotNullOrEmpty]
    public string Requester { get; set; } = string.Empty;

    /// <summary>
    /// Optional filter for certificates that expire within the specified number of days from now.
    /// When specified, this filter is only applied for Admin (OAuth2) connections and uses detailed
    /// certificate information. Ignored for legacy API connections, which do not expose an Admin-style
    /// expiry filter.
    /// </summary>
    [Parameter(ParameterSetName = ListParameterSet)]
    [ValidateRange(1, int.MaxValue)]
    public int? ExpiresWithinDays { get; set; }

    /// <summary>
    /// Optional upper bound for the certificate expiration date. When specified, only certificates
    /// expiring on or before this date (inclusive) are returned when using the Admin API.
    /// Ignored for the legacy API.
    /// </summary>
    [Parameter(ParameterSetName = ListParameterSet)]
    public DateTime? ExpiresBefore { get; set; }

    /// <summary>
    /// Optional lower bound for the certificate expiration date. When specified, only certificates
    /// expiring on or after this date (inclusive) are returned when using the Admin API.
    /// Ignored for the legacy API.
    /// </summary>
    [Parameter(ParameterSetName = ListParameterSet)]
    public DateTime? ExpiresAfter { get; set; }

    /// <summary>
    /// When specified, retrieves full certificate details for each entry when using the Admin Operations API.
    /// Ignored when using the legacy API, which already returns detailed certificate objects.
    /// </summary>
    [Parameter(ParameterSetName = ListParameterSet)]
    public SwitchParameter Detailed { get; set; }

    /// <summary>Optional cancellation token.</summary>
    [Parameter]
    public CancellationToken CancellationToken { get; set; }

    /// <summary>Executes the cmdlet.</summary>
    /// <para>Routes certificate retrieval through <see cref="CertificateService"/> using the active connection.</para>
    protected override async Task ProcessRecordAsync() {
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(CancelToken, CancellationToken);
        var effectiveToken = linked.Token;

        CertificateService? service = null;
        var usingAdmin = false;
        try {
            if (ConnectionHelper.TryGetAdminConfig(SessionState, out var adminConfig) && adminConfig is not null) {
                service = new CertificateService(adminConfig);
                usingAdmin = true;
            } else {
                var config = ConnectionHelper.GetLegacyConfig(SessionState);
                service = new CertificateService(config);
            }

            if (ParameterSetName == ByIdParameterSet) {
                try {
                    var certificate = await service
                        .GetAsync(CertificateId, effectiveToken)
                        .ConfigureAwait(false);
                    WriteObject(certificate);
                } catch (ApiException ex) {
                    HandlePipelineException(ex);
                } catch (HttpRequestException ex) {
                    HandlePipelineException(ex);
                }
                return;
            }

            IReadOnlyList<Models.Certificate> certificates;
            var statusFilter = string.IsNullOrWhiteSpace(Status) ? null : Status;
            int? orgIdFilter = OrgId > 0 ? OrgId : null;
            var requesterFilter = string.IsNullOrWhiteSpace(Requester) ? null : Requester;
            DateTimeOffset? expiresBeforeFilter = ExpiresBefore is null ? null : new DateTimeOffset(ExpiresBefore.Value);
            DateTimeOffset? expiresAfterFilter = ExpiresAfter is null ? null : new DateTimeOffset(ExpiresAfter.Value);
            try {
                if (ExpiresWithinDays.HasValue) {
                    if (!usingAdmin) {
                        throw new PSInvalidOperationException("The ExpiresWithinDays parameter is only supported with an Admin (OAuth2) connection. Connect-Sectigo with -ClientId/-ClientSecret to use this filter.");
                    }

                    var expiresWithin = ExpiresWithinDays.Value;
                    IProgress<int>? progress = null;
                    if (MyInvocation.BoundParameters.ContainsKey("Verbose")) {
                        var activityId = 1;
                        var activity = $"Finding certificates expiring within {expiresWithin} days";
                        progress = new Progress<int>(count => {
                            var record = new ProgressRecord(activityId, activity, $"Processed {count} certificates...");
                            record.PercentComplete = -1;
                            WriteProgress(record);
                            WriteVerbose($"Processed {count} certificates while searching for expiring certificates.");
                        });
                    }

                    certificates = await service
                        .ListExpiringAsync(expiresWithin, statusFilter, orgIdFilter, requesterFilter, effectiveToken, progress)
                        .ConfigureAwait(false);
                } else if (Detailed.IsPresent) {
                    certificates = await service
                        .ListDetailedAsync(Size, Position, statusFilter, orgIdFilter, requesterFilter, expiresBeforeFilter, expiresAfterFilter, effectiveToken)
                        .ConfigureAwait(false);
                } else {
                    certificates = await service
                        .ListAsync(Size, Position, statusFilter, orgIdFilter, requesterFilter, expiresBeforeFilter, expiresAfterFilter, effectiveToken)
                        .ConfigureAwait(false);
                }

                WriteObject(certificates, enumerateCollection: true);
            } catch (ApiException ex) {
                HandlePipelineException(ex);
            } catch (HttpRequestException ex) {
                HandlePipelineException(ex);
            }
        } finally {
            service?.Dispose();
        }
    }

    private void HandlePipelineException(Exception exception) {
        if (ShouldTreatAsTerminating()) {
            var record = new ErrorRecord(
                exception,
                "SectigoCertificateManagerError",
                ErrorCategory.InvalidOperation,
                null);
            ThrowTerminatingError(record);
        } else {
            WriteWarning(exception.Message);
        }
    }

    private bool ShouldTreatAsTerminating() {
        if (MyInvocation.BoundParameters.TryGetValue("ErrorAction", out var value)) {
            if (value is ActionPreference pref) {
                return pref == ActionPreference.Stop;
            }

            if (value is string text && Enum.TryParse<ActionPreference>(text, ignoreCase: true, out var parsed)) {
                return parsed == ActionPreference.Stop;
            }
        }

        return false;
    }
}
