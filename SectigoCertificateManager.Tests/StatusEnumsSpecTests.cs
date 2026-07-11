using Xunit;
using System.Text.Json;

namespace SectigoCertificateManager.Tests;

/// <summary>
/// Validates enum values against specification constants.
/// </summary>
public sealed class StatusEnumsSpecTests {
    [Fact]
    public void OrderStatus_ValuesMatchSpec() {
        Assert.Equal(0, (int)OrderStatus.NotInitiated);
        Assert.Equal(1, (int)OrderStatus.Submitted);
        Assert.Equal(2, (int)OrderStatus.Completed);
        Assert.Equal(3, (int)OrderStatus.Cancelled);
    }

    [Fact]
    public void CertificateStatus_ValuesAreUnique() {
        var values = Enum.GetValues(typeof(CertificateStatus)).Cast<CertificateStatus>().Select(static status => (int)status).ToArray();

        Assert.Equal(values.Length, values.Distinct().Count());
    }

    [Theory]
    [InlineData(0, CertificateStatus.Any)]
    [InlineData(1, CertificateStatus.Requested)]
    [InlineData(2, CertificateStatus.Issued)]
    [InlineData(3, CertificateStatus.Revoked)]
    [InlineData(4, CertificateStatus.Expired)]
    [InlineData(5, CertificateStatus.EnrolledPendingDownload)]
    [InlineData(6, CertificateStatus.NotEnrolled)]
    [InlineData(7, CertificateStatus.AwaitingApproval)]
    [InlineData(8, CertificateStatus.Approved)]
    [InlineData(9, CertificateStatus.Applied)]
    [InlineData(10, CertificateStatus.Downloaded)]
    [InlineData(11, CertificateStatus.External)]
    public void CertificateStatus_LegacyNumericWireCodesRemainStable(int code, CertificateStatus expected) {
        Assert.Equal(expected, JsonSerializer.Deserialize<CertificateStatus>(code.ToString()));
        Assert.Equal(expected, JsonSerializer.Deserialize<CertificateStatus>($"\"{code}\""));
    }
}
