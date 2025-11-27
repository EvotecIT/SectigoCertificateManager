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
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

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
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

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
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

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
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        await ApiErrorHandler.ThrowIfErrorAsync(response, cancellationToken).ConfigureAwait(false);
    }

}
