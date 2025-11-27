namespace SectigoCertificateManager.AdminApi;

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

/// <summary>
/// Administrator account type.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AdminAccountType {
    /// <summary>Standard interactive administrator account.</summary>
    Standard,

    /// <summary>API-only administrator account used for scripted integrations.</summary>
    Api,

    /// <summary>Administrator account managed via Sectigo SaaS integrations.</summary>
    Sas,

    /// <summary>Administrator account federated through an external identity provider (IdP).</summary>
    Idp
}

/// <summary>
/// Administrator active state.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AdminActiveState {
    /// <summary>Account is active and allowed to sign in.</summary>
    Active,

    /// <summary>Account is suspended and cannot sign in.</summary>
    Suspended
}

/// <summary>
/// Password state for administrator accounts.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AdminPasswordState {
    /// <summary>Password is valid and not expired.</summary>
    ALIVE,

    /// <summary>Password has expired.</summary>
    EXPIRED,

    /// <summary>Password is configured to never expire.</summary>
    NEVER_EXPIRE
}

/// <summary>
/// Administrator role.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AdminRole {
    /// <summary>Master Registration Authority Officer.</summary>
    MRAO,

    /// <summary>Registration Authority Officer for SSL.</summary>
    RAO_SSL,

    /// <summary>Delegated Registration Authority Officer for S/MIME.</summary>
    RAO_SMIME,

    /// <summary>Registration Authority Officer for code-signing.</summary>
    RAO_CS,

    /// <summary>Registration Authority Officer for device certificates.</summary>
    RAO_DEVICE,

    /// <summary>Delegated Registration Authority Officer for SSL.</summary>
    DRAO_SSL,

    /// <summary>Delegated Registration Authority Officer for S/MIME.</summary>
    DRAO_SMIME,

    /// <summary>Delegated Registration Authority Officer for code-signing.</summary>
    DRAO_CS,

    /// <summary>Delegated Registration Authority Officer for device certificates.</summary>
    DRAO_DEVICE
}

/// <summary>
/// Basic administrator identity returned from the list endpoint.
/// </summary>
public sealed class AdminIdentity {
    /// <summary>Unique numeric identifier of the administrator.</summary>
    public int Id { get; set; }

    /// <summary>Account type (standard, API, SAS, or IdP).</summary>
    public AdminAccountType? Type { get; set; }

    /// <summary>Login name used to sign in to the portal.</summary>
    public string? Login { get; set; }

    /// <summary>Primary email address associated with the administrator.</summary>
    public string? Email { get; set; }

    /// <summary>Given name of the administrator.</summary>
    public string? Forename { get; set; }

    /// <summary>Surname of the administrator.</summary>
    public string? Surname { get; set; }

    /// <summary>Active state flag (active or suspended).</summary>
    public AdminActiveState? ActiveState { get; set; }
}

/// <summary>
/// Detailed administrator information.
/// </summary>
public sealed class AdminDetails {
    /// <summary>Unique numeric identifier of the administrator.</summary>
    public int Id { get; set; }

    /// <summary>Account type (standard, API, SAS, or IdP).</summary>
    public AdminAccountType? Type { get; set; }

    /// <summary>Account status text as reported by the API.</summary>
    public string? Status { get; set; }

    /// <summary>Given name of the administrator.</summary>
    public string? Forename { get; set; }

    /// <summary>Surname of the administrator.</summary>
    public string? Surname { get; set; }

    /// <summary>Login name used to sign in.</summary>
    public string? Login { get; set; }

    /// <summary>Primary email address of the administrator.</summary>
    public string? Email { get; set; }

    /// <summary>Job title or role description.</summary>
    public string? Title { get; set; }

    /// <summary>Street address line.</summary>
    public string? Address { get; set; }

    /// <summary>City or locality.</summary>
    public string? City { get; set; }

    /// <summary>State, province, or region.</summary>
    public string? State { get; set; }

    /// <summary>Country or region code.</summary>
    public string? Country { get; set; }

    /// <summary>Postal or ZIP code.</summary>
    public string? Zip { get; set; }

    /// <summary>Contact telephone number.</summary>
    public string? Phone { get; set; }

    /// <summary>Locale (language and region) preference.</summary>
    public string? Locale { get; set; }

    /// <summary>UTC timestamp when the administrator was created.</summary>
    public DateTime? Created { get; set; }

    /// <summary>UTC timestamp of the last modification.</summary>
    public DateTime? Modified { get; set; }

    /// <summary>UTC timestamp when the administrator was deleted, if applicable.</summary>
    public DateTime? Deleted { get; set; }

    /// <summary>UTC timestamp of the last reset event.</summary>
    public DateTime? Reseted { get; set; }

    /// <summary>UTC timestamp of the last password change.</summary>
    public DateTime? LastPasswordChange { get; set; }

    /// <summary>Assigned roles and organization/department scopes.</summary>
    public IReadOnlyList<AdminCredential> Credentials { get; set; } = Array.Empty<AdminCredential>();

    /// <summary>Password state for the administrator account.</summary>
    public AdminPasswordState? PasswordState { get; set; }

    /// <summary>UTC date when the current password will expire.</summary>
    public DateTime? PasswordExpiryDate { get; set; }

