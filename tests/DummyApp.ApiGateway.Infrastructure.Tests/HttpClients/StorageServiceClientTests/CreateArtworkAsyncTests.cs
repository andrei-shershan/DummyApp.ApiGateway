using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using DummyApp.ApiGateway.Infrastructure.Http;
using DummyApp.ApiGateway.Infrastructure.Models.Dtos;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DummyApp.ApiGateway.Infrastructure.Tests.HttpClients.StorageServiceClientTests;

public class CreateArtworkAsyncTests : StorageServiceClientTestsBase
{
    [Fact]
    public async Task CreateArtworkAsync_ReturnsNull_WhenResponseIsNotSuccessful()
    {
        using var httpClient = CreateHttpClient(new HttpResponseMessage(HttpStatusCode.BadRequest));
        var client = new StorageServiceClient(httpClient, LoggerMock.Object);

        var result = await client.CreateArtworkAsync(new CreateArtworkRequestDto(
            Name: string.Empty,
            FileName: string.Empty,
            Description: string.Empty,
            CreationDate: DateTime.UtcNow,
            UploadDate: DateTime.UtcNow,
            ImgUrl: string.Empty,
            ThumbnailUrl: string.Empty,
            IsActive: false,
            CreatorId: string.Empty,
            ExistingTagIds: Array.Empty<Guid>(),
            NewTags: Array.Empty<CreateArtworkTagDto>()), CancellationToken.None);

        Assert.Null(result);
        VerifyLog(LogLevel.Error, "Failed to create artwork via storage service. Status code:", Times.Once());
    }

    [Fact]
    public async Task CreateArtworkAsync_ReturnsNull_WhenResponseContentIsNull()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create<ArtworkDto>(null)
        };

        using var httpClient = CreateHttpClient(response);
        var client = new StorageServiceClient(httpClient, LoggerMock.Object);

        var result = await client.CreateArtworkAsync(new CreateArtworkRequestDto(
            Name: string.Empty,
            FileName: string.Empty,
            Description: string.Empty,
            CreationDate: DateTime.UtcNow,
            UploadDate: DateTime.UtcNow,
            ImgUrl: string.Empty,
            ThumbnailUrl: string.Empty,
            IsActive: false,
            CreatorId: string.Empty,
            ExistingTagIds: Array.Empty<Guid>(),
            NewTags: Array.Empty<CreateArtworkTagDto>()), CancellationToken.None);

        Assert.Null(result);
        VerifyLog(LogLevel.Error, "Storage service returned a successful status code but the response content was null when creating artwork.", Times.Once());
    }

    [Fact]
    public async Task CreateArtworkAsync_ReturnsNull_WhenResponseIsInvalidJson()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("not-json", Encoding.UTF8, "application/json")
        };

        using var httpClient = CreateHttpClient(response);
        var client = new StorageServiceClient(httpClient, LoggerMock.Object);

        var result = await client.CreateArtworkAsync(new CreateArtworkRequestDto(
            Name: string.Empty,
            FileName: string.Empty,
            Description: string.Empty,
            CreationDate: DateTime.UtcNow,
            UploadDate: DateTime.UtcNow,
            ImgUrl: string.Empty,
            ThumbnailUrl: string.Empty,
            IsActive: false,
            CreatorId: string.Empty,
            ExistingTagIds: Array.Empty<Guid>(),
            NewTags: Array.Empty<CreateArtworkTagDto>()), CancellationToken.None);

        Assert.Null(result);
        VerifyLog(LogLevel.Error, "Failed to read response content from storage service when creating artwork.", Times.Once());
    }

    [Fact]
    public async Task CreateArtworkAsync_ReturnsArtwork_WhenResponseIsSuccessful()
    {
        var artwork = new ArtworkDto
        {
            Id = Guid.NewGuid(),
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

        var request = new CreateArtworkRequestDto(
            Name: "name",
            FileName: "file.png",
            Description: "desc",
            CreationDate: DateTime.UtcNow,
            UploadDate: DateTime.UtcNow,
            ImgUrl: "img",
            ThumbnailUrl: "small",
            IsActive: false,
            CreatorId: "creator",
            ExistingTagIds: new[] { Guid.NewGuid() },
            NewTags: new[] { new CreateArtworkTagDto("tag", "None") });

        var result = await client.CreateArtworkAsync(request, CancellationToken.None);

        Assert.Equal(artwork, result);
        VerifyNoLogs();
    }

    [Fact]
    public async Task CreateArtworkAsync_SendsCorrectRequestPayload()
    {
        CreateArtworkRequestDto? sentRequest = null;
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new ArtworkDto { Id = Guid.NewGuid(), CreatorId = "creator", Name = "name", Description = "desc", UploadDate = DateTime.UtcNow, ImgUrl = "img", ThumbnailUrl = "small", IsActive = true })
        };

        using var httpClient = CreateHttpClient(response, async request =>
        {
            var body = await request.Content!.ReadAsStringAsync();
            sentRequest = JsonSerializer.Deserialize<CreateArtworkRequestDto>(body, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        });

        var client = new StorageServiceClient(httpClient, LoggerMock.Object);

        var request = new CreateArtworkRequestDto(
            Name: "name",
            FileName: "file.png",
            Description: "desc",
            CreationDate: DateTime.UtcNow,
            UploadDate: DateTime.UtcNow,
            ImgUrl: "img",
            ThumbnailUrl: "small",
            IsActive: true,
            CreatorId: "creator",
            ExistingTagIds: new[] { Guid.NewGuid() },
            NewTags: new[] { new CreateArtworkTagDto("tag", "None") });

        var result = await client.CreateArtworkAsync(request, CancellationToken.None);

        Assert.NotNull(result);
        Assert.NotNull(sentRequest);
        Assert.Equal(request.Name, sentRequest!.Name);
        Assert.Equal(request.FileName, sentRequest.FileName);
        Assert.Equal(request.Description, sentRequest.Description);
        Assert.Equal(request.CreatorId, sentRequest.CreatorId);
        Assert.Equal(request.ImgUrl, sentRequest.ImgUrl);
        Assert.Equal(request.ThumbnailUrl, sentRequest.ThumbnailUrl);
        Assert.Equal(request.ExistingTagIds, sentRequest.ExistingTagIds);
        Assert.Equal(request.NewTags, sentRequest.NewTags);
    }
}
