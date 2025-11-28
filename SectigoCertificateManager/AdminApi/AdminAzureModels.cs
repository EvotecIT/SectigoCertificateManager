namespace SectigoCertificateManager.AdminApi;

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

/// <summary>
/// Controls how an Azure Key Vault account is delegated across organizations.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AzureDelegationMode {
    /// <summary>The account is available to all organizations in the customer tenant.</summary>
    [EnumMember(Value = "GLOBAL_FOR_CUSTOMER")]
    GlobalForCustomer,
    /// <summary>The account is delegated only to explicitly configured organizations.</summary>
    [EnumMember(Value = "CUSTOMIZED")]
    Customized
}

/// <summary>
/// Azure cloud environment in which the account is registered.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AzureEnvironment {
    /// <summary>Public Azure cloud.</summary>
    [EnumMember(Value = "AZURE")]
    Azure,
    /// <summary>Azure US Government cloud.</summary>
    [EnumMember(Value = "AZURE_US_GOVERNMENT")]
    AzureUsGovernment,
    /// <summary>Azure Germany cloud.</summary>
    [EnumMember(Value = "AZURE_GERMANY")]
    AzureGermany,
    /// <summary>Azure China cloud.</summary>
    [EnumMember(Value = "AZURE_CHINA")]
    AzureChina
}

/// <summary>
/// Basic Azure account information returned from list operations.
/// </summary>
public sealed class AzureAccountItem {
    /// <summary>Gets or sets the friendly name of the Azure account.</summary>
    public string? Name { get; set; }

    /// <summary>Gets or sets the Azure AD application (client) identifier.</summary>
    public string? ApplicationId { get; set; }

    /// <summary>Gets or sets the Azure AD directory (tenant) identifier.</summary>
    public string? DirectoryId { get; set; }

    /// <summary>Gets or sets the internal Azure account identifier.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the current delegation mode for this account.</summary>
    public AzureDelegationMode? DelegationMode { get; set; }
}

/// <summary>
/// Detailed Azure account information including delegation configuration.
/// </summary>
public sealed class AzureAccountDetails {
    /// <summary>Gets or sets the friendly name of the Azure account.</summary>
    public string? Name { get; set; }

    /// <summary>Gets or sets the Azure AD application (client) identifier.</summary>
    public string? ApplicationId { get; set; }

    /// <summary>Gets or sets the Azure AD directory (tenant) identifier.</summary>
    public string? DirectoryId { get; set; }

    /// <summary>Gets or sets the Azure environment for this account.</summary>
    public AzureEnvironment Environment { get; set; }

    /// <summary>Gets or sets the delegation mode for this account.</summary>
    public AzureDelegationMode? DelegationMode { get; set; }

    /// <summary>
    /// Gets or sets the identifiers of organizations to which this account is delegated
    /// when <see cref="DelegationMode"/> is <see cref="AzureDelegationMode.Customized"/>.
    /// </summary>
    public IReadOnlyList<int> OrgDelegations { get; set; } = Array.Empty<int>();
}

/// <summary>
/// Request payload used to create a new Azure Key Vault account integration.
/// </summary>
public sealed class AzureAccountCreateRequest {
    /// <summary>Gets or sets the friendly name of the Azure account.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the Azure AD application (client) identifier.</summary>
    public string ApplicationId { get; set; } = string.Empty;

    /// <summary>Gets or sets the Azure AD directory (tenant) identifier.</summary>
    public string DirectoryId { get; set; } = string.Empty;

    /// <summary>Gets or sets the Azure environment for this account.</summary>
    public AzureEnvironment Environment { get; set; }

    /// <summary>Gets or sets the client secret for the Azure AD application.</summary>
    public string ApplicationSecret { get; set; } = string.Empty;
}

/// <summary>
/// Request payload used to update an existing Azure account integration.
/// </summary>
public sealed class AzureAccountUpdateRequest {
    /// <summary>Gets or sets the friendly name of the Azure account.</summary>
    public string? Name { get; set; }

    /// <summary>Gets or sets the Azure AD application (client) identifier.</summary>
    public string? ApplicationId { get; set; }

    /// <summary>Gets or sets the Azure AD directory (tenant) identifier.</summary>
    public string? DirectoryId { get; set; }

    /// <summary>Gets or sets the Azure environment for this account.</summary>
    public AzureEnvironment? Environment { get; set; }

    /// <summary>Gets or sets the client secret for the Azure AD application.</summary>
    public string? ApplicationSecret { get; set; }
}

/// <summary>
/// Result of an Azure account configuration or connectivity check.
/// </summary>
public sealed class AzureAccountCheckStatus {
    /// <summary>Gets or sets the name of the check (for example, "permissions" or "connectivity").</summary>
    public string? CheckName { get; set; }

    /// <summary>Gets or sets a human‑readable message describing the result of the check.</summary>
    public string? Message { get; set; }
}

/// <summary>
/// Azure Key Vault metadata returned by the Admin API.
/// </summary>
public sealed class AzureVault {
    /// <summary>Gets or sets the vault name.</summary>
    public string? Name { get; set; }

    /// <summary>Gets or sets the internal vault key used by Sectigo.</summary>
    public string? Key { get; set; }

    /// <summary>Gets or sets the vault SKU name.</summary>
    public string? SkuName { get; set; }
}

/// <summary>
/// Azure resource group metadata returned by the Admin API.
/// </summary>
public sealed class AzureResource {
    /// <summary>Gets or sets the resource group name.</summary>
    public string? Name { get; set; }

    /// <summary>Gets or sets the internal resource key used by Sectigo.</summary>
    public string? Key { get; set; }

    /// <summary>Gets or sets the SKU name where applicable.</summary>
    public string? SkuName { get; set; }

    /// <summary>Gets or sets the Azure subscription identifier.</summary>
    public string? SubscriptionId { get; set; }
}

/// <summary>
/// Request payload used to delegate the Azure account to one or more organizations.
/// </summary>
public sealed class AzureDelegateRequest {
    /// <summary>Gets or sets the desired delegation mode.</summary>
    public AzureDelegationMode DelegationMode { get; set; }

    /// <summary>
    /// Gets or sets the identifiers of organizations that should have access to the account
    /// when <see cref="DelegationMode"/> is <see cref="AzureDelegationMode.Customized"/>.
    /// </summary>
    public IReadOnlyList<int>? OrgDelegations { get; set; }
}

/// <summary>
/// Response containing the identifiers of organizations delegated to an Azure account.
/// </summary>
public sealed class AzureDelegatedOrgsResponse {
    /// <summary>Gets or sets the identifiers of delegated organizations.</summary>
    public IReadOnlyList<int> OrgDelegations { get; set; } = Array.Empty<int>();
}
