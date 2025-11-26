namespace SectigoCertificateManager.PowerShell;

using SectigoCertificateManager;
using SectigoCertificateManager.AdminApi;
using System.Management.Automation;

internal static class ConnectionHelper {
    internal static bool TryGetAdminConfig(SessionState sessionState, out AdminApiConfig config) {
        var obj = sessionState.PSVariable.GetValue("SectigoAdminApiConfig");
        if (obj is AdminApiConfig admin) {
            config = admin;
            return true;
        }

        config = null!;
        return false;
    }

    internal static ApiConfig GetLegacyConfig(SessionState sessionState) {
        var obj = sessionState.PSVariable.GetValue("SectigoApiConfig");
        if (obj is ApiConfig config) {
            return config;
        }

        throw new PSInvalidOperationException("No Sectigo legacy connection is configured. Run Connect-Sectigo with legacy credentials before calling this cmdlet.");
    }
}

