using DummyApp.ApiGateway.Infrastructure.CQRS.CommandHandlers;
using DummyApp.ApiGateway.Infrastructure.CQRS.Commands;
using DummyApp.ApiGateway.Infrastructure.HttpClients;
using DummyApp.ApiGateway.Infrastructure.Models.Dtos;
using DummyApp.ApiGateway.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DummyApp.ApiGateway.Infrastructure.Tests.CQRS.CommandHandlers.UpdateArtworkIsActiveCommandHandlerTests;

public sealed class HandleTests
{
    private readonly Mock<ILogger<UpdateArtworkIsActiveCommandHandler>> _loggerMock = new();
    private readonly Mock<IStorageServiceHttpClient> _storageServiceClientMock = new();
    private readonly Mock<IArtworkQueryFilterService> _artworkQueryFilterServiceMock = new();

    private UpdateArtworkIsActiveCommandHandler CreateHandler()
        => new UpdateArtworkIsActiveCommandHandler(
            _storageServiceClientMock.Object,
            _artworkQueryFilterServiceMock.Object,
            _loggerMock.Object);

    private static CancellationToken None => CancellationToken.None;

    [Fact]
    public async Task Handle_ReturnsNull_WhenArtworkIdIsInvalid()
    {
        var handler = CreateHandler();
        var command = new UpdateArtworkIsActiveCommand(Guid.Empty, true);

        var result = await handler.Handle(command, None);

        Assert.Null(result);
        _loggerMock.VerifyLog(LogLevel.Error, "Invalid artwork id 00000000-0000-0000-0000-000000000000 supplied for active state update.", Times.Once());
        _storageServiceClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_CallsStorageService_WhenArtworkIdIsValid()
    {
        var artworkId = Guid.NewGuid();
        var expected = new ArtworkDto
        {
            Id = artworkId,
            CreatorId = "creator",
            Name = "Artwork",
            Description = "Description",
            CreationDate = DateTime.UtcNow,
            UploadDate = DateTime.UtcNow,
            ImgUrl = "img",
            ThumbnailUrl = "thumb",
            IsActive = true
        };

        _storageServiceClientMock
            .Setup(x => x.GetArtworkByIdAsync(artworkId, false, None))
            .ReturnsAsync(expected);

        _artworkQueryFilterServiceMock
            .Setup(x => x.AdminOrCreatorsArtwork(expected))
            .Returns(true);

        _storageServiceClientMock
            .Setup(x => x.UpdateArtworkIsActiveAsync(artworkId, true, None))
            .ReturnsAsync(expected);

        var handler = CreateHandler();
        var command = new UpdateArtworkIsActiveCommand(artworkId, true);

        var result = await handler.Handle(command, None);

        Assert.Equal(expected, result);
        _storageServiceClientMock.Verify(x => x.UpdateArtworkIsActiveAsync(artworkId, true, None), Times.Once());
    }

    [Fact]
    public async Task Handle_ReturnsNull_WhenUserIsNotAuthorizedToUpdateArtwork()
    {
        var artworkId = Guid.NewGuid();
        var artwork = new ArtworkDto
        {
            Id = artworkId,
            CreatorId = "creator",
            Name = "Artwork",
            Description = "Description",
            CreationDate = DateTime.UtcNow,
            UploadDate = DateTime.UtcNow,
            ImgUrl = "img",
            ThumbnailUrl = "thumb",
            IsActive = true
        };

        _storageServiceClientMock
            .Setup(x => x.GetArtworkByIdAsync(artworkId, false, None))
            .ReturnsAsync(artwork);

        _artworkQueryFilterServiceMock
            .Setup(x => x.AdminOrCreatorsArtwork(artwork))
            .Returns(false);

        var handler = CreateHandler();
        var command = new UpdateArtworkIsActiveCommand(artworkId, true);

        var result = await handler.Handle(command, None);

        Assert.Null(result);
        _storageServiceClientMock.Verify(x => x.UpdateArtworkIsActiveAsync(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never());
    }
}
