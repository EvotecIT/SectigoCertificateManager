namespace SectigoCertificateManager.PowerShell;

using System.Management.Automation;

/// <summary>Clears shared defaults set by <see cref="ConnectSectigoCommand"/>.</summary>
/// <para>Removes Sectigo entries from <c>PSDefaultParameterValues</c> so subsequent cmdlets no longer inherit the connection parameters.</para>
/// <example>
///   <summary>Clear stored defaults</summary>
///   <prefix>PS&gt; </prefix>
///   <code>Disconnect-Sectigo</code>
///   <para>After running, you must provide connection parameters again or call <c>Connect-Sectigo</c>.</para>
/// </example>
[Cmdlet(VerbsCommunications.Disconnect, "Sectigo")]
[CmdletBinding()]
public sealed class DisconnectSectigoCommand : PSCmdlet {
    /// <summary>Clears default parameter values.</summary>
    protected override void ProcessRecord() {
        WriteVerbose("Clearing Sectigo connection defaults and configuration variables.");
        DefaultParameterHelper.ClearDefaults(SessionState);
        SessionState.PSVariable.Remove("SectigoApiConfig");
        SessionState.PSVariable.Remove("SectigoAdminApiConfig");
    }
}
