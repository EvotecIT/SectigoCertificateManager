namespace SectigoCertificateManager.AdminApi;

using SectigoCertificateManager;
using SectigoCertificateManager.Utilities;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Minimal client for the Sectigo Admin Operations API administrator endpoints.
/// </summary>
public sealed class AdminAdminClient : AdminApiClientBase {
    private static readonly JsonSerializerOptions s_json = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminAdminClient"/> class.
    /// </summary>
    /// <param name="config">Admin API configuration.</param>
    /// <param name="httpClient">
    /// Optional <see cref="HttpClient"/> instance. When not provided, a new instance is created
    /// and disposed with this client.
    /// </param>
    public AdminAdminClient(AdminApiConfig config, HttpClient? httpClient = null)
        : base(config, httpClient) {
    }

    /// <summary>
    /// Lists administrators according to the specified filter and pagination.
    /// </summary>
    public async Task<IReadOnlyList<AdminIdentity>> ListAsync(
        int? size = null,
        int? position = null,
        string? login = null,
        string? email = null,
        AdminActiveState? activeState = null,
        int? orgId = null,
        AdminAccountType? type = null,
        int? templateId = null,
        int? identityProviderId = null,
        string? role = null,
        CancellationToken cancellationToken = default) {
        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        var path = QueryStringBuilder.Build("api/admin/v1", q => {
            q.AddInt("size", size);
            q.AddInt("position", position);
            q.AddString("login", login);
            q.AddString("email", email);
            if (activeState.HasValue) {
                q.AddString("activeState", MapActiveState(activeState.Value));
            }

            q.AddInt("orgId", orgId);
            if (type.HasValue) {
                q.AddString("type", MapAccountType(type.Value));
            }

            q.AddInt("templateId", templateId);
            q.AddInt("identityProviderId", identityProviderId);
            q.AddString("role", role);
        });

        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        SetBearer(request, token);

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        await ApiErrorHandler.ThrowIfErrorAsync(response, cancellationToken).ConfigureAwait(false);

        var items = await response.Content
            .ReadFromJsonAsyncSafe<IReadOnlyList<AdminIdentity>>(s_json, cancellationToken)
            .ConfigureAwait(false);

        return items ?? Array.Empty<AdminIdentity>();
    }

    /// <summary>
    /// Retrieves detailed information about an administrator.
    /// </summary>
    public async Task<AdminDetails?> GetAsync(
        int id,
        CancellationToken cancellationToken = default) {
        if (id <= 0) {
            throw new ArgumentOutOfRangeException(nameof(id));
        }

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/admin/v1/{id}");
        SetBearer(request, token);

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        await ApiErrorHandler.ThrowIfErrorAsync(response, cancellationToken).ConfigureAwait(false);

        var details = await response.Content
            .ReadFromJsonAsyncSafe<AdminDetails>(s_json, cancellationToken)
            .ConfigureAwait(false);

        return details;
    }

