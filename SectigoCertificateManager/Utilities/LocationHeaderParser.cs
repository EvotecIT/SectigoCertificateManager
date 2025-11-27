namespace SectigoCertificateManager.Utilities;

using System;
using System.Net.Http;

/// <summary>
/// Helper for extracting identifiers from <c>Location</c> headers.
/// </summary>
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

