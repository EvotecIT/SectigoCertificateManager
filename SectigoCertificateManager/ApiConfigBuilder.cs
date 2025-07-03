using System.Net.Http;

namespace SectigoCertificateManager;



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
    private Action<HttpClientHandler>? _configureHandler;



    /// <summary>

    /// Sets the base URL for the API endpoint.

    /// </summary>

    /// <param name="baseUrl">Base URL of the API.</param>

    /// <returns>The builder instance.</returns>

    public ApiConfigBuilder WithBaseUrl(string baseUrl)

    {

        _baseUrl = baseUrl;

        return this;

    }



    /// <summary>

    /// Sets the credentials used for authentication.

    /// </summary>

    /// <param name="username">User name.</param>

    /// <param name="password">Password.</param>

    /// <returns>The builder instance.</returns>

    public ApiConfigBuilder WithCredentials(string username, string password)

    {

        _username = username;

        _password = password;

        return this;

    }



    /// <summary>

    /// Sets the customer URI header value.

    /// </summary>

    /// <param name="customerUri">Customer URI value.</param>

    /// <returns>The builder instance.</returns>

    public ApiConfigBuilder WithCustomerUri(string customerUri)

    {

        _customerUri = customerUri;

        return this;

    }



    /// <summary>

    /// Sets the API version.

    /// </summary>

    /// <param name="version">Version of the API.</param>

    /// <returns>The builder instance.</returns>

    public ApiConfigBuilder WithApiVersion(ApiVersion version)

    {

        _apiVersion = version;

        return this;

    }

    /// <summary>
    /// Sets a callback used to configure the <see cref="HttpClientHandler"/> created for HTTP communication.
    /// </summary>
    /// <param name="configure">Callback that configures the handler.</param>
    /// <returns>The builder instance.</returns>
    public ApiConfigBuilder WithHandlerConfiguration(Action<HttpClientHandler> configure)
    {
        _configureHandler = configure;
        return this;
    }



    /// <summary>

    /// Builds a new <see cref="ApiConfig"/> instance using configured values.

    /// </summary>

    /// <returns>An <see cref="ApiConfig"/>.</returns>

    public ApiConfig Build()

        => new ApiConfig(_baseUrl, _username, _password, _customerUri, _apiVersion, _configureHandler);

}

