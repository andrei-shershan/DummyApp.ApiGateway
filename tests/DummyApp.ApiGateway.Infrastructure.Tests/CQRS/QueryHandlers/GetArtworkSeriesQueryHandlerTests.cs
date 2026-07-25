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

public sealed class GetArtworkSeriesQueryHandlerTests
{
    private readonly Mock<IStorageServiceHttpClient> _storageServiceClientMock = new();

    private GetArtworkSeriesQueryHandler CreateHandler()
        => new GetArtworkSeriesQueryHandler(_storageServiceClientMock.Object);

    [Fact]
    public async Task Handle_ReturnsEmptyList_WhenStorageServiceReturnsNull()
    {
        _storageServiceClientMock
            .Setup(x => x.GetSeriesAsync("creator-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync((IEnumerable<SeriesDto>?)null);

        var handler = CreateHandler();
        var result = await handler.Handle(new GetArtworkSeriesQuery("creator-1"), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_ReturnsSeriesList_WhenStorageServiceReturnsSeries()
    {
        var series = new[]
        {
            new SeriesDto { Id = Guid.NewGuid(), CreatorId = "creator-1", Name = "Series A" }
        };

        _storageServiceClientMock
            .Setup(x => x.GetSeriesAsync("creator-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(series);

        var handler = CreateHandler();
        var result = await handler.Handle(new GetArtworkSeriesQuery("creator-1"), CancellationToken.None);

        Assert.Equal(series, result);
    }
}
