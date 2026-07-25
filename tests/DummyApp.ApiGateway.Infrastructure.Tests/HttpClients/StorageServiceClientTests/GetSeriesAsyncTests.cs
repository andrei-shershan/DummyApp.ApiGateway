using System.Net;
using System.Net.Http.Json;
using System.Text;
using DummyApp.ApiGateway.Infrastructure.Http;
using DummyApp.ApiGateway.Infrastructure.Models.Dtos;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DummyApp.ApiGateway.Infrastructure.Tests.HttpClients.StorageServiceClientTests;

public class GetSeriesAsyncTests : StorageServiceClientTestsBase
{
    [Fact]
    public async Task GetSeriesAsync_ReturnsNull_WhenResponseIsNotSuccessful()
    {
        using var httpClient = CreateHttpClient(new HttpResponseMessage(HttpStatusCode.InternalServerError));
        var client = new StorageServiceClient(httpClient, LoggerMock.Object);

        var result = await client.GetSeriesAsync("creator-1", CancellationToken.None);

        Assert.Null(result);
        VerifyNoLogs();
    }

    [Fact]
    public async Task GetSeriesAsync_ReturnsNull_WhenResponseIsInvalidJson()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("not-json", Encoding.UTF8, "application/json")
        };

        using var httpClient = CreateHttpClient(response);
        var client = new StorageServiceClient(httpClient, LoggerMock.Object);

        var result = await client.GetSeriesAsync("creator-1", CancellationToken.None);

        Assert.Null(result);
        VerifyLog(LogLevel.Error, "Failed to read response content from storage service.", Times.Once());
    }

    [Fact]
    public async Task GetSeriesAsync_ReturnsSeriesList_WhenResponseIsSuccessful()
    {
        var seriesList = new[]
        {
            new SeriesDto { Id = Guid.NewGuid(), CreatorId = "creator-1", Name = "Series A" }
        };

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create<IEnumerable<SeriesDto>>(seriesList)
        };

        using var httpClient = CreateHttpClient(response);
        var client = new StorageServiceClient(httpClient, LoggerMock.Object);

        var result = await client.GetSeriesAsync("creator-1", CancellationToken.None);

        Assert.Equal(seriesList, result);
        VerifyNoLogs();
    }
}
