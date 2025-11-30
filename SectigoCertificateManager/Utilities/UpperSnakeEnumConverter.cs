namespace SectigoCertificateManager.Utilities;

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// JSON converter that serializes enums as UPPER_SNAKE_CASE and parses case-insensitively,
/// ignoring underscores when matching back to enum names.
/// </summary>
/// <typeparam name="T">Enum type to convert.</typeparam>
public sealed class UpperSnakeEnumConverter<T> : JsonConverter<T> where T : struct, Enum {
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        var text = reader.GetString();
        if (string.IsNullOrWhiteSpace(text)) {
            throw new JsonException($"Cannot convert empty value to {typeof(T).Name}.");
        }

        // Normalize by stripping underscores and comparing case-insensitively
        var normalized = text.Replace("_", string.Empty, StringComparison.Ordinal);

        foreach (var name in Enum.GetNames(typeof(T))) {
            var candidate = name.Replace("_", string.Empty, StringComparison.Ordinal);
            if (string.Equals(candidate, normalized, StringComparison.OrdinalIgnoreCase)) {
                return (T)Enum.Parse(typeof(T), name, ignoreCase: true);
            }
        }

        throw new JsonException($"Unknown {typeof(T).Name} value '{text}'.");
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options) {
        var name = Enum.GetName(typeof(T), value) ?? value.ToString();
        // Insert underscores before capital letters (except the first), then uppercase.
        var snake = UpperSnake(name);
        writer.WriteStringValue(snake);
    }

    private static string UpperSnake(string name) {
        Span<char> buffer = stackalloc char[name.Length * 2];
        var idx = 0;
        for (var i = 0; i < name.Length; i++) {
            var c = name[i];
            if (i > 0 && char.IsUpper(c) && !char.IsUpper(name[i - 1])) {
                buffer[idx++] = '_';
            }
            buffer[idx++] = char.ToUpperInvariant(c);
        }

        return buffer[..idx].ToString();
    }
}
