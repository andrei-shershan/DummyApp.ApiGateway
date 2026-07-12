using System.Net;
using System.Net.Http.Json;
using System.Text;
using DummyApp.ApiGateway.Infrastructure.Http;
using DummyApp.ApiGateway.Infrastructure.HttpClients;
using DummyApp.ApiGateway.Infrastructure.Models.Dtos;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DummyApp.ApiGateway.Infrastructure.Tests.HttpClients.StorageServiceClientTests;

public class GetArtworkByIdAsyncTests : StorageServiceClientTestsBase
{
    [Fact]
    public async Task GetArtworkByIdAsync_ReturnsNull_WhenResponseIsNotSuccessful()
    {
        var artworkId = Guid.NewGuid();
        using var httpClient = CreateHttpClient(new HttpResponseMessage(HttpStatusCode.NotFound));
        var client = new StorageServiceClient(httpClient, LoggerMock.Object);

        var result = await client.GetArtworkByIdAsync(artworkId, true, CancellationToken.None);

        Assert.Null(result);
        VerifyNoLogs();
    }

    [Fact]
    public async Task GetArtworkByIdAsync_ReturnsNull_WhenResponseContentIsNull()
    {
        var artworkId = Guid.NewGuid();
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create<ArtworkDto>(null)
        };

        using var httpClient = CreateHttpClient(response);
        var client = new StorageServiceClient(httpClient, LoggerMock.Object);

        var result = await client.GetArtworkByIdAsync(artworkId, true, CancellationToken.None);

        Assert.Null(result);
        VerifyNoLogs();
    }

    [Fact]
    public async Task GetArtworkByIdAsync_ReturnsNull_WhenResponseIsInvalidJson()
    {
        var artworkId = Guid.NewGuid();
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("not-json", Encoding.UTF8, "application/json")
        };

        using var httpClient = CreateHttpClient(response);
        var client = new StorageServiceClient(httpClient, LoggerMock.Object);

        var result = await client.GetArtworkByIdAsync(artworkId, true, CancellationToken.None);

        Assert.Null(result);
        VerifyLog(LogLevel.Error, "Failed to read response content from storage service.", Times.Once());
    }

    [Fact]
    public async Task GetArtworkByIdAsync_ReturnsArtwork_WhenResponseIsSuccessful()
    {
        var artworkId = Guid.NewGuid();
        var artwork = new ArtworkDto
        {
            Id = artworkId,
            CreatorId = "creator",
            Name = "name",
            Description = "desc",
            UploadDate = DateTime.UtcNow,
            ImgUrl = "img",
            ThumbnailUrl = "small",
            IsActive = true
        };

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(artwork)
        };

        using var httpClient = CreateHttpClient(response);
        var client = new StorageServiceClient(httpClient, LoggerMock.Object);

        var result = await client.GetArtworkByIdAsync(artworkId, true, CancellationToken.None);

        Assert.Equal(artwork, result);
        VerifyNoLogs();
    }
}
