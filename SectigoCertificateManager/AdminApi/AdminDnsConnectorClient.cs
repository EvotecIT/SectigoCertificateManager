namespace SectigoCertificateManager.AdminApi;

using SectigoCertificateManager.Utilities;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Admin Operations API client for DNS connectors.
/// </summary>
public sealed class AdminDnsConnectorClient : AdminApiClientBase {
    private static readonly JsonSerializerOptions s_json = new(JsonSerializerDefaults.Web);

    public AdminDnsConnectorClient(AdminApiConfig config, HttpClient? httpClient = null)
        : base(config, httpClient) {
    }

    /// <summary>
    /// Lists DNS connectors.
    /// </summary>
    public async Task<(IReadOnlyList<DnsConnectorListItem> Items, int? TotalCount)> ListAsync(
        int? size = null,
        int? position = null,
        string? name = null,
        DnsConnectorStatus? status = null,
        IReadOnlyList<int>? orgIds = null,
        CancellationToken cancellationToken = default) {
        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        var path = QueryStringBuilder.Build("api/connector/v1/dns", q => {
            if (size.HasValue && size.Value > 0) {
                q.AddInt("size", size.Value);
            }

            if (position.HasValue && position.Value >= 0) {
                q.AddInt("position", position.Value);
            }

            if (!string.IsNullOrWhiteSpace(name)) {
                q.AddString("name", name);
            }

            if (status.HasValue) {
                q.AddString("status", MapStatus(status.Value));
            }

            if (orgIds is { Count: > 0 }) {
                foreach (var id in orgIds) {
                    if (id > 0) {
                        q.AddInt("orgIds", id);
                    }
                }
            }
        });

        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        SetBearer(request, token);

        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
            .ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var items = await response.Content
            .ReadFromJsonAsyncSafe<List<DnsConnectorListItem>>(s_json, cancellationToken)
            .ConfigureAwait(false) ?? new List<DnsConnectorListItem>();

        int? total = null;
        if (response.Headers.TryGetValues("X-Total-Count", out var values)) {
            foreach (var value in values) {
                if (int.TryParse(value, out var parsed)) {
                    total = parsed;
                    break;
                }
            }
        }

        return (items, total);
    }

    /// <summary>
    /// Gets details for a single DNS connector.
    /// </summary>
    public async Task<DnsConnectorDetails?> GetAsync(string uuid, CancellationToken cancellationToken = default) {
        GuardAgainstNullOrWhiteSpace(uuid, nameof(uuid));

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);
        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/connector/v1/dns/{Uri.EscapeDataString(uuid)}");
        SetBearer(request, token);

        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
            .ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsyncSafe<DnsConnectorDetails>(s_json, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Lists provider identifiers available for a DNS connector.
    /// </summary>
    public async Task<(IReadOnlyList<string> Providers, int? TotalCount)> ListProvidersAsync(string uuid, CancellationToken cancellationToken = default) {
        GuardAgainstNullOrWhiteSpace(uuid, nameof(uuid));

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);
        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/connector/v1/dns/{Uri.EscapeDataString(uuid)}/provider");
        SetBearer(request, token);

        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
            .ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var providers = await response.Content
            .ReadFromJsonAsyncSafe<List<string>>(s_json, cancellationToken)
            .ConfigureAwait(false) ?? new List<string>();

        int? total = null;
        if (response.Headers.TryGetValues("X-Total-Count", out var values)) {
            foreach (var value in values) {
                if (int.TryParse(value, out var parsed)) {
                    total = parsed;
                    break;
                }
            }
        }

        return (providers, total);
    }

    private static void GuardAgainstNullOrWhiteSpace(string? value, string paramName) {
        if (string.IsNullOrWhiteSpace(value)) {
            throw new ArgumentException("Value cannot be empty.", paramName);
        }
    }

    internal static string MapStatus(DnsConnectorStatus status) {
        return status switch {
            DnsConnectorStatus.NotAvailable => "NOT_AVAILABLE",
            DnsConnectorStatus.NotConnected => "NOT_CONNECTED",
            DnsConnectorStatus.Connected => "CONNECTED",
            _ => "NOT_AVAILABLE"
        };
    }
}

