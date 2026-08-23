using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DummyApp.ApiGateway.Infrastructure.Http;
using DummyApp.ApiGateway.Infrastructure.Models.Dtos;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DummyApp.ApiGateway.Infrastructure.Tests.HttpClients.StorageServiceClientTests;

public sealed class GetFilteredTagsAsyncTests : StorageServiceClientTestsBase
{
    [Fact]
    public async Task GetFilteredTagsAsync_ReturnsNull_WhenResponseIsNotSuccessful()
    {
        using var httpClient = CreateHttpClient(new HttpResponseMessage(HttpStatusCode.InternalServerError));
        var client = new StorageServiceClient(httpClient, LoggerMock.Object);

        var result = await client.GetFilteredTagsAsync(CancellationToken.None);

        Assert.Null(result);
        VerifyNoLogs();
    }

    [Fact]
    public async Task GetFilteredTagsAsync_ReturnsNull_WhenResponseContentIsNull()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create<IEnumerable<TagDto>>(null)
        };

        using var httpClient = CreateHttpClient(response);
        var client = new StorageServiceClient(httpClient, LoggerMock.Object);

        var result = await client.GetFilteredTagsAsync(CancellationToken.None);

        Assert.Null(result);
        VerifyNoLogs();
    }

    [Fact]
    public async Task GetFilteredTagsAsync_ReturnsNull_WhenResponseIsInvalidJson()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("not-json", Encoding.UTF8, "application/json")
        };

        using var httpClient = CreateHttpClient(response);
        var client = new StorageServiceClient(httpClient, LoggerMock.Object);

        var result = await client.GetFilteredTagsAsync(CancellationToken.None);

        Assert.Null(result);
        VerifyLog(LogLevel.Error, "Failed to read response content from storage service when getting filtered tags.", Times.Once());
    }

    [Fact]
    public async Task GetFilteredTagsAsync_ReturnsTagList_WhenResponseIsSuccessful()
    {
        var tags = new[]
        {
            new TagDto { Id = Guid.NewGuid(), Name = "Food", Type = "Category" },
            new TagDto { Id = Guid.NewGuid(), Name = "Nature", Type = "Category" }
        };

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create<IEnumerable<TagDto>>(tags)
        };

        using var httpClient = CreateHttpClient(response);
        var client = new StorageServiceClient(httpClient, LoggerMock.Object);

        var result = await client.GetFilteredTagsAsync(CancellationToken.None);

        Assert.Equal(tags, result);
        VerifyNoLogs();
    }
}
