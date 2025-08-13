using SectigoCertificateManager.Requests;
using System;
using Xunit;

namespace SectigoCertificateManager.Tests;

/// <summary>
/// Tests for <see cref="IssueCertificateRequest"/> and <see cref="IssueCertificateRequestBuilder"/>.
/// </summary>
public sealed class IssueCertificateRequestTests {
    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    public void TermSetter_InvalidValue_Throws(int term) {
        var request = new IssueCertificateRequest();
        Assert.Throws<ArgumentOutOfRangeException>(() => request.Term = term);
    }

    [Fact]
    public void Builder_ThrowsForInvalidTerm() {
        var builder = new IssueCertificateRequestBuilder(new[] { 12, 24 });
        Assert.Throws<ArgumentOutOfRangeException>(() => builder.WithTerm(36));
    }

    [Fact]
    public void Builder_BuildsRequest() {
        var builder = new IssueCertificateRequestBuilder(new[] { 12, 24 });
        var request = builder
            .WithCommonName("example.com")
            .WithProfileId(5)
            .WithTerm(12)
            .Build();

        Assert.Equal("example.com", request.CommonName);
        Assert.Equal(5, request.ProfileId);
        Assert.Equal(12, request.Term);
    }

    [Fact]
    public void Builder_WithSubjectAlternativeNames_CollapsesDuplicates() {
        var builder = new IssueCertificateRequestBuilder(new[] { 12 });
        var request = builder
            .WithSubjectAlternativeNames(new[] { "a.example.com", "a.example.com", "b.example.com" })
            .WithTerm(12)
            .Build();

        Assert.Equal(2, request.SubjectAlternativeNames.Count);
        Assert.Contains("a.example.com", request.SubjectAlternativeNames);
        Assert.Contains("b.example.com", request.SubjectAlternativeNames);
    }
}