namespace SectigoCertificateManager.AdminApi;

using SectigoCertificateManager;
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
/// Minimal client for the Sectigo Admin Operations API organization validations endpoints.
/// </summary>
public sealed class AdminOrganizationValidationsClient : AdminApiClientBase {
    private static readonly JsonSerializerOptions s_json = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminOrganizationValidationsClient"/> class.
    /// </summary>
    /// <param name="config">Admin API configuration.</param>
    /// <param name="httpClient">
    /// Optional <see cref="HttpClient"/> instance. When not provided, a new instance is created
    /// and disposed with this client.
    /// </param>
    public AdminOrganizationValidationsClient(AdminApiConfig config, HttpClient? httpClient = null)
        : base(config, httpClient) {
    }

    /// <summary>
    /// Lists validations for the specified organization.
    /// </summary>
    /// <param name="organizationId">Organization identifier.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<IReadOnlyList<AdminValidationSummary>> ListAsync(
        int organizationId,
        CancellationToken cancellationToken = default) {
        if (organizationId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(organizationId));
        }

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"api/organization/v2/{organizationId}/validations");
        SetBearer(request, token);

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        await ApiErrorHandler.ThrowIfErrorAsync(response, cancellationToken).ConfigureAwait(false);

        var items = await response.Content
            .ReadFromJsonAsyncSafe<IReadOnlyList<AdminValidationSummary>>(s_json, cancellationToken)
            .ConfigureAwait(false);

        return items ?? Array.Empty<AdminValidationSummary>();
    }

    /// <summary>
    /// Retrieves detailed information about a specific organization validation.
    /// </summary>
    /// <param name="organizationId">Organization identifier.</param>
    /// <param name="validationId">Validation identifier.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<AdminValidationDetails?> GetAsync(
        int organizationId,
        int validationId,
        CancellationToken cancellationToken = default) {
        if (organizationId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(organizationId));
        }

        if (validationId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(validationId));
        }

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"api/organization/v2/{organizationId}/validations/{validationId}");
        SetBearer(request, token);

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        await ApiErrorHandler.ThrowIfErrorAsync(response, cancellationToken).ConfigureAwait(false);

        var details = await response.Content
            .ReadFromJsonAsyncSafe<AdminValidationDetails>(s_json, cancellationToken)
            .ConfigureAwait(false);

        return details;
    }

    /// <summary>
    /// Synchronizes the specified organization validation with the backend and returns updated details.
    /// </summary>
    /// <param name="organizationId">Organization identifier.</param>
    /// <param name="validationId">Validation identifier.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<AdminValidationDetails?> SyncAsync(
        int organizationId,
        int validationId,
        CancellationToken cancellationToken = default) {
        if (organizationId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(organizationId));
        }

        if (validationId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(validationId));
        }

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"api/organization/v2/{organizationId}/validations/{validationId}/sync");
        SetBearer(request, token);

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        await ApiErrorHandler.ThrowIfErrorAsync(response, cancellationToken).ConfigureAwait(false);

        var details = await response.Content
            .ReadFromJsonAsyncSafe<AdminValidationDetails>(s_json, cancellationToken)
            .ConfigureAwait(false);

        return details;
    }

    /// <summary>
    /// Deletes (resets and removes) the specified organization validation.
    /// </summary>
    /// <param name="organizationId">Organization identifier.</param>
    /// <param name="validationId">Validation identifier.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task DeleteAsync(
        int organizationId,
        int validationId,
        CancellationToken cancellationToken = default) {
        if (organizationId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(organizationId));
        }

        if (validationId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(validationId));
        }

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        using var request = new HttpRequestMessage(
            HttpMethod.Delete,
            $"api/organization/v2/{organizationId}/validations/{validationId}");
        SetBearer(request, token);

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        await ApiErrorHandler.ThrowIfErrorAsync(response, cancellationToken).ConfigureAwait(false);
    }

}
