namespace SectigoCertificateManager.AdminApi;

using SectigoCertificateManager.Utilities;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Admin Operations API client for Azure Key Vault accounts.
/// </summary>
public sealed class AdminAzureClient : AdminApiClientBase {
    private static readonly JsonSerializerOptions s_json = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Creates a new Azure client using the provided Admin API configuration.
    /// </summary>
    /// <param name="config">Admin API configuration (base URL and OAuth2 client credentials).</param>
    /// <param name="httpClient">Optional HTTP client instance; when omitted a new one is created and disposed with the client.</param>
    public AdminAzureClient(AdminApiConfig config, HttpClient? httpClient = null)
        : base(config, httpClient) {
    }

    /// <summary>
    /// Lists Azure accounts.
    /// </summary>
    public async Task<(IReadOnlyList<AzureAccountItem> Items, int? TotalCount)> ListAccountsAsync(
        int? size = null,
        int? position = null,
        CancellationToken cancellationToken = default) {
        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        var path = QueryStringBuilder.Build("api/azure/v1/accounts", q => {
            if (size.HasValue && size.Value > 0) {
                q.AddInt("size", size.Value);
            }

            if (position.HasValue && position.Value >= 0) {
                q.AddInt("position", position.Value);
            }
        });

        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        SetBearer(request, token);

        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
            .ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var items = await response.Content
            .ReadFromJsonAsyncSafe<List<AzureAccountItem>>(s_json, cancellationToken)
            .ConfigureAwait(false) ?? new List<AzureAccountItem>();

        int? total = null;
        if (response.Headers.TryGetValues("X-Total-Count", out var values)) {
            foreach (var value in values) {
                if (int.TryParse(value, out var parsed)) {
                    total = parsed;
                    break;
                }
            }
        }

        return (items, total);
    }

    /// <summary>
    /// Retrieves details of a single Azure account.
    /// </summary>
    public async Task<AzureAccountDetails?> GetAccountAsync(int id, CancellationToken cancellationToken = default) {
        GuardAgainstNonPositive(id, nameof(id));

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);
        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/azure/v1/accounts/{id}");
        SetBearer(request, token);

        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
            .ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsyncSafe<AzureAccountDetails>(s_json, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Creates a new Azure account and returns its identifier.
    /// </summary>
    public async Task<int> CreateAccountAsync(AzureAccountCreateRequest request, CancellationToken cancellationToken = default) {
        Guard.AgainstNull(request, nameof(request));
        GuardAgainstNullOrWhiteSpace(request.Name, nameof(request.Name));
        GuardAgainstNullOrWhiteSpace(request.ApplicationId, nameof(request.ApplicationId));
        GuardAgainstNullOrWhiteSpace(request.DirectoryId, nameof(request.DirectoryId));
        GuardAgainstNullOrWhiteSpace(request.ApplicationSecret, nameof(request.ApplicationSecret));

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "api/azure/v1/accounts") {
            Content = JsonContent.Create(MapCreateRequest(request), options: s_json)
        };
        SetBearer(httpRequest, token);

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var location = response.Headers.Location;
        if (location is null) {
            return 0;
        }

        return LocationHeaderParser.ParseId(response);
    }

