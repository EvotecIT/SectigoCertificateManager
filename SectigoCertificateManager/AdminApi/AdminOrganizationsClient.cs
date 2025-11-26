namespace SectigoCertificateManager.AdminApi;

using SectigoCertificateManager.Utilities;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Minimal client for the Sectigo Admin Operations API organization endpoints.
/// </summary>
public sealed class AdminOrganizationsClient : IDisposable {
    private readonly AdminApiConfig _config;
    private readonly HttpClient _httpClient;
    private readonly bool _ownsHttpClient;
    private string _cachedToken = string.Empty;
    private DateTimeOffset _tokenExpiresAt;
    private static readonly JsonSerializerOptions s_json = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminOrganizationsClient"/> class.
    /// </summary>
    /// <param name="config">Admin API configuration.</param>
    /// <param name="httpClient">
    /// Optional <see cref="HttpClient"/> instance. When not provided, a new instance is created
    /// and disposed with this client.
    /// </param>
    public AdminOrganizationsClient(AdminApiConfig config, HttpClient? httpClient = null) {
        _config = Guard.AgainstNull(config, nameof(config));
        if (httpClient is null) {
            _httpClient = new HttpClient();
            _ownsHttpClient = true;
        } else {
            _httpClient = httpClient;
            _ownsHttpClient = false;
        }

        if (!_config.BaseUrl.EndsWith("/", StringComparison.Ordinal)) {
            _httpClient.BaseAddress = new Uri(_config.BaseUrl + "/");
        } else {
            _httpClient.BaseAddress = new Uri(_config.BaseUrl);
        }
    }

    /// <summary>
    /// Lists organizations available to the current administrator.
    /// </summary>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<IReadOnlyList<AdminOrganizationBasic>> ListAsync(
        CancellationToken cancellationToken = default) {
        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        using var request = new HttpRequestMessage(HttpMethod.Get, "api/organization/v1");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var items = await response.Content
            .ReadFromJsonAsyncSafe<IReadOnlyList<AdminOrganizationBasic>>(s_json, cancellationToken)
            .ConfigureAwait(false);

        return items ?? Array.Empty<AdminOrganizationBasic>();
    }

    /// <summary>
    /// Retrieves detailed information for the specified organization.
    /// </summary>
    /// <param name="organizationId">Organization identifier.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<AdminOrganizationDetails?> GetAsync(
        int organizationId,
        CancellationToken cancellationToken = default) {
        if (organizationId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(organizationId));
        }

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/organization/v1/{organizationId}");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var details = await response.Content
            .ReadFromJsonAsyncSafe<AdminOrganizationDetails>(s_json, cancellationToken)
            .ConfigureAwait(false);

        return details;
    }

    /// <summary>
    /// Lists organizations by certificate type report.
    /// </summary>
    /// <param name="reportType">Certificate type (for example, SSL, Client, Device, CodeSign).</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<IReadOnlyList<AdminOrganizationBasic>> ListByReportTypeAsync(
        string reportType,
        CancellationToken cancellationToken = default) {
        Guard.AgainstNullOrWhiteSpace(reportType, nameof(reportType));

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"api/organization/v1/report-type/{Uri.EscapeDataString(reportType)}");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var items = await response.Content
            .ReadFromJsonAsyncSafe<IReadOnlyList<AdminOrganizationBasic>>(s_json, cancellationToken)
            .ConfigureAwait(false);

        return items ?? Array.Empty<AdminOrganizationBasic>();
    }

    /// <summary>
    /// Lists organizations delegated to the specified role.
    /// </summary>
    /// <param name="role">
    /// Client admin role allowed to manage requested organizations (for example, MRAO, RAO_SSL).
    /// </param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<IReadOnlyList<AdminOrganizationBasic>> ListManagedByRoleAsync(
        string role,
        CancellationToken cancellationToken = default) {
        Guard.AgainstNullOrWhiteSpace(role, nameof(role));

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"api/organization/v1/managed-by/{Uri.EscapeDataString(role)}");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var items = await response.Content
            .ReadFromJsonAsyncSafe<IReadOnlyList<AdminOrganizationBasic>>(s_json, cancellationToken)
            .ConfigureAwait(false);

        return items ?? Array.Empty<AdminOrganizationBasic>();
    }

    /// <inheritdoc />
    public void Dispose() {
        if (_ownsHttpClient) {
            _httpClient.Dispose();
        }
    }

    private async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken) {
        if (!string.IsNullOrEmpty(_cachedToken) && DateTimeOffset.UtcNow < _tokenExpiresAt) {
            return _cachedToken;
        }

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

        _cachedToken = model.AccessToken;
        var lifetimeSeconds = model.ExpiresIn > 0 ? model.ExpiresIn : 300;
        var expiry = DateTimeOffset.UtcNow.AddSeconds(lifetimeSeconds);
        _tokenExpiresAt = expiry.AddMinutes(-1);

        return _cachedToken;
    }

    private sealed class TokenResponse {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
    }
}

