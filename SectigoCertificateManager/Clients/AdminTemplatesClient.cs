namespace SectigoCertificateManager.Clients;

using SectigoCertificateManager.Models;
using SectigoCertificateManager.Requests;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using SectigoCertificateManager.Utilities;

/// <summary>
/// Provides access to IdP template endpoints.
/// </summary>
public sealed class AdminTemplatesClient : BaseClient {

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminTemplatesClient"/> class.
    /// </summary>
    /// <param name="client">HTTP client wrapper.</param>
    public AdminTemplatesClient(ISectigoClient client) : base(client) {
    }

    /// <summary>Retrieves template details by identifier.</summary>
    public async Task<IdpTemplate?> GetAsync(int templateId, CancellationToken cancellationToken = default) {
        if (templateId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(templateId));
        }

        var response = await _client.GetAsync($"admin-template/v1/{templateId}", cancellationToken).ConfigureAwait(false);
        return await response.Content.ReadFromJsonAsyncSafe<IdpTemplate>(s_json, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Retrieves templates using the provided filter.</summary>
    public async Task<IReadOnlyList<IdpTemplate>> ListAsync(
        int? size = null,
        int? position = null,
        string? name = null,
        int? orgId = null,
        int? identityProviderId = null,
        CancellationToken cancellationToken = default) {
        var query = BuildQuery(size, position, name, orgId, identityProviderId);
        var response = await _client.GetAsync($"admin-template/v1{query}", cancellationToken).ConfigureAwait(false);
        var list = await response.Content.ReadFromJsonAsyncSafe<IReadOnlyList<IdpTemplate>>(s_json, cancellationToken).ConfigureAwait(false);
        return list ?? Array.Empty<IdpTemplate>();
    }

    /// <summary>Streams templates page by page.</summary>
    public async IAsyncEnumerable<IdpTemplate> EnumerateAsync(
        int pageSize = 200,
        [EnumeratorCancellation] CancellationToken cancellationToken = default) {
        var position = 0;
        var firstPage = true;
        while (true) {
            var list = await ListAsync(pageSize, position, null, null, null, cancellationToken).ConfigureAwait(false);
            if (list.Count == 0) {
                if (firstPage) {
                    yield break;
                }
                yield break;
            }

            foreach (var item in list) {
                yield return item;
            }

            if (list.Count < pageSize) {
                yield break;
            }

            position += pageSize;
            firstPage = false;
        }
    }

    /// <summary>Creates a new template.</summary>
    public async Task<int> CreateAsync(CreateIdpTemplateRequest request, CancellationToken cancellationToken = default) {
        Guard.AgainstNull(request, nameof(request));

        var response = await _client.PostAsync("admin-template/v1/", JsonContent.Create(request, options: s_json), cancellationToken).ConfigureAwait(false);
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

    /// <summary>Updates an existing template.</summary>
    public async Task UpdateAsync(int templateId, UpdateIdpTemplateRequest request, CancellationToken cancellationToken = default) {
        if (templateId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(templateId));
        }
        Guard.AgainstNull(request, nameof(request));

        var response = await _client.PutAsync($"admin-template/v1/{templateId}", JsonContent.Create(request, options: s_json), cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }

    /// <summary>Deletes a template.</summary>
    public async Task DeleteAsync(
        int templateId,
        RelatedAdminsAction action = RelatedAdminsAction.Unlink,
        int? replacingRequesterId = null,
        CancellationToken cancellationToken = default) {
        if (templateId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(templateId));
        }

        var builder = new StringBuilder();
        builder.Append("?").Append("relatedAdminsAction=").Append(action);
        if (replacingRequesterId.HasValue) {
            builder.Append("&replacingRequesterId=").Append(replacingRequesterId.Value);
        }

        var response = await _client.DeleteAsync($"admin-template/v1/{templateId}{builder}", cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }

    private static string BuildQuery(int? size, int? position, string? name, int? orgId, int? idpId) {
        var builder = new StringBuilder();

        void Append(string n, string? value) {
            if (string.IsNullOrEmpty(value)) {
                return;
            }

            _ = builder.Length == 0 ? builder.Append('?') : builder.Append('&');
            builder.Append(n).Append('=').Append(Uri.EscapeDataString(value));
        }

        void AppendInt(string n, int? value) {
            if (!value.HasValue) {
                return;
            }

            _ = builder.Length == 0 ? builder.Append('?') : builder.Append('&');
            builder.Append(n).Append('=').Append(value.Value);
        }

        AppendInt("size", size);
        AppendInt("position", position);
        Append("name", name);
        AppendInt("orgId", orgId);
        AppendInt("identityProviderId", idpId);

        return builder.ToString();
    }
}
