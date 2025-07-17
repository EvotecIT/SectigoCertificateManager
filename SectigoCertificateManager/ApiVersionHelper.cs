namespace SectigoCertificateManager;

using System;

/// <summary>
/// Provides helpers for parsing Sectigo Certificate Manager API version strings.
/// </summary>
public static class ApiVersionHelper {
    /// <summary>Parses a version string into <see cref="ApiVersion"/>.</summary>
    /// <param name="value">String representation of the version.</param>
    public static ApiVersion Parse(string? value) {
        if (string.IsNullOrWhiteSpace(value)) {
            return ApiVersion.V25_6;
        }

        var trimmed = value.Trim();
        if (trimmed.StartsWith("v", StringComparison.OrdinalIgnoreCase)) {
            trimmed = trimmed.Substring(1);
        }

        trimmed = trimmed.Replace('.', '_');
        if (!trimmed.StartsWith("V", StringComparison.OrdinalIgnoreCase)) {
            trimmed = "V" + trimmed;
        }

        return Enum.TryParse<ApiVersion>(trimmed, ignoreCase: true, out var v)
            ? v
            : ApiVersion.V25_6;
    }
}
