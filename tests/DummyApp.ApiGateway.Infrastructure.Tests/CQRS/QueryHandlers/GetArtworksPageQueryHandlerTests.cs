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
    private readonly Mock<IStorageUrlService> _storageUrlServiceMock = new();
    private readonly Mock<IArtworkQueryFilterService> _artworkQueryFilterServiceMock = new();

    private GetArtworksPageQueryHandler CreateHandler()
        => new(_storageServiceClientMock.Object, _storageUrlServiceMock.Object, _artworkQueryFilterServiceMock.Object);

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
            .Setup(x => x.GetArtworksPageAsync("creator", true, 2, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaginatedResult<ArtworkDto>(new[] { artwork }, 2, 5, 7));

        _artworkQueryFilterServiceMock
            .Setup(x => x.CanAccessArtworkById(artwork))
            .Returns(true);

        _storageUrlServiceMock
            .Setup(x => x.GetBlobUrl("blob/path.png"))
            .Returns("https://storage.example.com/blob/path.png");

        _storageUrlServiceMock
            .Setup(x => x.GetBlobUrl("small/blob.png"))
            .Returns("https://storage.example.com/small/blob.png");

        var handler = CreateHandler();

        var result = await handler.Handle(new GetArtworksPageQuery("creator", true, 2, 5), CancellationToken.None);

        Assert.Equal(2, result.PageNumber);
        Assert.Equal(5, result.PageSize);
        Assert.Equal(7, result.TotalCount);
        Assert.Single(result.Items);
        Assert.Equal("https://storage.example.com/blob/path.png", result.Items.Single().ImgUrl);
        Assert.Equal("https://storage.example.com/small/blob.png", result.Items.Single().ThumbnailUrl);
    }
}
