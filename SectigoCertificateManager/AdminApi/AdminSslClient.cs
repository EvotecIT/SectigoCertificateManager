namespace SectigoCertificateManager.AdminApi;

using SectigoCertificateManager.Requests;
using SectigoCertificateManager.Responses;
using SectigoCertificateManager.Utilities;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Minimal client for the Sectigo Admin Operations API SSL endpoints.
/// </summary>
public sealed class AdminSslClient {
    private readonly AdminApiConfig _config;
    private readonly HttpClient _httpClient;
    private static readonly JsonSerializerOptions s_json = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminSslClient"/> class.
    /// </summary>
    /// <param name="config">Admin API configuration.</param>
    /// <param name="httpClient">
    /// Optional <see cref="HttpClient"/> instance. When not provided, a new instance is created.
    /// </param>
    public AdminSslClient(AdminApiConfig config, HttpClient? httpClient = null) {
        _config = Guard.AgainstNull(config, nameof(config));
        _httpClient = httpClient ?? new HttpClient();
        if (!_config.BaseUrl.EndsWith("/", StringComparison.Ordinal)) {
            _httpClient.BaseAddress = new Uri(_config.BaseUrl + "/");
        } else {
            _httpClient.BaseAddress = new Uri(_config.BaseUrl);
        }
    }

