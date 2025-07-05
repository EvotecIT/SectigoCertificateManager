namespace SectigoCertificateManager;

using System;

/// <summary>
/// Represents an authentication token and its expiration time.
/// </summary>
public sealed class TokenInfo {
    /// <summary>Initializes a new instance of the <see cref="TokenInfo"/> class.</summary>
    /// <param name="token">Token value.</param>
    /// <param name="expiresAt">UTC time when the token expires.</param>
    public TokenInfo(string token, DateTimeOffset expiresAt) {
        Token = token;
        ExpiresAt = expiresAt;
    }

    /// <summary>Gets the token value.</summary>
    public string Token { get; }

    /// <summary>Gets the expiration time of the token.</summary>
    public DateTimeOffset ExpiresAt { get; }
}
