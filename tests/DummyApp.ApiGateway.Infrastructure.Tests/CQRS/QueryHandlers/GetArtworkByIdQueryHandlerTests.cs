using DummyApp.ApiGateway.Infrastructure.CQRS.QueryHandlers;
using DummyApp.ApiGateway.Infrastructure.CQRS.Queries;
using DummyApp.ApiGateway.Infrastructure.HttpClients;
using DummyApp.ApiGateway.Infrastructure.Models.Dtos;
using DummyApp.ApiGateway.Infrastructure.Services;
using Moq;
using Xunit;

namespace DummyApp.ApiGateway.Infrastructure.Tests.CQRS.QueryHandlers;

public class GetArtworkByIdQueryHandlerTests
{
    private readonly Mock<IStorageServiceHttpClient> _storageServiceClientMock = new();
    private readonly Mock<IStorageUrlService> _storageUrlServiceMock = new();
    private readonly Mock<IArtworkQueryFilterService> _artworkQueryFilterServiceMock = new();

    private GetArtworkByIdQueryHandler CreateHandler()
        => new(_storageServiceClientMock.Object, _storageUrlServiceMock.Object, _artworkQueryFilterServiceMock.Object);

    [Fact]
    public async Task Handle_ReturnsNull_WhenStorageServiceReturnsNull()
    {
        _artworkQueryFilterServiceMock
            .Setup(x => x.GetArtworkByIdActiveOnly())
            .Returns(true);

        _storageServiceClientMock
            .Setup(x => x.GetArtworkByIdAsync(It.IsAny<int>(), true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ArtworkDto?)null);

        var handler = CreateHandler();
        var result = await handler.Handle(new GetArtworkByIdQuery(1), CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task Handle_ReturnsArtworkWithNormalizedUrls_WhenArtworkExists()
    {
        var artwork = new ArtworkDto
        {
            Id = 1,
            CreatorId = "creator",
            Name = "Test",
            Description = "desc",
            CreationDate = DateTime.UtcNow,
            UploadDate = DateTime.UtcNow,
            ImgUrl = "blob/path.png",
            ThumbnailUrl = "small/blob.png",
            IsActive = true
        };

        _artworkQueryFilterServiceMock
            .Setup(x => x.GetArtworkByIdActiveOnly())
            .Returns(true);

        _storageServiceClientMock
            .Setup(x => x.GetArtworkByIdAsync(artwork.Id, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(artwork);

        _storageUrlServiceMock
            .Setup(x => x.GetBlobUrl("blob/path.png"))
            .Returns("https://storage.example.com/blob/path.png");

        _storageUrlServiceMock
            .Setup(x => x.GetBlobUrl("small/blob.png"))
            .Returns("https://storage.example.com/small/blob.png");

        var handler = CreateHandler();
        var result = await handler.Handle(new GetArtworkByIdQuery(artwork.Id), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("https://storage.example.com/blob/path.png", result!.ImgUrl);
        Assert.Equal("https://storage.example.com/small/blob.png", result.ThumbnailUrl);
        Assert.Equal(artwork.Id, result.Id);
        Assert.Equal(artwork.CreatorId, result.CreatorId);
    }
}
