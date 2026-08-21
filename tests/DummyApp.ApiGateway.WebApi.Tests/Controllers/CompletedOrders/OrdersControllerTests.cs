using DummyApp.ApiGateway.Infrastructure.CQRS.Queries;
using DummyApp.ApiGateway.Infrastructure.Models.Dtos;
using DummyApp.ApiGateway.WebApi.Controllers;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DummyApp.ApiGateway.WebApi.Tests.Controllers.CompletedOrders;

public sealed class OrdersControllerTests
{
    [Fact]
    public async Task GetCompletedOrders_ReturnsNotFound_WhenCookieIsMissing()
    {
        var mediatorMock = new Mock<IMediator>();
        var loggerMock = new Mock<ILogger<OrdersController>>();
        var controller = CreateController(mediatorMock, loggerMock);

        var result = await controller.GetCompletedOrders();

        Assert.IsType<NotFoundResult>(result);
        mediatorMock.Verify(m => m.Send(It.IsAny<IRequest<IEnumerable<OrderSummaryDto>?>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetCompletedOrders_ReturnsNotFound_WhenCookieIsInvalidGuid()
    {
        var mediatorMock = new Mock<IMediator>();
        var loggerMock = new Mock<ILogger<OrdersController>>();
        var controller = CreateController(mediatorMock, loggerMock);
        controller.HttpContext.Request.Headers["Cookie"] = "CompletedOrders=not-a-guid";

        var result = await controller.GetCompletedOrders();

        Assert.IsType<NotFoundResult>(result);
        mediatorMock.Verify(m => m.Send(It.IsAny<IRequest<IEnumerable<OrderSummaryDto>?>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetCompletedOrders_ReturnsNotFound_WhenMediatorReturnsNull()
    {
        var token = Guid.NewGuid();
        var mediatorMock = new Mock<IMediator>();
        mediatorMock.Setup(m => m.Send(It.Is<GetCompletedOrdersQuery>(q => q.Token == token), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IEnumerable<OrderSummaryDto>?)null);

        var loggerMock = new Mock<ILogger<OrdersController>>();
        var controller = CreateController(mediatorMock, loggerMock);
        controller.HttpContext.Request.Headers["Cookie"] = $"CompletedOrders={token:D}";

        var result = await controller.GetCompletedOrders();

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetCompletedOrders_ReturnsOk_WhenMediatorReturnsSummaries()
    {
        var token = Guid.NewGuid();
        var expectedSummaries = new[]
        {
            new OrderSummaryDto { OrderId = Guid.NewGuid(), Status = "Completed", Email = "user@example.com" }
        };

        var mediatorMock = new Mock<IMediator>();
        mediatorMock.Setup(m => m.Send(It.Is<GetCompletedOrdersQuery>(q => q.Token == token), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedSummaries);

        var loggerMock = new Mock<ILogger<OrdersController>>();
        var controller = CreateController(mediatorMock, loggerMock);
        controller.HttpContext.Request.Headers["Cookie"] = $"CompletedOrders={token:D}";

        var result = await controller.GetCompletedOrders();

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(expectedSummaries, Assert.IsAssignableFrom<IEnumerable<OrderSummaryDto>>(okResult.Value));
    }

    private static OrdersController CreateController(Mock<IMediator> mediatorMock, Mock<ILogger<OrdersController>> loggerMock)
    {
        var controller = new OrdersController(mediatorMock.Object, loggerMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };

        return controller;
    }
}
