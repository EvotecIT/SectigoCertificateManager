using SectigoCertificateManager;
using Xunit;

namespace SectigoCertificateManager.Tests;

public sealed class RevocationReasonTests {
    [Theory]
    [InlineData(0, "unspecified")]
    [InlineData(1, "keyCompromise")]
    [InlineData(3, "affiliationChanged")]
    [InlineData(4, "superseded")]
    [InlineData(5, "cessationOfOperation")]
    public void GetRevocationReasonDescription_ReturnsExpected(int code, string expected) {
        var result = RevocationReasons.GetRevocationReasonDescription(code);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetRevocationReasonDescription_Unknown_ReturnsNull() {
        var result = RevocationReasons.GetRevocationReasonDescription(99);
        Assert.Null(result);
    }
}
