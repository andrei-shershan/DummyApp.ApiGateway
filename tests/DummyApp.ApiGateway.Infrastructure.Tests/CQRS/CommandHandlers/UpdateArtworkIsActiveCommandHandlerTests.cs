using DummyApp.ApiGateway.Infrastructure.CQRS.CommandHandlers;
using DummyApp.ApiGateway.Infrastructure.CQRS.Commands;
using DummyApp.ApiGateway.Infrastructure.HttpClients;
using DummyApp.ApiGateway.Infrastructure.Models.Dtos;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DummyApp.ApiGateway.Infrastructure.Tests.CQRS.CommandHandlers.UpdateArtworkIsActiveCommandHandlerTests;

public sealed class HandleTests
{
    private readonly Mock<ILogger<UpdateArtworkIsActiveCommandHandler>> _loggerMock = new();
    private readonly Mock<IStorageServiceHttpClient> _storageServiceClientMock = new();

    private UpdateArtworkIsActiveCommandHandler CreateHandler()
        => new UpdateArtworkIsActiveCommandHandler(
            _storageServiceClientMock.Object,
            _loggerMock.Object);

    private static CancellationToken None => CancellationToken.None;

    [Fact]
    public async Task Handle_ReturnsNull_WhenArtworkIdIsInvalid()
    {
        var handler = CreateHandler();
        var command = new UpdateArtworkIsActiveCommand(0, true);

        var result = await handler.Handle(command, None);

        Assert.Null(result);
        _loggerMock.VerifyLog(LogLevel.Error, "Invalid artwork id 0 supplied for active state update.", Times.Once());
        _storageServiceClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_CallsStorageService_WhenArtworkIdIsValid()
    {
        var expected = new ArtworkDto
        {
            Id = 1,
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
            .Setup(x => x.UpdateArtworkIsActiveAsync(1, true, None))
            .ReturnsAsync(expected);

        var handler = CreateHandler();
        var command = new UpdateArtworkIsActiveCommand(1, true);

        var result = await handler.Handle(command, None);

        Assert.Equal(expected, result);
        _storageServiceClientMock.Verify(x => x.UpdateArtworkIsActiveAsync(1, true, None), Times.Once());
    }
}
