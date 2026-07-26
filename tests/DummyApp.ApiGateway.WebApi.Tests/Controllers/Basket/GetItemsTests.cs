using DummyApp.ApiGateway.Infrastructure.CQRS.Commands;
using DummyApp.ApiGateway.Infrastructure.CQRS.Queries;
using DummyApp.ApiGateway.Infrastructure.Models.Dtos;
using DummyApp.ApiGateway.WebApi.Controllers;
using DummyApp.ApiGateway.WebApi.Models;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Moq;
using Xunit;

namespace DummyApp.ApiGateway.WebApi.Tests.Controllers.Basket;

public sealed class GetItemsTests
{
    [Fact]
    public async Task GetItems_ReturnsNotFound_WhenCookieMissing()
    {
        var mediatorMock = new Mock<IMediator>();
        var loggerMock = new Mock<ILogger<BasketController>>();
        var controller = CreateController(mediatorMock, loggerMock);

        var result = await controller.GetItems();

        Assert.IsType<NotFoundResult>(result);
        mediatorMock.Verify(m => m.Send(It.IsAny<GetOrderItemsQuery>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetItems_ReturnsNotFound_WhenHandlerReturnsNull()
    {
        var orderId = Guid.NewGuid();
        var mediatorMock = new Mock<IMediator>();
        mediatorMock.Setup(m => m.Send(It.IsAny<GetOrderItemsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IEnumerable<OrderItemDto>?)null);

        var loggerMock = new Mock<ILogger<BasketController>>();
        var controller = CreateController(mediatorMock, loggerMock);
        controller.HttpContext.Request.Headers[HeaderNames.Cookie] = $"BasketId={orderId:D}";

        var result = await controller.GetItems();

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetItems_ReturnsOk_WhenHandlerReturnsItems()
    {
        var orderId = Guid.NewGuid();
        var items = new[]
        {
            new OrderItemDto { OrderId = orderId, ArtworkId = Guid.NewGuid(), Quantity = 1, Name = "Test art", Description = "Desc", ImgUrl = "img", ThumbnailUrl = "thumb" }
        };

        var mediatorMock = new Mock<IMediator>();
        mediatorMock.Setup(m => m.Send(It.Is<GetOrderItemsQuery>(q => q.OrderId == orderId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(items);

        var loggerMock = new Mock<ILogger<BasketController>>();
        var controller = CreateController(mediatorMock, loggerMock);
        controller.HttpContext.Request.Headers[HeaderNames.Cookie] = $"BasketId={orderId:D}";

        var result = await controller.GetItems();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedItems = Assert.IsAssignableFrom<IEnumerable<OrderItemDto>>(okResult.Value);
        Assert.Equal(items, returnedItems);
    }

    private static BasketController CreateController(Mock<IMediator> mediatorMock, Mock<ILogger<BasketController>> loggerMock)
    {
        return new BasketController(mediatorMock.Object, loggerMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }
}
