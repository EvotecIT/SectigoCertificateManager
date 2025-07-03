namespace SectigoCertificateManager;

using System;
using System.Net;

public class ApiException : Exception
{
    public HttpStatusCode StatusCode { get; }

    public ApiException(string message, HttpStatusCode statusCode)
        : base(message)
    {
        StatusCode = statusCode;
    }
}

public sealed class ApiAuthenticationException : ApiException
{
    public ApiAuthenticationException(string message, HttpStatusCode statusCode)
        : base(message, statusCode)
    {
    }
}

public sealed class ApiValidationException : ApiException
{
    public ApiValidationException(string message, HttpStatusCode statusCode)
        : base(message, statusCode)
    {
    }
}
