namespace SectigoCertificateManager;

using System;

/// <summary>
/// Exception thrown when a configuration file cannot be parsed.
/// </summary>
public sealed class ConfigParseException : Exception {
    /// <summary>Gets the path to the configuration file.</summary>
    public string Path { get; }

    /// <summary>Initializes a new instance of the <see cref="ConfigParseException"/> class.</summary>
    /// <param name="path">Path to the configuration file.</param>
    /// <param name="inner">The exception that caused the parsing failure.</param>
    public ConfigParseException(string path, Exception inner)
        : base($"Invalid configuration file: {path}", inner) {
        Path = path;
    }
}
