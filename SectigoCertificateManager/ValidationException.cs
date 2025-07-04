namespace SectigoCertificateManager;

/// <summary>
/// Exception thrown when a request fails validation.
/// </summary>
public sealed class ValidationException : ApiException {
    /// <summary>Initializes a new instance of the <see cref="ValidationException"/> class.</summary>
    /// <param name="error">Error information returned by the API.</param>
    public ValidationException(ApiError error)
        : base(error) {
    }
}
