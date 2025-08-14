using SectigoCertificateManager;
using SectigoCertificateManager.Clients;
using SectigoCertificateManager.Requests;
using System;
using System.Management.Automation;
using System.Threading;

namespace SectigoCertificateManager.PowerShell;

/// <summary>Renews a certificate using an order number.</summary>
/// <para>Builds an API client and submits a <see cref="RenewCertificateRequest"/> identified by order number.</para>
/// <list type="alertSet">
///   <item>
///     <term>Network</term>
///     <description>Contacts the Sectigo API and issues a new certificate for the order.</description>
///   </item>
/// </list>
/// <example>
///   <summary>Renew by order number</summary>
///   <prefix>PS&gt; </prefix>
///   <code>Renew-SectigoCertificate -BaseUrl "https://api.example.com" -Username "user" -Password "pass" -CustomerUri "example" -OrderNumber 10 -Csr "CSR" -DcvMode "Email"</code>
///   <para>Renews the certificate associated with order 10.</para>
/// </example>
/// <example>
///   <summary>Specify a DCV email</summary>
///   <prefix>PS&gt; </prefix>
///   <code>Renew-SectigoCertificate -BaseUrl "https://api.example.com" -Username "user" -Password "pass" -CustomerUri "example" -OrderNumber 10 -Csr "CSR" -DcvMode "Email" -DcvEmail "admin@example.com"</code>
///   <para>Sends the domain control validation to a specific address.</para>
/// </example>
/// <seealso href="https://learn.microsoft.com/powershell/scripting/developer/cmdlet/writing-a-cmdlet"/>
/// <seealso href="https://github.com/SectigoCertificateManager/SectigoCertificateManager"/>
[Cmdlet("Renew", "SectigoCertificate", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
[CmdletBinding()]
[OutputType(typeof(int))]
public sealed class RenewSectigoCertificateCommand : PSCmdlet {
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

    /// <summary>The order number used to identify the certificate.</summary>
    [Parameter(Mandatory = true, Position = 0)]
    public long OrderNumber { get; set; }

    /// <summary>The certificate signing request.</summary>
    [Parameter(Mandatory = true)]
    public string Csr { get; set; } = string.Empty;

    /// <summary>The domain control validation mode.</summary>
    [Parameter(Mandatory = true)]
    public string DcvMode { get; set; } = string.Empty;

    /// <summary>The domain control validation email address.</summary>
    [Parameter]
    public string? DcvEmail { get; set; }

    /// <summary>Optional cancellation token.</summary>
    [Parameter]
    public CancellationToken CancellationToken { get; set; }

    /// <summary>Renews a certificate using provided parameters.</summary>
    /// <para>Builds an API client and submits a <see cref="RenewCertificateRequest"/>.</para>
    protected override void ProcessRecord() {
        if (OrderNumber <= 0) {
            var ex = new ArgumentOutOfRangeException(nameof(OrderNumber));
            var record = new ErrorRecord(ex, "InvalidOrderNumber", ErrorCategory.InvalidArgument, OrderNumber);
            ThrowTerminatingError(record);
        }

        if (!ShouldProcess($"Order {OrderNumber}", "Renew")) {
            return;
        }

        var config = new ApiConfig(BaseUrl, Username, Password, CustomerUri, ApiVersion);
        ISectigoClient? client = null;
        try {
            client = TestHooks.ClientFactory?.Invoke(config) ?? new SectigoClient(config);
            TestHooks.CreatedClient = client;
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
