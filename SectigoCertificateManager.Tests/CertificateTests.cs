using SectigoCertificateManager;
using SectigoCertificateManager.Models;
using System;
using System.Security.Cryptography.X509Certificates;
using Xunit;

namespace SectigoCertificateManager.Tests;

/// <summary>
/// Tests for the <see cref="Certificate"/> model.
/// </summary>
public sealed class CertificateTests {
    private const string Base64Cert = "MIIC/zCCAeegAwIBAgIULTQw6ATwfRI/1hVSQooJNHPEit8wDQYJKoZIhvcNAQELBQAwDzENMAsGA1UEAwwEdGVzdDAeFw0yNTA3MDQxMzE2NDRaFw0yNTA3MDUxMzE2NDRaMA8xDTALBgNVBAMMBHRlc3QwggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQDiO8kIwsJLCi3d8bX31IIISKSoA24iCcfV7m+uMm8CMdJlY2NGf8ThiF3suG2lHQCxESQacUrPFMN/J3cM7L+5R8p24CCnrmAP2WhMuO2IwFhgfjo4PsmnmCGNx5fDAPI+lnSS6pnHfZfAPw3dbPT2/cgbeil0q2ByFR6C2YXU+mFdOg7cJJ1f2GXbUL3QYRBuaDYCHRrDAym4e/8DkKjjaroDxw1BPD6sjvzrDdEDusJANDCp8K6Cr99nvG+YCLjueN+xvUXHbsp9gUfLI39X73p+M9zGcYGAeYyD/i+VM/+Kde5CEfS34eOKfRIJX6DHAbVu1SrJPNFFvQV0keb/AgMBAAGjUzBRMB0GA1UdDgQWBBQ8PwJEkQsHvU7i5i45XLLyJUi4eTAfBgNVHSMEGDAWgBQ8PwJEkQsHvU7i5i45XLLyJUi4eTAPBgNVHRMBAf8EBTADAQH/MA0GCSqGSIb3DQEBCwUAA4IBAQAjWADB2IC5xBHKOROcXZDa8mp3DaasUwL5mWjG7Ppr4LHrY1uCEojstJCg6s2FLBjGTs+0DTQ5UiBqSVJDK1GVhYG02xJSPoXNS4wNTp4a56NtbkDT96lO0BrH91lclMNXHU9NpMUFea0tt7h5tUeVtZ2CVK0nuy5MOifMdURVyhWsFgQVemmTNTYisVD5sNRvBJEq0M+3+JSjFYvRZVqfRSM3z1K4XcZJfhxv7Gq1ebb93R1QunIdGC0HiFnBZxpxhDCbcVOpbdbQOJ22dLSe5/4f+1V+D/bPCZJx5kF0yvM0jEhuQNxNV3H/DasvBhH/24JIjpe+WfKPw0jx7vR6";

    /// <summary>Creates certificate from base64 data.</summary>
    [Fact]
    public void FromBase64_CreatesCertificate() {
        using var result = Certificate.FromBase64(Base64Cert);

        Assert.Equal("51A908D14C9C984231B7E2F6C37ABB1368A57F1F", result.Thumbprint);
    }

    [Fact]
    public void FromBase64_WithInvalidData_Throws() {
        var ex = Assert.Throws<ValidationException>(() => Certificate.FromBase64("invalid"));
        Assert.Equal("Certificate data is not valid Base64.", ex.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void FromBase64_WithMissingData_Throws(string? input) {
        Assert.Throws<ArgumentException>(() => Certificate.FromBase64(input!));
    }

    [Fact]
    public void FromBase64_StreamOverload_CreatesCertificate() {
        var bytes = System.Text.Encoding.ASCII.GetBytes(Base64Cert);
        using var stream = new System.IO.MemoryStream(bytes);

        using var result = Certificate.FromBase64(stream);

        Assert.Equal("51A908D14C9C984231B7E2F6C37ABB1368A57F1F", result.Thumbprint);
    }

    [Fact]
    public void FromBase64_StreamOverload_ReportsProgress() {
        var bytes = System.Text.Encoding.ASCII.GetBytes(Base64Cert);
        using var stream = new System.IO.MemoryStream(bytes);
        var progress = new TestProgress();

        using var _ = Certificate.FromBase64(stream, progress);

        Assert.Equal(1d, progress.Value, 3);
    }

    [Fact]
    public void FromBase64_StreamPrePositioned_CreatesCertificate() {
        var bytes = System.Text.Encoding.ASCII.GetBytes(Base64Cert);
        using var stream = new System.IO.MemoryStream(bytes);
        stream.Position = 10;

        using var result = Certificate.FromBase64(stream);

        Assert.Equal("51A908D14C9C984231B7E2F6C37ABB1368A57F1F", result.Thumbprint);
    }

    private sealed class TestProgress : IProgress<double> {
        public double Value { get; private set; }
        public void Report(double value) => Value = value;
    }
}