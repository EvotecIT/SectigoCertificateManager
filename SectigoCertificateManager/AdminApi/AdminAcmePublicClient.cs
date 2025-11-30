namespace SectigoCertificateManager.AdminApi;

using SectigoCertificateManager;
using SectigoCertificateManager.Requests;
using SectigoCertificateManager.Utilities;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Minimal client for Sectigo Public ACME accounts and related endpoints.
/// </summary>
public sealed class AdminAcmePublicClient : AdminApiClientBase {
    private static readonly JsonSerializerOptions s_json = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Creates a new instance of the public ACME client using the specified Admin API configuration.
    /// </summary>
    /// <param name="config">Admin API configuration including base URL and OAuth2 client credentials.</param>
    /// <param name="httpClient">
    /// Optional <see cref="HttpClient"/> to use for outbound requests. When omitted, a new instance is created and
    /// disposed with the client.
    /// </param>
    public AdminAcmePublicClient(AdminApiConfig config, HttpClient? httpClient = null)
        : base(config, httpClient) {
    }

    /// <summary>
    /// Lists public ACME accounts using the supplied filters.
    /// </summary>
    public async Task<IReadOnlyList<AdminPublicAcmeAccount>> ListAccountsAsync(
        int? size = null,
        int? position = null,
        int? organizationId = null,
        string? certValidationType = null,
        string? name = null,
        string? acmeServer = null,
        CancellationToken cancellationToken = default) {
        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        var path = QueryStringBuilder.Build("api/acme/v2/account", q => q
            .AddInt("size", size)
            .AddInt("position", position)
            .AddInt("organizationId", organizationId)
            .AddString("certValidationType", certValidationType)
            .AddString("name", name)
            .AddString("acmeServer", acmeServer));

        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        SetBearer(request, token);

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        await ApiErrorHandler.ThrowIfErrorAsync(response, cancellationToken).ConfigureAwait(false);

        var items = await response.Content
            .ReadFromJsonAsyncSafe<IReadOnlyList<AdminPublicAcmeAccount>>(s_json, cancellationToken)
            .ConfigureAwait(false);

        return items ?? Array.Empty<AdminPublicAcmeAccount>();
    }

    /// <summary>
    /// Retrieves details for the specified public ACME account.
    /// </summary>
    public async Task<AdminPublicAcmeAccount?> GetAccountAsync(
        int id,
        CancellationToken cancellationToken = default) {
        if (id <= 0) {
            throw new ArgumentOutOfRangeException(nameof(id));
        }

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/acme/v2/account/{id}");
        SetBearer(request, token);

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        await ApiErrorHandler.ThrowIfErrorAsync(response, cancellationToken).ConfigureAwait(false);

        var account = await response.Content
            .ReadFromJsonAsyncSafe<AdminPublicAcmeAccount>(s_json, cancellationToken)
            .ConfigureAwait(false);

        return account;
    }

    /// <summary>
    /// Creates a new public ACME account and returns its identifier.
    /// </summary>
    public async Task<int> CreateAccountAsync(
        AdminPublicAcmeAccountCreateRequest request,
        CancellationToken cancellationToken = default) {
        Guard.AgainstNull(request, nameof(request));
        if (request.OrganizationId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(request.OrganizationId));
        }

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        using var message = new HttpRequestMessage(HttpMethod.Post, "api/acme/v2/account") {
            Content = JsonContent.Create(request, options: s_json)
        };
        SetBearer(message, token);

        using var response = await _httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        await ApiErrorHandler.ThrowIfErrorAsync(response, cancellationToken).ConfigureAwait(false);

