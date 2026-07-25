using System.Net;
using System.Net.Http.Json;
using System.Text;
using DummyApp.ApiGateway.Infrastructure.Http;
using DummyApp.ApiGateway.Infrastructure.Models.Dtos;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DummyApp.ApiGateway.Infrastructure.Tests.HttpClients.StorageServiceClientTests;

public class CreateSeriesAsyncTests : StorageServiceClientTestsBase
{
    [Fact]
    public async Task CreateSeriesAsync_ReturnsNull_WhenResponseIsNotSuccessful()
    {
        using var httpClient = CreateHttpClient(new HttpResponseMessage(HttpStatusCode.InternalServerError));
        var client = new StorageServiceClient(httpClient, LoggerMock.Object);

        var result = await client.CreateSeriesAsync("Series A", CancellationToken.None);

        Assert.Null(result);
        VerifyLog(LogLevel.Error, "Failed to create series via storage service. Status code:", Times.Once());
    }

    [Fact]
    public async Task CreateSeriesAsync_ReturnsNull_WhenResponseIsInvalidJson()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("not-json", Encoding.UTF8, "application/json")
        };

        using var httpClient = CreateHttpClient(response);
        var client = new StorageServiceClient(httpClient, LoggerMock.Object);

        var result = await client.CreateSeriesAsync("Series A", CancellationToken.None);

        Assert.Null(result);
        VerifyLog(LogLevel.Error, "Failed to read response content from storage service when creating series.", Times.Once());
    }

    [Fact]
    public async Task CreateSeriesAsync_ReturnsSeries_WhenResponseIsSuccessful()
    {
        var series = new SeriesDto { Id = Guid.NewGuid(), CreatorId = "creator-1", Name = "Series A" };
        var response = new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = JsonContent.Create(series)
        };

        using var httpClient = CreateHttpClient(response);
        var client = new StorageServiceClient(httpClient, LoggerMock.Object);

        var result = await client.CreateSeriesAsync("Series A", CancellationToken.None);

        Assert.Equal(series, result);
        VerifyNoLogs();
    }
}
