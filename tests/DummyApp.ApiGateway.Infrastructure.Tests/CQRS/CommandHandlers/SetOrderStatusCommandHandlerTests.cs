using System;
using System.Collections.Generic;
using System.Threading;
using DummyApp.ApiGateway.Infrastructure.CQRS.CommandHandlers;
using DummyApp.ApiGateway.Infrastructure.CQRS.Commands;
using DummyApp.ApiGateway.Infrastructure.HttpClients;
using DummyApp.ApiGateway.Infrastructure.Models.Dtos;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DummyApp.ApiGateway.Infrastructure.Tests.CQRS.CommandHandlers.SetOrderStatusCommandHandlerTests;

public sealed class HandleTests
{
    private readonly Mock<IStorageServiceHttpClient> _storageServiceClientMock = new();
    private readonly Mock<ILogger<SetOrderStatusCommandHandler>> _loggerMock = new();

    private SetOrderStatusCommandHandler CreateHandler()
        => new SetOrderStatusCommandHandler(_storageServiceClientMock.Object, _loggerMock.Object);

    [Fact]
    public async Task Handle_ReturnsFalse_WhenOrderIdIsEmpty()
    {
        var handler = CreateHandler();
        var command = new SetOrderStatusCommand(Guid.Empty, "Processing");

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.False(result);
        _storageServiceClientMock.Verify(x => x.SetOrderStatusAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ReturnsFalse_WhenStatusIsMissing()
    {
        var handler = CreateHandler();
        var command = new SetOrderStatusCommand(Guid.NewGuid(), string.Empty);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.False(result);
        _storageServiceClientMock.Verify(x => x.SetOrderStatusAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ReturnsFalse_WhenReviewStatusAndSummaryIsMissing()
    {
        var orderId = Guid.NewGuid();
        _storageServiceClientMock.Setup(x => x.GetOrderSummaryAsync(orderId, CancellationToken.None)).ReturnsAsync((OrderSummaryDto?)null);

        var handler = CreateHandler();
        var command = new SetOrderStatusCommand(orderId, "Processing");

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.False(result);
        _storageServiceClientMock.Verify(x => x.SetOrderStatusAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ReturnsFalse_WhenReviewStatusAndItemIsIncomplete()
    {
        var orderId = Guid.NewGuid();
        var summary = new OrderSummaryDto
        {
            Items = new List<OrderItemDto>
            {
                new OrderItemDto { OrderId = orderId, ArtworkId = Guid.NewGuid(), Quantity = 1, PrintSizeId = null, PriceId = 1 }
            },
            Status = "Active"
        };

        _storageServiceClientMock.Setup(x => x.GetOrderSummaryAsync(orderId, CancellationToken.None)).ReturnsAsync(summary);

        var handler = CreateHandler();
        var command = new SetOrderStatusCommand(orderId, "Processing");

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.False(result);
        _storageServiceClientMock.Verify(x => x.SetOrderStatusAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_CallsSetOrderStatusAsync_WhenStatusIsActive()
    {
        var orderId = Guid.NewGuid();
        _storageServiceClientMock.Setup(x => x.SetOrderStatusAsync(orderId, "Active", CancellationToken.None)).ReturnsAsync(true);

        var handler = CreateHandler();
        var command = new SetOrderStatusCommand(orderId, "Active");

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result);
        _storageServiceClientMock.Verify(x => x.SetOrderStatusAsync(orderId, "Active", CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handle_CallsSetOrderStatusAsync_WhenReviewStatusAndAllItemsComplete()
    {
        var orderId = Guid.NewGuid();
        var summary = new OrderSummaryDto
        {
            Items = new List<OrderItemDto>
            {
                new OrderItemDto { OrderId = orderId, ArtworkId = Guid.NewGuid(), Quantity = 1, PrintSizeId = 1, PriceId = 2 }
            },
            Status = "Active"
        };

        _storageServiceClientMock.Setup(x => x.GetOrderSummaryAsync(orderId, CancellationToken.None)).ReturnsAsync(summary);
        _storageServiceClientMock.Setup(x => x.SetOrderStatusAsync(orderId, "Processing", CancellationToken.None)).ReturnsAsync(true);

        var handler = CreateHandler();
        var command = new SetOrderStatusCommand(orderId, "Processing");

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result);
        _storageServiceClientMock.Verify(x => x.SetOrderStatusAsync(orderId, "Processing", CancellationToken.None), Times.Once);
    }
}
