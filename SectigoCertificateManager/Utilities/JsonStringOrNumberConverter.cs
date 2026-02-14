namespace SectigoCertificateManager.Utilities;

using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Allows deserializing a JSON value that may be either a string or a number into a string.
/// </summary>
public sealed class JsonStringOrNumberConverter : JsonConverter<string?> {
    /// <inheritdoc />
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        return reader.TokenType switch {
            JsonTokenType.Null => null,
            JsonTokenType.String => reader.GetString(),
            JsonTokenType.Number => ReadNumberAsString(ref reader),
            _ => throw new JsonException($"Unexpected token type '{reader.TokenType}' when parsing a string-or-number value.")
        };
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options) {
        if (value is null) {
            writer.WriteNullValue();
            return;
        }
        writer.WriteStringValue(value);
    }

    private static string ReadNumberAsString(ref Utf8JsonReader reader) {
        if (reader.TryGetInt64(out var i64)) {
            return i64.ToString(CultureInfo.InvariantCulture);
        }
        if (reader.TryGetDecimal(out var dec)) {
            return dec.ToString(CultureInfo.InvariantCulture);
        }
        // Fallback: preserve numeric text as-is.
        return reader.GetDouble().ToString("R", CultureInfo.InvariantCulture);
    }
}
