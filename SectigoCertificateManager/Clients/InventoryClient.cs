namespace SectigoCertificateManager.Clients;

using SectigoCertificateManager.Models;
using SectigoCertificateManager.Requests;
using SectigoCertificateManager.Utilities;
using System.Globalization;
using System.Text;
using System.Globalization;

/// <summary>
/// Provides access to inventory related endpoints.
/// </summary>
public sealed class InventoryClient : BaseClient {

    /// <summary>
    /// Initializes a new instance of the <see cref="InventoryClient"/> class.
    /// </summary>
    /// <param name="client">HTTP client wrapper.</param>
    public InventoryClient(ISectigoClient client) : base(client) {
    }

    /// <summary>
    /// Downloads certificate inventory in CSV format.
    /// </summary>
    /// <param name="request">Request describing the filter.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<IReadOnlyList<InventoryRecord>> DownloadCsvAsync(
        InventoryCsvRequest request,
        CancellationToken cancellationToken = default) {
        Guard.AgainstNull(request, nameof(request));

        var query = BuildQuery(request);
        var response = await _client
            .GetAsync($"v1/inventory.csv{query}", cancellationToken)
            .ConfigureAwait(false);
#if NETSTANDARD2_0 || NET472
        var csv = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
#else
        var csv = await response.Content.ReadAsStringAsync(cancellationToken)
            .ConfigureAwait(false);
#endif
        return ParseCsv(csv);
    }

    private static IReadOnlyList<InventoryRecord> ParseCsv(string csv) {
        var list = new List<InventoryRecord>();
        using var reader = new StringReader(csv);
        var header = reader.ReadLine();
        if (header is null) {
            return list;
        }

        var buffer = new List<string>();
        var builder = new StringBuilder();

        var columns = SplitCsvLine(header, buffer, builder);
        string? line;
        while ((line = reader.ReadLine()) != null) {
            var values = SplitCsvLine(line, buffer, builder);
            var record = new InventoryRecord();
            for (var i = 0; i < columns.Length && i < values.Length; i++) {
                var value = values[i];
                switch (columns[i]) {
                    case "id":
                        if (int.TryParse(value, out var id)) {
                            record.Id = id;
                        }
                        break;
                    case "commonName":
                        record.CommonName = value;
                        break;
                    case "organizationName":
                        record.OrganizationName = value;
                        break;
                    case "status":
                        record.Status = value;
                        break;
                    case "expires":
                        record.Expires = value;
                        break;
                }
            }
            list.Add(record);
        }
        return list;
    }

    private static string[] SplitCsvLine(
        string line,
        List<string> values,
        StringBuilder builder) {
        values.Clear();
        builder.Clear();
        var vsb = new ValueStringBuilder(stackalloc char[64]);
        var inQuotes = false;
        for (var i = 0; i < line.Length; i++) {
            var ch = line[i];
            if (ch == '"') {
                inQuotes = !inQuotes;
                continue;
            }
            if (ch == ',' && !inQuotes) {
                values.Add(vsb.ToString());
                builder.Clear();
                vsb.Clear();
                continue;
            }
            vsb.Append(ch);
        }
        values.Add(vsb.ToString());
        vsb.Dispose();
        return values.ToArray();
    }

    private static string BuildQuery(InventoryCsvRequest request) {
        var builder = new StringBuilder();

        void AppendSeparator() {
            _ = builder.Length == 0 ? builder.Append('?') : builder.Append('&');
        }


        void AppendInt(string name, int value) {
            AppendSeparator();
            builder.Append(name).Append('=').Append(value);
        }

        void AppendDate(string name, DateTime? value) {
            if (!value.HasValue) {
                return;
            }

            AppendSeparator();
            builder.Append(name).Append('=')
                .Append(value.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        }

        if (request.Size.HasValue) {
            AppendInt("size", request.Size.Value);
        }

        if (request.Position.HasValue) {
            AppendInt("position", request.Position.Value);
        }

        AppendDate("from", request.DateFrom);
        AppendDate("to", request.DateTo);

        return builder.ToString();
    }
}
