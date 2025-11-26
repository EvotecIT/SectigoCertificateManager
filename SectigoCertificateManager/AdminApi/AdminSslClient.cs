namespace SectigoCertificateManager.AdminApi;

using SectigoCertificateManager.Utilities;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
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
        public string AccessToken { get; set; } = string.Empty;
    }
}
