using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DummyApp.ApiGateway.Infrastructure.CQRS.Queries;
using DummyApp.ApiGateway.Infrastructure.Models.Dtos;
using DummyApp.ApiGateway.WebApi.Controllers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace DummyApp.ApiGateway.WebApi.Tests.Controllers.Admin;

public sealed class GetAnalyticsTests : AdminControllerTestBase
{
    [Fact]
    public async Task GetAnalytics_ReturnsOkResult_WhenPeriodDaysIsPositive()
    {
        var expectedAnalytics = new[]
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

        var mediatorMock = new Mock<IMediator>();
        mediatorMock.Setup(m => m.Send(It.IsAny<GetAnalyticsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedAnalytics);

        var controller = CreateController(mediatorMock);
        var result = await controller.GetAnalytics(14);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(expectedAnalytics, Assert.IsAssignableFrom<IEnumerable<AnalyticsEventDto>>(okResult.Value));
        mediatorMock.Verify(m => m.Send(It.Is<GetAnalyticsQuery>(q => q.PeriodDays == 14), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public async Task GetAnalytics_ReturnsBadRequest_WhenPeriodDaysIsNotPositive(int periodDays)
    {
        var mediatorMock = new Mock<IMediator>();
        var controller = CreateController(mediatorMock);

        var result = await controller.GetAnalytics(periodDays);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("periodDays must be greater than 0.", badRequestResult.Value);
        mediatorMock.Verify(m => m.Send(It.IsAny<GetAnalyticsQuery>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
