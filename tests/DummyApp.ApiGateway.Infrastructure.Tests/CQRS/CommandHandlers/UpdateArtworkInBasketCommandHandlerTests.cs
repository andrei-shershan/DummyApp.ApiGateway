using DummyApp.ApiGateway.Infrastructure.CQRS.CommandHandlers;
using DummyApp.ApiGateway.Infrastructure.CQRS.Commands;
using DummyApp.ApiGateway.Infrastructure.HttpClients;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DummyApp.ApiGateway.Infrastructure.Tests.CQRS.CommandHandlers.UpdateArtworkInBasketCommandHandlerTests;

public sealed class HandleTests
{
    private readonly Mock<IStorageServiceHttpClient> _storageServiceClientMock = new();
    private readonly Mock<ILogger<UpdateArtworkInBasketCommandHandler>> _loggerMock = new();

    private UpdateArtworkInBasketCommandHandler CreateHandler()
        => new(_storageServiceClientMock.Object, _loggerMock.Object);

    [Fact]
    public async Task Handle_ReturnsFalse_WhenArtworkIdIsEmpty()
    {
        var handler = CreateHandler();
        var command = new UpdateArtworkInBasketCommand(Guid.NewGuid(), Guid.Empty, 1);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.False(result);
        _loggerMock.VerifyLog(LogLevel.Warning, "UpdateArtworkInBasketCommand received an empty ArtworkId.", Times.Once());
        _storageServiceClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_ReturnsFalse_WhenQuantityIsNegative()
    {
        var handler = CreateHandler();
        var artworkId = Guid.NewGuid();
        var command = new UpdateArtworkInBasketCommand(Guid.NewGuid(), artworkId, -1);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.False(result);
        _loggerMock.VerifyLog(LogLevel.Warning, "UpdateArtworkInBasketCommand received a negative quantity for artwork", Times.Once());
        _storageServiceClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_ReturnsFalse_WhenStorageServiceFails()
    {
        var orderId = Guid.NewGuid();
        var artworkId = Guid.NewGuid();
        _storageServiceClientMock
            .Setup(x => x.UpdateOrderItemAsync(orderId, artworkId, 2, 5, 7, CancellationToken.None))
            .ReturnsAsync(false);

        var handler = CreateHandler();
        var command = new UpdateArtworkInBasketCommand(orderId, artworkId, 2, 5, 7);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.False(result);
        _loggerMock.VerifyLog(LogLevel.Error, "Storage service failed to update artwork", Times.Once());
        _storageServiceClientMock.Verify(x => x.UpdateOrderItemAsync(orderId, artworkId, 2, 5, 7, CancellationToken.None), Times.Once());
    }

    [Fact]
    public async Task Handle_ReturnsTrue_WhenStorageServiceSucceeds()
    {
        var orderId = Guid.NewGuid();
        var artworkId = Guid.NewGuid();
        _storageServiceClientMock
            .Setup(x => x.UpdateOrderItemAsync(orderId, artworkId, 3, null, null, CancellationToken.None))
            .ReturnsAsync(true);

        var handler = CreateHandler();
        var command = new UpdateArtworkInBasketCommand(orderId, artworkId, 3, null, null);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result);
        _storageServiceClientMock.Verify(x => x.UpdateOrderItemAsync(orderId, artworkId, 3, null, null, CancellationToken.None), Times.Once());
        _loggerMock.VerifyNoOtherCalls();
    }
}
