using SectigoCertificateManager;
using SectigoCertificateManager.Clients;
using SectigoCertificateManager.AdminApi;
using SectigoCertificateManager.Requests;
using System;
using System.Management.Automation;
using System.Threading;

namespace SectigoCertificateManager.PowerShell;

/// <summary>
/// Renews a certificate (legacy by order number, Admin by certificate id).
/// </summary>
/// <para>
/// Legacy mode: uses order number with the legacy API.
/// Admin mode: uses certificate id with the Admin Operations API.
/// </para>
/// <list type="alertSet">
///   <item>
///     <term>Network</term>
///     <description>Contacts the Sectigo API and issues a renewed certificate.</description>
///   </item>
/// </list>
/// <seealso href="https://learn.microsoft.com/powershell/scripting/developer/cmdlet/writing-a-cmdlet"/>
/// <seealso href="https://github.com/SectigoCertificateManager/SectigoCertificateManager"/>
[Cmdlet(VerbsLifecycle.Invoke, "SectigoCertificateRenewal", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium, DefaultParameterSetName = LegacyParameterSet)]
[CmdletBinding()]
[OutputType(typeof(int))]
public sealed class RenewSectigoCertificateCommand : PSCmdlet {
    private const string LegacyParameterSet = "Legacy";
    private const string AdminByIdParameterSet = "AdminById";

    /// <summary>The API version to use (legacy only).</summary>
    [Parameter(ParameterSetName = LegacyParameterSet)]
    public ApiVersion ApiVersion { get; set; } = ApiVersion.V25_6;

    /// <summary>The legacy order number.</summary>
    [Parameter(Mandatory = true, Position = 0, ParameterSetName = LegacyParameterSet)]
    public long OrderNumber { get; set; }

    /// <summary>Certificate identifier (Admin API only).</summary>
    [Parameter(Mandatory = true, Position = 0, ParameterSetName = AdminByIdParameterSet)]
    public int CertificateId { get; set; }

    /// <summary>The certificate signing request.</summary>
    [Parameter(Mandatory = true, ParameterSetName = LegacyParameterSet)]
    [Parameter(Mandatory = true, ParameterSetName = AdminByIdParameterSet)]
    public string Csr { get; set; } = string.Empty;

    /// <summary>The domain control validation mode.</summary>
    [Parameter(Mandatory = true, ParameterSetName = LegacyParameterSet)]
    [Parameter(Mandatory = true, ParameterSetName = AdminByIdParameterSet)]
    public DcvMode DcvMode { get; set; } = DcvMode.Email;

    /// <summary>The domain control validation email address (required when DcvMode is Email).</summary>
    [Parameter(ParameterSetName = LegacyParameterSet)]
    [Parameter(ParameterSetName = AdminByIdParameterSet)]
    public string? DcvEmail { get; set; }

    /// <summary>Optional cancellation token.</summary>
    [Parameter(ParameterSetName = LegacyParameterSet)]
    [Parameter(ParameterSetName = AdminByIdParameterSet)]
    public CancellationToken CancellationToken { get; set; }

    /// <summary>Renews a certificate using the active connection.</summary>
    protected override void ProcessRecord() {
        if (ParameterSetName == LegacyParameterSet && OrderNumber <= 0) {
            ThrowTerminatingError(new ErrorRecord(
                new ArgumentOutOfRangeException(nameof(OrderNumber)),
                "InvalidOrderNumber",
                ErrorCategory.InvalidArgument,
                OrderNumber));
        }

        if (ParameterSetName == AdminByIdParameterSet && CertificateId <= 0) {
            ThrowTerminatingError(new ErrorRecord(
                new ArgumentOutOfRangeException(nameof(CertificateId)),
                "InvalidCertificateId",
                ErrorCategory.InvalidArgument,
                CertificateId));
        }

        var target = ParameterSetName == LegacyParameterSet ? $"Order {OrderNumber}" : $"Certificate {CertificateId}";
        if (!ShouldProcess(target, "Renew")) {
            return;
        }

        var adminConfigObj = SessionState.PSVariable.GetValue("SectigoAdminApiConfig");

        if (ParameterSetName == AdminByIdParameterSet) {
            if (adminConfigObj is not AdminApiConfig adminConfig) {
                throw new PSInvalidOperationException("Renewing by certificate id requires an Admin (OAuth2) connection. Run Connect-Sectigo with -ClientId/-ClientSecret first.");
            }

            var service = new CertificateService(adminConfig);
            WriteVerbose($"Renewing certificate Id={CertificateId} using the Admin API with DCV mode '{DcvMode}'.");
            var request = new RenewCertificateRequest {
                Csr = Csr,
                DcvMode = DcvMode,
                DcvEmail = DcvEmail
            };
            var newId = service.RenewByIdAsync(CertificateId, request, CancellationToken)
                .GetAwaiter()
                .GetResult();
            WriteObject(newId);
            return;
        }

        // Legacy path (order-number based)
        if (adminConfigObj is not null) {
            throw new PSInvalidOperationException("You are connected with Admin (OAuth2) credentials. Use -CertificateId instead of -OrderNumber to renew via the Admin API.");
        }

        var config = ConnectionHelper.GetLegacyConfig(SessionState);
        ISectigoClient? client = null;
        try {
            client = TestHooks.ClientFactory?.Invoke(config) ?? new SectigoClient(config);
            TestHooks.CreatedClient = client;
            WriteVerbose(
                $"Renewing certificate for order number {OrderNumber} using the legacy API at '{config.BaseUrl}' with DCV mode '{DcvMode}'.");
            var certificates = new CertificatesClient(client);
            var request = new RenewCertificateRequest {
                Csr = Csr,
                DcvMode = DcvMode,
                DcvEmail = DcvEmail
            };
            var newId = certificates.RenewByOrderNumberAsync(OrderNumber, request, CancellationToken)
                .GetAwaiter()
                .GetResult();
            WriteObject(newId);
        } finally {
            if (client is IDisposable disposable) {
                disposable.Dispose();
            }
        }
    }
}
