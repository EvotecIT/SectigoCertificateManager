using System.Runtime.CompilerServices;

namespace SectigoCertificateManager.Tests;

internal static class TestEnvironment {
    [ModuleInitializer]
    internal static void Initialize() {
        // The test process must never inherit live authentication or fall back to user files.
        foreach (var name in ApiConfigEnvironmentScope.VariableNames) {
            Environment.SetEnvironmentVariable(name, null);
        }
        string tempRoot = Path.GetTempPath().TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        string path = tempRoot
            + Path.DirectorySeparatorChar + "SectigoCertificateManager.Tests"
            + Path.DirectorySeparatorChar + Guid.NewGuid().ToString("N")
            + Path.DirectorySeparatorChar + "token.json";
        Environment.SetEnvironmentVariable("SECTIGO_TOKEN_CACHE_PATH", path);
        Environment.SetEnvironmentVariable("SECTIGO_CREDENTIALS_PATH", Path.Combine(Path.GetDirectoryName(path)!, "credentials.json"));
        AppDomain.CurrentDomain.ProcessExit += (_, _) => {
            var directory = Path.GetDirectoryName(path)!;
            if (Directory.Exists(directory)) {
                Directory.Delete(directory, recursive: true);
            }
        };
    }
}
