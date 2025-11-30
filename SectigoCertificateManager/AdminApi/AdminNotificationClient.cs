namespace SectigoCertificateManager.AdminApi;

using SectigoCertificateManager.Utilities;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Client for the Admin Operations API notification endpoints.
/// </summary>
public sealed class AdminNotificationClient : AdminApiClientBase {
    private static readonly JsonSerializerOptions s_json = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminNotificationClient"/> class.
    /// </summary>
    /// <param name="config">Admin API configuration.</param>
    /// <param name="httpClient">
    /// Optional <see cref="HttpClient"/> instance. When not provided, a new instance is created
    /// and disposed with this client.
    /// </param>
    public AdminNotificationClient(AdminApiConfig config, HttpClient? httpClient = null)
        : base(config, httpClient) {
    }

    /// <summary>
    /// Lists notifications according to the specified filter and pagination.
    /// </summary>
    /// <param name="size">Optional page size.</param>
    /// <param name="position">Optional position offset.</param>
    /// <param name="description">Optional description filter.</param>
    /// <param name="id">Optional notification identifier filter.</param>
    /// <param name="orgId">Optional organization identifier filter.</param>
    /// <param name="selectedOrgType">Optional organization selection type filter.</param>
    /// <param name="type">Optional notification type filter as a display string.</param>
    /// <param name="certTypeId">Optional certificate profile identifier filter.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>
    /// A tuple containing the list of notifications and the optional total count of matching entries.
    /// </returns>
    public async Task<(IReadOnlyList<AdminNotification> Items, int? TotalCount)> ListAsync(
        int? size = null,
        int? position = null,
        string? description = null,
        int? id = null,
        int? orgId = null,
        NotificationOrgSelectionType? selectedOrgType = null,
        string? type = null,
        int? certTypeId = null,
        CancellationToken cancellationToken = default) {
        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        var builder = new StringBuilder("api/notification/v1");
        var hasQuery = false;

        void Append(string key, string? value) {
            if (string.IsNullOrWhiteSpace(value)) { return; }
            _ = hasQuery ? builder.Append('&') : builder.Append('?');
            builder.Append(key).Append('=').Append(Uri.EscapeDataString(value));
            hasQuery = true;
        }

        if (size.HasValue) { Append("size", size.Value.ToString()); }
        if (position.HasValue) { Append("position", position.Value.ToString()); }
        Append("description", description);
        if (id.HasValue) { Append("id", id.Value.ToString()); }
        if (orgId.HasValue) { Append("orgId", orgId.Value.ToString()); }
        if (selectedOrgType.HasValue) { Append("selectedOrgType", selectedOrgType.Value.ToString()); }
        Append("type", type);
        if (certTypeId.HasValue) { Append("certTypeId", certTypeId.Value.ToString()); }

        var path = builder.ToString();

        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        SetBearer(request, token);

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        await ApiErrorHandler.ThrowIfErrorAsync(response, cancellationToken).ConfigureAwait(false);

        int? total = null;
        if (response.Headers.TryGetValues("X-Total-Count", out var values)) {
            foreach (var value in values) {
                if (int.TryParse(value, out var parsed)) {
                    total = parsed;
                    break;
                }
            }
        }

        var items = await response.Content
            .ReadFromJsonAsyncSafe<IReadOnlyList<AdminNotification>>(s_json, cancellationToken)
            .ConfigureAwait(false);

        return (items ?? Array.Empty<AdminNotification>(), total);
    }

    /// <summary>
    /// Retrieves the set of notification types available to the current administrator.
    /// </summary>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<IReadOnlyList<string>> ListTypesAsync(
        CancellationToken cancellationToken = default) {
        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        using var request = new HttpRequestMessage(HttpMethod.Get, "api/notification/v1/types");
        SetBearer(request, token);

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        await ApiErrorHandler.ThrowIfErrorAsync(response, cancellationToken).ConfigureAwait(false);

        var types = await response.Content
            .ReadFromJsonAsyncSafe<IReadOnlyList<string>>(s_json, cancellationToken)
            .ConfigureAwait(false);

        return types ?? Array.Empty<string>();
    }

    /// <summary>
    /// Updates an existing notification definition.
    /// </summary>
    /// <param name="notificationId">Notification identifier.</param>
    /// <param name="request">Notification update payload.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task UpdateAsync(
        int notificationId,
        AdminNotificationRequest request,
        CancellationToken cancellationToken = default) {
        if (notificationId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(notificationId));
        }

        Guard.AgainstNull(request, nameof(request));

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        var httpRequest = new HttpRequestMessage(HttpMethod.Put, $"api/notification/v1/{notificationId}") {
            Content = JsonContent.Create(request, options: s_json)
        };
        SetBearer(httpRequest, token);

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);
        await ApiErrorHandler.ThrowIfErrorAsync(response, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Deletes the specified notification definition.
    /// </summary>
    /// <param name="notificationId">Notification identifier.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task DeleteAsync(
        int notificationId,
        CancellationToken cancellationToken = default) {
        if (notificationId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(notificationId));
        }

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        using var request = new HttpRequestMessage(HttpMethod.Delete, $"api/notification/v1/{notificationId}");
        SetBearer(request, token);

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        await ApiErrorHandler.ThrowIfErrorAsync(response, cancellationToken).ConfigureAwait(false);
    }
}
