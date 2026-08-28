using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DummyApp.ApiGateway.Infrastructure.CQRS.QueryHandlers;
using DummyApp.ApiGateway.Infrastructure.CQRS.Queries;
using DummyApp.ApiGateway.Infrastructure.HttpClients;
using DummyApp.ApiGateway.Infrastructure.Models.Dtos;
using DummyApp.ApiGateway.Infrastructure.Tests.CQRS.CommandHandlers;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DummyApp.ApiGateway.Infrastructure.Tests.CQRS.QueryHandlers;

public sealed class GetAnalyticsQueryHandlerTests
{
    private readonly Mock<IAnalyticsServiceHttpClient> _analyticsServiceClientMock = new();
    private readonly Mock<ILogger<GetAnalyticsQueryHandler>> _loggerMock = new();

    private GetAnalyticsQueryHandler CreateHandler()
        => new(_analyticsServiceClientMock.Object, _loggerMock.Object);

    [Fact]
    public async Task Handle_ReturnsAnalytics_WhenClientReturnsData()
    {
        var expected = new[]
        {
            new AnalyticsEventDto(
                Id: "analytics-1",
                OrderId: Guid.NewGuid(),
                Status: "Completed",
                Email: "test@example.com",
                SiteId: "site-1",
                Address: null,
                Items: Array.Empty<AnalyticsOrderItem>(),
                Tags: Array.Empty<string>(),
                EventTimestamp: DateTimeOffset.UtcNow)
        };

        _analyticsServiceClientMock
            .Setup(x => x.GetAnalyticsAsync(7, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var handler = CreateHandler();
        var result = await handler.Handle(new GetAnalyticsQuery(7), CancellationToken.None);

        Assert.Equal(expected, result);
        _analyticsServiceClientMock.Verify(x => x.GetAnalyticsAsync(7, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Handle_ReturnsEmptyList_WhenPeriodDaysIsNotPositive(int periodDays)
    {
        var handler = CreateHandler();
        var result = await handler.Handle(new GetAnalyticsQuery(periodDays), CancellationToken.None);

        Assert.Empty(result);
        _analyticsServiceClientMock.Verify(x => x.GetAnalyticsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        _loggerMock.VerifyLog(LogLevel.Warning, $"GetAnalyticsQueryHandler received invalid periodDays {periodDays}.", Times.Once());
    }
}
