using System.Net;
using System.Net.Http.Json;
using System.Text;
using DummyApp.ApiGateway.Infrastructure.Http;
using DummyApp.ApiGateway.Infrastructure.Models.Dtos;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DummyApp.ApiGateway.Infrastructure.Tests.HttpClients.StorageServiceClientTests;

public class GetArtworksAsyncTests : StorageServiceClientTestsBase
{
    [Fact]
    public async Task GetArtworksAsync_ReturnsNull_WhenResponseIsNotSuccessful()
    {
        using var httpClient = CreateHttpClient(new HttpResponseMessage(HttpStatusCode.InternalServerError));
        var client = new StorageServiceClient(httpClient, LoggerMock.Object);

        var result = await client.GetArtworksAsync(CancellationToken.None);

        Assert.Null(result);
        VerifyNoLogs();
    }

    [Fact]
    public async Task GetArtworksAsync_ReturnsNull_WhenResponseContentIsNull()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create<IEnumerable<ArtworkDto>>(null)
        };

        using var httpClient = CreateHttpClient(response);
        var client = new StorageServiceClient(httpClient, LoggerMock.Object);

        var result = await client.GetArtworksAsync(CancellationToken.None);

        Assert.Null(result);
        VerifyNoLogs();
    }

    [Fact]
    public async Task GetArtworksAsync_ReturnsNull_WhenResponseIsInvalidJson()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("not-json", Encoding.UTF8, "application/json")
        };

        using var httpClient = CreateHttpClient(response);
        var client = new StorageServiceClient(httpClient, LoggerMock.Object);

        var result = await client.GetArtworksAsync(CancellationToken.None);

        Assert.Null(result);
        VerifyLog(LogLevel.Error, "Failed to read response content from storage service.", Times.Once());
    }

    [Fact]
    public async Task GetArtworksAsync_ReturnsArtworkList_WhenResponseIsSuccessful()
    {
        var artworks = new[]
        {
            new ArtworkDto
            {
                Id = 1,
                CreatorId = "creator",
                Name = "name",
                PublicName = "public",
                Description = "desc",
                UploadDate = DateTime.UtcNow,
                ImgUrl = "img",
                ThumbnailUrl = "small",
                IsActive = true
            }
        };

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create<IEnumerable<ArtworkDto>>(artworks)
        };

        using var httpClient = CreateHttpClient(response);
        var client = new StorageServiceClient(httpClient, LoggerMock.Object);

        var result = await client.GetArtworksAsync(CancellationToken.None);

        Assert.Equal(artworks, result);
        VerifyNoLogs();
    }
}
