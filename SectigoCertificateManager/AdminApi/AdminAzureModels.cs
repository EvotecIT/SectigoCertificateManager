namespace SectigoCertificateManager.AdminApi;

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

/// <summary>
/// Azure account delegation mode.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AzureDelegationMode {
    GlobalForCustomer,
    Customized
}

/// <summary>
/// Azure environment supported by the Admin API.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AzureEnvironment {
    Azure,
    AzureUsGovernment,
    AzureGermany,
    AzureChina
}

/// <summary>
/// Basic Azure account list item.
/// </summary>
public sealed class AzureAccountItem {
    public string? Name { get; set; }

    public string? ApplicationId { get; set; }

    public string? DirectoryId { get; set; }

    public int Id { get; set; }

    public AzureDelegationMode? DelegationMode { get; set; }
}

/// <summary>
/// Detailed Azure account information.
/// </summary>
public sealed class AzureAccountDetails {
    public string? Name { get; set; }

    public string? ApplicationId { get; set; }

    public string? DirectoryId { get; set; }

    public AzureEnvironment Environment { get; set; }

    public AzureDelegationMode? DelegationMode { get; set; }

    public IReadOnlyList<int> OrgDelegations { get; set; } = Array.Empty<int>();
}

/// <summary>
/// Request payload used to create an Azure account.
/// </summary>
public sealed class AzureAccountCreateRequest {
    public string Name { get; set; } = string.Empty;

    public string ApplicationId { get; set; } = string.Empty;

    public string DirectoryId { get; set; } = string.Empty;

    public AzureEnvironment Environment { get; set; }

    public string ApplicationSecret { get; set; } = string.Empty;
}

/// <summary>
/// Request payload used to update an Azure account.
/// </summary>
public sealed class AzureAccountUpdateRequest {
    public string? Name { get; set; }

    public string? ApplicationId { get; set; }

    public string? DirectoryId { get; set; }

    public AzureEnvironment? Environment { get; set; }

    public string? ApplicationSecret { get; set; }
}

/// <summary>
/// Result of Azure account configuration or connectivity check.
/// </summary>
public sealed class AzureAccountCheckStatus {
    public string? CheckName { get; set; }

    public string? Message { get; set; }
}

/// <summary>
/// Azure Key Vault metadata.
/// </summary>
public sealed class AzureVault {
    public string? Name { get; set; }

    public string? Key { get; set; }

    public string? SkuName { get; set; }
}

/// <summary>
/// Azure resource-group metadata.
/// </summary>
public sealed class AzureResource {
    public string? Name { get; set; }

    public string? Key { get; set; }

    public string? SkuName { get; set; }

    public string? SubscriptionId { get; set; }
}

/// <summary>
/// Request payload used to delegate organizations to an Azure account.
/// </summary>
public sealed class AzureDelegateRequest {
    public AzureDelegationMode DelegationMode { get; set; }

    public IReadOnlyList<int>? OrgDelegations { get; set; }
}

/// <summary>
/// Response containing delegated organization identifiers.
/// </summary>
public sealed class AzureDelegatedOrgsResponse {
    public IReadOnlyList<int> OrgDelegations { get; set; } = Array.Empty<int>();
}
