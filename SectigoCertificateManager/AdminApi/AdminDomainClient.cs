namespace SectigoCertificateManager.AdminApi;

using SectigoCertificateManager.Requests;
using SectigoCertificateManager.Utilities;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Minimal client for the Sectigo Admin Operations API domain endpoints.
/// </summary>
public sealed class AdminDomainClient : IDisposable {
    private readonly AdminApiConfig _config;
    private readonly HttpClient _httpClient;
    private readonly bool _ownsHttpClient;
    private string _cachedToken = string.Empty;
    private DateTimeOffset _tokenExpiresAt;
    private static readonly JsonSerializerOptions s_json = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminDomainClient"/> class.
    /// </summary>
    /// <param name="config">Admin API configuration.</param>
    /// <param name="httpClient">
    /// Optional <see cref="HttpClient"/> instance. When not provided, a new instance is created
    /// and disposed with this client.
    /// </param>
    public AdminDomainClient(AdminApiConfig config, HttpClient? httpClient = null) {
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
    /// Lists domains according to the specified filter and pagination.
    /// </summary>
    /// <param name="size">Optional page size.</param>
    /// <param name="position">Optional position offset.</param>
    /// <param name="name">Optional domain name filter.</param>
    /// <param name="state">Optional state filter ("active" or "inactive").</param>
    /// <param name="status">Optional requested status filter ("requested" or "approved").</param>
    /// <param name="orgId">Optional organization identifier filter.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<IReadOnlyList<AdminDomainInfo>> ListAsync(
        int? size = null,
        int? position = null,
        string? name = null,
        string? state = null,
        string? status = null,
        int? orgId = null,
        CancellationToken cancellationToken = default) {
        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        var builder = new StringBuilder("api/domain/v1");
        var hasQuery = false;

        void AppendInt(string key, int? value) {
            if (!value.HasValue) {
                return;
            }

            _ = hasQuery ? builder.Append('&') : builder.Append('?');
            builder.Append(key).Append('=').Append(value.Value);
            hasQuery = true;
        }

        void AppendString(string key, string? value) {
            if (string.IsNullOrWhiteSpace(value)) {
                return;
            }

            _ = hasQuery ? builder.Append('&') : builder.Append('?');
            builder.Append(key).Append('=').Append(Uri.EscapeDataString(value));
            hasQuery = true;
        }

        AppendInt("size", size);
        AppendInt("position", position);
        AppendString("name", name);
        AppendString("state", state);
        AppendString("status", status);
        AppendInt("orgId", orgId);

        using var request = new HttpRequestMessage(HttpMethod.Get, builder.ToString());
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var items = await response.Content
            .ReadFromJsonAsyncSafe<IReadOnlyList<AdminDomainInfo>>(s_json, cancellationToken)
            .ConfigureAwait(false);

        return items ?? Array.Empty<AdminDomainInfo>();
    }

    /// <summary>
    /// Creates a new domain.
    /// </summary>
    /// <param name="request">Domain creation request.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The identifier of the created domain, or 0 when not available.</returns>
    public async Task<int> CreateAsync(
        CreateDomainRequest request,
        CancellationToken cancellationToken = default) {
        Guard.AgainstNull(request, nameof(request));
        Guard.AgainstNullOrWhiteSpace(request.Name, nameof(request.Name));

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        using var message = new HttpRequestMessage(HttpMethod.Post, "api/domain/v1") {
            Content = JsonContent.Create(request, options: s_json)
        };
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var location = response.Headers.Location;
        if (location is not null) {
            var url = location.ToString().Trim().TrimEnd('/');
            var segments = url.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length > 0 && int.TryParse(segments[segments.Length - 1], out var id)) {
                return id;
            }
        }

