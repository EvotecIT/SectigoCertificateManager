namespace SectigoCertificateManager.PowerShell;

using SectigoCertificateManager;
using System.Collections;
using System.Management.Automation;

internal static class DefaultParameterHelper {
    internal static readonly string[] CmdletNames = new[] {
        "Export-SectigoCertificate", "Get-SectigoCertificate", "Get-SectigoCertificateRevocation",
        "Get-SectigoCertificateStatus", "Get-SectigoCertificateTypes", "Get-SectigoOrderHistory",
        "Get-SectigoCertificates", "Get-SectigoInventory", "Get-SectigoEnrollCertificates", "Get-SectigoOrders", "Get-SectigoOrdersPage", "Get-SectigoOrganizations",
        "Get-SectigoProfile", "Get-SectigoProfiles", "New-SectigoOrder",
        "New-SectigoOrganization", "Remove-SectigoCertificate", "Renew-SectigoCertificate",
        "Stop-SectigoOrder", "Update-SectigoCertificate", "Wait-SectigoOrder"
    };

    private static readonly string[] s_parameterNames = { "BaseUrl", "Username", "Password", "CustomerUri", "ApiVersion" };

    internal static void SetDefaults(
        SessionState sessionState,
        string baseUrl,
        string username,
        string password,
        string customerUri,
        ApiVersion apiVersion) {
        var defaults = GetDefaultsTable(sessionState);
        foreach (var cmd in CmdletNames) {
            defaults[$"{cmd}:BaseUrl"] = baseUrl;
            defaults[$"{cmd}:Username"] = username;
            defaults[$"{cmd}:Password"] = password;
            defaults[$"{cmd}:CustomerUri"] = customerUri;
            defaults[$"{cmd}:ApiVersion"] = apiVersion;
        }
    }

    internal static void ClearDefaults(SessionState sessionState) {
        var obj = sessionState.PSVariable.GetValue("PSDefaultParameterValues");
        if (obj is not Hashtable defaults) {
            return;
        }

        foreach (var cmd in CmdletNames) {
            foreach (var param in s_parameterNames) {
                var key = $"{cmd}:{param}";
                if (defaults.ContainsKey(key)) {
                    defaults.Remove(key);
                }
            }
        }
    }

    private static Hashtable GetDefaultsTable(SessionState sessionState) {
        var obj = sessionState.PSVariable.GetValue("PSDefaultParameterValues");
        if (obj is Hashtable table) {
            return table;
        }

        table = new Hashtable(StringComparer.OrdinalIgnoreCase);
        sessionState.PSVariable.Set("PSDefaultParameterValues", table);
        return table;
    }
}
