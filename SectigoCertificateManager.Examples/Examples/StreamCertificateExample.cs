using SectigoCertificateManager.Models;
using System.IO;
using System.Text;

namespace SectigoCertificateManager.Examples.Examples;

/// <summary>
/// Demonstrates loading a certificate from a stream with progress reporting.
/// </summary>
public static class StreamCertificateExample {
    public static void Run() {
        const string base64 = "<base64 certificate>";
        using var stream = new MemoryStream(Encoding.ASCII.GetBytes(base64));
        var progress = new Progress<double>(p => Console.WriteLine($"Read {p:P0}"));
        using var cert = Certificate.FromBase64(stream, progress);
        Console.WriteLine($"Thumbprint: {cert.Thumbprint}");
    }
}
