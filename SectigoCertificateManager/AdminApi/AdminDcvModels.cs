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

/// <summary>
/// Represents HTTP-based DCV response details (HTTP/HTTPS).
/// </summary>
public sealed class DomainHttpResponse {
    /// <summary>Gets or sets the validation URL.</summary>
    public string? Url { get; set; }

    /// <summary>Gets or sets the first line of the validation file content.</summary>
    public string? FirstLine { get; set; }

    /// <summary>Gets or sets the second line of the validation file content.</summary>
    public string? SecondLine { get; set; }
}

/// <summary>
/// Represents HTTPS-based DCV response details.
/// </summary>
public sealed class DomainHttpsResponse {
    /// <summary>Gets or sets the validation URL.</summary>
    public string? Url { get; set; }

    /// <summary>Gets or sets the first line of the validation file content.</summary>
    public string? FirstLine { get; set; }

    /// <summary>Gets or sets the second line of the validation file content.</summary>
    public string? SecondLine { get; set; }
}

/// <summary>
/// Represents DCV email options for a domain.
/// </summary>
public sealed class DomainDcvEmails {
    /// <summary>Domain name for which email validation options are provided.</summary>
    public string? DomainName { get; set; }

    /// <summary>WHOIS email addresses available for DCV.</summary>
    public IReadOnlyList<string> WhoisEmails { get; set; } = Array.Empty<string>();

    /// <summary>Administrative email addresses (admin/administrator/webmaster/etc.).</summary>
    public IReadOnlyList<string> AdminEmails { get; set; } = Array.Empty<string>();

    /// <summary>DCV email addresses discovered from DNS TXT records.</summary>
    public IReadOnlyList<string> DnsTxtEmails { get; set; } = Array.Empty<string>();
}

/// <summary>
/// Represents the response returned for email DCV start/submit operations.
/// </summary>
public sealed class DomainEmailResponse {
    /// <summary>All eligible email addresses for the requested domain.</summary>
    public IReadOnlyList<string> Emails { get; set; } = Array.Empty<string>();

    /// <summary>Grouped DCV email options per domain when multiple domains are involved.</summary>
    public IReadOnlyList<DomainDcvEmails> DcvEmails { get; set; } = Array.Empty<DomainDcvEmails>();
}

/// <summary>
/// Represents CNAME DCV response details.
/// </summary>
public sealed class DomainCnameResponse {
    /// <summary>Host name to create for the CNAME DCV record.</summary>
    public string? Host { get; set; }

    /// <summary>Target value that the CNAME record should point to.</summary>
    public string? Point { get; set; }
}

/// <summary>
/// Represents the result of DCV submit operations.
/// </summary>
public sealed class SubmitDomainResponse {
    /// <summary>Overall DCV status (for example, PENDING, COMPLETED).</summary>
    public string? Status { get; set; }

    /// <summary>Order-level status returned by the API.</summary>
    public string? OrderStatus { get; set; }

    /// <summary>Message returned by the API describing the outcome.</summary>
    public string? Message { get; set; }

    /// <summary>Backend order identifier associated with the DCV request.</summary>
    public string? OrderBackendId { get; set; }

    /// <summary>Email validation reference number, when email DCV is used.</summary>
    public string? EmailValidationReferenceNumber { get; set; }

    /// <summary>Host value for CNAME/TXT validation (when applicable).</summary>
    public string? Host { get; set; }

    /// <summary>Target value for CNAME/TXT validation (when applicable).</summary>
    public string? Point { get; set; }
}

/// <summary>
/// Represents a simple message response (used by TXT start).
/// </summary>
public sealed class ResponseMessage {
    /// <summary>Message text returned by the API.</summary>
    public string? Message { get; set; }
}
