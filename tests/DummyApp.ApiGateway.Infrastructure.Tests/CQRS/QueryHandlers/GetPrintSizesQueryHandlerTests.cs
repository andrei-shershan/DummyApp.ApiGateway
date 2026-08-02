using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DummyApp.ApiGateway.Infrastructure.CQRS.QueryHandlers;
using DummyApp.ApiGateway.Infrastructure.CQRS.Queries;
using DummyApp.ApiGateway.Infrastructure.HttpClients;
using DummyApp.ApiGateway.Infrastructure.Models.Dtos;
using Moq;
using Xunit;

namespace DummyApp.ApiGateway.Infrastructure.Tests.CQRS.QueryHandlers;

public sealed class GetPrintSizesQueryHandlerTests
{
    private readonly Mock<IStorageServiceHttpClient> _storageServiceClientMock = new();

    private GetPrintSizesQueryHandler CreateHandler()
        => new(_storageServiceClientMock.Object);

    [Fact]
    public async Task Handle_ReturnsEmptyList_WhenStorageServiceReturnsNull()
    {
        _storageServiceClientMock
            .Setup(x => x.GetPrintSizesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((IEnumerable<PrintSizeDto>?)null);

        var handler = CreateHandler();
        var result = await handler.Handle(new GetPrintSizesQuery(), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_ReturnsPrintSizes_WhenStorageServiceReturnsValues()
    {
        var expected = new[]
        {
            new PrintSizeDto { Id = 1, Name = "A4", Prices = Array.Empty<PriceDto>() }
        };

        _storageServiceClientMock
            .Setup(x => x.GetPrintSizesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var handler = CreateHandler();
        var result = await handler.Handle(new GetPrintSizesQuery(), CancellationToken.None);

        Assert.Equal(expected, result);
    }
}
