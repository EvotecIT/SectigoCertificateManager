namespace SectigoCertificateManager.Clients;

using SectigoCertificateManager.Models;
using SectigoCertificateManager.Utilities;
using System.Runtime.CompilerServices;

/// <summary>
/// Provides access to audit log endpoints.
/// </summary>
public sealed class AuditClient : BaseClient {
    /// <summary>
    /// Initializes a new instance of the <see cref="AuditClient"/> class.
    /// </summary>
    /// <param name="client">HTTP client wrapper.</param>
    public AuditClient(ISectigoClient client) : base(client) {
    }

    /// <summary>
    /// Retrieves a single page of audit log entries.
    /// </summary>
    /// <param name="from">Optional start date filter.</param>
    /// <param name="to">Optional end date filter.</param>
    /// <param name="position">Optional result offset.</param>
    /// <param name="size">Optional page size.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<IReadOnlyList<AuditLogEntry>> GetAsync(
        DateTimeOffset? from = null,
        DateTimeOffset? to = null,
        int? position = null,
        int? size = null,
        CancellationToken cancellationToken = default) {
        var query = new List<string>();
        if (from.HasValue) {
            query.Add($"from={Uri.EscapeDataString(from.Value.ToString("yyyy-MM-dd"))}");
        }
        if (to.HasValue) {
            query.Add($"to={Uri.EscapeDataString(to.Value.ToString("yyyy-MM-dd"))}");
        }
        if (position.HasValue) {
            query.Add($"position={position.Value}");
        }
        if (size.HasValue) {
            query.Add($"size={size.Value}");
        }
        var path = "report/v1/activity";
        if (query.Count > 0) {
            path += "?" + string.Join("&", query);
        }

        var response = await _client.GetAsync(path, cancellationToken).ConfigureAwait(false);
        var root = await response.Content
            .ReadFromJsonAsyncSafe<ActivityLogResponse>(s_json, cancellationToken)
            .ConfigureAwait(false);
        return root?.Reports ?? Array.Empty<AuditLogEntry>();
    }

    /// <summary>
    /// Retrieves all audit log entries using paging.
    /// </summary>
    /// <param name="from">Optional start date filter.</param>
    /// <param name="to">Optional end date filter.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<IReadOnlyList<AuditLogEntry>> ListAsync(
        DateTimeOffset? from = null,
        DateTimeOffset? to = null,
        CancellationToken cancellationToken = default) {
        var list = new List<AuditLogEntry>();
        await foreach (var entry in EnumerateAsync(from, to, cancellationToken: cancellationToken).ConfigureAwait(false)) {
            list.Add(entry);
        }

        return list;
    }

    /// <summary>
    /// Streams audit log entries page by page.
    /// </summary>
    /// <param name="from">Optional start date filter.</param>
    /// <param name="to">Optional end date filter.</param>
    /// <param name="pageSize">Number of entries per page.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async IAsyncEnumerable<AuditLogEntry> EnumerateAsync(
        DateTimeOffset? from = null,
        DateTimeOffset? to = null,
        int pageSize = 200,
        [EnumeratorCancellation] CancellationToken cancellationToken = default) {
        var position = 0;

        while (true) {
            var page = await GetAsync(from, to, position, pageSize, cancellationToken).ConfigureAwait(false);
            if (page.Count == 0) {
                yield break;
            }

            foreach (var entry in page) {
                yield return entry;
            }

            if (page.Count < pageSize) {
                yield break;
            }

            position += pageSize;
        }
    }

    private sealed class ActivityLogResponse {
        public IReadOnlyList<AuditLogEntry> Reports { get; set; } = Array.Empty<AuditLogEntry>();
    }
}
