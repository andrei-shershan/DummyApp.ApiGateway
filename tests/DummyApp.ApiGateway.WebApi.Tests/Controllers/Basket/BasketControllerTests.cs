using DummyApp.ApiGateway.Infrastructure.CQRS.Commands;
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

public sealed class BasketControllerTests
{
    [Fact]
    public async Task AddItem_ReturnsBadRequest_WhenRequestIsInvalid()
    {
        var mediatorMock = new Mock<IMediator>();
        var loggerMock = new Mock<ILogger<BasketController>>();
        var controller = CreateController(mediatorMock, loggerMock);

        var result = await controller.AddItem(null!);

        Assert.IsType<BadRequestObjectResult>(result);
        mediatorMock.Verify(m => m.Send(It.IsAny<AddArtworkToBasketCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AddItem_CreatesCookie_WhenNoneExists()
    {
        var artworkId = Guid.NewGuid();
        var mediatorMock = new Mock<IMediator>();
        mediatorMock.Setup(m => m.Send(It.IsAny<AddArtworkToBasketCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var loggerMock = new Mock<ILogger<BasketController>>();
        var controller = CreateController(mediatorMock, loggerMock);

        var result = await controller.AddItem(new AddArtworkToBasketRequest { ArtworkId = artworkId });

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        Assert.True(controller.Response.Headers.ContainsKey("Set-Cookie"));
        mediatorMock.Verify(m => m.Send(It.Is<AddArtworkToBasketCommand>(c => c.ArtworkId == artworkId && c.Quantity == 1), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddItem_ReusesCookie_WhenCookieExists()
    {
        var artworkId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var mediatorMock = new Mock<IMediator>();
        mediatorMock.Setup(m => m.Send(It.IsAny<AddArtworkToBasketCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var loggerMock = new Mock<ILogger<BasketController>>();
        var controller = CreateController(mediatorMock, loggerMock);
        controller.HttpContext.Request.Headers[HeaderNames.Cookie] = $"BasketId={orderId:D}";

        var result = await controller.AddItem(new AddArtworkToBasketRequest { ArtworkId = artworkId });

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        Assert.False(controller.Response.Headers.ContainsKey("Set-Cookie"));
        mediatorMock.Verify(m => m.Send(It.Is<AddArtworkToBasketCommand>(c => c.OrderId == orderId && c.ArtworkId == artworkId && c.Quantity == 1), It.IsAny<CancellationToken>()), Times.Once);
    }

    private static BasketController CreateController(Mock<IMediator> mediatorMock, Mock<ILogger<BasketController>> loggerMock)
    {
        var controller = new BasketController(mediatorMock.Object, loggerMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };

        return controller;
    }
}