    /// <summary>
    /// Lists SSL certificates using the Admin API.
    /// </summary>
    /// <param name="size">Number of entries to request.</param>
    /// <param name="position">The first position to return from the result set.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<IReadOnlyList<AdminSslIdentity>> ListAsync(
        int? size = null,
        int? position = null,
        CancellationToken cancellationToken = default) {
        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        using var request = new HttpRequestMessage(HttpMethod.Get, BuildListUri(size, position));
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var identities = await response.Content
            .ReadFromJsonAsyncSafe<IReadOnlyList<AdminSslIdentity>>(s_json, cancellationToken)
            .ConfigureAwait(false);

        return identities ?? Array.Empty<AdminSslIdentity>();
    }

    /// <summary>
    /// Retrieves detailed SSL certificate information by identifier.
    /// </summary>
    /// <param name="sslId">Certificate identifier.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<AdminSslCertificateDetails?> GetAsync(
        int sslId,
        CancellationToken cancellationToken = default) {
        if (sslId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(sslId));
        }

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/ssl/v2/{sslId}");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var details = await response.Content
            .ReadFromJsonAsyncSafe<AdminSslCertificateDetails>(s_json, cancellationToken)
            .ConfigureAwait(false);

        return details;
    }

    /// <summary>
    /// Revokes an SSL certificate by identifier.
    /// </summary>
    /// <param name="sslId">Certificate identifier.</param>
    /// <param name="reasonCode">
    /// Optional revocation reason code string as defined by the Admin API
    /// (for example, "0", "1", "3", "4", "5").
    /// When <c>null</c>, "0" (unspecified) is used.
    /// </param>
    /// <param name="reason">Optional human-readable revocation reason.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task RevokeByIdAsync(
        int sslId,
        string? reasonCode = null,
        string? reason = null,
        CancellationToken cancellationToken = default) {
        if (sslId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(sslId));
        }

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);
        var payload = new RevokeRequest {
            ReasonCode = string.IsNullOrWhiteSpace(reasonCode) ? "0" : reasonCode,
            Reason = reason
        };

        using var message = new HttpRequestMessage(HttpMethod.Post, $"api/ssl/v2/revoke/{sslId}") {
            Content = JsonContent.Create(payload, options: s_json)
        };
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Revokes an SSL certificate by serial number.
    /// </summary>
    /// <param name="serialNumber">Certificate serial number.</param>
    /// <param name="reasonCode">
    /// Optional revocation reason code string as defined by the Admin API
    /// (for example, "0", "1", "3", "4", "5").
    /// When <c>null</c>, "0" (unspecified) is used.
    /// </param>
    /// <param name="reason">Optional human-readable revocation reason.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task RevokeBySerialAsync(
        string serialNumber,
        string? reasonCode = null,
        string? reason = null,
        CancellationToken cancellationToken = default) {
        if (string.IsNullOrWhiteSpace(serialNumber)) {
            throw new ArgumentException("Serial number cannot be null or empty.", nameof(serialNumber));
        }

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);
        var payload = new RevokeRequest {
            ReasonCode = string.IsNullOrWhiteSpace(reasonCode) ? "0" : reasonCode,
            Reason = reason
        };

        var encodedSerial = Uri.EscapeDataString(serialNumber);
        using var message = new HttpRequestMessage(HttpMethod.Post, $"api/ssl/v2/revoke/serial/{encodedSerial}") {
            Content = JsonContent.Create(payload, options: s_json)
        };
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Marks an SSL certificate as revoked in SCM without contacting the CA.
    /// </summary>
    /// <param name="certId">Optional certificate identifier.</param>
    /// <param name="serialNumber">Optional certificate serial number.</param>
    /// <param name="issuer">Optional certificate issuer used together with <paramref name="serialNumber"/>.</param>
    /// <param name="revokeDate">Optional revocation date.</param>
    /// <param name="reasonCode">
    /// Optional revocation reason code string as defined by the Admin API
    /// (for example, "0", "1", "3", "4", "5").
    /// When <c>null</c>, "0" (unspecified) is used.
    /// </param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task MarkAsRevokedAsync(
        int? certId = null,
        string? serialNumber = null,
        string? issuer = null,
        DateTimeOffset? revokeDate = null,
        string? reasonCode = null,
        CancellationToken cancellationToken = default) {
        if (!certId.HasValue && string.IsNullOrWhiteSpace(serialNumber)) {
            throw new ArgumentException("Either certId or serialNumber must be provided.");
        }

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        var payload = new MarkAsRevokedRequest {
            CertId = certId,
            SerialNumber = serialNumber,
            Issuer = issuer,
            RevokeDate = revokeDate,
            ReasonCode = string.IsNullOrWhiteSpace(reasonCode) ? "0" : reasonCode
        };

        using var message = new HttpRequestMessage(HttpMethod.Post, "api/ssl/v2/revoke/manual") {
            Content = JsonContent.Create(payload, options: s_json)
        };
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Downloads an issued certificate as a raw byte stream.
    /// </summary>
    /// <param name="sslId">Certificate identifier.</param>
    /// <param name="format">
    /// Optional format type. When <c>null</c>, the API default (<c>base64</c>) is used.
    /// Supported values are documented in the Admin API (for example, <c>base64</c>, <c>x509</c>, <c>pem</c>).
    /// </param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<Stream> CollectAsync(
        int sslId,
        string? format = null,
        CancellationToken cancellationToken = default) {
        if (sslId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(sslId));
        }

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        var path = new StringBuilder($"api/ssl/v2/collect/{sslId}");
        if (!string.IsNullOrEmpty(format)) {
            path.Append("?format=").Append(Uri.EscapeDataString(format));
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, path.ToString());
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

#if NETSTANDARD2_0 || NET472
        return await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
#else
        return await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
