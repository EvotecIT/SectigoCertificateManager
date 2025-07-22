namespace SectigoCertificateManager.Clients;

using System;
using System.Text.Json;
using SectigoCertificateManager.Utilities;

/// <summary>
/// Base class for API clients.
/// </summary>
public abstract class BaseClient {
    /// <summary>
    /// HTTP client wrapper used for requests.
    /// </summary>
    protected readonly ISectigoClient _client;

    /// <summary>
    /// JSON options with camel case naming policy.
    /// </summary>
    protected static readonly JsonSerializerOptions s_json = new() {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    static BaseClient() {
        s_json.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseClient"/> class.
    /// </summary>
    /// <param name="client">HTTP client wrapper.</param>
    protected BaseClient(ISectigoClient client) {
        _client = Guard.AgainstNull(client, nameof(client));
    }
}
