namespace SectigoCertificateManager;

using System;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;

/// <summary>
/// Provides a builder for creating instances of <see cref="ApiConfig"/> using a fluent API.
/// </summary>
public sealed class ApiConfigBuilder
{
    private string _baseUrl = string.Empty;
    private string _username = string.Empty;
    private string _password = string.Empty;
    private string _customerUri = string.Empty;
    private ApiVersion _apiVersion = ApiVersion.V25_4;
    private X509Certificate2? _clientCertificate;
    private Action<HttpClientHandler>? _configureHandler;

    /// <summary>Sets the base URL for the API endpoint.</summary>
    public ApiConfigBuilder WithBaseUrl(string baseUrl)
    {
        _baseUrl = baseUrl;
        return this;
    }

    /// <summary>Sets the credentials used for authentication.</summary>
    public ApiConfigBuilder WithCredentials(string username, string password)
    {
        _username = username;
        _password = password;
        return this;
    }

    /// <summary>Sets the customer URI header value.</summary>
    public ApiConfigBuilder WithCustomerUri(string customerUri)
    {
        _customerUri = customerUri;
        return this;
    }

    /// <summary>Sets the API version.</summary>
    public ApiConfigBuilder WithApiVersion(ApiVersion version)
    {
        _apiVersion = version;
        return this;
    }

    /// <summary>Attaches a client certificate for mutual TLS authentication.</summary>
    public ApiConfigBuilder WithClientCertificate(X509Certificate2 certificate)
    {
        _clientCertificate = certificate;
        return this;
    }

    /// <summary>Allows configuration of the <see cref="HttpClientHandler"/> used by <see cref="SectigoClient"/>.</summary>
    public ApiConfigBuilder WithHttpClientHandler(Action<HttpClientHandler> configure)
    {
        _configureHandler = configure;
        return this;
    }

    /// <summary>Builds a new <see cref="ApiConfig"/> instance using configured values.</summary>
    public ApiConfig Build()
        => new ApiConfig(_baseUrl, _username, _password, _customerUri, _apiVersion, _clientCertificate, _configureHandler);
}
