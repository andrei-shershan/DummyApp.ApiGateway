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
    private readonly Mock<IArtworkQueryFilterService> _artworkQueryFilterServiceMock = new();

    private GetArtworksQueryHandler CreateHandler()
        => new GetArtworksQueryHandler(_storageServiceClientMock.Object, _artworkQueryFilterServiceMock.Object);

    [Fact]
    public async Task Handle_ReturnsEmptyList_WhenStorageServiceReturnsNull()
    {
        _artworkQueryFilterServiceMock
            .Setup(x => x.ShouldRequestActiveOnly(true))
            .Returns(true);

        _storageServiceClientMock
            .Setup(x => x.GetArtworksAsync(null, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((IEnumerable<ArtworkDto>?)null);

        var handler = CreateHandler();
        var result = await handler.Handle(new GetArtworksQuery(), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_ReturnsArtworkWithNormalizedUrls_WhenStorageServiceReturnsArtworks()
    {
        var artwork = new ArtworkDto { Id = Guid.NewGuid(), CreatorId = "creator", Name = "Test", Description = "desc", CreationDate = DateTime.UtcNow, UploadDate = DateTime.UtcNow, ImgUrl = "blob/path.png", ThumbnailUrl = "small/blob.png", IsActive = true };

        _artworkQueryFilterServiceMock
            .Setup(x => x.ShouldRequestActiveOnly(true))
            .Returns(true);

        _storageServiceClientMock
            .Setup(x => x.GetArtworksAsync(null, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { artwork });

        _artworkQueryFilterServiceMock
            .Setup(x => x.CanAccessArtworkById(artwork))
            .Returns(true);

        var handler = CreateHandler();
        var result = await handler.Handle(new GetArtworksQuery(), CancellationToken.None);

        Assert.Single(result);
        var mapped = result.Single();
        Assert.Equal(artwork.ImgUrl, mapped.ImgUrl);
        Assert.Equal(artwork.ThumbnailUrl, mapped.ThumbnailUrl);
        Assert.Equal(artwork.Id, mapped.Id);
        Assert.Equal(artwork.CreatorId, mapped.CreatorId);
        Assert.Equal(artwork.Name, mapped.Name);
    }

    [Fact]
    public async Task Handle_UsesFilteredQuery_WhenArtworkQueryFilterServiceChangesIsActive()
    {
        _artworkQueryFilterServiceMock
            .Setup(x => x.ShouldRequestActiveOnly(true))
            .Returns(false);

        _storageServiceClientMock
            .Setup(x => x.GetArtworksAsync("creator-1", false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<ArtworkDto>());

        var handler = CreateHandler();
        await handler.Handle(new GetArtworksQuery("creator-1", true), CancellationToken.None);

        _storageServiceClientMock.Verify(x => x.GetArtworksAsync("creator-1", false, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_FiltersOutArtworks_WhenCanAccessArtworkByIdReturnsFalse()
    {
        var accessibleArtwork = new ArtworkDto { Id = Guid.NewGuid(), CreatorId = "creator", ImgUrl = "blob/path.png", ThumbnailUrl = "small/blob.png", IsActive = true };
        var blockedArtwork = new ArtworkDto { Id = Guid.NewGuid(), CreatorId = "creator", ImgUrl = "blob/path-2.png", ThumbnailUrl = "small/blob-2.png", IsActive = true };

        _artworkQueryFilterServiceMock
            .Setup(x => x.ShouldRequestActiveOnly(true))
            .Returns(true);

        _storageServiceClientMock
            .Setup(x => x.GetArtworksAsync(null, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { accessibleArtwork, blockedArtwork });

        _artworkQueryFilterServiceMock
            .Setup(x => x.CanAccessArtworkById(accessibleArtwork))
            .Returns(true);

        _artworkQueryFilterServiceMock
            .Setup(x => x.CanAccessArtworkById(blockedArtwork))
            .Returns(false);

        var handler = CreateHandler();
        var result = await handler.Handle(new GetArtworksQuery(), CancellationToken.None);

        Assert.Single(result);
        Assert.Equal(accessibleArtwork.Id, result.Single().Id);
    }
}
