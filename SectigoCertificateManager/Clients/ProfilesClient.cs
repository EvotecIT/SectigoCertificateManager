namespace SectigoCertificateManager.Clients;

using SectigoCertificateManager.Models;
using SectigoCertificateManager.Utilities;
using System.Net.Http.Json;
using System.Text.Json;

/// <summary>
/// Provides access to profile related endpoints.
/// </summary>
public sealed class ProfilesClient : BaseClient {

    /// <summary>
    /// Initializes a new instance of the <see cref="ProfilesClient"/> class.
    /// </summary>
    /// <param name="client">HTTP client wrapper.</param>
    public ProfilesClient(ISectigoClient client) : base(client) {
    }

    /// <summary>
    /// Retrieves a profile by identifier.
    /// </summary>
    /// <param name="profileId">Identifier of the profile to retrieve.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<Profile?> GetAsync(int profileId, CancellationToken cancellationToken = default) {
        var response = await _client.GetAsync($"v1/profile/{profileId}", cancellationToken).ConfigureAwait(false);
        return await response.Content
            .ReadFromJsonAsyncSafe<Profile>(s_json, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Retrieves all profiles visible to the user.
    /// </summary>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<IReadOnlyList<Profile>> ListProfilesAsync(CancellationToken cancellationToken = default) {
        var response = await _client.GetAsync("v1/profile", cancellationToken).ConfigureAwait(false);
        return await response.Content
            .ReadFromJsonAsyncSafe<IReadOnlyList<Profile>>(s_json, cancellationToken)
            .ConfigureAwait(false) ?? Array.Empty<Profile>();
    }
}