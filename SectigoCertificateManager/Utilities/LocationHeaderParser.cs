namespace SectigoCertificateManager.Utilities;

using System;
using System.Net.Http;

/// <summary>
/// Helper for extracting identifiers from <c>Location</c> headers.
/// </summary>
/// <remarks>
/// When the <c>Location</c> header is missing, empty, or the last
/// path segment cannot be parsed as an integer, this helper returns
/// <c>0</c>. Callers should treat <c>0</c> as "no identifier found".
/// </remarks>
internal static class LocationHeaderParser {
    public static int ParseId(HttpResponseMessage response) {
        if (response is null) {
            throw new ArgumentNullException(nameof(response));
        }

        var location = response.Headers.Location;
        if (location is null) {
            return 0;
        }

        var url = location.ToString().Trim().TrimEnd('/');
        if (url.Length == 0) {
            return 0;
        }

        var segments = url.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length == 0) {
            return 0;
        }

        var lastSegment = segments[segments.Length - 1];
        return int.TryParse(lastSegment, out var id) ? id : 0;
    }
}
