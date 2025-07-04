namespace SectigoCertificateManager;

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

public sealed class SectigoClient : ISectigoClient
{
    private readonly HttpClient _client;

    public HttpClient HttpClient => _client;

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
        ConfigureHeaders(config);
    }

    public async Task<HttpResponseMessage> GetAsync(string requestUri, CancellationToken cancellationToken = default)
    {
        var response = await _client.GetAsync(requestUri, cancellationToken).ConfigureAwait(false);
        await ApiErrorHandler.ThrowIfErrorAsync(response).ConfigureAwait(false);
        return response;
    }

    public async Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content, CancellationToken cancellationToken = default)
    {
        var response = await _client.PostAsync(requestUri, content, cancellationToken).ConfigureAwait(false);
        await ApiErrorHandler.ThrowIfErrorAsync(response).ConfigureAwait(false);
        return response;
    }

    public async Task<HttpResponseMessage> PutAsync(string requestUri, HttpContent content, CancellationToken cancellationToken = default)
    {
        var response = await _client.PutAsync(requestUri, content, cancellationToken).ConfigureAwait(false);
        await ApiErrorHandler.ThrowIfErrorAsync(response).ConfigureAwait(false);
        return response;
    }

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
}
