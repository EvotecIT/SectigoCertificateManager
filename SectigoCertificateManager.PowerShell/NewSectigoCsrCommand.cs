namespace SectigoCertificateManager.PowerShell;

using System.Management.Automation;
using SectigoCertificateManager.Requests;
using SectigoCertificateManager.Responses;
using SectigoCertificateManager.Utilities;

/// <summary>
/// Generates a certificate signing request and returns the CSR and key material.
/// </summary>
[Cmdlet(VerbsCommon.New, "SectigoCsr")]
[OutputType(typeof(GeneratedCsr))]
public sealed class NewSectigoCsrCommand : PSCmdlet {
    private const int DefaultKeySize = 2048;

    /// <summary>Common name (CN) for the subject.</summary>
    [Parameter(Mandatory = true, Position = 0)]
    [ValidateNotNullOrEmpty]
    public string CommonName { get; set; } = string.Empty;

    /// <summary>Optional DNS names for Subject Alternative Name.</summary>
    [Parameter()]
    public string[] DnsName { get; set; } = System.Array.Empty<string>();

    /// <summary>Organization (O) field.</summary>
    [Parameter()]
    public string? Organization { get; set; }
        = null;

    /// <summary>Organizational Unit (OU) field.</summary>
    [Parameter()]
    public string? OrganizationalUnit { get; set; }
        = null;

    /// <summary>Locality / City (L) field.</summary>
    [Parameter()]
    public string? Locality { get; set; }
        = null;

    /// <summary>State or Province (ST) field.</summary>
    [Parameter()]
    public string? StateOrProvince { get; set; }
        = null;

    /// <summary>Country code (C) field.</summary>
    [Parameter()]
    public string? Country { get; set; }
        = null;

    /// <summary>Email address (E) field.</summary>
    [Parameter()]
    public string? EmailAddress { get; set; }
        = null;

    /// <summary>Key algorithm to use.</summary>
    [Parameter()]
    public CsrKeyType KeyType { get; set; } = CsrKeyType.Rsa;

    /// <summary>RSA key size (applies when KeyType is RSA).</summary>
    [Parameter()]
    [ValidateRange(1024, int.MaxValue)]
    public int KeySize { get; set; } = DefaultKeySize;

    /// <summary>Elliptic curve (applies when KeyType is Ecdsa).</summary>
    [Parameter()]
    public CsrCurve Curve { get; set; } = CsrCurve.P256;

    /// <summary>Hash algorithm name (e.g., SHA256).</summary>
    [Parameter()]
    [ValidateNotNullOrEmpty]
    public string HashAlgorithm { get; set; } = "SHA256";

    /// <inheritdoc />
    protected override void ProcessRecord() {
        var options = new CsrOptions {
            CommonName = CommonName,
            Organization = Organization,
            OrganizationalUnit = OrganizationalUnit,
            Locality = Locality,
            StateOrProvince = StateOrProvince,
            Country = Country,
            EmailAddress = EmailAddress,
            KeyType = KeyType,
            KeySize = KeySize,
            Curve = Curve,
            HashAlgorithm = HashAlgorithm
        };

        foreach (var dns in DnsName ?? System.Array.Empty<string>()) {
            options.DnsNames.Add(dns);
        }

        var result = CsrGenerator.Generate(options);
        WriteObject(result);
    }
}
