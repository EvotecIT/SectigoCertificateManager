namespace SectigoCertificateManager;

using System.Net.Http;

/// <summary>
/// Defines a factory for creating instances of <see cref="ISectigoClient"/>.
/// </summary>
public interface ISectigoClientFactory {
    /// <summary>
    /// Creates a new <see cref="ISectigoClient"/> instance.
    /// </summary>
    /// <param name="config">API configuration settings.</param>
    /// <param name="httpClient">Optional pre-configured HTTP client.</param>
    SectigoClient Create(ApiConfig config, HttpClient? httpClient = null);
}