namespace SectigoCertificateManager.Clients;

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
        var response = await _client.GetAsync($"v1/certificate/{certificateId}", cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Certificate>(cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Issues a new certificate.
    /// </summary>
    public async Task<Certificate?> IssueAsync(IssueCertificateRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _client.PostAsync("v1/certificate/issue", JsonContent.Create(request), cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Certificate>(cancellationToken: cancellationToken).ConfigureAwait(false);
    }
}