    /// <summary>Identifier of the client administrator who created this account, if available.</summary>
    public int? ClientAdminCreator { get; set; }

    /// <summary>Authentication certificate identifier or thumbprint, when cert-based auth is enabled.</summary>
    public string? AuthCert { get; set; }

    /// <summary>Active state of the administrator.</summary>
    public AdminActiveState? ActiveState { get; set; }

    /// <summary>Collection of privilege names granted to the administrator.</summary>
    public IReadOnlyList<string> Privileges { get; set; } = Array.Empty<string>();

    /// <summary>UTC timestamp of the last failed login attempt.</summary>
    public DateTime? FailedDate { get; set; }

    /// <summary>Number of consecutive failed login attempts.</summary>
    public int? FailedAttempts { get; set; }

    /// <summary>Identifier of the associated identity provider configuration, if any.</summary>
    public int? IdentityProviderId { get; set; }

    /// <summary>Name of the identity provider.</summary>
    public string? Idp { get; set; }

    /// <summary>Identifier of the user in the external identity provider.</summary>
    public string? IdpPersonId { get; set; }

    /// <summary>Relationship of the administrator to the organization (for example, employee or contractor).</summary>
    public string? Relationship { get; set; }

    /// <summary>UTC timestamp when the IdP login invitation was sent.</summary>
    public DateTime? IdpLoginInvited { get; set; }

    /// <summary>Localized active status string as returned by the API.</summary>
    public string? ActiveStatus { get; set; }

    /// <summary>Identifier of the administrator template associated with this account, if any.</summary>
    public int? TemplateId { get; set; }
}

/// <summary>
/// Administrator role + organization/department context.
/// </summary>
public sealed class AdminCredential {
    /// <summary>Role name (for example, MRAO, RAO_SSL, DRAO_DEVICE).</summary>
    public string? Role { get; set; }

    /// <summary>Identifier of the organization associated with the role.</summary>
    public int? OrgId { get; set; }
}

/// <summary>
/// Payload used to create or update administrators.
/// </summary>
public sealed class AdminCreateOrUpdateRequest {
    /// <summary>Type of administrator account to create or update.</summary>
    public AdminAccountType? Type { get; set; }

    /// <summary>Login name for the administrator.</summary>
    public string? Login { get; set; }

    /// <summary>Primary email address of the administrator.</summary>
    public string? Email { get; set; }

    /// <summary>Given name of the administrator.</summary>
    public string? Forename { get; set; }

    /// <summary>Surname of the administrator.</summary>
    public string? Surname { get; set; }

    /// <summary>Job title or role description.</summary>
    public string? Title { get; set; }

    /// <summary>Contact telephone number.</summary>
    public string? Telephone { get; set; }

    /// <summary>Street address line.</summary>
    public string? Street { get; set; }

    /// <summary>City or locality.</summary>
    public string? Locality { get; set; }

    /// <summary>State, province, or region.</summary>
    public string? State { get; set; }

    /// <summary>Postal or ZIP code.</summary>
    public string? PostalCode { get; set; }

    /// <summary>Country or region code.</summary>
    public string? Country { get; set; }

    /// <summary>Relationship of the administrator to the organization.</summary>
    public string? Relationship { get; set; }

    /// <summary>Serial number of the certificate used for authentication, if applicable.</summary>
    public string? CertificateSerialNumber { get; set; }

    /// <summary>Initial or replacement password for the administrator.</summary>
    public string? Password { get; set; }

    /// <summary>Privilege names assigned to the administrator.</summary>
    public IReadOnlyList<string>? Privileges { get; set; }

    /// <summary>Role and organization mappings for the administrator.</summary>
    public IReadOnlyList<AdminCredential>? Credentials { get; set; }

    /// <summary>Identifier of the associated identity provider configuration, if any.</summary>
    public int? IdentityProviderId { get; set; }

    /// <summary>Identifier of the user in the external identity provider.</summary>
    public string? IdpPersonId { get; set; }

    /// <summary>Desired active state for the administrator account.</summary>
    public AdminActiveState? ActiveState { get; set; }
}

/// <summary>
/// Request payload used to change the current administrator's password.
/// </summary>
public sealed class AdminChangePasswordRequest {
    /// <summary>New password that replaces the administrator's existing password.</summary>
    public string? NewPassword { get; set; }
}

/// <summary>
/// Describes available privileges in the system.
/// </summary>
public sealed class AdminPrivilegeDescription {
    /// <summary>Privilege name, as returned by the API (for example, MANAGE_USERS).</summary>
    public string? Name { get; set; }

    /// <summary>Human-readable description of the privilege.</summary>
    public string? Description { get; set; }
}

/// <summary>
/// Identity provider info descriptor.
/// </summary>
public sealed class AdminIdpInfo {
    /// <summary>Identifier of the configured identity provider.</summary>
    public int Id { get; set; }

    /// <summary>Display name of the identity provider.</summary>
    public string? Name { get; set; }
}

/// <summary>
/// Represents the administrator password status.
/// </summary>
public sealed class AdminPasswordStatus {
    /// <summary>UTC expiration date for the administrator's current password.</summary>
    public DateTime? ExpirationDate { get; set; }

    /// <summary>Current password state (alive, expired, never expire).</summary>
    public AdminPasswordState? State { get; set; }
}
