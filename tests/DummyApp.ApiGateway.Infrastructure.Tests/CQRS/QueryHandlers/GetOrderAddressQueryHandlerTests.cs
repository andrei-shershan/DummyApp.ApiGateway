using DummyApp.ApiGateway.Infrastructure.CQRS.QueryHandlers;
using DummyApp.ApiGateway.Infrastructure.CQRS.Queries;
using DummyApp.ApiGateway.Infrastructure.HttpClients;
using DummyApp.ApiGateway.Infrastructure.Models.Dtos;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DummyApp.ApiGateway.Infrastructure.Tests.CQRS.QueryHandlers;

public sealed class GetOrderAddressQueryHandlerTests
{
    private readonly Mock<IStorageServiceHttpClient> _storageServiceClientMock = new();
    private readonly Mock<ILogger<GetOrderAddressQueryHandler>> _loggerMock = new();

    private GetOrderAddressQueryHandler CreateHandler()
        => new(_storageServiceClientMock.Object, _loggerMock.Object);

    [Fact]
    public async Task Handle_ReturnsNull_WhenOrderIdIsEmpty()
    {
        var handler = CreateHandler();

        var result = await handler.Handle(new GetOrderAddressQuery(Guid.Empty), CancellationToken.None);

        Assert.Null(result);
        _storageServiceClientMock.Verify(x => x.GetOrderAddressAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ReturnsAddress_WhenStorageServiceReturnsAddress()
    {
        var orderId = Guid.NewGuid();
        var expected = new OrderAddressDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Phone = "+48123123123",
            Country = "PL",
            City = "Warsaw",
            Street = "Main",
            HouseNumber = "10",
            PostalCode = "00-001"
        };

        _storageServiceClientMock
            .Setup(x => x.GetOrderAddressAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var handler = CreateHandler();
        var result = await handler.Handle(new GetOrderAddressQuery(orderId), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(expected, result);
        _storageServiceClientMock.Verify(x => x.GetOrderAddressAsync(orderId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