#endif
    }

    /// <summary>
    /// Renews an SSL certificate by identifier.
    /// </summary>
    /// <param name="sslId">Certificate identifier.</param>
    /// <param name="request">Renewal request payload.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<int> RenewByIdAsync(
        int sslId,
        RenewCertificateRequest request,
        CancellationToken cancellationToken = default) {
        if (sslId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(sslId));
        }

        Guard.AgainstNull(request, nameof(request));

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        var body = new RenewInfo {
            Csr = request.Csr,
            DcvMode = request.DcvMode,
            DcvEmail = request.DcvEmail
        };

        using var message = new HttpRequestMessage(HttpMethod.Post, $"api/ssl/v2/renewById/{sslId}") {
            Content = JsonContent.Create(body, options: s_json)
        };
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var result = await response.Content
            .ReadFromJsonAsyncSafe<RenewCertificateResponse>(s_json, cancellationToken)
            .ConfigureAwait(false);
        return result?.SslId ?? 0;
    }

    /// <summary>
    /// Creates a keystore download link for the specified certificate.
    /// </summary>
    /// <param name="sslId">Certificate identifier.</param>
    /// <param name="formatType">
    /// Keystore format type as defined by the Admin API (for example, <c>key</c>, <c>p12</c>, <c>p12aes</c>, <c>jks</c>, <c>pem</c>).
    /// </param>
    /// <param name="passphrase">Optional passphrase used to protect the keystore.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<string> CreateKeystoreLinkAsync(
        int sslId,
        string formatType,
        string? passphrase = null,
        CancellationToken cancellationToken = default) {
        if (sslId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(sslId));
        }

        if (string.IsNullOrWhiteSpace(formatType)) {
            throw new ArgumentException("Format type cannot be null or empty.", nameof(formatType));
        }

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        var payload = new DownloadLinkRequest {
            Passphrase = passphrase
        };

        var path = $"api/ssl/v2/keystore/{sslId}/{Uri.EscapeDataString(formatType)}";
        using var message = new HttpRequestMessage(HttpMethod.Post, path) {
            Content = JsonContent.Create(payload, options: s_json)
        };
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var result = await response.Content
            .ReadFromJsonAsyncSafe<DownloadFromPkResponse>(s_json, cancellationToken)
            .ConfigureAwait(false);

        return result?.Link ?? string.Empty;
    }

    private string BuildListUri(int? size, int? position) {
        var builder = new StringBuilder("api/ssl/v2");
        var hasQuery = false;

        void AppendInt(string name, int value) {
            _ = hasQuery ? builder.Append('&') : builder.Append('?');
            builder.Append(name).Append('=').Append(value);
            hasQuery = true;
        }

        if (size is { } s) {
            AppendInt("size", s);
        }

        if (position is { } p) {
            AppendInt("position", p);
        }

        return builder.ToString();
    }

    private async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken) {
        using var content = new FormUrlEncodedContent(new Dictionary<string, string> {
            ["grant_type"] = "client_credentials",
            ["client_id"] = _config.ClientId,
            ["client_secret"] = _config.ClientSecret
        });

        using var response = await _httpClient
            .PostAsync(_config.TokenUrl, content, cancellationToken)
            .ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var model = await response.Content
            .ReadFromJsonAsyncSafe<TokenResponse>(s_json, cancellationToken)
            .ConfigureAwait(false);
        if (model is null || string.IsNullOrWhiteSpace(model.AccessToken)) {
            throw new InvalidOperationException("Access token was not present in the Admin API token response.");
        }

        return model.AccessToken;
    }

    private sealed class TokenResponse {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;
    }

    private sealed class RenewInfo {
        [JsonPropertyName("csr")]
        public string? Csr { get; set; }

        [JsonPropertyName("dcvMode")]
        public string? DcvMode { get; set; }

        [JsonPropertyName("dcvEmail")]
        public string? DcvEmail { get; set; }
    }

    private sealed class RevokeRequest {
        [JsonPropertyName("reasonCode")]
        public string? ReasonCode { get; set; }

        [JsonPropertyName("reason")]
        public string? Reason { get; set; }
    }

    private sealed class MarkAsRevokedRequest {
        [JsonPropertyName("certId")]
        public int? CertId { get; set; }

        [JsonPropertyName("serialNumber")]
        public string? SerialNumber { get; set; }

        [JsonPropertyName("issuer")]
        public string? Issuer { get; set; }

        [JsonPropertyName("revokeDate")]
        public DateTimeOffset? RevokeDate { get; set; }

        [JsonPropertyName("reasonCode")]
        public string? ReasonCode { get; set; }
    }

    private sealed class DownloadLinkRequest {
        [JsonPropertyName("passphrase")]
        public string? Passphrase { get; set; }
    }

    private sealed class DownloadFromPkResponse {
        [JsonPropertyName("link")]
        public string? Link { get; set; }
    }
}
