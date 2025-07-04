namespace SectigoCertificateManager;

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Provides a basic HTTP client wrapper for communicating with the Sectigo API.
/// </summary>
public sealed class SectigoClient : ISectigoClient {
    private readonly HttpClient _client;

    /// <summary>
    /// Gets the underlying <see cref="HttpClient"/> instance used for requests.
    /// </summary>
    public HttpClient HttpClient => _client;

    /// <summary>
    /// Initializes a new instance of the <see cref="SectigoClient"/> class.
    /// </summary>
    /// <param name="config">API configuration settings.</param>
    /// <param name="httpClient">Optional pre-configured HTTP client.</param>
    public SectigoClient(ApiConfig config, HttpClient? httpClient = null) {
        if (httpClient is null) {
            var handler = new HttpClientHandler();
            if (config.ClientCertificate is not null) {
                handler.ClientCertificates.Add(config.ClientCertificate);
            }

            config.ConfigureHandler?.Invoke(handler);
            httpClient = new HttpClient(handler);
        }

        _client = httpClient;
        _client.BaseAddress = new Uri(config.BaseUrl);
        ConfigureHeaders(config);
    }

    /// <summary>
    /// Sends a GET request to the specified endpoint.
    /// </summary>
    /// <param name="requestUri">Relative request URI.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<HttpResponseMessage> GetAsync(string requestUri, CancellationToken cancellationToken = default) {
        var response = await _client.GetAsync(requestUri, cancellationToken).ConfigureAwait(false);
        await ApiErrorHandler.ThrowIfErrorAsync(response).ConfigureAwait(false);
        return response;
    }

    /// <summary>
    /// Sends a POST request with the provided content.
    /// </summary>
    /// <param name="requestUri">Relative request URI.</param>
    /// <param name="content">HTTP content to send.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content, CancellationToken cancellationToken = default) {
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
    public async Task<HttpResponseMessage> PutAsync(string requestUri, HttpContent content, CancellationToken cancellationToken = default) {
        var response = await _client.PutAsync(requestUri, content, cancellationToken).ConfigureAwait(false);
        await ApiErrorHandler.ThrowIfErrorAsync(response).ConfigureAwait(false);
        return response;
    }

    /// <summary>
    /// Sends a DELETE request to the specified endpoint.
    /// </summary>
    /// <param name="requestUri">Relative request URI.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<HttpResponseMessage> DeleteAsync(string requestUri, CancellationToken cancellationToken = default) {
        var response = await _client.DeleteAsync(requestUri, cancellationToken).ConfigureAwait(false);
        await ApiErrorHandler.ThrowIfErrorAsync(response).ConfigureAwait(false);
        return response;
    }

    private void ConfigureHeaders(ApiConfig cfg) {
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _client.DefaultRequestHeaders.Add("customerUri", cfg.CustomerUri);

        if (!string.IsNullOrWhiteSpace(cfg.Token)) {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", cfg.Token);
        } else {
            _client.DefaultRequestHeaders.Add("login", cfg.Username);
            _client.DefaultRequestHeaders.Add("password", cfg.Password);
        }
    }
}