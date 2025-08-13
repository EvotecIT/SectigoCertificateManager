using SectigoCertificateManager.Utilities;
using System;
using System.IO;
using Xunit;

namespace SectigoCertificateManager.Tests;

/// <summary>
/// Tests for <see cref="ProgressStreamContent"/>.
/// </summary>
public sealed class ProgressStreamContentTests {
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_InvalidBufferSize_Throws(int bufferSize) {
        using var stream = new MemoryStream(new byte[1]);
        var ex = Assert.Throws<ArgumentOutOfRangeException>(
            () => new ProgressStreamContent(stream, progress: null, bufferSize: bufferSize));
        Assert.Equal("bufferSize", ex.ParamName);
    }
}
