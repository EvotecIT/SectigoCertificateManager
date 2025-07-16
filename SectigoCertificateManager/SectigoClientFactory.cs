namespace SectigoCertificateManager;

using System.Net.Http;

/// <summary>
/// Default implementation of <see cref="ISectigoClientFactory"/>.
/// </summary>
public sealed class SectigoClientFactory : ISectigoClientFactory {
    /// <inheritdoc />
    public SectigoClient Create(ApiConfig config, HttpClient? httpClient = null)
        => new SectigoClient(config, httpClient);
}