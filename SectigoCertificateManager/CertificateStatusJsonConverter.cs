namespace SectigoCertificateManager;

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Preserves legacy numeric wire codes while exposing unique status values to callers.
/// </summary>
public sealed class CertificateStatusJsonConverter : JsonConverter<CertificateStatus> {
    /// <inheritdoc />
    public override CertificateStatus Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        if (reader.TokenType == JsonTokenType.String) {
            string? value = reader.GetString();
            if (int.TryParse(value, out int numericCode)) {
                return FromLegacyNumericCode(numericCode);
            }

            if (Enum.TryParse(value, ignoreCase: true, out CertificateStatus status)) {
                return status;
            }

            throw new JsonException($"Unknown certificate status '{value}'.");
        }

        if (reader.TokenType == JsonTokenType.Number && reader.TryGetInt32(out int code)) {
            return FromLegacyNumericCode(code);
        }

        throw new JsonException("Certificate status must be a string name or legacy numeric code.");
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, CertificateStatus value, JsonSerializerOptions options) {
        writer.WriteStringValue(value.ToString());
    }

    private static CertificateStatus FromLegacyNumericCode(int code) => code switch {
        0 => CertificateStatus.Any,
        1 => CertificateStatus.Requested,
        2 => CertificateStatus.Issued,
        3 => CertificateStatus.Revoked,
        4 => CertificateStatus.Expired,
        5 => CertificateStatus.EnrolledPendingDownload,
        6 => CertificateStatus.NotEnrolled,
        7 => CertificateStatus.AwaitingApproval,
        8 => CertificateStatus.Approved,
        9 => CertificateStatus.Applied,
        10 => CertificateStatus.Downloaded,
        11 => CertificateStatus.External,
        _ => throw new JsonException($"Unknown numeric certificate status '{code}'.")
    };
}
