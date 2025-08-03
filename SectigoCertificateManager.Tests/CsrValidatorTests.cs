using SectigoCertificateManager.Utilities;
using Xunit;

namespace SectigoCertificateManager.Tests;

/// <summary>
/// Tests for <see cref="CsrValidator"/> helpers.
/// </summary>
public sealed class CsrValidatorTests {
    private const string ValidCsr = "MIICVDCCATwCAQAwDzENMAsGA1UEAwwEVGVzdDCCASIwDQYJKoZIhvcNAQEBBQADggEPADCCAQoCggEBAMwJzo64p670+fpPa/aEdbFBZJj1BBhOwhw7hHYrPp64mriZFjRfd7mIHaXpoXJ1ZqZm2wjovwh/ZSKv25LD8FaN83RrM175fAl1h3VAs31UE0yl56AUjs2mpMZXiU8E65wmfTdTuy6MhcDziGVmniasL9FC6gt05j2dSaNCZjTEUhP3Nv7abWU1eDuMZ0QdDqrMZXmiS6FaOIZfW4zg0X+oHLFoiy8hF2wa0yg5OcwxtTcrmQhCQxn3GxkZUQFZavhJUGydCFrtOyFULUCZHnDvfCyxZ8duwQCS/2ilOar1SrZ7AGcypV0yXH19LX/WjORdHb7CagDsPCzMQjsvhAECAwEAAaAAMA0GCSqGSIb3DQEBCwUAA4IBAQBWX3mWclTQvnak1cb2LR56QLsTCr9BmRLv9OatWcYG7P7aAJnpIJn57EJj16yBcukvQhmNz6TMMMnWPe6PeUmxB5FIvN7xhcvwVqGGB37SS6GBJOOQb70OAmuwe3plHMNR7Wk6bb/9S2+NYA8KNNXKymGSFhBFDSjC1cU3n7dqE5Smx1Gt2MMcNhidPeJWuxYUugEBtNglqO3sjRRVPV1Ybj1egVSbqxMw2bsGQBAsdSmPTkD0T61nkpVotexRmnts/8D30t744FGJeW1GoCREsy3/c9XJLQPkfJVKWCmLXfKo7p8HjgvbQcDqqmH3yv9vF97dDKAStz/mxxVzsEpr";
    private const string InvalidCsr = "MIICVDCCATwCAQAwDzENMAsGA1UEAwwEVGVzdA==";

    /// <summary>Returns true for valid CSR.</summary>
    [Fact]
    public void IsValid_ReturnsTrue_ForValidCsr() {
        Assert.True(CsrValidator.IsValid(ValidCsr));
    }

    /// <summary>Returns false for invalid CSR.</summary>
    [Fact]
    public void IsValid_ReturnsFalse_ForInvalidCsr() {
        Assert.False(CsrValidator.IsValid(InvalidCsr));
    }
}