        return 0;
    }

    /// <summary>
    /// Delegates a single domain to an organization or department.
    /// </summary>
    /// <param name="domainId">Domain identifier.</param>
    /// <param name="delegation">Delegation payload.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task DelegateAsync(
        int domainId,
        AdminDomainDelegation delegation,
        CancellationToken cancellationToken = default) {
        if (domainId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(domainId));
        }

        Guard.AgainstNull(delegation, nameof(delegation));

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        using var message = new HttpRequestMessage(HttpMethod.Post, $"api/domain/v1/{domainId}/delegation") {
            Content = JsonContent.Create(delegation, options: s_json)
        };
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Removes a domain delegation.
    /// </summary>
    /// <param name="domainId">Domain identifier.</param>
    /// <param name="delegation">Delegation payload.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task RemoveDelegationAsync(
        int domainId,
        AdminDomainDelegation delegation,
        CancellationToken cancellationToken = default) {
        if (domainId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(domainId));
        }

        Guard.AgainstNull(delegation, nameof(delegation));

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        using var message = new HttpRequestMessage(HttpMethod.Delete, $"api/domain/v1/{domainId}/delegation") {
            Content = JsonContent.Create(delegation, options: s_json)
        };
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Suspends a domain.
    /// </summary>
    /// <param name="domainId">Domain identifier.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task SuspendAsync(
        int domainId,
        CancellationToken cancellationToken = default) {
        if (domainId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(domainId));
        }

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        using var request = new HttpRequestMessage(HttpMethod.Put, $"api/domain/v1/{domainId}/suspend");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Activates a domain.
    /// </summary>
    /// <param name="domainId">Domain identifier.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task ActivateAsync(
        int domainId,
        CancellationToken cancellationToken = default) {
        if (domainId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(domainId));
        }

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        using var request = new HttpRequestMessage(HttpMethod.Put, $"api/domain/v1/{domainId}/activate");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Updates CT log monitoring settings for a domain.
    /// </summary>
    /// <param name="domainId">Domain identifier.</param>
    /// <param name="settings">Monitoring settings payload.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task UpdateMonitoringAsync(
        int domainId,
        AdminCtLogMonitoring settings,
        CancellationToken cancellationToken = default) {
        if (domainId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(domainId));
        }

        Guard.AgainstNull(settings, nameof(settings));

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        using var message = new HttpRequestMessage(HttpMethod.Put, $"api/domain/v1/{domainId}/monitoring") {
            Content = JsonContent.Create(settings, options: s_json)
        };
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Approves a domain delegation request.
    /// </summary>
    /// <param name="domainId">Domain identifier.</param>
    /// <param name="orgId">Organization or department identifier.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task ApproveDelegationAsync(
        int domainId,
        int orgId,
        CancellationToken cancellationToken = default) {
        if (domainId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(domainId));
        }

        if (orgId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(orgId));
        }

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        var body = new DomainApproveRequest {
            OrgId = orgId
        };

        using var message = new HttpRequestMessage(HttpMethod.Post, $"api/domain/v1/{domainId}/delegation/approve") {
            Content = JsonContent.Create(body, options: s_json)
        };
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Rejects a domain delegation request.
    /// </summary>
    /// <param name="domainId">Domain identifier.</param>
    /// <param name="orgId">Organization or department identifier.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task RejectDelegationAsync(
        int domainId,
        int orgId,
        CancellationToken cancellationToken = default) {
        if (domainId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(domainId));
        }

        if (orgId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(orgId));
        }

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        var body = new DomainApproveRequest {
            OrgId = orgId
        };

        using var message = new HttpRequestMessage(HttpMethod.Post, $"api/domain/v1/{domainId}/delegation/reject") {
            Content = JsonContent.Create(body, options: s_json)
        };
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Delegates multiple domains using a bulk payload.
    /// </summary>
    /// <param name="request">Bulk delegation request.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task DelegateManyAsync(
        AdminDomainsDelegation request,
        CancellationToken cancellationToken = default) {
        Guard.AgainstNull(request, nameof(request));

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        using var message = new HttpRequestMessage(HttpMethod.Post, "api/domain/v1/delegation") {
            Content = JsonContent.Create(request, options: s_json)
        };
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
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

    private sealed class DomainApproveRequest {
        [JsonPropertyName("orgId")]
        public int OrgId { get; set; }
    }

    /// <summary>
    /// Represents a bulk domains delegation request.
    /// </summary>
    public sealed class AdminDomainsDelegation {
        /// <summary>Gets or sets the organization identifier.</summary>
        public int OrgId { get; set; }

        /// <summary>Gets or sets the certificate types.</summary>
        public IReadOnlyList<string> CertTypes { get; set; } = Array.Empty<string>();

        /// <summary>Gets or sets domain certificate request privileges.</summary>
        public IReadOnlyList<string> DomainCertificateRequestPrivileges { get; set; } = Array.Empty<string>();

        /// <summary>Gets or sets the domain identifiers.</summary>
        public IReadOnlyList<int> DomainIds { get; set; } = Array.Empty<int>();
    }
}
