using Microsoft.Extensions.Logging;
using Moq;
using DummyApp.ApiGateway.Infrastructure.Http;

namespace DummyApp.ApiGateway.Infrastructure.Tests.HttpClients.BlobServiceHttpClientTests;

public abstract class BlobServiceHttpClientTestsBase
{
    protected readonly Mock<ILogger<BlobServiceHttpClient>> LoggerMock = new();

    protected BlobServiceHttpClient CreateClient(HttpResponseMessage response)
        => new BlobServiceHttpClient(CreateHttpClient(response), LoggerMock.Object);

    protected static HttpClient CreateHttpClient(HttpResponseMessage response)
    {
        var handler = new TestHttpMessageHandler(response);
        return new HttpClient(handler)
        {
            BaseAddress = new Uri("https://blobservice.local/")
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

        public TestHttpMessageHandler(HttpResponseMessage response)
        {
            _response = response;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(_response);
    }
}
