namespace SectigoCertificateManager.Responses;

using SectigoCertificateManager.Models;

/// <summary>
/// Represents a response containing profile information.
/// </summary>
public sealed class ProfileResponse {
    /// <summary>
    /// Gets or sets the profile.
    /// </summary>
    public Profile? Profile { get; set; }
}