        return LocationHeaderParser.ParseId(response);
    }

    /// <summary>
    /// Lists domains associated with a public ACME account.
    /// </summary>
    public async Task<IReadOnlyList<AdminPublicAcmeDomain>> ListDomainsAsync(
        int accountId,
        int? size = null,
        int? position = null,
        string? name = null,
        int? expiresWithinNextDays = null,
        int? stickyExpiresWithinNextDays = null,
        CancellationToken cancellationToken = default) {
        if (accountId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(accountId));
        }

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        var path = QueryStringBuilder.Build($"api/acme/v2/account/{accountId}/domain", q => q
            .AddInt("size", size)
            .AddInt("position", position)
            .AddString("name", name)
            .AddInt("expiresWithinNextDays", expiresWithinNextDays)
            .AddInt("stickyExpiresWithinNextDays", stickyExpiresWithinNextDays));

        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        SetBearer(request, token);

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        await ApiErrorHandler.ThrowIfErrorAsync(response, cancellationToken).ConfigureAwait(false);

        var items = await response.Content
            .ReadFromJsonAsyncSafe<IReadOnlyList<AdminPublicAcmeDomain>>(s_json, cancellationToken)
            .ConfigureAwait(false);

        return items ?? Array.Empty<AdminPublicAcmeDomain>();
    }

    /// <summary>
    /// Adds domains to a public ACME account and returns names that were not added.
    /// </summary>
    public async Task<IReadOnlyList<string>> AddDomainsAsync(
        int accountId,
        IReadOnlyList<AdminAcmeDomainNameRequest> domains,
        CancellationToken cancellationToken = default) {
        if (accountId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(accountId));
        }

        Guard.AgainstNull(domains, nameof(domains));
        if (domains.Count == 0) {
            throw new ArgumentException("At least one domain must be provided.", nameof(domains));
        }

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        var payload = new PublicAcmeDomainListWrapper {
            Domains = domains
        };

        using var message = new HttpRequestMessage(HttpMethod.Post, $"api/acme/v2/account/{accountId}/domain") {
            Content = JsonContent.Create(payload, options: s_json)
        };
        SetBearer(message, token);

        using var response = await _httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        await ApiErrorHandler.ThrowIfErrorAsync(response, cancellationToken).ConfigureAwait(false);

        var result = await response.Content
            .ReadFromJsonAsyncSafe<NotAddedDomainsResponse>(s_json, cancellationToken)
            .ConfigureAwait(false);

        return result?.NotAddedDomains ?? Array.Empty<string>();
    }

    /// <summary>
    /// Removes domains from a public ACME account and returns names that were not removed.
    /// </summary>
    public async Task<IReadOnlyList<string>> RemoveDomainsAsync(
        int accountId,
        IReadOnlyList<AdminAcmeDomainNameRequest> domains,
        CancellationToken cancellationToken = default) {
        if (accountId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(accountId));
        }

        Guard.AgainstNull(domains, nameof(domains));
        if (domains.Count == 0) {
            throw new ArgumentException("At least one domain must be provided.", nameof(domains));
        }

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        var payload = new PublicAcmeDomainListWrapper {
            Domains = domains
        };

        using var message = new HttpRequestMessage(HttpMethod.Delete, $"api/acme/v2/account/{accountId}/domain") {
            Content = JsonContent.Create(payload, options: s_json)
        };
        SetBearer(message, token);

        using var response = await _httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        await ApiErrorHandler.ThrowIfErrorAsync(response, cancellationToken).ConfigureAwait(false);

        var result = await response.Content
            .ReadFromJsonAsyncSafe<NotRemovedDomainsResponse>(s_json, cancellationToken)
            .ConfigureAwait(false);

        return result?.NotRemovedDomains ?? Array.Empty<string>();
    }

    /// <summary>
    /// Lists ACME clients for the given public ACME account.
    /// </summary>
    public async Task<IReadOnlyList<AdminPublicAcmeClient>> ListClientsAsync(
        int accountId,
        int? size = null,
        int? position = null,
        string? contacts = null,
        string? userAgent = null,
        string? ipAddress = null,
        int? lastActivityWithinPrevDays = null,
        CancellationToken cancellationToken = default) {
        if (accountId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(accountId));
        }

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        var path = QueryStringBuilder.Build($"api/acme/v2/account/{accountId}/client", q => q
            .AddInt("size", size)
            .AddInt("position", position)
            .AddString("contacts", contacts)
            .AddString("userAgent", userAgent)
            .AddString("ipAddress", ipAddress)
            .AddInt("lastActivityWithinPrevDays", lastActivityWithinPrevDays));

        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        SetBearer(request, token);

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        await ApiErrorHandler.ThrowIfErrorAsync(response, cancellationToken).ConfigureAwait(false);

        var items = await response.Content
            .ReadFromJsonAsyncSafe<IReadOnlyList<AdminPublicAcmeClient>>(s_json, cancellationToken)
            .ConfigureAwait(false);

        return items ?? Array.Empty<AdminPublicAcmeClient>();
    }

    /// <summary>
    /// Deactivates a specific public ACME client for the given account.
    /// </summary>
    public async Task DeactivateClientAsync(
        int accountId,
        string clientId,
        CancellationToken cancellationToken = default) {
        if (accountId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(accountId));
        }

        Guard.AgainstNullOrWhiteSpace(clientId, nameof(clientId));

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        using var request = new HttpRequestMessage(
            HttpMethod.Put,
            $"api/acme/v2/account/{accountId}/client/{Uri.EscapeDataString(clientId)}");
        SetBearer(request, token);

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        await ApiErrorHandler.ThrowIfErrorAsync(response, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Lists Sectigo public ACME servers available for account configuration.
    /// </summary>
    public async Task<IReadOnlyList<AcmeServerInfo>> ListServersAsync(
        int? size = null,
        int? position = null,
        int? caId = null,
        string? certValidationType = null,
        string? url = null,
        string? name = null,
        CancellationToken cancellationToken = default) {
        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        var builder = new StringBuilder("api/acme/v1/server");
        var hasQuery = false;

        static string Encode(string? value) {
            var encoded = Uri.EscapeDataString(value ?? string.Empty);
            encoded = encoded
                .Replace(":", "%3A")
                .Replace("/", "%2F")
                .Replace("%3a", "%3A")
                .Replace("%2f", "%2F")
                .Replace("+", "%20");
            return encoded;
        }

        void Append(string key, string? value) {
            if (string.IsNullOrWhiteSpace(value)) { return; }
            _ = hasQuery ? builder.Append('&') : builder.Append('?');
            var encoded = Encode(value);
            builder.Append(key).Append('=').Append(encoded);
            hasQuery = true;
        }

        if (size.HasValue) { Append("size", size.Value.ToString()); }
        if (position.HasValue) { Append("position", position.Value.ToString()); }
        if (caId.HasValue) { Append("caId", caId.Value.ToString()); }
        Append("certValidationType", certValidationType);
        Append("url", url);
        Append("name", name);

        var path = builder.ToString();

        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        SetBearer(request, token);

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        await ApiErrorHandler.ThrowIfErrorAsync(response, cancellationToken).ConfigureAwait(false);

        var servers = await response.Content
            .ReadFromJsonAsyncSafe<IReadOnlyList<AcmeServerInfo>>(s_json, cancellationToken)
            .ConfigureAwait(false);

        return servers ?? Array.Empty<AcmeServerInfo>();
    }

    private sealed class PublicAcmeDomainListWrapper {
        public IReadOnlyList<AdminAcmeDomainNameRequest> Domains { get; set; } = Array.Empty<AdminAcmeDomainNameRequest>();
    }

    private sealed class NotAddedDomainsResponse {
        public IReadOnlyList<string> NotAddedDomains { get; set; } = Array.Empty<string>();
    }

    private sealed class NotRemovedDomainsResponse {
        public IReadOnlyList<string> NotRemovedDomains { get; set; } = Array.Empty<string>();
    }
}
