using System.Net;
using DummyApp.ApiGateway.Infrastructure.HttpClients;
using Microsoft.Extensions.Logging;
using Moq;

namespace DummyApp.ApiGateway.Infrastructure.Tests.HttpClients.IdentityServiceClientTests;

public abstract class IdentityServiceClientTestsBase
{
    protected readonly Mock<ILogger<IdentityServiceClient>> LoggerMock = new();

    protected IdentityServiceClient CreateClient(HttpClient httpClient)
        => new IdentityServiceClient(httpClient, LoggerMock.Object);

    protected static HttpClient CreateHttpClient(HttpResponseMessage response, Func<HttpRequestMessage, Task>? onRequest = null)
    {
        var handler = new TestHttpMessageHandler(response, onRequest);
        return new HttpClient(handler)
        {
            BaseAddress = new Uri("https://identityservice.local/")
        };
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

    protected void VerifyNoLogs()
        => LoggerMock.Verify(
            l => l.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>() ),
            Times.Never);

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
