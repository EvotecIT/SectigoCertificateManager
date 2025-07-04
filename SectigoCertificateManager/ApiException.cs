namespace SectigoCertificateManager;

using System;

/// <summary>
/// Base type for API related exceptions.
/// </summary>
public class ApiException : Exception {
    /// <summary>Gets the API error code.</summary>
    public int ErrorCode { get; }

    /// <summary>Initializes a new instance of the <see cref="ApiException"/> class.</summary>
    /// <param name="error">Error information returned by the API.</param>
    public ApiException(ApiError error)
        : base(error.Description) {
        ErrorCode = error.Code;
    }
}
