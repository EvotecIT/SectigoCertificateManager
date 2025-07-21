using SectigoCertificateManager;
using System.Management.Automation;

namespace SectigoCertificateManager.PowerShell;

/// <summary>Retrieves cached token information.</summary>
/// <para>Reads the token cache using <see cref="ApiConfigLoader"/>.</para>
[Cmdlet(VerbsCommon.Get, "SectigoToken")]
[CmdletBinding()]
[OutputType(typeof(TokenInfo))]
public sealed class GetSectigoTokenCommand : PSCmdlet {
    /// <summary>Optional path to the token file.</summary>
    [Parameter]
    public string? Path { get; set; }

    /// <summary>Reads the token cache.</summary>
    protected override void ProcessRecord() {
        var info = ApiConfigLoader.ReadToken(Path);
        if (info is not null) {
            WriteObject(info);
        }
    }
}
