using System.Net;
using DummyApp.ApiGateway.Infrastructure.HttpClients;
using DummyApp.ApiGateway.Infrastructure.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace DummyApp.ApiGateway.Infrastructure.Tests.HttpClients.AnalyticsServiceClientTests;

public abstract class AnalyticsServiceClientTestsBase
{
    protected readonly Mock<ILogger<AnalyticsServiceClient>> LoggerMock = new();

    protected AnalyticsServiceClient CreateClient(HttpResponseMessage response, string? secretKey = null, Func<HttpRequestMessage, Task>? onRequest = null)
    {
        var handler = new TestHttpMessageHandler(response, onRequest);
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://analyticsservice.local/")
        };

        return new AnalyticsServiceClient(
            httpClient,
            LoggerMock.Object,
            Options.Create(new AnalyticsServiceOptions { SecretKey = secretKey }));
    }

    protected void VerifyLog(LogLevel level, string message, Times times)
        => LoggerMock.Verify(
            l => l.Log(
                level,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(message)),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>() ),
            times);

    private sealed class TestHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpResponseMessage _response;
        private readonly Func<HttpRequestMessage, Task>? _onRequest;

        public TestHttpMessageHandler(HttpResponseMessage response, Func<HttpRequestMessage, Task>? onRequest)
        {
            _response = response;
            _onRequest = onRequest;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (_onRequest is not null)
            {
                await _onRequest(request);
            }

            return _response;
        }
    }
}
