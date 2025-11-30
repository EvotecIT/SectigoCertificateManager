namespace SectigoCertificateManager.AdminApi;

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

/// <summary>
/// Minimal id/name pair used across Admin API responses.
/// </summary>
public sealed class IdName {
    /// <summary>Gets or sets the identifier.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the display name.</summary>
    public string? Name { get; set; }
}

/// <summary>
/// DNS connector status as reported by the Admin API.
/// </summary>
[JsonConverter(typeof(UpperSnakeEnumConverter<DnsConnectorStatus>))]
public enum DnsConnectorStatus {
    /// <summary>The connector is not available (for example, never connected).</summary>
    [EnumMember(Value = "NOT_AVAILABLE")]
    NotAvailable,
    /// <summary>The connector is registered but not currently connected.</summary>
    [EnumMember(Value = "NOT_CONNECTED")]
    NotConnected,
    /// <summary>The connector is online and connected.</summary>
    [EnumMember(Value = "CONNECTED")]
    Connected
}

/// <summary>
/// DNS connector delegation mode.
/// </summary>
[JsonConverter(typeof(UpperSnakeEnumConverter<DnsConnectorDelegationMode>))]
public enum DnsConnectorDelegationMode {
    /// <summary>The connector is available to all organizations in the customer tenant.</summary>
    [EnumMember(Value = "GLOBAL_FOR_CUSTOMER")]
    GlobalForCustomer,
    /// <summary>The connector is delegated only to explicitly configured organizations.</summary>
    [EnumMember(Value = "CUSTOMIZED")]
    Customized
}

/// <summary>
/// Lightweight DNS connector list item.
/// </summary>
public sealed class DnsConnectorListItem {
    /// <summary>Gets or sets the connector name.</summary>
    public string? Name { get; set; }

    /// <summary>Gets or sets optional connector comments.</summary>
    public string? Comments { get; set; }

    /// <summary>Gets or sets the connector identifier.</summary>
    public string? Id { get; set; }

    /// <summary>Gets or sets the connector version.</summary>
    public string? Version { get; set; }

    /// <summary>Gets or sets the connector revision.</summary>
    public string? Revision { get; set; }

    /// <summary>Gets or sets the hostname where the connector is running.</summary>
    public string? Hostname { get; set; }

    /// <summary>Gets or sets the operating system reported by the connector.</summary>
    public string? Os { get; set; }

    /// <summary>Gets or sets the connector status.</summary>
    public DnsConnectorStatus? Status { get; set; }

    /// <summary>Gets or sets the connector delegation mode.</summary>
    public DnsConnectorDelegationMode? DelegationMode { get; set; }
}

/// <summary>
/// Detailed DNS connector information.
/// </summary>
public sealed class DnsConnectorDetails {
    /// <summary>Gets or sets the connector name.</summary>
    public string? Name { get; set; }

    /// <summary>Gets or sets optional connector comments.</summary>
    public string? Comments { get; set; }

    /// <summary>Gets or sets the connector identifier.</summary>
    public string? Id { get; set; }

    /// <summary>Gets or sets the connector version.</summary>
    public string? Version { get; set; }

    /// <summary>Gets or sets the connector revision.</summary>
    public string? Revision { get; set; }

    /// <summary>Gets or sets the hostname where the connector is running.</summary>
    public string? Hostname { get; set; }

    /// <summary>Gets or sets the operating system reported by the connector.</summary>
    public string? Os { get; set; }

    /// <summary>Gets or sets the connector status.</summary>
    public DnsConnectorStatus? Status { get; set; }

    /// <summary>Gets or sets the connector delegation mode.</summary>
    public DnsConnectorDelegationMode? DelegationMode { get; set; }

    /// <summary>
    /// Gets or sets the organizations that have been delegated to this connector
    /// when delegation mode is customized.
    /// </summary>
    public IReadOnlyList<IdName> DelegatedOrganizations { get; set; } = Array.Empty<IdName>();
}
