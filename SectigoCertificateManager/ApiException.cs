namespace SectigoCertificateManager;

using System;

/// <summary>
/// Base type for API related exceptions.
/// </summary>
public class ApiException : Exception
{
    /// <summary>Gets the API error code.</summary>
    public int ErrorCode { get; }

    /// <summary>Initializes a new instance of the <see cref="ApiException"/> class.</summary>
    /// <param name="error">Error information returned by the API.</param>
    public ApiException(ApiError error)
        : base(error.Description)
    {
        ErrorCode = error.Code;
    }
}

/// <summary>Exception thrown when authentication fails.</summary>
public sealed class AuthenticationException : ApiException
{
    /// <summary>Initializes a new instance of the <see cref="AuthenticationException"/> class.</summary>
    /// <param name="error">Error information returned by the API.</param>
    public AuthenticationException(ApiError error)
        : base(error)
    {
    }
}

/// <summary>Exception thrown when a request fails validation.</summary>
public sealed class ValidationException : ApiException
{
    /// <summary>Initializes a new instance of the <see cref="ValidationException"/> class.</summary>
    /// <param name="error">Error information returned by the API.</param>
    public ValidationException(ApiError error)
        : base(error)
    {
    }
}
