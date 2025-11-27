namespace SectigoCertificateManager.Models;

using System;
using System.Collections.Generic;

/// <summary>
/// Represents DCV information for a certificate/domain pair.
/// </summary>
public sealed class AdminSslDcvInfo {
    /// <summary>Gets or sets DCV log details.</summary>
    public AdminSslDcvLog? DcvLog { get; set; }

    /// <summary>Gets or sets DCV instructions.</summary>
    public IReadOnlyList<AdminSslDcvInstruction> Instructions { get; set; } = Array.Empty<AdminSslDcvInstruction>();
}

/// <summary>
/// Represents DCV log information.
/// </summary>
public sealed class AdminSslDcvLog {
    /// <summary>Gets or sets error details, when present.</summary>
    public AdminSslDcvErrorDetails? Error { get; set; }

    /// <summary>Gets or sets log entries.</summary>
    public IReadOnlyList<AdminSslDcvLogEntry> Log { get; set; } = Array.Empty<AdminSslDcvLogEntry>();
}

/// <summary>
/// Represents error details for DCV operations.
/// </summary>
public sealed class AdminSslDcvErrorDetails {
    /// <summary>Gets or sets the error code.</summary>
    public int Code { get; set; }

    /// <summary>Gets or sets the error description.</summary>
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Represents a single DCV log entry.
/// </summary>
public sealed class AdminSslDcvLogEntry {
    /// <summary>Gets or sets the DCV status text.</summary>
    public string? DcvStatus { get; set; }

    /// <summary>Gets or sets the domain name.</summary>
    public string? DomainName { get; set; }

    /// <summary>Gets or sets the time of last DCV check.</summary>
    public DateTimeOffset? LastCheck { get; set; }

    /// <summary>Gets or sets the time of next scheduled DCV check.</summary>
    public DateTimeOffset? NextCheck { get; set; }

    /// <summary>Gets or sets the DCV completion time.</summary>
    public DateTimeOffset? DcvDate { get; set; }

    /// <summary>Gets or sets the DCV email reference number.</summary>
    public int? DcvEmailRefNumber { get; set; }
}

/// <summary>
/// Represents DCV instructions for a domain.
/// </summary>
public sealed class AdminSslDcvInstruction {
    /// <summary>Gets or sets the domain name.</summary>
    public string? DomainName { get; set; }

    /// <summary>Gets or sets the DCV mode (EMAIL, CNAME, HTTP, HTTPS, AUTO, TXT).</summary>
    public string? DcvMode { get; set; }

    /// <summary>Gets or sets the host value used for validation.</summary>
    public string? Host { get; set; }

    /// <summary>Gets or sets the point/target of the validation record.</summary>
    public string? Point { get; set; }

    /// <summary>Gets or sets the URL used for HTTP/HTTPS DCV.</summary>
    public string? Url { get; set; }

    /// <summary>Gets or sets the file name for HTTP/HTTPS DCV.</summary>
    public string? File { get; set; }

    /// <summary>Gets or sets the DCV email address, when applicable.</summary>
    public string? DcvEmail { get; set; }
}

