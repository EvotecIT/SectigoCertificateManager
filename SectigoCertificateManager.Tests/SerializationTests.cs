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
}