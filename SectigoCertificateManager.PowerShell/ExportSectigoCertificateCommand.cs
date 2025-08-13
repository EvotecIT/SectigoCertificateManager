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
/// <example>
///   <summary>Export a certificate as PEM</summary>
///   <prefix>PS&gt; </prefix>
///   <code>Export-SectigoCertificate -BaseUrl "https://api.example.com" -Username "user" -Password "pass" -CustomerUri "example" -CertificateId 10 -Path "cert.pem" -Format Pem</code>
///   <para>Downloads certificate 10 and saves it in PEM format.</para>
/// </example>
/// <example>
///   <summary>Export as PFX with password</summary>
///   <prefix>PS&gt; </prefix>
///   <code>Export-SectigoCertificate -BaseUrl "https://api.example.com" -Username "user" -Password "pass" -CustomerUri "example" -CertificateId 10 -Path "cert.pfx" -Format Pfx -PfxPassword "pwd"</code>
///   <para>Downloads certificate 10 and saves it as a password protected PFX.</para>
/// </example>
/// <seealso href="https://learn.microsoft.com/powershell/scripting/developer/cmdlet/writing-a-cmdlet"/>
/// <seealso href="https://github.com/SectigoCertificateManager/SectigoCertificateManager"/>
[Cmdlet(VerbsData.Export, "SectigoCertificate")]
[CmdletBinding()]
public sealed class ExportSectigoCertificateCommand : PSCmdlet {
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

        var config = new ApiConfig(BaseUrl, Username, Password, CustomerUri, ApiVersion);
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

