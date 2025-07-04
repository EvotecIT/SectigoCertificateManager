namespace SectigoCertificateManager.Clients;

using System.Net.Http.Json;
using System.Text.Json;

/// <summary>
/// Provides access to order status information.
/// </summary>
public sealed class OrderStatusClient {
    private readonly ISectigoClient _client;
    private static readonly JsonSerializerOptions s_json = new() {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="OrderStatusClient"/> class.
    /// </summary>
    /// <param name="client">HTTP client wrapper.</param>
    public OrderStatusClient(ISectigoClient client) => _client = client;

    /// <summary>
    /// Retrieves the status of an order by identifier.
    /// </summary>
    /// <param name="orderId">Identifier of the order.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<OrderStatus?> GetStatusAsync(int orderId, CancellationToken cancellationToken = default) {
        var response = await _client.GetAsync($"v1/order/{orderId}/status", cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<StatusResponse>(s_json, cancellationToken).ConfigureAwait(false);
        return result?.Status;
    }

    private sealed class StatusResponse {
        public OrderStatus Status { get; set; }
    }
}
