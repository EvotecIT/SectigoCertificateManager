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
/// Minimal client for universal (private) ACME accounts.
/// </summary>
public sealed class AdminAcmePrivateClient : AdminApiClientBase {
    private static readonly JsonSerializerOptions s_json = new(JsonSerializerDefaults.Web);

    public AdminAcmePrivateClient(AdminApiConfig config, HttpClient? httpClient = null)
        : base(config, httpClient) {
    }

    /// <summary>
    /// Lists universal ACME accounts with optional filters.
    /// </summary>
    public async Task<IReadOnlyList<AdminPrivateAcmeAccount>> ListAccountsAsync(
        int? size = null,
        int? position = null,
        int? organizationId = null,
        string? name = null,
        string? acmeServer = null,
        CancellationToken cancellationToken = default) {
        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        var builder = new StringBuilder("api/acme/v1/pca/account");
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
        Append("name", name);
        Append("acmeServer", acmeServer);

        using var request = new HttpRequestMessage(HttpMethod.Get, builder.ToString());
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var items = await response.Content
            .ReadFromJsonAsyncSafe<IReadOnlyList<AdminPrivateAcmeAccount>>(s_json, cancellationToken)
            .ConfigureAwait(false);

        return items ?? Array.Empty<AdminPrivateAcmeAccount>();
    }

    /// <summary>
    /// Creates a new universal ACME account and returns its identifier.
    /// </summary>
    public async Task<int> CreateAccountAsync(
        AdminPrivateAcmeAccountCreateRequest request,
        CancellationToken cancellationToken = default) {
        Guard.AgainstNull(request, nameof(request));
        if (request.OrganizationId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(request.OrganizationId));
        }

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        using var message = new HttpRequestMessage(HttpMethod.Post, "api/acme/v1/pca/account") {
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

}
