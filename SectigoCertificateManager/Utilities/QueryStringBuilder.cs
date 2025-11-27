namespace SectigoCertificateManager.Utilities;

using System;
using System.Text;

/// <summary>
/// Helper for building query string URLs in a composable way.
/// </summary>
internal static class QueryStringBuilder {
    public static string Build(string basePath, Action<QueryBuilder> configure) {
        if (basePath is null) {
            throw new ArgumentNullException(nameof(basePath));
        }

        if (configure is null) {
            throw new ArgumentNullException(nameof(configure));
        }

        var builder = new QueryBuilder(basePath);
        configure(builder);
        return builder.ToString();
    }

    internal sealed class QueryBuilder {
        private readonly StringBuilder _builder;
        private bool _hasQuery;

        public QueryBuilder(string basePath) {
            _builder = new StringBuilder(basePath);
        }

        public QueryBuilder AddInt(string name, int? value) {
            if (!value.HasValue) {
                return this;
            }

            Append(name, value.Value.ToString());
            return this;
        }

        public QueryBuilder AddString(string name, string? value) {
            if (string.IsNullOrWhiteSpace(value)) {
                return this;
            }

            Append(name, Uri.EscapeDataString(value));
            return this;
        }

        private void Append(string name, string value) {
            _ = _hasQuery ? _builder.Append('&') : _builder.Append('?');
            _builder.Append(name).Append('=').Append(value);
            _hasQuery = true;
        }

        public override string ToString() => _builder.ToString();
    }
}

