using SectigoCertificateManager.Utilities;
using System.Management.Automation;

namespace SectigoCertificateManager.PowerShell;

/// <summary>Validates a certificate signing request.</summary>
/// <para>Determines whether the provided CSR is structurally valid.</para>
/// <example>
///   <summary>Validate a CSR</summary>
///   <prefix>PS> </prefix>
///   <code>Test-SectigoCsr -Csr $csr</code>
///   <para>Returns <c>$true</c> when the CSR is valid.</para>
/// </example>
[Cmdlet(VerbsDiagnostic.Test, "SectigoCsr")]
[CmdletBinding()]
[OutputType(typeof(bool))]
public sealed class TestSectigoCsrCommand : PSCmdlet {
    /// <summary>The base64-encoded certificate signing request.</summary>
    [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true)]
    public string Csr { get; set; } = string.Empty;

    /// <summary>Executes the cmdlet.</summary>
    /// <para>Outputs <c>true</c> if the CSR is valid; otherwise, <c>false</c>.</para>
    protected override void ProcessRecord() {
        var result = CsrValidator.IsValid(Csr);
        WriteObject(result);
    }
}
