namespace SectigoCertificateManager.AdminApi;

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

/// <summary>
/// Organization selection type for notifications.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum NotificationOrgSelectionType {
    /// <summary>Applies to any organization in the tenant.</summary>
    ANY,
    /// <summary>Applies to any department within organizations.</summary>
    ANYDEPT,
    /// <summary>Applies only to explicitly selected organizations.</summary>
    SELECTED
}

/// <summary>
/// Recipient role for notifications.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum NotificationRecipientRole {
    /// <summary>Certificate approver.</summary>
    APPROVER,
    /// <summary>Certificate requester.</summary>
    REQUESTER,
    /// <summary>Master Registration Authority Officer.</summary>
    MRAO,
    /// <summary>SSL Registration Authority Officer.</summary>
    SSL_RAO,
    /// <summary>SSL Delegated Registration Authority Officer.</summary>
    SSL_DRAO,
    /// <summary>S/MIME Registration Authority Officer.</summary>
    SMIME_RAO,
    /// <summary>S/MIME Delegated Registration Authority Officer.</summary>
    SMIME_DRAO,
    /// <summary>Code-signing Registration Authority Officer.</summary>
    CS_RAO,
    /// <summary>Code-signing Delegated Registration Authority Officer.</summary>
    CS_DRAO,
    /// <summary>Device Registration Authority Officer.</summary>
    DEVICE_RAO,
    /// <summary>Device Delegated Registration Authority Officer.</summary>
    DEVICE_DRAO,
    /// <summary>External requester not represented by a client admin role.</summary>
    EXTERNAL_REQUESTER,
    /// <summary>Organization contact person.</summary>
    ORGANIZATION_CONTACT
}

/// <summary>
/// Notification recipient type.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum NotificationRecipientType {
    /// <summary>Email recipient.</summary>
    EMAIL,
    /// <summary>Slack channel or user recipient.</summary>
    SLACK,
    /// <summary>Microsoft Teams recipient.</summary>
    TEAMS,
    /// <summary>Generic webhook endpoint.</summary>
    WEBHOOK
}

/// <summary>
/// Notification frequency.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum NotificationFrequency {
    /// <summary>Send notification once when the condition is met.</summary>
    ONCE,
    /// <summary>Send notification daily while the condition remains true.</summary>
    DAILY
}

/// <summary>
/// Organization details for a notification.
/// </summary>
public sealed class AdminNotificationOrgDetails {
    /// <summary>Organization selection mode (any, any department, or explicit selection).</summary>
    public NotificationOrgSelectionType? SelectedOrgType { get; set; }

    /// <summary>List of organization or department identifiers targeted by the notification.</summary>
    public IReadOnlyList<int>? OrgDelegations { get; set; }
}

/// <summary>
/// Notification recipient details.
/// </summary>
public sealed class AdminNotificationRecipientDetails {
    /// <summary>Recipient roles to notify (for example, approver, requester, MRAO).</summary>
    public IReadOnlyList<NotificationRecipientRole>? NotifyRoles { get; set; }

    /// <summary>Explicit recipient entries (email, Slack, Teams, webhook).</summary>
    public IReadOnlyList<AdminNotificationRecipient>? Recipients { get; set; }
}

/// <summary>
/// Notification recipient entry.
/// </summary>
public sealed class AdminNotificationRecipient {
    /// <summary>Channel/type of the recipient (email, Slack, Teams, webhook).</summary>
    public NotificationRecipientType Type { get; set; }

    /// <summary>Destination value (email address, webhook URL, or channel identifier).</summary>
    public string? Value { get; set; }
}

/// <summary>
/// Additional notification details.
/// </summary>
public sealed class AdminNotificationAdditionalDetails {
    /// <summary>Day threshold used by the notification (for example, days before expiry).</summary>
    public int? Days { get; set; }

    /// <summary>Certificate type identifier the notification applies to.</summary>
    public int? CertTypeId { get; set; }

    /// <summary>Notification frequency (once or daily).</summary>
    public NotificationFrequency? Freq { get; set; }

    /// <summary>True to notify when revoked by an administrator.</summary>
    public bool? RevokedByAdmin { get; set; }

    /// <summary>True to notify when revoked by the end user.</summary>
    public bool? RevokedByUser { get; set; }
}

/// <summary>
/// Notification definition used when creating or updating a notification.
/// </summary>
public sealed class AdminNotificationRequest {
    /// <summary>Human-readable description of the notification.</summary>
    public string? Description { get; set; }

    /// <summary>True to keep the notification active; false to disable it.</summary>
    public bool? Active { get; set; }

    /// <summary>Organization scope for the notification.</summary>
    public AdminNotificationOrgDetails? OrgData { get; set; }

    /// <summary>Recipients and roles that will receive the notification.</summary>
    public AdminNotificationRecipientDetails? RecipientData { get; set; }

    /// <summary>Additional parameters such as days-to-expiry and certificate type.</summary>
    public AdminNotificationAdditionalDetails? AdditionalData { get; set; }
}

/// <summary>
/// Notification definition returned by the Admin API.
/// </summary>
public sealed class AdminNotification {
    /// <summary>Notification identifier.</summary>
    public int Id { get; set; }

    /// <summary>Human-readable description of the notification.</summary>
    public string? Description { get; set; }

    /// <summary>Indicates whether the notification is currently active.</summary>
    public bool Active { get; set; }

    /// <summary>Organization scope for the notification.</summary>
    public AdminNotificationOrgDetails? OrgData { get; set; }

    /// <summary>Recipients and roles that will receive the notification.</summary>
    public AdminNotificationRecipientDetails? RecipientData { get; set; }

    /// <summary>Additional parameters such as days-to-expiry and certificate type.</summary>
    public AdminNotificationAdditionalDetails? AdditionalData { get; set; }

    /// <summary>
    /// Notification type as a display string (for example, "SSL Certificate Expiration").
    /// </summary>
    public string? Type { get; set; }

    /// <summary>UTC timestamp when the notification was created.</summary>
    public DateTime? Created { get; set; }

    /// <summary>User that created the notification.</summary>
    public string? CreatedBy { get; set; }

    /// <summary>UTC timestamp when the notification was last modified.</summary>
    public DateTime? Modified { get; set; }

    /// <summary>User that last modified the notification.</summary>
    public string? ModifiedBy { get; set; }
}
