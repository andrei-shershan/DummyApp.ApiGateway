using DummyApp.ApiGateway.Infrastructure.CQRS.CommandHandlers;
using DummyApp.ApiGateway.Infrastructure.CQRS.Commands;
using DummyApp.ApiGateway.Infrastructure.HttpClients;
using DummyApp.ApiGateway.Infrastructure.Models.Dtos;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DummyApp.ApiGateway.Infrastructure.Tests.CQRS.CommandHandlers.SaveOrderAddressCommandHandlerTests;

public sealed class HandleTests
{
    private readonly Mock<IStorageServiceHttpClient> _storageServiceClientMock = new();
    private readonly Mock<ILogger<SaveOrderAddressCommandHandler>> _loggerMock = new();

    private SaveOrderAddressCommandHandler CreateHandler()
        => new(_storageServiceClientMock.Object, _loggerMock.Object);

    [Fact]
    public async Task Handle_ReturnsFalse_WhenOrderIdIsEmpty()
    {
        var handler = CreateHandler();
        var command = new SaveOrderAddressCommand(Guid.Empty, new OrderAddressDto());

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.False(result);
        _storageServiceClientMock.Verify(x => x.SaveOrderAddressAsync(It.IsAny<Guid>(), It.IsAny<OrderAddressDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ReturnsFalse_WhenAddressIsNull()
    {
        var handler = CreateHandler();
        var command = new SaveOrderAddressCommand(Guid.NewGuid(), null!);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.False(result);
        _storageServiceClientMock.Verify(x => x.SaveOrderAddressAsync(It.IsAny<Guid>(), It.IsAny<OrderAddressDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_CallsStorageService_WhenCommandIsValid()
    {
        var orderId = Guid.NewGuid();
        var address = new OrderAddressDto { FirstName = "John", LastName = "Doe" };

        _storageServiceClientMock
            .Setup(x => x.SaveOrderAddressAsync(orderId, address, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = CreateHandler();
        var result = await handler.Handle(new SaveOrderAddressCommand(orderId, address), CancellationToken.None);

        Assert.True(result);
        _storageServiceClientMock.Verify(x => x.SaveOrderAddressAsync(orderId, address, It.IsAny<CancellationToken>()), Times.Once);
    }
}
