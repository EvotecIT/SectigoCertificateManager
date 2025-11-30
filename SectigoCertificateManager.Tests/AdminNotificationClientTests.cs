using SectigoCertificateManager.AdminApi;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SectigoCertificateManager.Tests;

public sealed class AdminNotificationClientTests {
    private static AdminApiConfig CreateConfig() {
        return new AdminApiConfig(
            "https://admin.enterprise.sectigo.com/",
            "https://auth.sso.sectigo.com/auth/realms/apiclients/protocol/openid-connect/token",
            "client-id",
            "client-secret");
    }

    [Fact]
    public async Task ListAsync_BuildsQueryAndParsesResponse() {
        var tokenResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent("{\"access_token\":\"abc\",\"expires_in\":300}")
        };

        var payload = @"[
  {
    ""id"": 5,
    ""description"": ""SSL Certificate Expiration"",
    ""active"": true,
    ""type"": ""SSL Certificate Expiration"",
    ""recipientData"": {
      ""notifyRoles"": [""REQUESTER""],
      ""recipients"": [
        { ""type"": ""EMAIL"", ""value"": ""ops@example.com"" }
      ]
    },
    ""additionalData"": {
      ""days"": 30,
      ""certTypeId"": 123,
      ""freq"": ""DAILY"",
      ""revokedByAdmin"": false,
      ""revokedByUser"": false
    },
    ""created"": ""2025-01-01T00:00:00.000Z"",
    ""createdBy"": ""admin"",
    ""modified"": ""2025-01-02T00:00:00.000Z"",
    ""modifiedBy"": ""admin2""
  }
]";

        var apiResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent(payload, Encoding.UTF8, "application/json")
        };
        apiResponse.Headers.Add("X-Total-Count", "42");

        using var handler = new TestHandler(tokenResponse, apiResponse);
        using var httpClient = new HttpClient(handler) {
            BaseAddress = new Uri("https://admin.enterprise.sectigo.com/")
        };

        var client = new AdminNotificationClient(CreateConfig(), httpClient);

        var (items, total) = await client.ListAsync(
            size: 10,
            position: 5,
            description: "Expiration",
            id: 5,
            orgId: 1234,
            selectedOrgType: NotificationOrgSelectionType.ANY,
            type: "SSL Certificate Expiration",
            certTypeId: 123,
            cancellationToken: CancellationToken.None);

        Assert.Equal("https://auth.sso.sectigo.com/auth/realms/apiclients/protocol/openid-connect/token", handler.TokenRequest?.RequestUri?.ToString());

        Assert.NotNull(handler.ApiRequest);
        Assert.Equal(HttpMethod.Get, handler.ApiRequest!.Method);
        Assert.Equal(
            "https://admin.enterprise.sectigo.com/api/notification/v1?size=10&position=5&description=Expiration&id=5&orgId=1234&selectedOrgType=ANY&type=SSL%20Certificate%20Expiration&certTypeId=123",
            handler.ApiRequest.RequestUri!.ToString());

        Assert.Equal(42, total);
        Assert.Single(items);
        var notification = items[0];
        Assert.Equal(5, notification.Id);
        Assert.Equal("SSL Certificate Expiration", notification.Description);
        Assert.True(notification.Active);
        Assert.Equal("SSL Certificate Expiration", notification.Type);
        Assert.NotNull(notification.RecipientData);
        Assert.NotNull(notification.AdditionalData);
        Assert.Equal(30, notification.AdditionalData!.Days);
        Assert.Equal(NotificationFrequency.DAILY, notification.AdditionalData.Freq);
    }

    [Fact]
    public async Task ListTypesAsync_BuildsUriAndParsesResponse() {
        var tokenResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent("{\"access_token\":\"abc\",\"expires_in\":300}")
        };

        var payload = "[\"SSL Certificate Expiration\",\"Client Certificate Expiration\"]";
        var apiResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent(payload, Encoding.UTF8, "application/json")
        };

        using var handler = new TestHandler(tokenResponse, apiResponse);
        using var httpClient = new HttpClient(handler) {
            BaseAddress = new Uri("https://admin.enterprise.sectigo.com/")
        };

        var client = new AdminNotificationClient(CreateConfig(), httpClient);

        var types = await client.ListTypesAsync(CancellationToken.None);

        Assert.NotNull(handler.ApiRequest);
        Assert.Equal(HttpMethod.Get, handler.ApiRequest!.Method);
        Assert.Equal("https://admin.enterprise.sectigo.com/api/notification/v1/types", handler.ApiRequest.RequestUri!.ToString());

        Assert.Equal(2, types.Count);
        Assert.Contains("SSL Certificate Expiration", types);
        Assert.Contains("Client Certificate Expiration", types);
    }

    [Fact]
    public async Task UpdateAsync_PutsPayload() {
        var tokenResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent("{\"access_token\":\"abc\",\"expires_in\":300}")
        };

        var apiResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(new { id = 5 })
        };

        using var handler = new TestHandler(tokenResponse, apiResponse);
        using var httpClient = new HttpClient(handler) {
            BaseAddress = new Uri("https://admin.enterprise.sectigo.com/")
        };

        var client = new AdminNotificationClient(CreateConfig(), httpClient);

        var request = new AdminNotificationRequest {
            Description = "SSL Certificate Expiration",
            Active = true,
            OrgData = new AdminNotificationOrgDetails {
                SelectedOrgType = NotificationOrgSelectionType.ANY
            },
            RecipientData = new AdminNotificationRecipientDetails {
                NotifyRoles = new[] { NotificationRecipientRole.REQUESTER },
                Recipients = new[] {
                    new AdminNotificationRecipient {
                        Type = NotificationRecipientType.EMAIL,
                        Value = "ops@example.com"
                    }
                }
            },
            AdditionalData = new AdminNotificationAdditionalDetails {
                Days = 30,
                CertTypeId = 123,
                Freq = NotificationFrequency.ONCE
            }
        };

        await client.UpdateAsync(5, request, CancellationToken.None);

        Assert.NotNull(handler.ApiRequest);
        Assert.Equal(HttpMethod.Put, handler.ApiRequest!.Method);
        Assert.Equal("https://admin.enterprise.sectigo.com/api/notification/v1/5", handler.ApiRequest.RequestUri!.ToString());

        using var doc = JsonDocument.Parse(handler.Body!);
        var root = doc.RootElement;
        Assert.Equal("SSL Certificate Expiration", root.GetProperty("description").GetString());
        Assert.True(root.GetProperty("active").GetBoolean());
        Assert.Equal("ANY", root.GetProperty("orgData").GetProperty("selectedOrgType").GetString());
        var recipients = root.GetProperty("recipientData").GetProperty("recipients");
        Assert.Equal(1, recipients.GetArrayLength());
    }

    [Fact]
    public async Task DeleteAsync_SendsDelete() {
        var tokenResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent("{\"access_token\":\"abc\",\"expires_in\":300}")
        };

        var apiResponse = new HttpResponseMessage(HttpStatusCode.OK);

        using var handler = new TestHandler(tokenResponse, apiResponse);
        using var httpClient = new HttpClient(handler) {
            BaseAddress = new Uri("https://admin.enterprise.sectigo.com/")
        };

        var client = new AdminNotificationClient(CreateConfig(), httpClient);

        await client.DeleteAsync(10, CancellationToken.None);

        Assert.NotNull(handler.ApiRequest);
        Assert.Equal(HttpMethod.Delete, handler.ApiRequest!.Method);
        Assert.Equal("https://admin.enterprise.sectigo.com/api/notification/v1/10", handler.ApiRequest.RequestUri!.ToString());
    }

    private sealed class TestHandler : HttpMessageHandler {
        private readonly HttpResponseMessage _tokenResponse;
        private readonly HttpResponseMessage _apiResponse;

        public HttpRequestMessage? TokenRequest { get; private set; }

        public HttpRequestMessage? ApiRequest { get; private set; }
        public string? Body { get; private set; }

        public TestHandler(HttpResponseMessage tokenResponse, HttpResponseMessage apiResponse) {
            _tokenResponse = tokenResponse ?? throw new ArgumentNullException(nameof(tokenResponse));
            _apiResponse = apiResponse ?? throw new ArgumentNullException(nameof(apiResponse));
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
            if (request.RequestUri!.AbsoluteUri.IndexOf("protocol/openid-connect/token", StringComparison.OrdinalIgnoreCase) >= 0) {
                TokenRequest = request;
                return Task.FromResult(_tokenResponse);
            }

            ApiRequest = request;
            if (request.Content is not null) {
                Body = request.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            }
            return Task.FromResult(_apiResponse);
        }
    }
}
