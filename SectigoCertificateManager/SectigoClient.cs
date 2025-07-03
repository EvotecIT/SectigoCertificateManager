namespace SectigoCertificateManager;

using System;
using System.Net.Http;
using System.Net.Http.Headers;
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
            config.ConfigureHandler?.Invoke(handler);
            httpClient = new HttpClient(handler, disposeHandler: true);
        }

        _client = httpClient;
        _client.BaseAddress = new Uri(config.BaseUrl);
        ConfigureHeaders(config);
    }

    public Task<HttpResponseMessage> GetAsync(string requestUri, CancellationToken cancellationToken = default)
        => _client.GetAsync(requestUri, cancellationToken);

    public Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content, CancellationToken cancellationToken = default)
        => _client.PostAsync(requestUri, content, cancellationToken);

    public Task<HttpResponseMessage> PutAsync(string requestUri, HttpContent content, CancellationToken cancellationToken = default)
        => _client.PutAsync(requestUri, content, cancellationToken);

    public Task<HttpResponseMessage> DeleteAsync(string requestUri, CancellationToken cancellationToken = default)
        => _client.DeleteAsync(requestUri, cancellationToken);

    private void ConfigureHeaders(ApiConfig cfg)
    {
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _client.DefaultRequestHeaders.Add("login", cfg.Username);
        _client.DefaultRequestHeaders.Add("password", cfg.Password);
        _client.DefaultRequestHeaders.Add("customerUri", cfg.CustomerUri);
    }
}
