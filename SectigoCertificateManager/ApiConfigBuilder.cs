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
    /// <param name="baseUrl">The root URL of the Sectigo API.</param>
    public ApiConfigBuilder WithBaseUrl(string baseUrl)
    {
        _baseUrl = baseUrl;
        return this;
    }

    /// <summary>Sets the credentials used for authentication.</summary>
    /// <param name="username">User name for API authentication.</param>
    /// <param name="password">Password associated with <paramref name="username"/>.</param>
    public ApiConfigBuilder WithCredentials(string username, string password)
    {
        _username = username;
        _password = password;
        return this;
    }

    /// <summary>Sets the customer URI header value.</summary>
    /// <param name="customerUri">Value of the <c>customerUri</c> header.</param>
    public ApiConfigBuilder WithCustomerUri(string customerUri)
    {
        _customerUri = customerUri;
        return this;
    }

    /// <summary>Sets the API version.</summary>
    /// <param name="version">Desired API version.</param>
    public ApiConfigBuilder WithApiVersion(ApiVersion version)
    {
        _apiVersion = version;
        return this;
    }

    /// <summary>Attaches a client certificate for mutual TLS authentication.</summary>
    /// <param name="certificate">The certificate used for client authentication.</param>
    public ApiConfigBuilder WithClientCertificate(X509Certificate2 certificate)
    {
        _clientCertificate = certificate;
        return this;
    }

    /// <summary>Allows configuration of the <see cref="HttpClientHandler"/> used by <see cref="SectigoClient"/>.</summary>
    /// <param name="configure">Delegate used to configure the handler.</param>
    public ApiConfigBuilder WithHttpClientHandler(Action<HttpClientHandler> configure)
    {
        _configureHandler = configure;
        return this;
    }

    /// <summary>Builds a new <see cref="ApiConfig"/> instance using configured values.</summary>
    public ApiConfig Build()
        => new ApiConfig(_baseUrl, _username, _password, _customerUri, _apiVersion, _clientCertificate, _configureHandler);
}
