using SectigoCertificateManager.Models;
using System.Text.Json;
using Xunit;

namespace SectigoCertificateManager.Tests;

public sealed class SerializationTests {
    [Fact]
    public void Deserialize_Certificate_Succeeds() {
        const string json = "{\"id\":1}";
        var obj = JsonSerializer.Deserialize<Certificate>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.NotNull(obj);
    }

    [Fact]
    public void Deserialize_Profile_Succeeds() {
        const string json = "{\"id\":1}";
        var obj = JsonSerializer.Deserialize<Profile>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.NotNull(obj);
    }

    [Fact]
    public void CertificateStatus_RoundTrip_Succeeds() {
        foreach (var status in Enum.GetValues<CertificateStatus>()) {
            var json = JsonSerializer.Serialize(status);
            var result = JsonSerializer.Deserialize<CertificateStatus>(json);
            Assert.Equal(status, result);
        }
    }

    [Fact]
    public void OrderStatus_RoundTrip_Succeeds() {
        foreach (var status in Enum.GetValues<OrderStatus>()) {
            var json = JsonSerializer.Serialize(status);
            var result = JsonSerializer.Deserialize<OrderStatus>(json);
            Assert.Equal(status, result);
        }
    }
}