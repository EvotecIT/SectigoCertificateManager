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
/// Minimal client for the Sectigo Admin Operations API DCV v2 endpoints.
/// </summary>
public sealed class AdminDcvClient : AdminApiClientBase {
    private static readonly JsonSerializerOptions s_json = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminDcvClient"/> class.
    /// </summary>
    /// <param name="config">Admin API configuration.</param>
    /// <param name="httpClient">
    /// Optional <see cref="HttpClient"/> instance. When not provided, a new instance is created
    /// and disposed with this client.
    /// </param>
    public AdminDcvClient(AdminApiConfig config, HttpClient? httpClient = null)
        : base(config, httpClient) {
    }

    /// <summary>
    /// Lists domain validations using the specified filter.
    /// </summary>
    /// <param name="domain">Optional domain filter.</param>
    /// <param name="expiresInDays">Optional "expires in (days)" filter.</param>
    /// <param name="organizationId">Optional organization identifier filter.</param>
    /// <param name="departmentId">Optional department identifier filter.</param>
    /// <param name="dcvStatus">Optional DCV status filter.</param>
    /// <param name="orderStatus">Optional order status filter.</param>
    /// <param name="size">Optional page size.</param>
    /// <param name="position">Optional position offset.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<IReadOnlyList<AdminDcvValidationSummary>> ListAsync(
        string? domain = null,
        int? expiresInDays = null,
        int? organizationId = null,
        int? departmentId = null,
        string? dcvStatus = null,
        string? orderStatus = null,
        int? size = null,
        int? position = null,
        CancellationToken cancellationToken = default) {
        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        var path = QueryStringBuilder.Build("api/dcv/v2/validation", q => q
            .AddString("domain", domain)
            .AddInt("expiresIn", expiresInDays)
            .AddInt("org", organizationId)
            .AddInt("department", departmentId)
            .AddString("dcvStatus", dcvStatus)
            .AddString("orderStatus", orderStatus)
            .AddInt("size", size)
            .AddInt("position", position));

        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        SetBearer(request, token);

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        await ApiErrorHandler.ThrowIfErrorAsync(response, cancellationToken).ConfigureAwait(false);

        var items = await response.Content
            .ReadFromJsonAsyncSafe<IReadOnlyList<AdminDcvValidationSummary>>(s_json, cancellationToken)
            .ConfigureAwait(false);

        return items ?? Array.Empty<AdminDcvValidationSummary>();
    }

    /// <summary>
    /// Retrieves validation status details for the specified domain.
    /// </summary>
    /// <param name="domain">Domain name.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<AdminDcvStatus?> GetStatusAsync(
        string domain,
        CancellationToken cancellationToken = default) {
        Guard.AgainstNullOrWhiteSpace(domain, nameof(domain));

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        var payload = new AdminDomainRequest {
            Domain = domain
        };

        using var message = new HttpRequestMessage(HttpMethod.Post, "api/dcv/v2/validation/status") {
            Content = JsonContent.Create(payload, options: s_json)
        };
        SetBearer(message, token);

        using var response = await _httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        await ApiErrorHandler.ThrowIfErrorAsync(response, cancellationToken).ConfigureAwait(false);

        var status = await response.Content
            .ReadFromJsonAsyncSafe<AdminDcvStatus>(s_json, cancellationToken)
            .ConfigureAwait(false);

        return status;
    }

    /// <summary>
    /// Clears (resets) the DCV state for the specified domain.
    /// </summary>
    /// <param name="domain">Domain to reset validation for.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task ClearAsync(
        string domain,
        CancellationToken cancellationToken = default) {
        Guard.AgainstNullOrWhiteSpace(domain, nameof(domain));

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        var payload = new AdminDomainRequest {
            Domain = domain
        };

        using var message = new HttpRequestMessage(HttpMethod.Post, "api/dcv/v2/validation/clear") {
            Content = JsonContent.Create(payload, options: s_json)
        };
        SetBearer(message, token);

        using var response = await _httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        await ApiErrorHandler.ThrowIfErrorAsync(response, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Deletes the DCV record for the specified domain.
    /// </summary>
    /// <param name="domain">Domain to delete validation for.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task DeleteAsync(
        string domain,
        CancellationToken cancellationToken = default) {
        Guard.AgainstNullOrWhiteSpace(domain, nameof(domain));

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        var payload = new AdminDomainRequest {
            Domain = domain
        };

        using var message = new HttpRequestMessage(HttpMethod.Post, "api/dcv/v2/validation/delete") {
            Content = JsonContent.Create(payload, options: s_json)
        };
        SetBearer(message, token);

        using var response = await _httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        await ApiErrorHandler.ThrowIfErrorAsync(response, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Starts DNS TXT DCV and returns the informational message.
    /// </summary>
    public async Task<string?> StartTxtAsync(
        string domain,
        CancellationToken cancellationToken = default) {
        Guard.AgainstNullOrWhiteSpace(domain, nameof(domain));

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        var payload = new AdminDomainRequest {
            Domain = domain
        };

        using var message = new HttpRequestMessage(HttpMethod.Post, "api/dcv/v2/validation/start/domain/txt") {
            Content = JsonContent.Create(payload, options: s_json)
        };
        SetBearer(message, token);

        using var response = await _httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        await ApiErrorHandler.ThrowIfErrorAsync(response, cancellationToken).ConfigureAwait(false);

        var result = await response.Content
            .ReadFromJsonAsyncSafe<ResponseMessage>(s_json, cancellationToken)
            .ConfigureAwait(false);

        return result?.Message;
    }

    /// <summary>
    /// Starts HTTPS DCV and returns validation file details.
    /// </summary>
    public async Task<DomainHttpsResponse?> StartHttpsAsync(
        string domain,
        CancellationToken cancellationToken = default) {
        Guard.AgainstNullOrWhiteSpace(domain, nameof(domain));

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        var payload = new AdminDomainRequest {
            Domain = domain
        };

        using var message = new HttpRequestMessage(HttpMethod.Post, "api/dcv/v2/validation/start/domain/https") {
            Content = JsonContent.Create(payload, options: s_json)
        };
        SetBearer(message, token);

        using var response = await _httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        await ApiErrorHandler.ThrowIfErrorAsync(response, cancellationToken).ConfigureAwait(false);

        return await response.Content
            .ReadFromJsonAsyncSafe<DomainHttpsResponse>(s_json, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Starts HTTP DCV and returns validation file details.
    /// </summary>
    public async Task<DomainHttpResponse?> StartHttpAsync(
        string domain,
        CancellationToken cancellationToken = default) {
        Guard.AgainstNullOrWhiteSpace(domain, nameof(domain));

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        var payload = new AdminDomainRequest {
            Domain = domain
        };

        using var message = new HttpRequestMessage(HttpMethod.Post, "api/dcv/v2/validation/start/domain/http") {
            Content = JsonContent.Create(payload, options: s_json)
        };
        SetBearer(message, token);

        using var response = await _httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        await ApiErrorHandler.ThrowIfErrorAsync(response, cancellationToken).ConfigureAwait(false);

        return await response.Content
            .ReadFromJsonAsyncSafe<DomainHttpResponse>(s_json, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Starts email DCV and returns available email addresses.
    /// </summary>
    public async Task<DomainEmailResponse?> StartEmailAsync(
        string domain,
        CancellationToken cancellationToken = default) {
        Guard.AgainstNullOrWhiteSpace(domain, nameof(domain));

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        var payload = new AdminDomainRequest {
            Domain = domain
        };

        using var message = new HttpRequestMessage(HttpMethod.Post, "api/dcv/v2/validation/start/domain/email") {
            Content = JsonContent.Create(payload, options: s_json)
        };
        SetBearer(message, token);

        using var response = await _httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        await ApiErrorHandler.ThrowIfErrorAsync(response, cancellationToken).ConfigureAwait(false);

        return await response.Content
            .ReadFromJsonAsyncSafe<DomainEmailResponse>(s_json, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Starts CNAME DCV and returns host and point details.
    /// </summary>
    public async Task<DomainCnameResponse?> StartCnameAsync(
        string domain,
        CancellationToken cancellationToken = default) {
        Guard.AgainstNullOrWhiteSpace(domain, nameof(domain));

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        var payload = new AdminDomainRequest {
            Domain = domain
        };

        using var message = new HttpRequestMessage(HttpMethod.Post, "api/dcv/v2/validation/start/domain/cname") {
            Content = JsonContent.Create(payload, options: s_json)
        };
        SetBearer(message, token);

        using var response = await _httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        await ApiErrorHandler.ThrowIfErrorAsync(response, cancellationToken).ConfigureAwait(false);

        return await response.Content
            .ReadFromJsonAsyncSafe<DomainCnameResponse>(s_json, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Submits TXT DCV for the specified domain.
    /// </summary>
    public async Task<SubmitDomainResponse?> SubmitTxtAsync(
        string domain,
        CancellationToken cancellationToken = default) {
        Guard.AgainstNullOrWhiteSpace(domain, nameof(domain));

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        var payload = new AdminDomainRequest {
            Domain = domain
        };

        using var message = new HttpRequestMessage(HttpMethod.Post, "api/dcv/v2/validation/submit/domain/txt") {
            Content = JsonContent.Create(payload, options: s_json)
        };
        SetBearer(message, token);

        using var response = await _httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        await ApiErrorHandler.ThrowIfErrorAsync(response, cancellationToken).ConfigureAwait(false);

        return await response.Content
            .ReadFromJsonAsyncSafe<SubmitDomainResponse>(s_json, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Submits HTTPS DCV for the specified domain.
    /// </summary>
    public async Task<SubmitDomainResponse?> SubmitHttpsAsync(
        string domain,
        CancellationToken cancellationToken = default) {
        Guard.AgainstNullOrWhiteSpace(domain, nameof(domain));

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        var payload = new AdminDomainRequest {
            Domain = domain
        };

        using var message = new HttpRequestMessage(HttpMethod.Post, "api/dcv/v2/validation/submit/domain/https") {
            Content = JsonContent.Create(payload, options: s_json)
        };
        SetBearer(message, token);

        using var response = await _httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        await ApiErrorHandler.ThrowIfErrorAsync(response, cancellationToken).ConfigureAwait(false);

        return await response.Content
            .ReadFromJsonAsyncSafe<SubmitDomainResponse>(s_json, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Submits HTTP DCV for the specified domain.
    /// </summary>
    public async Task<SubmitDomainResponse?> SubmitHttpAsync(
        string domain,
        CancellationToken cancellationToken = default) {
        Guard.AgainstNullOrWhiteSpace(domain, nameof(domain));

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        var payload = new AdminDomainRequest {
            Domain = domain
        };

        using var message = new HttpRequestMessage(HttpMethod.Post, "api/dcv/v2/validation/submit/domain/http") {
            Content = JsonContent.Create(payload, options: s_json)
        };
        SetBearer(message, token);

        using var response = await _httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        await ApiErrorHandler.ThrowIfErrorAsync(response, cancellationToken).ConfigureAwait(false);

        return await response.Content
            .ReadFromJsonAsyncSafe<SubmitDomainResponse>(s_json, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Submits email DCV for the specified domain.
    /// </summary>
    public async Task<DomainEmailResponse?> SubmitEmailAsync(
        string domain,
        string email,
        CancellationToken cancellationToken = default) {
        Guard.AgainstNullOrWhiteSpace(domain, nameof(domain));
        Guard.AgainstNullOrWhiteSpace(email, nameof(email));

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        var payload = new DomainEmailRequest {
            Domain = domain,
            Email = email
        };

        using var message = new HttpRequestMessage(HttpMethod.Post, "api/dcv/v2/validation/submit/domain/email") {
            Content = JsonContent.Create(payload, options: s_json)
        };
        SetBearer(message, token);

        using var response = await _httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        await ApiErrorHandler.ThrowIfErrorAsync(response, cancellationToken).ConfigureAwait(false);

        return await response.Content
            .ReadFromJsonAsyncSafe<DomainEmailResponse>(s_json, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Submits bulk email DCV for the specified domains.
    /// </summary>
    public async Task<DomainEmailResponse?> SubmitBulkEmailAsync(
        IReadOnlyList<string> domains,
        string email,
        CancellationToken cancellationToken = default) {
        Guard.AgainstNull(domains, nameof(domains));
        if (domains.Count == 0) {
            throw new ArgumentException("At least one domain is required.", nameof(domains));
        }

        Guard.AgainstNullOrWhiteSpace(email, nameof(email));

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        var payload = new DomainEmailBulkRequest {
            Domains = domains,
            Email = email
        };

        using var message = new HttpRequestMessage(HttpMethod.Post, "api/dcv/v2/validation/submit-bulk/domain/email") {
            Content = JsonContent.Create(payload, options: s_json)
        };
        SetBearer(message, token);

        using var response = await _httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        await ApiErrorHandler.ThrowIfErrorAsync(response, cancellationToken).ConfigureAwait(false);

        return await response.Content
            .ReadFromJsonAsyncSafe<DomainEmailResponse>(s_json, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Submits CNAME DCV for the specified domain.
    /// </summary>
    public async Task<DomainHttpResponse?> SubmitCnameAsync(
        string domain,
        string? dnsAgentUuid = null,
        string? dnsProviderName = null,
        CancellationToken cancellationToken = default) {
        Guard.AgainstNullOrWhiteSpace(domain, nameof(domain));

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        var payload = new DomainCnameRequest {
            Domain = domain,
            Domains = null,
            DnsAgentUuid = dnsAgentUuid,
            DnsProviderName = dnsProviderName
        };

        using var message = new HttpRequestMessage(HttpMethod.Post, "api/dcv/v2/validation/submit/domain/cname") {
            Content = JsonContent.Create(payload, options: s_json)
        };
        SetBearer(message, token);

        using var response = await _httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        await ApiErrorHandler.ThrowIfErrorAsync(response, cancellationToken).ConfigureAwait(false);

        return await response.Content
            .ReadFromJsonAsyncSafe<DomainHttpResponse>(s_json, cancellationToken)
            .ConfigureAwait(false);
    }
}
