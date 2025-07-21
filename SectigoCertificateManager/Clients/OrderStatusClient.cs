namespace SectigoCertificateManager.Clients;

using System.Net.Http.Json;
using System.Text.Json;
using SectigoCertificateManager.Utilities;

/// <summary>
/// Provides access to order status information.
/// </summary>
public sealed class OrderStatusClient : BaseClient {

    /// <summary>
    /// Initializes a new instance of the <see cref="OrderStatusClient"/> class.
    /// </summary>
    /// <param name="client">HTTP client wrapper.</param>
    public OrderStatusClient(ISectigoClient client) : base(client) {
    }

    /// <summary>
    /// Retrieves the status of an order by identifier.
    /// </summary>
    /// <param name="orderId">Identifier of the order.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<OrderStatus?> GetStatusAsync(int orderId, CancellationToken cancellationToken = default) {
        if (orderId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(orderId));
        }

        var response = await _client.GetAsync($"v1/order/{orderId}/status", cancellationToken).ConfigureAwait(false);
        var result = await response.Content
            .ReadFromJsonAsyncSafe<StatusResponse>(s_json, cancellationToken)
            .ConfigureAwait(false);
        return result?.Status;
    }

    /// <summary>
    /// Polls order status until it reaches a terminal value.
    /// </summary>
    /// <param name="orderId">Identifier of the order to watch.</param>
    /// <param name="pollInterval">Delay between status checks.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<OrderStatus?> WatchAsync(
        int orderId,
        TimeSpan pollInterval,
        CancellationToken cancellationToken = default) {
        if (orderId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(orderId));
        }

        OrderStatus? status = await GetStatusAsync(orderId, cancellationToken).ConfigureAwait(false);
        while (status is OrderStatus.NotInitiated or OrderStatus.Submitted) {
            if (pollInterval > TimeSpan.Zero) {
                await Task.Delay(pollInterval, cancellationToken).ConfigureAwait(false);
            } else {
                await Task.Yield();
            }

            status = await GetStatusAsync(orderId, cancellationToken).ConfigureAwait(false);
        }

        return status;
    }

    private sealed class StatusResponse {
        /// <summary>Gets or sets the order status.</summary>
        public OrderStatus Status { get; set; }
    }
}