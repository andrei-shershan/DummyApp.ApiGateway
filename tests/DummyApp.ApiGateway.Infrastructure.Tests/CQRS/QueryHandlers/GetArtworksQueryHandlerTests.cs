using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DummyApp.ApiGateway.Infrastructure.CQRS.QueryHandlers;
using DummyApp.ApiGateway.Infrastructure.CQRS.Queries;
using DummyApp.ApiGateway.Infrastructure.HttpClients;
using DummyApp.ApiGateway.Infrastructure.Models.Dtos;
using DummyApp.ApiGateway.Infrastructure.Services;
using Moq;
using Xunit;

namespace DummyApp.ApiGateway.Infrastructure.Tests.CQRS.QueryHandlers;

public class GetArtworksQueryHandlerTests
{
    private readonly Mock<IStorageServiceHttpClient> _storageServiceClientMock = new();
    private readonly Mock<IStorageUrlService> _storageUrlServiceMock = new();

    private GetArtworksQueryHandler CreateHandler()
        => new GetArtworksQueryHandler(_storageServiceClientMock.Object, _storageUrlServiceMock.Object);

    [Fact]
    public async Task Handle_ReturnsEmptyList_WhenStorageServiceReturnsNull()
    {
        _storageServiceClientMock
            .Setup(x => x.GetArtworksAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((IEnumerable<ArtworkDto>?)null);

        var handler = CreateHandler();
        var result = await handler.Handle(new GetArtworksQuery(), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_ReturnsArtworkWithNormalizedUrls_WhenStorageServiceReturnsArtworks()
    {
        var artworks = new[]
        {
            new ArtworkDto { Id = 1, CreatorId = "creator", Name = "Test", PublicName = "Test", Description = "desc", CreationDate = DateTime.UtcNow, UploadDate = DateTime.UtcNow, ImgUrl = "blob/path.png", SmallImgUrl = "small/blob.png", IsActive = true }
        };

        _storageServiceClientMock
            .Setup(x => x.GetArtworksAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(artworks);

        _storageUrlServiceMock
            .Setup(x => x.GetBlobUrl("blob/path.png"))
            .Returns("https://storage.example.com/blob/path.png");

        _storageUrlServiceMock
            .Setup(x => x.GetBlobUrl("small/blob.png"))
            .Returns("https://storage.example.com/small/blob.png");

        var handler = CreateHandler();
        var result = await handler.Handle(new GetArtworksQuery(), CancellationToken.None);

        Assert.Single(result);
        var mapped = result.Single();
        Assert.Equal("https://storage.example.com/blob/path.png", mapped.ImgUrl);
        Assert.Equal("https://storage.example.com/small/blob.png", mapped.SmallImgUrl);
        Assert.Equal(artworks[0].Id, mapped.Id);
        Assert.Equal(artworks[0].CreatorId, mapped.CreatorId);
        Assert.Equal(artworks[0].Name, mapped.Name);
    }
}
