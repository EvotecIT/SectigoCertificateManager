using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using SectigoCertificateManager.Models;
using System.IO;
using System.Text;

namespace SectigoCertificateManager.Benchmarks;

[MemoryDiagnoser]
public class CertificateFromBase64Benchmark {
    private byte[] _data = Encoding.ASCII.GetBytes(Base64Cert);

    private const string Base64Cert = "MIIC/zCCAeegAwIBAgIULTQw6ATwfRI/1hVSQooJNHPEit8wDQYJKoZIhvcNAQELBQAwDzENMAsGA1UEAwwEdGVzdDAeFw0yNTA3MDQxMzE2NDRaFw0yNTA3MDUxMzE2NDRaMA8xDTALBgNVBAMMBHRlc3QwggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQDiO8kIwsJLCi3d8bX31IIISKSoA24iCcfV7m+uMm8CMdJlY2NGf8ThiF3suG2lHQCxESQacUrPFMN/J3cM7L+5R8p24CCnrmAP2WhMuO2IwFhgfjo4PsmnmCGNx5fDAPI+lnSS6pnHfZfAPw3dbPT2/cgbeil0q2ByFR6C2YXU+mFdOg7cJJ1f2GXbUL3QYRBuaDYCHRrDAym4e/8DkKjjaroDxw1BPD6sjvzrDdEDusJANDCp8K6Cr99nvG+YCLjueN+xvUXHbsp9gUfLI39X73p+M9zGcYGAeYyD/i+VM/+Kde5CEfS34eOKfRIJX6DHAbVu1SrJPNFFvQV0keb/AgMBAAGjUzBRMB0GA1UdDgQWBBQ8PwJEkQsHvU7i5i45XLLyJUi4eTAfBgNVHSMEGDAWgBQ8PwJEkQsHvU7i5i45XLLyJUi4eTAPBgNVHRMBAf8EBTADAQH/MA0GCSqGSIb3DQEBCwUAA4IBAQAjWADB2IC5xBHKOROcXZDa8mp3DaasUwL5mWjG7Ppr4LHrY1uCEojstJCg6s2FLBjGTs+0DTQ5UiBqSVJDK1GVhYG02xJSPoXNS4wNTp4a56NtbkDT96lO0BrH91lclMNXHU9NpMUFea0tt7h5tUeVtZ2CVK0nuy5MOifMdURVyhWsFgQVemmTNTYisVD5sNRvBJEq0M+3+JSjFYvRZVqfRSM3z1K4XcZJfhxv7Gq1ebb93R1QunIdGC0HiFnBZxpxhDCbcVOpbdbQOJ22dLSe5/4f+1V+D/bPCZJx5kF0yvM0jEhuQNxNV3H/DasvBhH/24JIjpe+WfKPw0jx7vR6";

    [Benchmark]
    public void FromStream() {
        using var stream = new MemoryStream(_data, writable: false);
        using var _ = Certificate.FromBase64(stream);
    }
}

public class Program {
    public static void Main(string[] args) {
        BenchmarkRunner.Run<CertificateFromBase64Benchmark>();
    }
}
