# SectigoCertificateManager



This library provides a simple client for the Sectigo Certificate Manager API.



## Fluent API



Create an `ApiConfig` using the fluent builder:



```csharp

var config = new ApiConfigBuilder()

    .WithBaseUrl("https://example.com")

    .WithCredentials("user", "pass")

    .WithCustomerUri("cst1")

    .WithApiVersion(ApiVersion.V25_5)

    .Build();

```



Use the resulting `ApiConfig` to instantiate `SectigoClient`.


### Customizing the HTTP handler

If you need to supply client certificates or modify other `HttpClientHandler` settings, configure a handler callback:

```csharp
var config = new ApiConfigBuilder()
    .WithBaseUrl("https://example.com")
    .WithCredentials("user", "pass")
    .WithCustomerUri("cst1")
    .WithHandlerConfiguration(handler =>
    {
        handler.ClientCertificates.Add(myCertificate);
        handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
    })
    .Build();
```

This callback is invoked when `SectigoClient` creates its own `HttpClient` and allows any handler property to be customized.
