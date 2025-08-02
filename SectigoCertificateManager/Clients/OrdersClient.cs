namespace SectigoCertificateManager.Clients;

using SectigoCertificateManager.Models;
using SectigoCertificateManager.Responses;
using SectigoCertificateManager.Requests;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using SectigoCertificateManager.Utilities;
using System.Text.Json;

/// <summary>
/// Provides access to order related endpoints.
/// </summary>
public sealed partial class OrdersClient : BaseClient {

    /// <summary>
    /// Initializes a new instance of the <see cref="OrdersClient"/> class.
    /// </summary>
    /// <param name="client">HTTP client wrapper.</param>
    public OrdersClient(ISectigoClient client) : base(client) {
    }

    /// <summary>
    /// Retrieves an order by identifier.
    /// </summary>
    /// <param name="orderId">Identifier of the order to retrieve.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<Order?> GetAsync(int orderId, CancellationToken cancellationToken = default) {
        if (orderId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(orderId));
        }

        var response = await _client.GetAsync($"v1/order/{orderId}", cancellationToken).ConfigureAwait(false);
        return await response.Content
            .ReadFromJsonAsyncSafe<Order>(s_json, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Retrieves all orders visible to the user.
    /// </summary>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<IReadOnlyList<Order>> ListOrdersAsync(CancellationToken cancellationToken = default) {
        var list = new List<Order>();
        await foreach (var order in EnumerateOrdersAsync(cancellationToken: cancellationToken).ConfigureAwait(false)) {
            list.Add(order);
        }

        return list;
    }

    /// <summary>
    /// Streams orders page by page.
    /// </summary>
    /// <param name="pageSize">Number of orders to request per page.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async IAsyncEnumerable<Order> EnumerateOrdersAsync(
        int pageSize = 200,
        [EnumeratorCancellation] CancellationToken cancellationToken = default) {
        var position = 0;
        var firstPage = true;

        while (true) {
            var response = await _client
                .GetAsync($"v1/order?size={pageSize}&position={position}", cancellationToken)
                .ConfigureAwait(false);
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

            position += pageSize;
            firstPage = false;
        }
    }

    /// <summary>
    /// Uploads a CSV or JSON file containing multiple orders.
    /// </summary>
    /// <param name="stream">Stream providing the order data.</param>
    /// <param name="contentType">MIME type of the data. Use <c>text/csv</c> or <c>application/json</c>.</param>
    /// <param name="progress">Optional progress reporter.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task UploadAsync(
        Stream stream,
        string contentType,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default) {
        Guard.AgainstNull(stream, nameof(stream));
        Guard.AgainstNullOrEmpty(contentType, nameof(contentType), "Content type cannot be null or empty.");

        using var content = new Utilities.ProgressStreamContent(stream, progress);
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
        var response = await _client.PostAsync("v1/order/bulk", content, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Cancels an order by identifier.
    /// </summary>
    /// <param name="orderId">Identifier of the order to cancel.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task CancelAsync(int orderId, CancellationToken cancellationToken = default) {
        if (orderId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(orderId));
        }

        var response = await _client.PostAsync($"v1/order/{orderId}/cancel", new StringContent(string.Empty), cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Renews a certificate associated with an order.
    /// </summary>
    /// <param name="orderId">Identifier of the order.</param>
    /// <param name="request">Payload describing renewal parameters.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The identifier of the newly issued certificate.</returns>
    public async Task<int> RenewCertificateAsync(int orderId, RenewCertificateRequest request, CancellationToken cancellationToken = default) {
        if (orderId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(orderId));
        }

        Guard.AgainstNull(request, nameof(request));

        var response = await _client.PostAsync($"v1/order/{orderId}/renew", JsonContent.Create(request, options: s_json), cancellationToken).ConfigureAwait(false);
        var result = await response.Content
            .ReadFromJsonAsyncSafe<RenewCertificateResponse>(s_json, cancellationToken)
            .ConfigureAwait(false);
        return result?.SslId ?? 0;
    }

    /// <summary>
    /// Retrieves the history of an order by identifier.
    /// </summary>
    /// <param name="orderId">Identifier of the order.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<IReadOnlyList<OrderHistoryEntry>> GetHistoryAsync(int orderId, CancellationToken cancellationToken = default) {
        if (orderId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(orderId));
        }

        var response = await _client.GetAsync($"v1/order/{orderId}/history", cancellationToken).ConfigureAwait(false);
        var entries = await response.Content
            .ReadFromJsonAsyncSafe<IReadOnlyList<OrderHistoryEntry>>(s_json, cancellationToken)
            .ConfigureAwait(false);
        return entries ?? Array.Empty<OrderHistoryEntry>();
    }
}