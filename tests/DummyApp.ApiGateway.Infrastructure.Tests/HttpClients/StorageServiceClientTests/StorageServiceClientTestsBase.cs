using Microsoft.Extensions.Logging;
using Moq;
using DummyApp.ApiGateway.Infrastructure.Http;

namespace DummyApp.ApiGateway.Infrastructure.Tests.HttpClients.StorageServiceClientTests;

public abstract class StorageServiceClientTestsBase
{
    protected readonly Mock<ILogger<StorageServiceClient>> LoggerMock = new();

    protected StorageServiceClient CreateClient(HttpResponseMessage response)
        => new StorageServiceClient(CreateHttpClient(response), LoggerMock.Object);

    protected static HttpClient CreateHttpClient(HttpResponseMessage response)
    {
        var handler = new TestHttpMessageHandler(response);
        return new HttpClient(handler)
        {
            BaseAddress = new Uri("https://storageservice.local/")
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
