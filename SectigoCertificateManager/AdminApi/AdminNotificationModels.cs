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
    public NotificationOrgSelectionType? SelectedOrgType { get; set; }

    public IReadOnlyList<int>? OrgDelegations { get; set; }
}

/// <summary>
/// Notification recipient details.
/// </summary>
public sealed class AdminNotificationRecipientDetails {
    public IReadOnlyList<NotificationRecipientRole>? NotifyRoles { get; set; }

    public IReadOnlyList<AdminNotificationRecipient>? Recipients { get; set; }
}

/// <summary>
/// Notification recipient entry.
/// </summary>
public sealed class AdminNotificationRecipient {
    public NotificationRecipientType Type { get; set; }

    public string? Value { get; set; }
}

/// <summary>
/// Additional notification details.
/// </summary>
public sealed class AdminNotificationAdditionalDetails {
    public int? Days { get; set; }

    public int? CertTypeId { get; set; }

    public NotificationFrequency? Freq { get; set; }

    public bool? RevokedByAdmin { get; set; }

    public bool? RevokedByUser { get; set; }
}

/// <summary>
/// Notification definition used when creating or updating a notification.
/// </summary>
public sealed class AdminNotificationRequest {
    public string? Description { get; set; }

    public bool? Active { get; set; }

    public AdminNotificationOrgDetails? OrgData { get; set; }

    public AdminNotificationRecipientDetails? RecipientData { get; set; }

    public AdminNotificationAdditionalDetails? AdditionalData { get; set; }
}

/// <summary>
/// Notification definition returned by the Admin API.
/// </summary>
public sealed class AdminNotification {
    public int Id { get; set; }

    public string? Description { get; set; }

    public bool Active { get; set; }

    public AdminNotificationOrgDetails? OrgData { get; set; }

    public AdminNotificationRecipientDetails? RecipientData { get; set; }

    public AdminNotificationAdditionalDetails? AdditionalData { get; set; }

    /// <summary>
    /// Notification type as a display string (for example, "SSL Certificate Expiration").
    /// </summary>
    public string? Type { get; set; }

    public DateTime? Created { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? Modified { get; set; }

    public string? ModifiedBy { get; set; }
}
