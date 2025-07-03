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

