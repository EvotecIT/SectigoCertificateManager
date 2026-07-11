using System.Runtime.CompilerServices;

namespace SectigoCertificateManager.Tests;

internal static class TestEnvironment {
    [ModuleInitializer]
    internal static void Initialize() {
        string tempRoot = Path.GetTempPath().TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        string path = tempRoot
            + Path.DirectorySeparatorChar + "SectigoCertificateManager.Tests"
            + Path.DirectorySeparatorChar + Guid.NewGuid().ToString("N")
            + Path.DirectorySeparatorChar + "token.json";
        Environment.SetEnvironmentVariable("SECTIGO_TOKEN_CACHE_PATH", path);
    }
}
