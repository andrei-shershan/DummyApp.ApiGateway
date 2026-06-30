using System.Net;
using DummyApp.ApiGateway.Infrastructure.HttpClients;
using DummyApp.ApiGateway.Infrastructure.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace DummyApp.ApiGateway.Infrastructure.Tests.HttpClients.EmailServiceClientTests;

public abstract class EmailServiceClientTestsBase
{
    protected readonly Mock<ILogger<EmailServiceClient>> LoggerMock = new();

    protected EmailServiceClient CreateClient(HttpClient httpClient, string? secretKey = "test-secret")
        => new(
            httpClient,
            LoggerMock.Object,
            Options.Create(new EmailServiceOptions
            {
                SecretKey = secretKey
            }));

    protected static HttpClient CreateHttpClient(HttpResponseMessage response, Func<HttpRequestMessage, Task>? onRequest = null)
    {
        var handler = new TestHttpMessageHandler(response, onRequest);
        return new HttpClient(handler)
        {
            BaseAddress = new Uri("https://emailservice.local/")
        };
    }

    protected void VerifyLog(LogLevel level, string message, Times times)
        => LoggerMock.Verify(
            l => l.Log(
                level,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(message)),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            times);

    protected void VerifyNoLogs()
        => LoggerMock.Verify(
            l => l.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
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
