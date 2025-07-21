namespace SectigoCertificateManager.Clients;

using SectigoCertificateManager.Models;
using SectigoCertificateManager.Requests;
using SectigoCertificateManager.Responses;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using SectigoCertificateManager.Utilities;

public sealed partial class CertificatesClient : BaseClient {
    /// <summary>
    /// Searches for certificates using the provided filter.
    /// </summary>
    public async Task<CertificateResponse?> SearchAsync(
        CertificateSearchRequest request,
        CancellationToken cancellationToken = default) {
        Guard.AgainstNull(request, nameof(request));

        var list = new List<Certificate>();
        await foreach (var certificate in EnumerateSearchAsync(request, cancellationToken: cancellationToken).ConfigureAwait(false)) {
            list.Add(certificate);
        }

        return list.Count == 0 ? null : new CertificateResponse { Certificates = list };
    }

    /// <summary>
    /// Streams all certificates visible to the caller.
    /// </summary>
    /// <param name="pageSize">Number of certificates to request per page.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public IAsyncEnumerable<Certificate> EnumerateCertificatesAsync(
        int pageSize = 200,
        CancellationToken cancellationToken = default) {
        var request = new CertificateSearchRequest { Size = pageSize };
        return EnumerateSearchAsync(request, cancellationToken);
    }

    /// <summary>
    /// Streams issued certificates page by page and downloads each one.
    /// </summary>
    /// <param name="pageSize">Number of certificates to request per page.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public IAsyncEnumerable<X509Certificate2> StreamCertificatesAsync(
        int pageSize = 200,
        CancellationToken cancellationToken = default) {
        var request = new CertificateSearchRequest { Size = pageSize };
        return StreamCertificatesAsync(request, cancellationToken);
    }

    /// <summary>
    /// Streams issued certificates matching the provided filter.
    /// </summary>
    /// <param name="request">Filter describing certificates to retrieve.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async IAsyncEnumerable<X509Certificate2> StreamCertificatesAsync(
        CertificateSearchRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default) {
        Guard.AgainstNull(request, nameof(request));

        await foreach (var certificate in EnumerateSearchAsync(request, cancellationToken).ConfigureAwait(false)) {
            yield return await DownloadAsync(certificate.Id, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Streams search results page by page.
    /// </summary>
    /// <param name="request">Filter describing certificates to retrieve.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async IAsyncEnumerable<Certificate> EnumerateSearchAsync(
        CertificateSearchRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default) {
        Guard.AgainstNull(request, nameof(request));

        var originalSize = request.Size;
        var originalPosition = request.Position;
        var pageSize = request.Size ?? 200;
        var position = request.Position ?? 0;

        try {
            var query = BuildQuery(request);
            var response = await _client.GetAsync($"v1/certificate{query}", cancellationToken).ConfigureAwait(false);
            var page = await response.Content
                .ReadFromJsonAsyncSafe<IReadOnlyList<Certificate>>(s_json, cancellationToken)
                .ConfigureAwait(false);
            if (page is null || page.Count == 0) {
                yield break;
            }

            foreach (var certificate in page) {
                yield return certificate;
            }

            if (page.Count < pageSize) {
                yield break;
            }

            request.Size = pageSize;
            position += pageSize;

            while (true) {
                request.Position = position;
                query = BuildQuery(request);
                response = await _client.GetAsync($"v1/certificate{query}", cancellationToken).ConfigureAwait(false);
                page = await response.Content
                    .ReadFromJsonAsyncSafe<IReadOnlyList<Certificate>>(s_json, cancellationToken)
                    .ConfigureAwait(false);
                if (page is null || page.Count == 0) {
                    yield break;
                }

                foreach (var certificate in page) {
                    yield return certificate;
                }

                if (page.Count < pageSize) {
                    yield break;
                }

                position += pageSize;
            }
        } finally {
            request.Size = originalSize;
            request.Position = originalPosition;
        }
    }
}
