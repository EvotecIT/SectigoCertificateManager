namespace SectigoCertificateManager.Clients;

using SectigoCertificateManager.Models;
using SectigoCertificateManager.Responses;
using System.Net.Http.Json;
using System.Text.Json;

/// <summary>
/// Provides access to order related endpoints.
/// </summary>
public sealed class OrdersClient {
    private readonly ISectigoClient _client;
    private static readonly JsonSerializerOptions s_json = new() {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="OrdersClient"/> class.
    /// </summary>
    /// <param name="client">HTTP client wrapper.</param>
    public OrdersClient(ISectigoClient client) => _client = client;

    /// <summary>
    /// Retrieves an order by identifier.
    /// </summary>
    /// <param name="orderId">Identifier of the order to retrieve.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<Order?> GetAsync(int orderId, CancellationToken cancellationToken = default) {
        var response = await _client.GetAsync($"v1/order/{orderId}", cancellationToken).ConfigureAwait(false);
        return await response.Content.ReadFromJsonAsync<Order>(s_json, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Retrieves all orders visible to the user.
    /// </summary>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<IReadOnlyList<Order>?> ListOrdersAsync(CancellationToken cancellationToken = default) {
        var response = await _client.GetAsync("v1/order", cancellationToken).ConfigureAwait(false);
        return await response.Content.ReadFromJsonAsync<IReadOnlyList<Order>>(s_json, cancellationToken).ConfigureAwait(false);
    }
}