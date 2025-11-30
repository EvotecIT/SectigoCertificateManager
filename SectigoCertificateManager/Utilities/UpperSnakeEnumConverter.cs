namespace SectigoCertificateManager.Utilities;

using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// JSON converter that serializes enums as UPPER_SNAKE_CASE and parses case-insensitively,
/// ignoring underscores when matching back to enum names.
/// </summary>
/// <typeparam name="T">Enum type to convert.</typeparam>
public sealed class UpperSnakeEnumConverter<T> : JsonConverter<T> where T : struct, Enum {
    /// <summary>
    /// Reads an enum value from JSON that is formatted as upper snake case.
    /// Comparison is case-insensitive and ignores underscores so that both
    /// <c>API_USER</c> and <c>apiuser</c> map to the same value.
    /// </summary>
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        var text = reader.GetString();
        if (string.IsNullOrWhiteSpace(text)) {
            throw new JsonException($"Cannot convert empty value to {typeof(T).Name}.");
        }

        // Normalize by stripping underscores and comparing case-insensitively
        var normalized = RemoveUnderscores(text!).ToUpperInvariant();

        foreach (var name in Enum.GetNames(typeof(T))) {
            var candidate = RemoveUnderscores(name).ToUpperInvariant();
            if (candidate == normalized) {
                return (T)Enum.Parse(typeof(T), name, ignoreCase: true);
            }
        }

        throw new JsonException($"Unknown {typeof(T).Name} value '{text}'.");
    }

    /// <summary>
    /// Writes an enum value as upper snake case (e.g., <c>ApiUser</c> → <c>API_USER</c>).
    /// </summary>
    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options) {
        var name = Enum.GetName(typeof(T), value) ?? value.ToString();
        writer.WriteStringValue(ToUpperSnake(name));
    }

    private static string RemoveUnderscores(string value) {
        var builder = new StringBuilder(value.Length);
        foreach (var c in value) {
            if (c != '_') {
                builder.Append(c);
            }
        }
        return builder.ToString();
    }

    private static string ToUpperSnake(string name) {
        var builder = new StringBuilder(name.Length * 2);

        for (var i = 0; i < name.Length; i++) {
            var c = name[i];
            var isUpper = char.IsUpper(c);
            var prev = i > 0 ? name[i - 1] : char.MinValue;
            var needSeparator = i > 0 && isUpper && (!char.IsUpper(prev) && prev != '_');

            if (needSeparator) {
                builder.Append('_');
            }

            builder.Append(char.ToUpperInvariant(c));
        }

        return builder.ToString();
    }
}
