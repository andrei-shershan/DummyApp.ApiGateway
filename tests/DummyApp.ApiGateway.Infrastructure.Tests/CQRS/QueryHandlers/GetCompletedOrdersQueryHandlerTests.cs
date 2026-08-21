using DummyApp.ApiGateway.Infrastructure.CQRS.QueryHandlers;
using DummyApp.ApiGateway.Infrastructure.CQRS.Queries;
using DummyApp.ApiGateway.Infrastructure.HttpClients;
using DummyApp.ApiGateway.Infrastructure.Models.Dtos;
using DummyApp.ApiGateway.Infrastructure.Tests.CQRS.CommandHandlers;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DummyApp.ApiGateway.Infrastructure.Tests.CQRS.QueryHandlers;

public sealed class GetCompletedOrdersQueryHandlerTests
{
    private readonly Mock<IStorageServiceHttpClient> _storageServiceClientMock = new();
    private readonly Mock<ILogger<GetCompletedOrdersQueryHandler>> _loggerMock = new();

    private GetCompletedOrdersQueryHandler CreateHandler()
        => new(_storageServiceClientMock.Object, _loggerMock.Object);

    [Fact]
    public async Task Handle_ReturnsNull_WhenTokenIsEmpty()
    {
        var handler = CreateHandler();

        var result = await handler.Handle(new GetCompletedOrdersQuery(Guid.Empty), CancellationToken.None);

        Assert.Null(result);
        _storageServiceClientMock.Verify(x => x.GetCompletedOrdersByTokenAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _loggerMock.VerifyLog(LogLevel.Warning, "Invalid completed orders token supplied to GetCompletedOrdersQueryHandler.", Times.Once());
    }

    [Fact]
    public async Task Handle_ReturnsNull_WhenStorageServiceReturnsNull()
    {
        var token = Guid.NewGuid();

        _storageServiceClientMock
            .Setup(x => x.GetCompletedOrdersByTokenAsync(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync((IEnumerable<OrderSummaryDto>?)null);

        var handler = CreateHandler();
        var result = await handler.Handle(new GetCompletedOrdersQuery(token), CancellationToken.None);

        Assert.Null(result);
        _storageServiceClientMock.Verify(x => x.GetCompletedOrdersByTokenAsync(token, It.IsAny<CancellationToken>()), Times.Once);
        _loggerMock.VerifyLog(LogLevel.Warning, "Storage service returned null when getting completed orders for token", Times.Once());
    }

    [Fact]
    public async Task Handle_ReturnsCompletedOrders_WhenStorageServiceReturnsOrders()
    {
        var token = Guid.NewGuid();
        var expectedOrders = new[]
        {
            new OrderSummaryDto { OrderId = Guid.NewGuid(), Status = "Completed", Email = "customer@example.com" },
            new OrderSummaryDto { OrderId = Guid.NewGuid(), Status = "Completed", Email = "another@example.com" }
        };

        _storageServiceClientMock
            .Setup(x => x.GetCompletedOrdersByTokenAsync(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedOrders);

        var handler = CreateHandler();
        var result = await handler.Handle(new GetCompletedOrdersQuery(token), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(expectedOrders, result);
        _storageServiceClientMock.Verify(x => x.GetCompletedOrdersByTokenAsync(token, It.IsAny<CancellationToken>()), Times.Once);
    }
}
