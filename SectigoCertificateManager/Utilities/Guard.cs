namespace SectigoCertificateManager.Utilities;

using System;

/// <summary>
/// Provides helper methods for validating arguments.
/// </summary>
public static class Guard {
    /// <summary>Throws <see cref="ArgumentNullException"/> when the value is null.</summary>
    public static T AgainstNull<T>(T? value, string paramName, string? message = null) where T : class {
        if (value is null) {
            throw new ArgumentNullException(paramName, message);
        }

        return value;
    }

    /// <summary>Throws <see cref="ArgumentException"/> when the string is null or empty.</summary>
    public static string AgainstNullOrEmpty(string? value, string paramName, string? message = null) {
        if (string.IsNullOrEmpty(value)) {
            throw new ArgumentException(message ?? "Value cannot be null or empty.", paramName);
        }

        return value;
    }

    /// <summary>Throws <see cref="ArgumentException"/> when the string is null or consists only of whitespace.</summary>
    public static string AgainstNullOrWhiteSpace(string? value, string paramName, string? message = null) {
        if (string.IsNullOrWhiteSpace(value)) {
            throw new ArgumentException(message ?? "Value cannot be null or empty.", paramName);
        }

        return value;
    }
}
