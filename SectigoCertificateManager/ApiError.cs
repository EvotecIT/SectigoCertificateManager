namespace SectigoCertificateManager;

/// <summary>
/// Represents an error returned by the Sectigo API.
/// </summary>
public sealed class ApiError {
    /// <summary>Gets or sets the numeric error code.</summary>
    public ApiErrorCode Code { get; set; }

    /// <summary>Gets or sets the error description.</summary>
    public string Description { get; set; } = string.Empty;
}