using SectigoCertificateManager.Utilities;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace SectigoCertificateManager.Tests;

/// <summary>
/// Tests for <see cref="ProgressStreamContent"/>.
/// </summary>
public sealed class ProgressStreamContentTests {
    [Fact]
    public async Task ReadAsByteArrayAsync_UsesRemainingSeekableStreamLength() {
        using var stream = new MemoryStream(new byte[] { 1, 2, 3, 4 });
        stream.Position = 2;
        using var content = new ProgressStreamContent(stream);

        byte[] uploaded = await content.ReadAsByteArrayAsync();

        Assert.Equal(2, content.Headers.ContentLength);
        Assert.Equal(new byte[] { 3, 4 }, uploaded);
    }

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
