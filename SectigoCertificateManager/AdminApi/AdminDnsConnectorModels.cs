namespace SectigoCertificateManager.AdminApi;

using System;
using System.Collections.Generic;

/// <summary>
/// Minimal id/name pair used across Admin API responses.
/// </summary>
public sealed class IdName {
    public int Id { get; set; }

    public string? Name { get; set; }
}

/// <summary>
/// DNS connector status as reported by the Admin API.
/// </summary>
public enum DnsConnectorStatus {
    NotAvailable,
    NotConnected,
    Connected
}

/// <summary>
/// DNS connector delegation mode.
/// </summary>
public enum DnsConnectorDelegationMode {
    GlobalForCustomer,
    Customized
}

/// <summary>
/// Lightweight DNS connector list item.
/// </summary>
public sealed class DnsConnectorListItem {
    public string? Name { get; set; }

    public string? Comments { get; set; }

    public string? Id { get; set; }

    public string? Version { get; set; }

    public string? Revision { get; set; }

    public string? Hostname { get; set; }

    public string? Os { get; set; }

    public DnsConnectorStatus? Status { get; set; }

    public DnsConnectorDelegationMode? DelegationMode { get; set; }
}

/// <summary>
/// Detailed DNS connector information.
/// </summary>
public sealed class DnsConnectorDetails {
    public string? Name { get; set; }

    public string? Comments { get; set; }

    public string? Id { get; set; }

    public string? Version { get; set; }

    public string? Revision { get; set; }

    public string? Hostname { get; set; }

    public string? Os { get; set; }

    public DnsConnectorStatus? Status { get; set; }

    public DnsConnectorDelegationMode? DelegationMode { get; set; }

    public IReadOnlyList<IdName> DelegatedOrganizations { get; set; } = Array.Empty<IdName>();
}
