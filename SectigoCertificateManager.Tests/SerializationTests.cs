using SectigoCertificateManager.Models;
using System.Text.Json;
using Xunit;

namespace SectigoCertificateManager.Tests;

/// <summary>
/// Tests for JSON serialization helpers.
/// </summary>
public sealed class SerializationTests {
    /// <summary>Ensures certificate JSON deserializes.</summary>
    [Fact]
    public void Deserialize_Certificate_Succeeds() {
        const string json = "{\"id\":1}";
        var options = new JsonSerializerOptions {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        var obj = JsonSerializer.Deserialize<Certificate>(json, options);
        Assert.NotNull(obj);
    }

    [Fact]
    public void Deserialize_Profile_Succeeds() {
        const string json = "{\"id\":1}";
        var options = new JsonSerializerOptions {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        var obj = JsonSerializer.Deserialize<Profile>(json, options);
        Assert.NotNull(obj);
    }

    [Fact]
    public void CertificateStatus_RoundTrip_Succeeds() {
        foreach (CertificateStatus status in Enum.GetValues(typeof(CertificateStatus))) {
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var json = JsonSerializer.Serialize(status, options);
            var result = JsonSerializer.Deserialize<CertificateStatus>(json, options);
            Assert.Equal(status, result);
        }
    }

    [Fact]
    public void OrderStatus_RoundTrip_Succeeds() {
        foreach (OrderStatus status in Enum.GetValues(typeof(OrderStatus))) {
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var json = JsonSerializer.Serialize(status, options);
            var result = JsonSerializer.Deserialize<OrderStatus>(json, options);
            Assert.Equal(status, result);
        }
    }
}