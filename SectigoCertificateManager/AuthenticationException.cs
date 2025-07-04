namespace SectigoCertificateManager;

/// <summary>
/// Exception thrown when authentication fails.
/// </summary>
public sealed class AuthenticationException : ApiException {
    /// <summary>Initializes a new instance of the <see cref="AuthenticationException"/> class.</summary>
    /// <param name="error">Error information returned by the API.</param>
    public AuthenticationException(ApiError error)
        : base(error) {
    }
}
