namespace SectigoCertificateManager.AdminApi;

using System;
using System.Collections.Generic;

/// <summary>
/// Represents a single domain validation entry returned by the DCV list endpoint.
/// </summary>
public sealed class AdminDcvValidationSummary {
    /// <summary>Gets or sets the domain name.</summary>
    public string? Domain { get; set; }

    /// <summary>Gets or sets the DCV status.</summary>
    public string? DcvStatus { get; set; }

    /// <summary>Gets or sets the validation order status.</summary>
    public string? OrderStatus { get; set; }

    /// <summary>Gets or sets the expiration date, when available.</summary>
    public string? Expires { get; set; }
}

/// <summary>
/// Represents domain validation status details.
/// </summary>
public sealed class AdminDcvStatus {
    /// <summary>Gets or sets the domain validation status.</summary>
    public string? Status { get; set; }

    /// <summary>Gets or sets the validation date.</summary>
    public DateTimeOffset? ValidationDate { get; set; }

    /// <summary>Gets or sets the validation expiration date.</summary>
    public DateTimeOffset? ExpirationDate { get; set; }

    /// <summary>Gets or sets the validation order status.</summary>
    public string? OrderStatus { get; set; }

    /// <summary>Gets or sets the validation order mode.</summary>
    public string? OrderMode { get; set; }

    /// <summary>Gets or sets the validation host value (for DNS based methods).</summary>
    public string? Host { get; set; }

    /// <summary>Gets or sets the validation point value (for DNS based methods).</summary>
    public string? Point { get; set; }

    /// <summary>Gets or sets the validation URL (for HTTP(S) based methods).</summary>
    public string? Url { get; set; }

    /// <summary>Gets or sets the first line of the validation file content.</summary>
    public string? FirstLine { get; set; }

    /// <summary>Gets or sets the second line of the validation file content.</summary>
    public string? SecondLine { get; set; }

    /// <summary>Gets or sets the validation email address, when applicable.</summary>
    public string? ValidationEmail { get; set; }

    /// <summary>Gets or sets the email validation reference number.</summary>
    public string? EmailValidationReferenceNumber { get; set; }
}

