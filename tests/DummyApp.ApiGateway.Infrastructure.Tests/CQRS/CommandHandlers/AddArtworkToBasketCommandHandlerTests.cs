using DummyApp.ApiGateway.Infrastructure.CQRS.CommandHandlers;
using DummyApp.ApiGateway.Infrastructure.CQRS.Commands;
using DummyApp.ApiGateway.Infrastructure.HttpClients;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DummyApp.ApiGateway.Infrastructure.Tests.CQRS.CommandHandlers.AddArtworkToBasketCommandHandlerTests;

public sealed class HandleTests
{
    private readonly Mock<IStorageServiceHttpClient> _storageServiceClientMock = new();
    private readonly Mock<ILogger<AddArtworkToBasketCommandHandler>> _loggerMock = new();

    private AddArtworkToBasketCommandHandler CreateHandler()
        => new AddArtworkToBasketCommandHandler(_storageServiceClientMock.Object, _loggerMock.Object);

    [Fact]
    public async Task Handle_ReturnsFalse_WhenArtworkIdIsEmpty()
    {
        var handler = CreateHandler();
        var command = new AddArtworkToBasketCommand(Guid.NewGuid(), Guid.Empty);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.False(result);
        _storageServiceClientMock.Verify(x => x.AddOrderItemAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_CallsStorageService_WhenCommandIsValid()
    {
        var orderId = Guid.NewGuid();
        var artworkId = Guid.NewGuid();
        _storageServiceClientMock
            .Setup(x => x.AddOrderItemAsync(orderId, artworkId, 1, CancellationToken.None))
            .ReturnsAsync(true);

        var handler = CreateHandler();
        var command = new AddArtworkToBasketCommand(orderId, artworkId);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result);
        _storageServiceClientMock.Verify(x => x.AddOrderItemAsync(orderId, artworkId, 1, CancellationToken.None), Times.Once);
    }
}
