using System.Net;
using DummyApp.ApiGateway.Infrastructure.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DummyApp.ApiGateway.Infrastructure.Tests.HttpClients.StorageServiceClientTests;

public class AddOrderItemAsyncTests : StorageServiceClientTestsBase
{
    [Fact]
    public async Task AddOrderItemAsync_ReturnsFalse_WhenResponseIsNotSuccessful()
    {
        using var httpClient = CreateHttpClient(new HttpResponseMessage(HttpStatusCode.BadRequest));
        var client = new StorageServiceClient(httpClient, LoggerMock.Object);

        var result = await client.AddOrderItemAsync(Guid.NewGuid(), Guid.NewGuid(), 1, null, null, CancellationToken.None);

        Assert.False(result);
        VerifyLog(LogLevel.Error, "Failed to add order item via storage service. Status code:", Times.Once());
    }

    [Fact]
    public async Task AddOrderItemAsync_ReturnsTrue_WhenResponseIsSuccessful()
    {
        using var httpClient = CreateHttpClient(new HttpResponseMessage(HttpStatusCode.OK));
        var client = new StorageServiceClient(httpClient, LoggerMock.Object);

        var result = await client.AddOrderItemAsync(Guid.NewGuid(), Guid.NewGuid(), 2, 3, 4, CancellationToken.None);

        Assert.True(result);
        VerifyNoLogs();
    }
}
