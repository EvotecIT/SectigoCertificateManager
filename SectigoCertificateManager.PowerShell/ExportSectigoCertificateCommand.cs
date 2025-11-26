using SectigoCertificateManager;
using SectigoCertificateManager.Clients;
using SectigoCertificateManager.Utilities;
using System.Management.Automation;
using System.Threading;

namespace SectigoCertificateManager.PowerShell;

/// <summary>Downloads and exports a certificate.</summary>
/// <para>Creates an API client, downloads the certificate, and saves it to disk.</para>
/// <list type="alertSet">
///   <item>
///     <term>Network</term>
///     <description>Requires connectivity to the Sectigo API.</description>
///   </item>
/// </list>
/// <seealso href="https://learn.microsoft.com/powershell/scripting/developer/cmdlet/writing-a-cmdlet"/>
/// <seealso href="https://github.com/SectigoCertificateManager/SectigoCertificateManager"/>
[Cmdlet(VerbsData.Export, "SectigoCertificate")]
[CmdletBinding()]
public sealed class ExportSectigoCertificateCommand : PSCmdlet {
    /// <summary>The API version to use.</summary>
    [Parameter]
    public ApiVersion ApiVersion { get; set; } = ApiVersion.V25_6;

    /// <summary>The certificate identifier.</summary>
    [Parameter(Mandatory = true, Position = 0)]
    public int CertificateId { get; set; }

    /// <summary>Destination file path.</summary>
    [Parameter(Mandatory = true)]
    [AllowEmptyString]
    public string Path { get; set; } = string.Empty;

    /// <summary>Export format.</summary>
    [Parameter]
    public CertificateFileFormat Format { get; set; } = CertificateFileFormat.Pem;

    /// <summary>Password protecting the PFX when <see cref="CertificateFileFormat.Pfx"/> is used.</summary>
    [Parameter]
    public string? PfxPassword { get; set; }

    /// <summary>Optional cancellation token.</summary>
    [Parameter]
    public CancellationToken CancellationToken { get; set; }

    /// <summary>Downloads and exports a certificate.</summary>
    /// <para>Builds an API client, downloads the certificate, and saves it to disk.</para>
    protected override void ProcessRecord() {
        if (CertificateId <= 0) {
            var ex = new ArgumentOutOfRangeException(nameof(CertificateId));
            var record = new ErrorRecord(ex, "InvalidCertificateId", ErrorCategory.InvalidArgument, CertificateId);
            ThrowTerminatingError(record);
        }
        if (string.IsNullOrEmpty(Path)) {
            var ex = new ArgumentException("Path cannot be null or empty.", nameof(Path));
            var record = new ErrorRecord(ex, "InvalidPath", ErrorCategory.InvalidArgument, Path);
            ThrowTerminatingError(record);
        }

        var adminConfigObj = SessionState.PSVariable.GetValue("SectigoAdminApiConfig");
        if (adminConfigObj is not null) {
            throw new PSInvalidOperationException("Export-SectigoCertificate is not yet supported with an Admin (OAuth2) connection. Connect with legacy credentials to use this cmdlet.");
        }

        var config = ConnectionHelper.GetLegacyConfig(SessionState);
        ISectigoClient? client = null;
        try {
            client = TestHooks.ClientFactory?.Invoke(config) ?? new SectigoClient(config);
            TestHooks.CreatedClient = client;
            var certificates = new CertificatesClient(client);
            var certificate = certificates.DownloadAsync(CertificateId, CancellationToken)
                .GetAwaiter()
                .GetResult();
            try {
                switch (Format) {
                    case CertificateFileFormat.Pem:
                        CertificateExport.SavePem(certificate, Path);
                        break;
                    case CertificateFileFormat.Der:
                        CertificateExport.SaveDer(certificate, Path);
                        break;
                    case CertificateFileFormat.Pfx:
                        CertificateExport.SavePfx(certificate, Path, PfxPassword);
                        break;
                }
            } finally {
                certificate.Dispose();
            }
        } finally {
            if (client is IDisposable disposable) {
                disposable.Dispose();
            }
        }
    }
}

/// <summary>Supported certificate file formats.</summary>
public enum CertificateFileFormat {
    /// <summary>Privacy Enhanced Mail.</summary>
    Pem,

    /// <summary>Distinguished Encoding Rules.</summary>
    Der,

    /// <summary>PFX container.</summary>
    Pfx
}

