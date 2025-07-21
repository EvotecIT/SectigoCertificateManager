namespace SectigoCertificateManager.Clients;

using SectigoCertificateManager.Models;
using SectigoCertificateManager.Requests;
using System;
using System.Net.Http.Json;
using System.Text.Json;
using SectigoCertificateManager.Utilities;

/// <summary>
/// Provides access to organization related endpoints.
/// </summary>
public sealed class OrganizationsClient : BaseClient {

    /// <summary>
    /// Initializes a new instance of the <see cref="OrganizationsClient"/> class.
    /// </summary>
    /// <param name="client">HTTP client wrapper.</param>
    public OrganizationsClient(ISectigoClient client) : base(client) {
    }

    /// <summary>
    /// Retrieves an organization by identifier.
    /// </summary>
    /// <param name="organizationId">Identifier of the organization to retrieve.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<Organization?> GetAsync(int organizationId, CancellationToken cancellationToken = default) {
        var response = await _client.GetAsync($"v1/organization/{organizationId}", cancellationToken).ConfigureAwait(false);
        return await response.Content
            .ReadFromJsonAsyncSafe<Organization>(s_json, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Creates a new organization.
    /// </summary>
    /// <param name="request">Payload describing the organization to create.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The identifier of the created organization.</returns>
    public async Task<int> CreateAsync(CreateOrganizationRequest request, CancellationToken cancellationToken = default) {
        Guard.AgainstNull(request, nameof(request));

        var response = await _client
            .PostAsync("v1/organization", JsonContent.Create(request, options: s_json), cancellationToken)
            .ConfigureAwait(false);

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
    /// Updates an existing organization.
    /// </summary>
    /// <param name="organizationId">Identifier of the organization to update.</param>
    /// <param name="request">Payload describing updated fields.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task UpdateAsync(int organizationId, UpdateOrganizationRequest request, CancellationToken cancellationToken = default) {
        if (organizationId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(organizationId));
        }
        Guard.AgainstNull(request, nameof(request));

        var response = await _client
            .PutAsync($"v1/organization/{organizationId}", JsonContent.Create(request, options: s_json), cancellationToken)
            .ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Updates an existing organization.
    /// </summary>
    /// <param name="request">Payload describing updated fields including identifier.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public Task UpdateAsync(UpdateOrganizationRequest request, CancellationToken cancellationToken = default) {
        Guard.AgainstNull(request, nameof(request));
        if (request.Id <= 0) {
            throw new ArgumentOutOfRangeException(nameof(request.Id));
        }

        return UpdateAsync(request.Id, request, cancellationToken);
    }

    /// <summary>
    /// Retrieves all organizations visible to the user.
    /// </summary>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<IReadOnlyList<Organization>> ListOrganizationsAsync(CancellationToken cancellationToken = default) {
        var response = await _client.GetAsync("v1/organization", cancellationToken).ConfigureAwait(false);
        var organizations = await response.Content
            .ReadFromJsonAsyncSafe<IReadOnlyList<Organization>>(s_json, cancellationToken)
            .ConfigureAwait(false);
        return organizations ?? Array.Empty<Organization>();
    }
}