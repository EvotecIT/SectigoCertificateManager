using SectigoCertificateManager.Requests;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SectigoCertificateManager.Tests;

public sealed class IssueCertificateRequestBuilderConcurrencyTests {
    [Fact]
    public async Task ConcurrentBuildsAreThreadSafe() {
        var builder = new IssueCertificateRequestBuilder(new[] { 12, 24 });

        var tasks = Enumerable.Range(0, 10)
            .Select(i => Task.Run(() => {
                builder.WithCommonName($"example{i}.com");
                builder.WithProfileId(i);
                builder.WithTerm(12);
                builder.WithSubjectAlternativeNames(new[] { $"alt{i}.com" });
                return builder.Build();
            }));

        var requests = await Task.WhenAll(tasks);
        foreach (var req in requests) {
            Assert.Equal(12, req.Term);
            Assert.Single(req.SubjectAlternativeNames);
        }
    }
}
