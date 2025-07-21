using SectigoCertificateManager.Utilities;
using System;

namespace SectigoCertificateManager.Examples.Examples;

/// <summary>
/// Demonstrates basic usage of <see cref="Guard"/> helpers.
/// </summary>
public static class GuardExample {
    /// <summary>Executes the example.</summary>
    public static void Run() {
        try {
            Guard.AgainstNull<string>(null, "value");
        } catch (ArgumentNullException ex) {
            Console.WriteLine($"Caught {ex.GetType().Name} for {ex.ParamName}");
        }
    }
}
