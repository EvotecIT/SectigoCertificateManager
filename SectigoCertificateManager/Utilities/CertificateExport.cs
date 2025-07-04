namespace SectigoCertificateManager.Utilities;

using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

/// <summary>
/// Provides utility methods for saving certificates in various formats.
/// </summary>
public static class CertificateExport {
    /// <summary>Saves the certificate as a PEM encoded file.</summary>
    /// <param name="certificate">Certificate to export.</param>
    /// <param name="path">Destination file path.</param>
    public static void SavePem(X509Certificate2 certificate, string path) {
        var builder = new StringBuilder();
        builder.AppendLine("-----BEGIN CERTIFICATE-----");
        builder.AppendLine(Convert.ToBase64String(certificate.Export(X509ContentType.Cert), Base64FormattingOptions.InsertLineBreaks));
        builder.AppendLine("-----END CERTIFICATE-----");
        File.WriteAllText(path, builder.ToString());
    }

    /// <summary>Saves the certificate in DER format.</summary>
    /// <param name="certificate">Certificate to export.</param>
    /// <param name="path">Destination file path.</param>
    public static void SaveDer(X509Certificate2 certificate, string path) {
        File.WriteAllBytes(path, certificate.Export(X509ContentType.Cert));
    }

    /// <summary>Saves the certificate and private key in a PFX container.</summary>
    /// <param name="certificate">Certificate to export.</param>
    /// <param name="path">Destination file path.</param>
    /// <param name="password">Optional password protecting the PFX.</param>
    public static void SavePfx(X509Certificate2 certificate, string path, string? password = null) {
        var bytes = password is null
            ? certificate.Export(X509ContentType.Pfx)
            : certificate.Export(X509ContentType.Pfx, password);
        File.WriteAllBytes(path, bytes);
#if NET6_0_OR_GREATER
        CryptographicOperations.ZeroMemory(bytes);
#else
        Array.Clear(bytes, 0, bytes.Length);
#endif
    }
}
