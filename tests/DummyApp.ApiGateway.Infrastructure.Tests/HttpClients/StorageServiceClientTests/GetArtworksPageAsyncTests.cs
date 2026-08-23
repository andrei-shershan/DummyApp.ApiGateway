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

public sealed class GetArtworksPageAsyncTests : StorageServiceClientTestsBase
{
    [Fact]
    public async Task GetArtworksPageAsync_ReturnsNull_WhenResponseIsNotSuccessful()
    {
        using var httpClient = CreateHttpClient(new HttpResponseMessage(HttpStatusCode.InternalServerError));
        var client = new StorageServiceClient(httpClient, LoggerMock.Object);

        var result = await client.GetArtworksPageAsync(null, true, 1, 10, null, CancellationToken.None);

        Assert.Null(result);
        VerifyNoLogs();
    }

    [Fact]
    public async Task GetArtworksPageAsync_ReturnsNull_WhenResponseContentIsNull()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create<PaginatedResult<ArtworkDto>>(null)
        };

        using var httpClient = CreateHttpClient(response);
        var client = new StorageServiceClient(httpClient, LoggerMock.Object);

        var result = await client.GetArtworksPageAsync("creator", true, 1, 10, null, CancellationToken.None);

        Assert.Null(result);
        VerifyNoLogs();
    }

    [Fact]
    public async Task GetArtworksPageAsync_ReturnsNull_WhenResponseIsInvalidJson()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("not-json", Encoding.UTF8, "application/json")
        };

        using var httpClient = CreateHttpClient(response);
        var client = new StorageServiceClient(httpClient, LoggerMock.Object);

        var result = await client.GetArtworksPageAsync(null, false, 1, 10, null, CancellationToken.None);

        Assert.Null(result);
        VerifyLog(LogLevel.Error, "Failed to read response content from storage service.", Times.Once());
    }

    [Fact]
    public async Task GetArtworksPageAsync_ReturnsPagedResult_WhenResponseIsSuccessful()
    {
        var expectedItems = new[]
        {
            new ArtworkDto
            {
                Id = Guid.NewGuid(),
                CreatorId = "creator",
                Name = "Test",
                Description = "desc",
                UploadDate = DateTime.UtcNow,
                ImgUrl = "img",
                ThumbnailUrl = "thumb",
                IsActive = true
            }
        };

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new PaginatedResult<ArtworkDto>(expectedItems, 1, 10, 1))
        };

        using var httpClient = CreateHttpClient(response);
        var client = new StorageServiceClient(httpClient, LoggerMock.Object);

        var result = await client.GetArtworksPageAsync("creator", true, 1, 10, null, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(1, result!.PageNumber);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(1, result.TotalCount);
        Assert.Equal(expectedItems, result.Items);
        VerifyNoLogs();
    }

    [Fact]
    public async Task GetArtworksPageAsync_UsesAllAllowedTagIds_InRequestUri()
    {
        var tagIds = new[] { Guid.NewGuid(), Guid.Empty, Guid.NewGuid() };
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new PaginatedResult<ArtworkDto>(Array.Empty<ArtworkDto>(), 1, 10, 0))
        };

        HttpRequestMessage? capturedRequest = null;
        using var httpClient = CreateHttpClient(response, request =>
        {
            capturedRequest = request;
            return Task.CompletedTask;
        });

        var client = new StorageServiceClient(httpClient, LoggerMock.Object);

        var result = await client.GetArtworksPageAsync("creator id", false, 1, 10, tagIds, CancellationToken.None);

        Assert.NotNull(result);
        Assert.NotNull(capturedRequest);
        Assert.Equal("GET", capturedRequest!.Method.Method);
        Assert.Contains("/api/artworks/page?", capturedRequest.RequestUri?.PathAndQuery);
        Assert.Contains("creatorId=creator%20id", capturedRequest.RequestUri?.PathAndQuery);
        Assert.Contains("isActive=false", capturedRequest.RequestUri?.PathAndQuery);
        Assert.Contains("pageNumber=1", capturedRequest.RequestUri?.PathAndQuery);
        Assert.Contains("pageSize=10", capturedRequest.RequestUri?.PathAndQuery);
        Assert.Contains($"tagIds={tagIds[0]}", capturedRequest.RequestUri?.PathAndQuery);
        Assert.Contains($"tagIds={tagIds[2]}", capturedRequest.RequestUri?.PathAndQuery);
        Assert.DoesNotContain("tagIds=00000000-0000-0000-0000-000000000000", capturedRequest.RequestUri?.PathAndQuery);
        VerifyNoLogs();
    }
}
