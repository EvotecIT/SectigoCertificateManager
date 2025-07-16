namespace SectigoCertificateManager.Clients;

using SectigoCertificateManager.Models;
using SectigoCertificateManager.Requests;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;

/// <summary>
/// Provides access to user related endpoints.
/// </summary>
public sealed class UsersClient {
    private readonly ISectigoClient _client;
    private static readonly JsonSerializerOptions s_json = new() {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="UsersClient"/> class.
    /// </summary>
    /// <param name="client">HTTP client wrapper.</param>
    public UsersClient(ISectigoClient client) => _client = client;

    /// <summary>
    /// Retrieves a user by identifier.
    /// </summary>
    /// <param name="userId">Identifier of the user to retrieve.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<User?> GetAsync(int userId, CancellationToken cancellationToken = default) {
        if (userId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(userId));
        }

        var response = await _client.GetAsync($"v1/user/{userId}", cancellationToken).ConfigureAwait(false);
        return await response.Content.ReadFromJsonAsync<User>(s_json, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Retrieves all users visible to the caller.
    /// </summary>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<IReadOnlyList<User>> ListUsersAsync(CancellationToken cancellationToken = default) {
        var list = new List<User>();
        await foreach (var user in EnumerateUsersAsync(cancellationToken: cancellationToken).ConfigureAwait(false)) {
            list.Add(user);
        }

        return list;
    }

    /// <summary>
    /// Streams users page by page.
    /// </summary>
    /// <param name="filter">Optional search filter.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async IAsyncEnumerable<User> EnumerateUsersAsync(
        UserSearchRequest? filter = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default) {
        filter ??= new UserSearchRequest();
        var pageSize = filter.Size ?? 200;
        var position = filter.Position ?? 0;

        while (true) {
            filter.Size = pageSize;
            filter.Position = position;
            var query = BuildQuery(filter);
            var response = await _client.GetAsync($"v1/user{query}", cancellationToken).ConfigureAwait(false);
            var page = await response.Content.ReadFromJsonAsync<IReadOnlyList<User>>(s_json, cancellationToken).ConfigureAwait(false);
            if (page is null || page.Count == 0) {
                yield break;
            }

            foreach (var user in page) {
                yield return user;
            }

            if (page.Count < pageSize) {
                yield break;
            }

            position += pageSize;
        }
    }

    /// <summary>
    /// Creates a new user.
    /// </summary>
    /// <param name="request">Payload describing the user to create.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The identifier of the created user.</returns>
    public async Task<int> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default) {
        if (request is null) {
            throw new ArgumentNullException(nameof(request));
        }

        var response = await _client.PostAsync("v1/user", JsonContent.Create(request, options: s_json), cancellationToken).ConfigureAwait(false);
        var location = response.Headers.Location;
        if (location is not null) {
            var url = location.ToString().Trim().TrimEnd('/');
            var segments = url.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length > 0 && int.TryParse(segments[segments.Length - 1], out var id)) {
                return id;
            }
        }

        return 0;
    }

    /// <summary>
    /// Updates an existing user.
    /// </summary>
    /// <param name="userId">Identifier of the user to update.</param>
    /// <param name="request">Payload describing updated fields.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task UpdateAsync(int userId, UpdateUserRequest request, CancellationToken cancellationToken = default) {
        if (userId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(userId));
        }
        if (request is null) {
            throw new ArgumentNullException(nameof(request));
        }

        var response = await _client.PutAsync($"v1/user/{userId}", JsonContent.Create(request, options: s_json), cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Deletes a user by identifier.
    /// </summary>
    /// <param name="userId">Identifier of the user to delete.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task DeleteAsync(int userId, CancellationToken cancellationToken = default) {
        if (userId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(userId));
        }

        var response = await _client.DeleteAsync($"v1/user/{userId}", cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }

    private static string BuildQuery(UserSearchRequest request) {
        var query = new List<string>();
        if (request.Size.HasValue) {
            query.Add($"size={request.Size.Value}");
        }
        if (request.Position.HasValue) {
            query.Add($"position={request.Position.Value}");
        }
        if (!string.IsNullOrEmpty(request.Name)) {
            query.Add($"name={Uri.EscapeDataString(request.Name)}");
        }
        if (request.OrganizationId.HasValue) {
            query.Add($"organizationId={request.OrganizationId.Value}");
        }
        if (!string.IsNullOrEmpty(request.Email)) {
            query.Add($"email={Uri.EscapeDataString(request.Email)}");
        }
        if (!string.IsNullOrEmpty(request.CommonName)) {
            query.Add($"commonName={Uri.EscapeDataString(request.CommonName)}");
        }
        if (!string.IsNullOrEmpty(request.SecondaryEmail)) {
            query.Add($"secondaryEmail={Uri.EscapeDataString(request.SecondaryEmail)}");
        }
        if (!string.IsNullOrEmpty(request.Phone)) {
            query.Add($"phone={Uri.EscapeDataString(request.Phone)}");
        }
        return query.Count == 0 ? string.Empty : "?" + string.Join("&", query);
    }
}
