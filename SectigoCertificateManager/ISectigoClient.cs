namespace SectigoCertificateManager;

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

public interface ISectigoClient
{
    HttpClient HttpClient { get; }
    Task<HttpResponseMessage> GetAsync(string requestUri, CancellationToken cancellationToken = default);

    Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content, CancellationToken cancellationToken = default);

    Task<HttpResponseMessage> PutAsync(string requestUri, HttpContent content, CancellationToken cancellationToken = default);

    Task<HttpResponseMessage> DeleteAsync(string requestUri, CancellationToken cancellationToken = default);
}
