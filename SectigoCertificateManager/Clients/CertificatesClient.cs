namespace SectigoCertificateManager.Clients;

using System.Collections.Generic;
using System.Net.Http.Json;
using SectigoCertificateManager.Models;
using SectigoCertificateManager.Requests;
using SectigoCertificateManager.Responses;

/// <summary>
/// Provides access to certificate related endpoints.
/// </summary>
public sealed class CertificatesClient
{
    private readonly ISectigoClient _client;

    /// <summary>
    /// Initializes a new instance of the <see cref="CertificatesClient"/> class.
    /// </summary>
    /// <param name="client">HTTP client wrapper.</param>
    public CertificatesClient(ISectigoClient client) => _client = client;

    /// <summary>
    /// Retrieves a certificate by identifier.
    /// </summary>
    public async Task<Certificate?> GetAsync(int certificateId, CancellationToken cancellationToken = default)
    {
        var response = await _client.GetAsync($"v1/certificate/{certificateId}", cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Certificate>(cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Issues a new certificate.
    /// </summary>
    public async Task<Certificate?> IssueAsync(IssueCertificateRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _client.PostAsync("v1/certificate/issue", JsonContent.Create(request), cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Certificate>(cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Lists certificates using paging.
    /// </summary>
    public async IAsyncEnumerable<Certificate> ListAsync(int pageNumber = 1, int pageSize = 200, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (pageNumber < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(pageNumber));
        }

        if (pageSize < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize));
        }

        var page = pageNumber;
        while (true)
        {
            var response = await _client.GetAsync($"v1/certificate?page={page}&size={pageSize}", cancellationToken);
            response.EnsureSuccessStatusCode();
            var results = await response.Content.ReadFromJsonAsync<CertificateResponse>(cancellationToken: cancellationToken);

            if (results?.Certificates is null || results.Certificates.Count == 0)
            {
                yield break;
            }

            foreach (var certificate in results.Certificates)
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return certificate;
            }

            if (results.Certificates.Count < pageSize)
            {
                yield break;
            }

            page++;
        }
    }
}
