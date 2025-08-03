namespace SectigoCertificateManager.Clients;

using SectigoCertificateManager.Models;
using SectigoCertificateManager.Requests;
using SectigoCertificateManager.Responses;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using SectigoCertificateManager.Utilities;
using System.Globalization;

public sealed partial class OrdersClient {
    /// <summary>
    /// Searches for orders using the provided filter.
    /// </summary>
    public async Task<OrderResponse?> SearchAsync(
        OrderSearchRequest request,
        CancellationToken cancellationToken = default) {
        Guard.AgainstNull(request, nameof(request));

        var list = new List<Order>();
        await foreach (var order in EnumerateSearchAsync(request, cancellationToken).ConfigureAwait(false)) {
            list.Add(order);
        }

        return list.Count == 0 ? null : new OrderResponse { Orders = list };
    }

    /// <summary>
    /// Streams orders matching the provided filter.
    /// </summary>
    public async IAsyncEnumerable<Order> EnumerateSearchAsync(
        OrderSearchRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default) {
        Guard.AgainstNull(request, nameof(request));

        var originalSize = request.Size;
        var originalPosition = request.Position;
        var pageSize = request.Size ?? 200;
        var firstPage = true;
        var position = request.Position ?? 0;

        try {
            var query = BuildQuery(request);
            var response = await _client.GetAsync($"v1/order{query}", cancellationToken).ConfigureAwait(false);
            var page = await response.Content
                .ReadFromJsonAsyncSafe<IReadOnlyList<Order>>(s_json, cancellationToken)
                .ConfigureAwait(false);
            if (page is null || page.Count == 0) {
                if (firstPage) {
                    yield break;
                }
                yield break;
            }

            foreach (var order in page) {
                yield return order;
            }

            if (page.Count < pageSize) {
                yield break;
            }

            request.Size = pageSize;
            firstPage = false;
            position += pageSize;

            while (true) {
                request.Position = position;
                query = BuildQuery(request);
                response = await _client.GetAsync($"v1/order{query}", cancellationToken).ConfigureAwait(false);
                page = await response.Content
                    .ReadFromJsonAsyncSafe<IReadOnlyList<Order>>(s_json, cancellationToken)
                    .ConfigureAwait(false);
                if (page is null || page.Count == 0) {
                    yield break;
                }

                foreach (var order in page) {
                    yield return order;
                }

                if (page.Count < pageSize) {
                    yield break;
                }

                position += pageSize;
                firstPage = false;
            }
        } finally {
            request.Size = originalSize;
            request.Position = originalPosition;
        }
    }

    private static string BuildQuery(OrderSearchRequest request) {
        var query = new List<string>();
        if (request.Size.HasValue) {
            query.Add($"size={request.Size.Value}");
        }
        if (request.Position.HasValue) {
            query.Add($"position={request.Position.Value}");
        }
        if (request.Status.HasValue) {
            query.Add($"status={request.Status.Value}");
        }
        if (request.OrderNumber.HasValue) {
            query.Add($"orderNumber={request.OrderNumber.Value}");
        }
        if (!string.IsNullOrEmpty(request.BackendCertId)) {
            query.Add($"backendCertId={Uri.EscapeDataString(request.BackendCertId)}");
        }
        if (request.UpdatedAfter.HasValue) {
            var updated = request.UpdatedAfter.Value.ToUniversalTime()
                .ToString("s", CultureInfo.InvariantCulture);
            query.Add($"updatedAfter={updated}");
        }
        return query.Count == 0 ? string.Empty : "?" + string.Join("&", query);
    }
}
