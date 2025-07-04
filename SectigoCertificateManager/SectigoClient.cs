namespace SectigoCertificateManager;

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Concurrent;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Provides a basic HTTP client wrapper for communicating with the Sectigo API.
/// </summary>
public sealed class SectigoClient : ISectigoClient
{
    private readonly HttpClient _client;
    private readonly ConcurrentDictionary<string, CacheEntry> _cache = new();
    private readonly TimeSpan _cacheExpiration;

    private sealed record CacheEntry(HttpResponseMessage Response, DateTimeOffset Expires);

    /// <summary>
    /// Gets the underlying <see cref="HttpClient"/> instance used for requests.
    /// </summary>
    public HttpClient HttpClient => _client;

    /// <summary>
    /// Initializes a new instance of the <see cref="SectigoClient"/> class.
    /// </summary>
    /// <param name="config">API configuration settings.</param>
    /// <param name="httpClient">Optional pre-configured HTTP client.</param>
    public SectigoClient(ApiConfig config, HttpClient? httpClient = null)
    {
        if (httpClient is null)
        {
            var handler = new HttpClientHandler();
            if (config.ClientCertificate is not null)
            {
                handler.ClientCertificates.Add(config.ClientCertificate);
            }

            config.ConfigureHandler?.Invoke(handler);
            httpClient = new HttpClient(handler);
        }

        _client = httpClient;
        _client.BaseAddress = new Uri(config.BaseUrl);
        _cacheExpiration = config.CacheExpiration;
        ConfigureHeaders(config);
    }

    /// <summary>
    /// Sends a GET request to the specified endpoint.
    /// </summary>
    /// <param name="requestUri">Relative request URI.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<HttpResponseMessage> GetAsync(string requestUri, CancellationToken cancellationToken = default)
    {
        if (_cacheExpiration > TimeSpan.Zero
            && _cache.TryGetValue(requestUri, out var cached)
            && cached.Expires > DateTimeOffset.UtcNow)
        {
            return CloneResponse(cached.Response);
        }

        var network = await _client.GetAsync(requestUri, cancellationToken).ConfigureAwait(false);
        var bytes = await network.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
        var temp = CreateResponse(network, bytes);
        await ApiErrorHandler.ThrowIfErrorAsync(temp).ConfigureAwait(false);

        var store = CreateResponse(network, bytes);
        if (_cacheExpiration > TimeSpan.Zero)
        {
            _cache[requestUri] = new CacheEntry(store, DateTimeOffset.UtcNow.Add(_cacheExpiration));
        }

        return CreateResponse(network, bytes);
    }

    /// <summary>
    /// Sends a POST request with the provided content.
    /// </summary>
    /// <param name="requestUri">Relative request URI.</param>
    /// <param name="content">HTTP content to send.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content, CancellationToken cancellationToken = default)
    {
        var response = await _client.PostAsync(requestUri, content, cancellationToken).ConfigureAwait(false);
        await ApiErrorHandler.ThrowIfErrorAsync(response).ConfigureAwait(false);
        return response;
    }

    /// <summary>
    /// Sends a PUT request with the provided content.
    /// </summary>
    /// <param name="requestUri">Relative request URI.</param>
    /// <param name="content">HTTP content to send.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<HttpResponseMessage> PutAsync(string requestUri, HttpContent content, CancellationToken cancellationToken = default)
    {
        var response = await _client.PutAsync(requestUri, content, cancellationToken).ConfigureAwait(false);
        await ApiErrorHandler.ThrowIfErrorAsync(response).ConfigureAwait(false);
        return response;
    }

    /// <summary>
    /// Sends a DELETE request to the specified endpoint.
    /// </summary>
    /// <param name="requestUri">Relative request URI.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<HttpResponseMessage> DeleteAsync(string requestUri, CancellationToken cancellationToken = default)
    {
        var response = await _client.DeleteAsync(requestUri, cancellationToken).ConfigureAwait(false);
        await ApiErrorHandler.ThrowIfErrorAsync(response).ConfigureAwait(false);
        return response;
    }

    private void ConfigureHeaders(ApiConfig cfg)
    {
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _client.DefaultRequestHeaders.Add("login", cfg.Username);
        _client.DefaultRequestHeaders.Add("password", cfg.Password);
        _client.DefaultRequestHeaders.Add("customerUri", cfg.CustomerUri);
    }

    private static HttpResponseMessage CreateResponse(HttpResponseMessage source, byte[] bytes)
    {
        var copy = new HttpResponseMessage(source.StatusCode)
        {
            ReasonPhrase = source.ReasonPhrase,
            Version = source.Version,
            RequestMessage = source.RequestMessage,
            Content = new ByteArrayContent(bytes)
        };

        foreach (var header in source.Headers)
        {
            copy.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        foreach (var header in source.Content.Headers)
        {
            copy.Content!.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        return copy;
    }

    private static HttpResponseMessage CloneResponse(HttpResponseMessage source)
    {
        var bytes = source.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();
        return CreateResponse(source, bytes);
    }
}
