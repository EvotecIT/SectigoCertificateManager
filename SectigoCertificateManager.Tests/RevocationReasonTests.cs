using SectigoCertificateManager;
using Xunit;

namespace SectigoCertificateManager.Tests;

public sealed class RevocationReasonTests {
    [Theory]
    [InlineData(RevocationReason.Unspecified, "unspecified")]
    [InlineData(RevocationReason.KeyCompromise, "keyCompromise")]
    [InlineData(RevocationReason.AffiliationChanged, "affiliationChanged")]
    [InlineData(RevocationReason.Superseded, "superseded")]
    [InlineData(RevocationReason.CessationOfOperation, "cessationOfOperation")]
    public void GetRevocationReasonDescription_ReturnsExpected(RevocationReason code, string expected) {
        var result = RevocationReasons.GetRevocationReasonDescription(code);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetRevocationReasonDescription_Unknown_ReturnsNull() {
        var result = RevocationReasons.GetRevocationReasonDescription((RevocationReason)99);
        Assert.Null(result);
    }
}
