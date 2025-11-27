namespace SectigoCertificateManager.AdminApi;

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

/// <summary>
/// Administrator account type.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AdminAccountType {
    Standard,
    Api,
    Sas,
    Idp
}

/// <summary>
/// Administrator active state.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AdminActiveState {
    Active,
    Suspended
}

/// <summary>
/// Password state for administrator accounts.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AdminPasswordState {
    ALIVE,
    EXPIRED,
    NEVER_EXPIRE
}

/// <summary>
/// Administrator role.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AdminRole {
    MRAO,
    RAO_SSL,
    RAO_SMIME,
    RAO_CS,
    RAO_DEVICE,
    DRAO_SSL,
    DRAO_SMIME,
    DRAO_CS,
    DRAO_DEVICE
}

/// <summary>
/// Basic administrator identity returned from the list endpoint.
/// </summary>
public sealed class AdminIdentity {
    public int Id { get; set; }

    public AdminAccountType? Type { get; set; }

    public string? Login { get; set; }

    public string? Email { get; set; }

    public string? Forename { get; set; }

    public string? Surname { get; set; }

    public AdminActiveState? ActiveState { get; set; }
}

/// <summary>
/// Detailed administrator information.
/// </summary>
public sealed class AdminDetails {
    public int Id { get; set; }

    public AdminAccountType? Type { get; set; }

    public string? Status { get; set; }

    public string? Forename { get; set; }

    public string? Surname { get; set; }

    public string? Login { get; set; }

    public string? Email { get; set; }

    public string? Title { get; set; }

    public string? Address { get; set; }

    public string? City { get; set; }

    public string? State { get; set; }

    public string? Country { get; set; }

    public string? Zip { get; set; }

    public string? Phone { get; set; }

    public string? Locale { get; set; }

    public DateTime? Created { get; set; }

    public DateTime? Modified { get; set; }

    public DateTime? Deleted { get; set; }

    public DateTime? Reseted { get; set; }

    public DateTime? LastPasswordChange { get; set; }

    public IReadOnlyList<AdminCredential> Credentials { get; set; } = Array.Empty<AdminCredential>();

    public AdminPasswordState? PasswordState { get; set; }

    public DateTime? PasswordExpiryDate { get; set; }

    public int? ClientAdminCreator { get; set; }

    public string? AuthCert { get; set; }

    public AdminActiveState? ActiveState { get; set; }

    public IReadOnlyList<string> Privileges { get; set; } = Array.Empty<string>();

    public DateTime? FailedDate { get; set; }

    public int? FailedAttempts { get; set; }

    public int? IdentityProviderId { get; set; }

    public string? Idp { get; set; }

    public string? IdpPersonId { get; set; }

    public string? Relationship { get; set; }

    public DateTime? IdpLoginInvited { get; set; }

    public string? ActiveStatus { get; set; }

    public int? TemplateId { get; set; }
}

/// <summary>
/// Administrator role + organization/department context.
/// </summary>
public sealed class AdminCredential {
    public string? Role { get; set; }

    public int? OrgId { get; set; }
}

/// <summary>
/// Payload used to create or update administrators.
/// </summary>
public sealed class AdminCreateOrUpdateRequest {
    public AdminAccountType? Type { get; set; }

    public string? Login { get; set; }

    public string? Email { get; set; }

    public string? Forename { get; set; }

    public string? Surname { get; set; }

    public string? Title { get; set; }

    public string? Telephone { get; set; }

    public string? Street { get; set; }

    public string? Locality { get; set; }

    public string? State { get; set; }

    public string? PostalCode { get; set; }

    public string? Country { get; set; }

    public string? Relationship { get; set; }

    public string? CertificateSerialNumber { get; set; }

    public string? Password { get; set; }

    public IReadOnlyList<string>? Privileges { get; set; }

    public IReadOnlyList<AdminCredential>? Credentials { get; set; }

    public int? IdentityProviderId { get; set; }

    public string? IdpPersonId { get; set; }

    public AdminActiveState? ActiveState { get; set; }
}

/// <summary>
/// Request payload used to change the current administrator's password.
/// </summary>
public sealed class AdminChangePasswordRequest {
    public string? NewPassword { get; set; }
}

/// <summary>
/// Describes available privileges in the system.
/// </summary>
public sealed class AdminPrivilegeDescription {
    public string? Name { get; set; }

    public string? Description { get; set; }
}

/// <summary>
/// Identity provider info descriptor.
/// </summary>
public sealed class AdminIdpInfo {
    public int Id { get; set; }

    public string? Name { get; set; }
}

/// <summary>
/// Represents the administrator password status.
/// </summary>
public sealed class AdminPasswordStatus {
    public DateTime? ExpirationDate { get; set; }

    public AdminPasswordState? State { get; set; }
}
