namespace SectigoCertificateManager.Clients;

using System;
using System.Net.Http.Json;
using SectigoCertificateManager.Models;
using SectigoCertificateManager.Requests;

/// <summary>
/// Provides access to organization related endpoints.
/// </summary>
public sealed class OrganizationsClient
{
    private readonly ISectigoClient _client;

    /// <summary>
    /// Initializes a new instance of the <see cref="OrganizationsClient"/> class.
    /// </summary>
    /// <param name="client">HTTP client wrapper.</param>
    public OrganizationsClient(ISectigoClient client) => _client = client;

    /// <summary>
    /// Retrieves an organization by identifier.
    /// </summary>
    /// <param name="organizationId">Identifier of the organization to retrieve.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<Organization?> GetAsync(int organizationId, CancellationToken cancellationToken = default)
    {
        var response = await _client.GetAsync($"v1/organization/{organizationId}", cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Organization>(cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Creates a new organization.
    /// </summary>
    /// <param name="request">Payload describing the organization to create.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The identifier of the created organization.</returns>
    public async Task<int> CreateAsync(CreateOrganizationRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _client.PostAsync("v1/organization", JsonContent.Create(request), cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        var location = response.Headers.Location;
        if (location is not null)
        {
            var last = location.Segments[location.Segments.Length - 1];
            if (int.TryParse(last, out var id))
            {
                return id;
            }
        }

        return 0;
    }
}
