using Xunit;

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
    public void CertificateStatus_ValuesMatchSpec() {
        Assert.Equal(0, (int)CertificateStatus.Any);
        Assert.Equal(1, (int)CertificateStatus.Requested);
        Assert.Equal(8, (int)CertificateStatus.Approved);
        Assert.Equal(9, (int)CertificateStatus.Applied);
        Assert.Equal(2, (int)CertificateStatus.Issued);
        Assert.Equal(3, (int)CertificateStatus.Declined);
        Assert.Equal(4, (int)CertificateStatus.Downloaded);
        Assert.Equal(5, (int)CertificateStatus.Rejected);
        Assert.Equal(6, (int)CertificateStatus.AwaitingApproval);
        Assert.Equal(7, (int)CertificateStatus.Invalid);
        Assert.Equal(8, (int)CertificateStatus.Replaced);
        Assert.Equal(9, (int)CertificateStatus.Unmanaged);
        Assert.Equal(10, (int)CertificateStatus.SAApproved);
        Assert.Equal(11, (int)CertificateStatus.Init);
        Assert.Equal(3, (int)CertificateStatus.Revoked);
        Assert.Equal(4, (int)CertificateStatus.Expired);
    }
}
