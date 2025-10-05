using SectigoCertificateManager;
using SectigoCertificateManager.Clients;
using SectigoCertificateManager.Models;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
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
        var obj = JsonSerializer.Deserialize<Certificate>(json, TestClient.JsonOptions);
        Assert.NotNull(obj);
    }

    [Fact]
    public void Deserialize_Profile_Succeeds() {
        const string json = "{\"id\":1}";
        var obj = JsonSerializer.Deserialize<Profile>(json, TestClient.JsonOptions);
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

    [Fact]
    public void BaseClient_JsonOptions_HandleCamelCasePayload() {
        const string json = "{\"id\":2}";
        var result = JsonSerializer.Deserialize<Certificate>(json, TestClient.JsonOptions);

        Assert.NotNull(result);
        Assert.True(TestClient.JsonOptions.PropertyNameCaseInsensitive);
    }

    private sealed class TestClient : BaseClient {
        private TestClient()
            : base(StubSectigoClient.Instance) {
        }

        public static JsonSerializerOptions JsonOptions => s_json;
    }

    private sealed class StubSectigoClient : ISectigoClient {
        private static readonly HttpClient s_httpClient = new();

        private StubSectigoClient() {
        }

        public static StubSectigoClient Instance { get; } = new();

        public HttpClient HttpClient => s_httpClient;

        public Task<HttpResponseMessage> GetAsync(string requestUri, CancellationToken cancellationToken = default) {
            throw new NotSupportedException();
        }

        public Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content, CancellationToken cancellationToken = default) {
            throw new NotSupportedException();
        }

        public Task<HttpResponseMessage> PutAsync(string requestUri, HttpContent content, CancellationToken cancellationToken = default) {
            throw new NotSupportedException();
        }

        public Task<HttpResponseMessage> DeleteAsync(string requestUri, CancellationToken cancellationToken = default) {
            throw new NotSupportedException();
        }
    }
}
