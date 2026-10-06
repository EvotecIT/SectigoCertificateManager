namespace SectigoCertificateManager.Tests;

/// <summary>Isolates process configuration for a test and restores it even when assertions fail.</summary>
/// <remarks>Use only in <see cref="ApiConfigEnvironmentCollection"/> to exclude parallel cache consumers.</remarks>
internal sealed class ApiConfigEnvironmentScope : IDisposable {
    internal static readonly string[] VariableNames = {
        "SECTIGO_BASE_URL", "SECTIGO_USERNAME", "SECTIGO_PASSWORD", "SECTIGO_TOKEN",
        "SECTIGO_CUSTOMER_URI", "SECTIGO_API_VERSION", "SECTIGO_CREDENTIALS_PATH", "SECTIGO_TOKEN_CACHE_PATH"
    };

    private readonly Dictionary<string, string?> _original = new();

    internal string RootDirectory { get; } = Path.Combine(Path.GetTempPath(), "SectigoCertificateManager.Tests", Guid.NewGuid().ToString("N"));
    internal string TokenCachePath => Path.Combine(RootDirectory, "token.json");

    internal ApiConfigEnvironmentScope() {
        Directory.CreateDirectory(RootDirectory);
        foreach (var name in VariableNames) {
            _original.Add(name, Environment.GetEnvironmentVariable(name));
        }
        try {
            foreach (var name in VariableNames) {
                Environment.SetEnvironmentVariable(name, null);
            }
            Environment.SetEnvironmentVariable("SECTIGO_CREDENTIALS_PATH", Path.Combine(RootDirectory, "credentials.json"));
            Environment.SetEnvironmentVariable("SECTIGO_TOKEN_CACHE_PATH", TokenCachePath);
        } catch {
            Dispose();
            throw;
        }
    }

    public void Dispose() {
        try {
            foreach (var entry in _original) {
                Environment.SetEnvironmentVariable(entry.Key, entry.Value);
            }
        } finally {
            if (Directory.Exists(RootDirectory)) {
                Directory.Delete(RootDirectory, recursive: true);
            }
        }
    }
}
