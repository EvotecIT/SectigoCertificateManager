namespace SectigoCertificateManager.Clients;

using System.Net.Http.Json;

/// <summary>
/// Provides access to certificate revocation endpoints.
/// </summary>
public sealed class RevocationsClient
{
    private readonly ISectigoClient _client;

    /// <summary>
    /// Initializes a new instance of the <see cref="RevocationsClient"/> class.
    /// </summary>
    /// <param name="client">HTTP client wrapper.</param>
    public RevocationsClient(ISectigoClient client) => _client = client;

    /// <summary>
    /// Revokes a certificate by identifier.
    /// </summary>
    /// <param name="certificateId">Identifier of the certificate to revoke.</param>
    /// <param name="reason">Reason for revocation.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    public async Task RevokeAsync(int certificateId, string reason, CancellationToken cancellationToken = default)
    {
        var payload = new { reason };
        var response = await _client.PostAsync($"v1/revoke/{certificateId}", JsonContent.Create(payload), cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
