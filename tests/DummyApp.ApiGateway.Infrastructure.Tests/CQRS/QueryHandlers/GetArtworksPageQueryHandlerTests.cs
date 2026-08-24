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

public sealed class GetArtworksPageQueryHandlerTests
{
    private readonly Mock<IStorageServiceHttpClient> _storageServiceClientMock = new();
    private readonly Mock<IArtworkQueryFilterService> _artworkQueryFilterServiceMock = new();

    private GetArtworksPageQueryHandler CreateHandler()
        => new(_storageServiceClientMock.Object, _artworkQueryFilterServiceMock.Object);

    [Fact]
    public async Task Handle_ReturnsPagedResultWithNormalizedUrls()
    {
        var artwork = new ArtworkDto
        {
            Id = Guid.NewGuid(),
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
            .Setup(x => x.ShouldRequestActiveOnly(true))
            .Returns(true);

        _storageServiceClientMock
            .Setup(x => x.GetArtworksPageAsync("creator", true, 2, 5, It.IsAny<IEnumerable<Guid>?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaginatedResult<ArtworkDto>(new[] { artwork }, 2, 5, 7));

        _artworkQueryFilterServiceMock
            .Setup(x => x.CanAccessArtworkById(artwork))
            .Returns(true);

        var handler = CreateHandler();

        var result = await handler.Handle(new GetArtworksPageQuery("creator", true, 2, 5), CancellationToken.None);

        Assert.Equal(2, result.PageNumber);
        Assert.Equal(5, result.PageSize);
        Assert.Equal(7, result.TotalCount);
        Assert.Single(result.Items);
        Assert.Equal(artwork.ImgUrl, result.Items.Single().ImgUrl);
        Assert.Equal(artwork.ThumbnailUrl, result.Items.Single().ThumbnailUrl);
    }

    [Fact]
    public async Task Handle_WhenActiveOnlyIsFalse_FiltersAndPaginatesResults()
    {
        var firstArtwork = new ArtworkDto
        {
            Id = Guid.NewGuid(),
            CreatorId = "creator",
            Name = "First",
            Description = "desc1",
            CreationDate = DateTime.UtcNow,
            UploadDate = DateTime.UtcNow,
            ImgUrl = "img1.png",
            ThumbnailUrl = "thumb1.png",
            IsActive = false
        };

        var secondArtwork = new ArtworkDto
        {
            Id = Guid.NewGuid(),
            CreatorId = "creator",
            Name = "Second",
            Description = "desc2",
            CreationDate = DateTime.UtcNow,
            UploadDate = DateTime.UtcNow,
            ImgUrl = "img2.png",
            ThumbnailUrl = "thumb2.png",
            IsActive = false
        };

        var thirdArtwork = new ArtworkDto
        {
            Id = Guid.NewGuid(),
            CreatorId = "creator",
            Name = "Third",
            Description = "desc3",
            CreationDate = DateTime.UtcNow,
            UploadDate = DateTime.UtcNow,
            ImgUrl = "img3.png",
            ThumbnailUrl = "thumb3.png",
            IsActive = false
        };

        _artworkQueryFilterServiceMock
            .Setup(x => x.ShouldRequestActiveOnly(false))
            .Returns(false);

        _storageServiceClientMock
            .Setup(x => x.GetArtworksAsync("creator", false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { firstArtwork, secondArtwork, thirdArtwork });

        _artworkQueryFilterServiceMock.Setup(x => x.CanAccessArtworkById(firstArtwork)).Returns(true);
        _artworkQueryFilterServiceMock.Setup(x => x.CanAccessArtworkById(secondArtwork)).Returns(false);
        _artworkQueryFilterServiceMock.Setup(x => x.CanAccessArtworkById(thirdArtwork)).Returns(true);


        var handler = CreateHandler();

        var result = await handler.Handle(new GetArtworksPageQuery("creator", false, 2, 1), CancellationToken.None);

        Assert.Equal(2, result.PageNumber);
        Assert.Equal(1, result.PageSize);
        Assert.Equal(2, result.TotalCount);
        Assert.Single(result.Items);
        Assert.Equal(thirdArtwork.Id, result.Items.Single().Id);
        Assert.Equal(thirdArtwork.ImgUrl, result.Items.Single().ImgUrl);
        Assert.Equal(thirdArtwork.ThumbnailUrl, result.Items.Single().ThumbnailUrl);
    }
}
