namespace SectigoCertificateManager.AdminApi;

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

        var builder = new StringBuilder("api/acme/v2/account");
        var hasQuery = false;

        void Append(string key, string? value) {
            if (string.IsNullOrWhiteSpace(value)) {
                return;
            }

            _ = hasQuery ? builder.Append('&') : builder.Append('?');
            builder.Append(key).Append('=').Append(Uri.EscapeDataString(value));
            hasQuery = true;
        }

        void AppendInt(string key, int? value) {
            if (!value.HasValue) {
                return;
            }

            _ = hasQuery ? builder.Append('&') : builder.Append('?');
            builder.Append(key).Append('=').Append(value.Value);
            hasQuery = true;
        }

        AppendInt("size", size);
        AppendInt("position", position);
        AppendInt("organizationId", organizationId);
        Append("certValidationType", certValidationType);
        Append("name", name);
        Append("acmeServer", acmeServer);

        using var request = new HttpRequestMessage(HttpMethod.Get, builder.ToString());
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

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
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

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
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var location = response.Headers.Location;
        if (location is null) {
            return 0;
        }

        var url = location.ToString().Trim().TrimEnd('/');
        var segments = url.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length == 0) {
            return 0;
        }

        var lastSegment = segments[segments.Length - 1];
        return int.TryParse(lastSegment, out var id) ? id : 0;
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

        var builder = new StringBuilder($"api/acme/v2/account/{accountId}/domain");
        var hasQuery = false;

        void Append(string key, string? value) {
            if (string.IsNullOrWhiteSpace(value)) {
                return;
            }

            _ = hasQuery ? builder.Append('&') : builder.Append('?');
            builder.Append(key).Append('=').Append(Uri.EscapeDataString(value));
            hasQuery = true;
        }

        void AppendInt(string key, int? value) {
            if (!value.HasValue) {
                return;
            }

            _ = hasQuery ? builder.Append('&') : builder.Append('?');
            builder.Append(key).Append('=').Append(value.Value);
            hasQuery = true;
        }

        AppendInt("size", size);
        AppendInt("position", position);
        Append("name", name);
        AppendInt("expiresWithinNextDays", expiresWithinNextDays);
        AppendInt("stickyExpiresWithinNextDays", stickyExpiresWithinNextDays);

        using var request = new HttpRequestMessage(HttpMethod.Get, builder.ToString());
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

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
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

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
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

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

        var builder = new StringBuilder($"api/acme/v2/account/{accountId}/client");
        var hasQuery = false;

        void Append(string key, string? value) {
            if (string.IsNullOrWhiteSpace(value)) {
                return;
            }

            _ = hasQuery ? builder.Append('&') : builder.Append('?');
            builder.Append(key).Append('=').Append(Uri.EscapeDataString(value));
            hasQuery = true;
        }

        void AppendInt(string key, int? value) {
            if (!value.HasValue) {
                return;
            }

            _ = hasQuery ? builder.Append('&') : builder.Append('?');
            builder.Append(key).Append('=').Append(value.Value);
            hasQuery = true;
        }

        AppendInt("size", size);
        AppendInt("position", position);
        Append("contacts", contacts);
        Append("userAgent", userAgent);
        Append("ipAddress", ipAddress);
        AppendInt("lastActivityWithinPrevDays", lastActivityWithinPrevDays);

        using var request = new HttpRequestMessage(HttpMethod.Get, builder.ToString());
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

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
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
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
