namespace SectigoCertificateManager.Clients;

using SectigoCertificateManager.Responses;
using SectigoCertificateManager.Utilities;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;

public sealed partial class CertificatesClient : BaseClient {
    /// <summary>
    /// Imports certificates using a zip archive.
    /// </summary>
    /// <param name="orgId">Identifier of the organization.</param>
    /// <param name="stream">Zip archive containing certificates.</param>
    /// <param name="fileName">File name to use for the upload.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<ImportCertificateResponse?> ImportAsync(
        int orgId,
        Stream stream,
        string fileName,
        CancellationToken cancellationToken = default) {
        if (orgId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(orgId));
        }
        Guard.AgainstNull(stream, nameof(stream));
        Guard.AgainstNullOrEmpty(fileName, nameof(fileName), "File name cannot be null or empty.");

        var content = new MultipartFormDataContent();
        var fileContent = new StreamContent(stream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/zip");
        content.Add(fileContent, "file", fileName);

        var response = await _client.PostAsync($"v1/certificate/import?orgId={orgId}", content, cancellationToken).ConfigureAwait(false);
        return await response.Content
            .ReadFromJsonAsyncSafe<ImportCertificateResponse>(s_json, cancellationToken)
            .ConfigureAwait(false);
    }
}
