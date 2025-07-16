namespace SectigoCertificateManager.Utilities;

using System.Collections.Generic;
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
        if (string.IsNullOrEmpty(path)) {
            throw new ArgumentException("Path cannot be null or empty.", nameof(path));
        }
        var builder = new StringBuilder();
        builder.AppendLine("-----BEGIN CERTIFICATE-----");
        builder.AppendLine(Convert.ToBase64String(certificate.Export(X509ContentType.Cert), Base64FormattingOptions.InsertLineBreaks));
        builder.AppendLine("-----END CERTIFICATE-----");
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, builder.ToString(), Encoding.UTF8);
    }

    /// <summary>Saves the certificate in DER format.</summary>
    /// <param name="certificate">Certificate to export.</param>
    /// <param name="path">Destination file path.</param>
    public static void SaveDer(X509Certificate2 certificate, string path) {
        if (string.IsNullOrEmpty(path)) {
            throw new ArgumentException("Path cannot be null or empty.", nameof(path));
        }
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllBytes(path, certificate.Export(X509ContentType.Cert));
    }

    /// <summary>Saves the certificate and private key in a PFX container.</summary>
    /// <param name="certificate">Certificate to export.</param>
    /// <param name="path">Destination file path.</param>
    /// <param name="password">Optional password protecting the PFX.</param>
    public static void SavePfx(X509Certificate2 certificate, string path, string? password = null) {
        if (string.IsNullOrEmpty(path)) {
            throw new ArgumentException("Path cannot be null or empty.", nameof(path));
        }
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var bytes = password is null
            ? certificate.Export(X509ContentType.Pfx)
            : certificate.Export(X509ContentType.Pfx, password);
        using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None)) {
            stream.Write(bytes, 0, bytes.Length);
        }
#if NET6_0_OR_GREATER
        CryptographicOperations.ZeroMemory(bytes);
#else
        Array.Clear(bytes, 0, bytes.Length);
#endif
    }

    /// <summary>
    /// Saves the certificate and private key in a PFX container and returns the cleared buffer for testing.
    /// </summary>
    /// <param name="certificate">Certificate to export.</param>
    /// <param name="path">Destination file path.</param>
    /// <param name="password">Optional password protecting the PFX.</param>
    /// <returns>The cleared buffer used to write the PFX.</returns>
    internal static byte[] SavePfxForTest(X509Certificate2 certificate, string path, string? password = null) {
        var bytes = password is null
            ? certificate.Export(X509ContentType.Pfx)
            : certificate.Export(X509ContentType.Pfx, password);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None)) {
            stream.Write(bytes, 0, bytes.Length);
        }
#if NET6_0_OR_GREATER
        CryptographicOperations.ZeroMemory(bytes);
#else
        Array.Clear(bytes, 0, bytes.Length);
#endif
        return bytes;
    }

    /// <summary>Saves the certificate chain as a PEM encoded file.</summary>
    /// <param name="certificate">Leaf certificate to export.</param>
    /// <param name="path">Destination file path.</param>
    /// <param name="extraCertificates">Certificates used to build the chain.</param>
    public static void SavePemChain(
        X509Certificate2 certificate,
        string path,
        IEnumerable<X509Certificate2>? extraCertificates = null) {
        using var chain = new X509Chain();
        chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
        chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority;

        if (extraCertificates is not null) {
            foreach (var cert in extraCertificates) {
                chain.ChainPolicy.ExtraStore.Add(cert);
            }
        }

        chain.Build(certificate);

        var builder = new StringBuilder();
        foreach (var element in chain.ChainElements) {
            builder.AppendLine("-----BEGIN CERTIFICATE-----");
            builder.AppendLine(Convert.ToBase64String(element.Certificate.Export(X509ContentType.Cert), Base64FormattingOptions.InsertLineBreaks));
            builder.AppendLine("-----END CERTIFICATE-----");
        }

        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, builder.ToString(), Encoding.UTF8);
    }
}