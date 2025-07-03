namespace SectigoCertificateManager.Clients;

using System.Net.Http.Json;
using SectigoCertificateManager.Models;

/// <summary>
/// Provides access to profile related endpoints.
/// </summary>
public sealed class ProfilesClient
{
    private readonly ISectigoClient _client;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProfilesClient"/> class.
    /// </summary>
    /// <param name="client">HTTP client wrapper.</param>
    public ProfilesClient(ISectigoClient client) => _client = client;

    /// <summary>
    /// Retrieves a profile by identifier.
    /// </summary>
    public async Task<Profile?> GetAsync(int profileId, CancellationToken cancellationToken = default)
    {
        var response = await _client.GetAsync($"v1/profile/{profileId}", cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Profile>(cancellationToken: cancellationToken);
    }
}
