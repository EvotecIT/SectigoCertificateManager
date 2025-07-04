namespace SectigoCertificateManager.Clients;

using System.Net.Http.Json;
using SectigoCertificateManager.Models;
using SectigoCertificateManager.Responses;

/// <summary>
/// Provides access to order related endpoints.
/// </summary>
public sealed class OrdersClient
{
    private readonly ISectigoClient _client;

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
    public async Task<Order?> GetAsync(int orderId, CancellationToken cancellationToken = default)
    {
        var response = await _client.GetAsync($"v1/order/{orderId}", cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Order>(cancellationToken: cancellationToken);
    }
}
