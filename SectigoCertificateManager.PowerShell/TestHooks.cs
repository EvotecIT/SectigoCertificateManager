namespace SectigoCertificateManager.PowerShell;

using SectigoCertificateManager;
using System;

/// <summary>
/// Provides hooks for testing internal operations.
/// </summary>
public static class TestHooks {
    /// <summary>Optional factory used to create a custom client.</summary>
    public static Func<ApiConfig, ISectigoClient>? ClientFactory { get; set; }

    /// <summary>Stores the most recently created client instance.</summary>
    public static ISectigoClient? CreatedClient { get; set; }
}