    /// <summary>
    /// Creates a new administrator account and returns its identifier.
    /// </summary>
    public async Task<int> CreateAsync(
        AdminCreateOrUpdateRequest request,
        CancellationToken cancellationToken = default) {
        Guard.AgainstNull(request, nameof(request));

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        using var httpContent = JsonContent.Create(request, options: s_json);
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "api/admin/v1") {
            Content = httpContent
        };
        SetBearer(httpRequest, token);

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);
        await ApiErrorHandler.ThrowIfErrorAsync(response, cancellationToken).ConfigureAwait(false);

        return LocationHeaderParser.ParseId(response);
    }

    /// <summary>
    /// Updates an administrator account.
    /// </summary>
    public async Task UpdateAsync(
        int id,
        AdminCreateOrUpdateRequest request,
        CancellationToken cancellationToken = default) {
        if (id <= 0) {
            throw new ArgumentOutOfRangeException(nameof(id));
        }

        Guard.AgainstNull(request, nameof(request));

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        using var httpContent = JsonContent.Create(request, options: s_json);
        using var httpRequest = new HttpRequestMessage(HttpMethod.Put, $"api/admin/v1/{id}") {
            Content = httpContent
        };
        SetBearer(httpRequest, token);

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);
        await ApiErrorHandler.ThrowIfErrorAsync(response, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Deletes an administrator account.
    /// </summary>
    public async Task DeleteAsync(
        int id,
        int? replacingRequesterId = null,
        CancellationToken cancellationToken = default) {
        if (id <= 0) {
            throw new ArgumentOutOfRangeException(nameof(id));
        }

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        var path = QueryStringBuilder.Build($"api/admin/v1/{id}", q => {
            q.AddInt("replacingRequesterId", replacingRequesterId);
        });

        using var request = new HttpRequestMessage(HttpMethod.Delete, path);
        SetBearer(request, token);

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        await ApiErrorHandler.ThrowIfErrorAsync(response, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Unlinks an IdP administrator from its template.
    /// </summary>
    public async Task UnlinkFromTemplateAsync(
        int id,
        CancellationToken cancellationToken = default) {
        if (id <= 0) {
            throw new ArgumentOutOfRangeException(nameof(id));
        }

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        using var request = new HttpRequestMessage(HttpMethod.Put, $"api/admin/v1/{id}/unlink");
        SetBearer(request, token);

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        await ApiErrorHandler.ThrowIfErrorAsync(response, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Changes the current administrator's password.
    /// </summary>
    public async Task ChangeOwnPasswordAsync(
        string currentPassword,
        string newPassword,
        CancellationToken cancellationToken = default) {
        Guard.AgainstNullOrWhiteSpace(currentPassword, nameof(currentPassword));
        Guard.AgainstNullOrWhiteSpace(newPassword, nameof(newPassword));

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        using var content = JsonContent.Create(
            new AdminChangePasswordRequest { NewPassword = newPassword },
            options: s_json);

        using var request = new HttpRequestMessage(HttpMethod.Post, "api/admin/v1/changepassword") {
            Content = content
        };
        SetBearer(request, token);
        request.Headers.Add("password", currentPassword);

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        await ApiErrorHandler.ThrowIfErrorAsync(response, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Retrieves the current administrator's password status.
    /// </summary>
    public async Task<AdminPasswordStatus?> GetPasswordStatusAsync(
        CancellationToken cancellationToken = default) {
        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        using var request = new HttpRequestMessage(HttpMethod.Get, "api/admin/v1/password");
        SetBearer(request, token);

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        await ApiErrorHandler.ThrowIfErrorAsync(response, cancellationToken).ConfigureAwait(false);

        var status = await response.Content
            .ReadFromJsonAsyncSafe<AdminPasswordStatus>(s_json, cancellationToken)
            .ConfigureAwait(false);

        return status;
    }

    /// <summary>
    /// Lists roles that can be assigned to administrators.
    /// </summary>
    public async Task<IReadOnlyList<AdminRole>> ListRolesAsync(
        bool? isForEdit = null,
        CancellationToken cancellationToken = default) {
        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        var path = QueryStringBuilder.Build("api/admin/v1/roles", q => {
            q.AddBool("isForEdit", isForEdit, emitFalse: true);
        });

        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        SetBearer(request, token);

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        await ApiErrorHandler.ThrowIfErrorAsync(response, cancellationToken).ConfigureAwait(false);

        var items = await response.Content
            .ReadFromJsonAsyncSafe<IReadOnlyList<AdminRole>>(s_json, cancellationToken)
            .ConfigureAwait(false);

        return items ?? Array.Empty<AdminRole>();
    }

    /// <summary>
    /// Lists available privileges for the specified roles.
    /// </summary>
    public async Task<IReadOnlyList<AdminPrivilegeDescription>> ListPrivilegesAsync(
        IReadOnlyList<AdminRole> roles,
        CancellationToken cancellationToken = default) {
        Guard.AgainstNull(roles, nameof(roles));
        if (roles.Count == 0) {
            throw new ArgumentException("At least one role must be specified.", nameof(roles));
        }

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        var path = QueryStringBuilder.Build("api/admin/v1/privileges", q => {
            foreach (var role in roles) {
                q.AddString("role", role.ToString());
            }
        });

        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        SetBearer(request, token);

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        await ApiErrorHandler.ThrowIfErrorAsync(response, cancellationToken).ConfigureAwait(false);

        var items = await response.Content
            .ReadFromJsonAsyncSafe<IReadOnlyList<AdminPrivilegeDescription>>(s_json, cancellationToken)
            .ConfigureAwait(false);

        return items ?? Array.Empty<AdminPrivilegeDescription>();
    }

    /// <summary>
    /// Lists available identity providers.
    /// </summary>
    public async Task<IReadOnlyList<AdminIdpInfo>> ListIdentityProvidersAsync(
        CancellationToken cancellationToken = default) {
        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        using var request = new HttpRequestMessage(HttpMethod.Get, "api/admin/v1/idp");
        SetBearer(request, token);

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        await ApiErrorHandler.ThrowIfErrorAsync(response, cancellationToken).ConfigureAwait(false);

        var items = await response.Content
            .ReadFromJsonAsyncSafe<IReadOnlyList<AdminIdpInfo>>(s_json, cancellationToken)
            .ConfigureAwait(false);

        return items ?? Array.Empty<AdminIdpInfo>();
    }

    private static string MapActiveState(AdminActiveState state) {
        return state switch {
            AdminActiveState.Active => "ACTIVE",
            AdminActiveState.Suspended => "SUSPENDED",
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "Unsupported active state value.")
        };
    }

    private static string MapAccountType(AdminAccountType type) {
        return type switch {
            AdminAccountType.Standard => "STANDARD",
            AdminAccountType.Api => "API",
            AdminAccountType.Sas => "SAS",
            AdminAccountType.Idp => "IDP",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unsupported account type value.")
        };
    }
}

