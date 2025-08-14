namespace SectigoCertificateManager;

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Defines a minimal wrapper around <see cref="HttpClient"/> used by this library.
/// </summary>
public interface ISectigoClient {
    /// <summary>
    /// Gets the underlying <see cref="HttpClient"/> instance.
    /// </summary>
    HttpClient HttpClient { get; }
    /// <summary>
    /// Gets a value indicating whether certificate download caching is enabled.
    /// </summary>
    bool EnableDownloadCache { get; }
    /// <summary>
    /// Sends a GET request.
    /// </summary>
    /// <param name="requestUri">Relative request URI.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    Task<HttpResponseMessage> GetAsync(string requestUri, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a POST request.
    /// </summary>
    /// <param name="requestUri">Relative request URI.</param>
    /// <param name="content">HTTP content to send.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a PUT request.
    /// </summary>
    /// <param name="requestUri">Relative request URI.</param>
    /// <param name="content">HTTP content to send.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    Task<HttpResponseMessage> PutAsync(string requestUri, HttpContent content, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a DELETE request.
    /// </summary>
    /// <param name="requestUri">Relative request URI.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    Task<HttpResponseMessage> DeleteAsync(string requestUri, CancellationToken cancellationToken = default);
}