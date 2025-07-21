using SectigoCertificateManager.Utilities;
using System;
using Xunit;

namespace SectigoCertificateManager.Tests;

/// <summary>
/// Tests for <see cref="Guard"/> helpers.
/// </summary>
public sealed class GuardTests {
    [Fact]
    public void AgainstNull_ThrowsForNull() {
        var ex = Assert.Throws<ArgumentNullException>(() => Guard.AgainstNull<string>(null, "value"));
        Assert.Equal("value", ex.ParamName);
    }

    [Fact]
    public void AgainstNull_ReturnsValue() {
        var result = Guard.AgainstNull("test", "value");
        Assert.Equal("test", result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void AgainstNullOrEmpty_Throws(string? input) {
        Assert.Throws<ArgumentException>(() => Guard.AgainstNullOrEmpty(input, "value"));
    }

    [Fact]
    public void AgainstNullOrEmpty_ReturnsValue() {
        var result = Guard.AgainstNullOrEmpty("ok", "value");
        Assert.Equal("ok", result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" \t\n")]
    public void AgainstNullOrWhiteSpace_Throws(string? input) {
        Assert.Throws<ArgumentException>(() => Guard.AgainstNullOrWhiteSpace(input, "value"));
    }

    [Fact]
    public void AgainstNullOrWhiteSpace_ReturnsValue() {
        var result = Guard.AgainstNullOrWhiteSpace("good", "value");
        Assert.Equal("good", result);
    }
}