    /// <summary>
    /// Updates an existing Azure account.
    /// </summary>
    public async Task UpdateAccountAsync(int id, AzureAccountUpdateRequest request, CancellationToken cancellationToken = default) {
        GuardAgainstNonPositive(id, nameof(id));
        Guard.AgainstNull(request, nameof(request));

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        using var httpRequest = new HttpRequestMessage(HttpMethod.Put, $"api/azure/v1/accounts/{id}") {
            Content = JsonContent.Create(MapUpdateRequest(request), options: s_json)
        };
        SetBearer(httpRequest, token);

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Deletes an Azure account by identifier.
    /// </summary>
    public async Task DeleteAccountAsync(int id, CancellationToken cancellationToken = default) {
        GuardAgainstNonPositive(id, nameof(id));

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);
        using var request = new HttpRequestMessage(HttpMethod.Delete, $"api/azure/v1/accounts/{id}");
        SetBearer(request, token);

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Delegates organizations to an existing Azure account.
    /// </summary>
    public async Task<AzureDelegatedOrgsResponse?> DelegateOrganizationsAsync(int id, AzureDelegateRequest request, CancellationToken cancellationToken = default) {
        GuardAgainstNonPositive(id, nameof(id));
        Guard.AgainstNull(request, nameof(request));

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        using var httpRequest = new HttpRequestMessage(HttpMethod.Put, $"api/azure/v1/accounts/{id}/delegations") {
            Content = JsonContent.Create(MapDelegateRequest(request), options: s_json)
        };
        SetBearer(httpRequest, token);

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsyncSafe<AzureDelegatedOrgsResponse>(s_json, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Checks an Azure account configuration and connectivity.
    /// </summary>
    public async Task<IReadOnlyList<AzureAccountCheckStatus>> CheckAccountAsync(int id, CancellationToken cancellationToken = default) {
        GuardAgainstNonPositive(id, nameof(id));

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);
        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/azure/v1/accounts/{id}/check");
        SetBearer(request, token);

        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
            .ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsyncSafe<List<AzureAccountCheckStatus>>(s_json, cancellationToken)
            .ConfigureAwait(false) ?? new List<AzureAccountCheckStatus>();
    }

    /// <summary>
    /// Lists Azure Key Vaults for a given account, subscription and resource group.
    /// </summary>
    public async Task<IReadOnlyList<AzureVault>> ListVaultsAsync(
        int accountId,
        string subscriptionId,
        string resourceGroup,
        CancellationToken cancellationToken = default) {
        GuardAgainstNonPositive(accountId, nameof(accountId));
        GuardAgainstNullOrWhiteSpace(subscriptionId, nameof(subscriptionId));
        GuardAgainstNullOrWhiteSpace(resourceGroup, nameof(resourceGroup));

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);
        var path = $"api/azure/v1/accounts/{accountId}/subscriptions/{Uri.EscapeDataString(subscriptionId)}/resource-groups/{Uri.EscapeDataString(resourceGroup)}/vaults";

        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        SetBearer(request, token);

        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
            .ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsyncSafe<List<AzureVault>>(s_json, cancellationToken)
            .ConfigureAwait(false) ?? new List<AzureVault>();
    }

    /// <summary>
    /// Lists Azure resource groups for a given account.
    /// </summary>
    public async Task<IReadOnlyList<AzureResource>> ListResourceGroupsAsync(int accountId, CancellationToken cancellationToken = default) {
        GuardAgainstNonPositive(accountId, nameof(accountId));

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);
        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/azure/v1/accounts/{accountId}/resource-groups");
        SetBearer(request, token);

        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
            .ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsyncSafe<List<AzureResource>>(s_json, cancellationToken)
            .ConfigureAwait(false) ?? new List<AzureResource>();
    }

    private static void GuardAgainstNonPositive(int value, string paramName) {
        if (value <= 0) {
            throw new ArgumentOutOfRangeException(paramName);
        }
    }

    private static void GuardAgainstNullOrWhiteSpace(string? value, string paramName) {
        if (string.IsNullOrWhiteSpace(value)) {
            throw new ArgumentException("Value cannot be empty.", paramName);
        }
    }

    private static object MapCreateRequest(AzureAccountCreateRequest request) {
        return new {
            name = request.Name,
            applicationId = request.ApplicationId,
            directoryId = request.DirectoryId,
            environment = MapEnvironment(request.Environment),
            applicationSecret = request.ApplicationSecret
        };
    }

    private static object MapUpdateRequest(AzureAccountUpdateRequest request) {
        return new {
            name = request.Name,
            applicationId = request.ApplicationId,
            directoryId = request.DirectoryId,
            environment = request.Environment.HasValue ? MapEnvironment(request.Environment.Value) : null,
            applicationSecret = request.ApplicationSecret
        };
    }

    private static object MapDelegateRequest(AzureDelegateRequest request) {
        return new {
            delegationMode = MapDelegationMode(request.DelegationMode),
            orgDelegations = request.OrgDelegations
        };
    }

    internal static string MapEnvironment(AzureEnvironment environment) {
        return environment switch {
            AzureEnvironment.Azure => "AZURE",
            AzureEnvironment.AzureUsGovernment => "AZURE_US_GOVERNMENT",
            AzureEnvironment.AzureGermany => "AZURE_GERMANY",
            AzureEnvironment.AzureChina => "AZURE_CHINA",
            _ => "AZURE"
        };
    }

    internal static string MapDelegationMode(AzureDelegationMode mode) {
        return mode switch {
            AzureDelegationMode.GlobalForCustomer => "GLOBAL_FOR_CUSTOMER",
            AzureDelegationMode.Customized => "CUSTOMIZED",
            _ => "GLOBAL_FOR_CUSTOMER"
        };
    }
}
