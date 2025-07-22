using SectigoCertificateManager;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SectigoCertificateManager.Tests;

public sealed class ApiConfigBuilderConcurrencyTests {
    [Fact]
    public async Task ConcurrentBuildsAreThreadSafe() {
        var builder = new ApiConfigBuilder()
            .WithBaseUrl("https://example.com")
            .WithCredentials("user", "pass")
            .WithCustomerUri("cst1");

        var tasks = Enumerable.Range(0, 10)
            .Select(i => Task.Run(() => {
                builder.WithToken($"tok{i}");
                builder.WithTokenExpiration(DateTimeOffset.UtcNow.AddMinutes(i));
                return builder.Build();
            }));

        var configs = await Task.WhenAll(tasks);
        foreach (var cfg in configs) {
            Assert.Equal("https://example.com", cfg.BaseUrl);
            Assert.Equal("cst1", cfg.CustomerUri);
        }
    }
}
