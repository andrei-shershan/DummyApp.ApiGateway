using DummyApp.ApiGateway.Infrastructure.CQRS.Commands;
using DummyApp.ApiGateway.Infrastructure.CQRS.Queries;
using DummyApp.ApiGateway.Infrastructure.Models.Dtos;
using DummyApp.ApiGateway.WebApi.Configuration;
using DummyApp.ApiGateway.WebApi.Controllers;
using DummyApp.ApiGateway.WebApi.Models;
using DummyApp.ApiGateway.WebApi.Services;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Moq;
using Stripe;
using Stripe.Checkout;
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

    [Fact]
    public async Task AddItem_ReturnsBadRequest_WhenQuantityIsNotPositive()
    {
        var artworkId = Guid.NewGuid();
        var mediatorMock = new Mock<IMediator>();
        var loggerMock = new Mock<ILogger<BasketController>>();
        var controller = CreateController(mediatorMock, loggerMock);

        var result = await controller.AddItem(new AddArtworkToBasketRequest { ArtworkId = artworkId, Quantity = 0 });

        Assert.IsType<BadRequestObjectResult>(result);
        mediatorMock.Verify(m => m.Send(It.IsAny<AddArtworkToBasketCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AddItem_ReturnsBadRequest_WhenMediatorReturnsFalse()
    {
        var artworkId = Guid.NewGuid();
        var mediatorMock = new Mock<IMediator>();
        mediatorMock.Setup(m => m.Send(It.IsAny<AddArtworkToBasketCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var loggerMock = new Mock<ILogger<BasketController>>();
        var controller = CreateController(mediatorMock, loggerMock);

        var result = await controller.AddItem(new AddArtworkToBasketRequest { ArtworkId = artworkId });

        Assert.IsType<BadRequestObjectResult>(result);
        mediatorMock.Verify(m => m.Send(It.IsAny<AddArtworkToBasketCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateItem_ReturnsBadRequest_WhenRequestIsInvalid()
    {
        var mediatorMock = new Mock<IMediator>();
        var loggerMock = new Mock<ILogger<BasketController>>();
        var controller = CreateController(mediatorMock, loggerMock);

        var result = await controller.UpdateItem(Guid.Empty, new UpdateBasketItemRequest { Quantity = 1 });

        Assert.IsType<BadRequestObjectResult>(result);
        mediatorMock.Verify(m => m.Send(It.IsAny<UpdateArtworkInBasketCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateItem_ReturnsBadRequest_WhenRequestQuantityIsNegative()
    {
        var mediatorMock = new Mock<IMediator>();
        var loggerMock = new Mock<ILogger<BasketController>>();
        var controller = CreateController(mediatorMock, loggerMock);

        var result = await controller.UpdateItem(Guid.NewGuid(), new UpdateBasketItemRequest { Quantity = -1 });

        Assert.IsType<BadRequestObjectResult>(result);
        mediatorMock.Verify(m => m.Send(It.IsAny<UpdateArtworkInBasketCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateItem_ReturnsBadRequest_WhenBasketCookieIsMissing()
    {
        var mediatorMock = new Mock<IMediator>();
        var loggerMock = new Mock<ILogger<BasketController>>();
        var controller = CreateController(mediatorMock, loggerMock);

        var result = await controller.UpdateItem(Guid.NewGuid(), new UpdateBasketItemRequest { Quantity = 1 });

        Assert.IsType<BadRequestObjectResult>(result);
        mediatorMock.Verify(m => m.Send(It.IsAny<UpdateArtworkInBasketCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateItem_ReturnsBadRequest_WhenMediatorReturnsFalse()
    {
        var artworkId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var mediatorMock = new Mock<IMediator>();
        mediatorMock.Setup(m => m.Send(It.IsAny<UpdateArtworkInBasketCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var loggerMock = new Mock<ILogger<BasketController>>();
        var controller = CreateController(mediatorMock, loggerMock);
        controller.HttpContext.Request.Headers[HeaderNames.Cookie] = $"BasketId={orderId:D}";

        var result = await controller.UpdateItem(artworkId, new UpdateBasketItemRequest { Quantity = 1 });

        Assert.IsType<BadRequestObjectResult>(result);
        mediatorMock.Verify(m => m.Send(It.Is<UpdateArtworkInBasketCommand>(c => c.OrderId == orderId && c.ArtworkId == artworkId && c.Quantity == 1), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateItem_ReturnsOk_WhenMediatorReturnsTrue()
    {
        var artworkId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var mediatorMock = new Mock<IMediator>();
        mediatorMock.Setup(m => m.Send(It.IsAny<UpdateArtworkInBasketCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var loggerMock = new Mock<ILogger<BasketController>>();
        var controller = CreateController(mediatorMock, loggerMock);
        controller.HttpContext.Request.Headers[HeaderNames.Cookie] = $"BasketId={orderId:D}";

        var result = await controller.UpdateItem(artworkId, new UpdateBasketItemRequest { Quantity = 1 });

        Assert.IsType<OkResult>(result);
        mediatorMock.Verify(m => m.Send(It.Is<UpdateArtworkInBasketCommand>(c => c.OrderId == orderId && c.ArtworkId == artworkId && c.Quantity == 1), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SetStatus_ReturnsBadRequest_WhenStatusIsInvalid()
    {
        var mediatorMock = new Mock<IMediator>();
        var loggerMock = new Mock<ILogger<BasketController>>();
        var controller = CreateController(mediatorMock, loggerMock);
        controller.HttpContext.Request.Headers[HeaderNames.Cookie] = $"BasketId={Guid.NewGuid():D}";

        var result = await controller.SetStatus(new SetBasketStatusRequest { Status = "Invalid" });

        Assert.IsType<BadRequestObjectResult>(result);
        mediatorMock.Verify(m => m.Send(It.IsAny<SetOrderStatusCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SetStatus_ReturnsOk_WhenStatusIsValid()
    {
        var orderId = Guid.NewGuid();
        var mediatorMock = new Mock<IMediator>();
        mediatorMock.Setup(m => m.Send(It.IsAny<SetOrderStatusCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var loggerMock = new Mock<ILogger<BasketController>>();
        var controller = CreateController(mediatorMock, loggerMock);
        controller.HttpContext.Request.Headers[HeaderNames.Cookie] = $"BasketId={orderId:D}";

        var result = await controller.SetStatus(new SetBasketStatusRequest { Status = "Processing" });

        Assert.IsType<OkResult>(result);
        mediatorMock.Verify(m => m.Send(It.Is<SetOrderStatusCommand>(c => c.OrderId == orderId && c.Status == "Processing"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetPrintSizes_ReturnsOkWithPrintSizes()
    {
        var printSizes = new List<PrintSizeDto>
        {
            new PrintSizeDto { Id = 1, Name = "Small", Prices = Array.Empty<PriceDto>() }
        };

        var mediatorMock = new Mock<IMediator>();
        mediatorMock.Setup(m => m.Send(It.IsAny<GetPrintSizesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IEnumerable<PrintSizeDto>)printSizes);

        var loggerMock = new Mock<ILogger<BasketController>>();
        var controller = CreateController(mediatorMock, loggerMock);

        var result = await controller.GetPrintSizes();

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(printSizes, okResult.Value);
        mediatorMock.Verify(m => m.Send(It.IsAny<GetPrintSizesQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Checkout_ReturnsBadRequest_WhenBasketCookieIsMissing()
    {
        var mediatorMock = new Mock<IMediator>();
        var stripeSessionServiceMock = new Mock<IStripeSessionService>();
        var loggerMock = new Mock<ILogger<BasketController>>();
        var controller = CreateController(mediatorMock, loggerMock, stripeSessionServiceMock);

        var result = await controller.Checkout();

        Assert.IsType<BadRequestObjectResult>(result);
        mediatorMock.Verify(m => m.Send(It.IsAny<GetOrderSummaryQuery>(), It.IsAny<CancellationToken>()), Times.Never);
        stripeSessionServiceMock.Verify(s => s.CreateAsync(It.IsAny<SessionCreateOptions>()), Times.Never);
    }

    [Fact]
    public async Task Checkout_ReturnsBadRequest_WhenOrderSummaryIsMissing()
    {
        var orderId = Guid.NewGuid();
        var mediatorMock = new Mock<IMediator>();
        mediatorMock.Setup(m => m.Send(It.IsAny<GetOrderSummaryQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrderSummaryDto?)null);
        var stripeSessionServiceMock = new Mock<IStripeSessionService>();
        var loggerMock = new Mock<ILogger<BasketController>>();
        var controller = CreateController(mediatorMock, loggerMock, stripeSessionServiceMock);
        controller.HttpContext.Request.Headers[HeaderNames.Cookie] = $"BasketId={orderId:D}";

        var result = await controller.Checkout();

        Assert.IsType<BadRequestObjectResult>(result);
        stripeSessionServiceMock.Verify(s => s.CreateAsync(It.IsAny<SessionCreateOptions>()), Times.Never);
    }

    [Fact]
    public async Task Checkout_ReturnsBadRequest_WhenOrderStatusIsInvalid()
    {
        var orderId = Guid.NewGuid();
        var summary = new OrderSummaryDto
        {
            Items = new List<OrderItemDto>
            {
                new OrderItemDto { OrderId = orderId, ArtworkId = Guid.NewGuid(), Quantity = 1, Name = "Art", Description = "Desc", PriceValue = 10m }
            },
            Status = "Completed"
        };

        var mediatorMock = new Mock<IMediator>();
        mediatorMock.Setup(m => m.Send(It.IsAny<GetOrderSummaryQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(summary);
        var stripeSessionServiceMock = new Mock<IStripeSessionService>();
        var loggerMock = new Mock<ILogger<BasketController>>();
        var controller = CreateController(mediatorMock, loggerMock, stripeSessionServiceMock);
        controller.HttpContext.Request.Headers[HeaderNames.Cookie] = $"BasketId={orderId:D}";

        var result = await controller.Checkout();

        Assert.IsType<BadRequestObjectResult>(result);
        stripeSessionServiceMock.Verify(s => s.CreateAsync(It.IsAny<SessionCreateOptions>()), Times.Never);
    }

    [Fact]
    public async Task Checkout_ReturnsOk_WhenCheckoutSessionCreated()
    {
        var orderId = Guid.NewGuid();
        var summary = new OrderSummaryDto
        {
            Items = new List<OrderItemDto>
            {
                new OrderItemDto { OrderId = orderId, ArtworkId = Guid.NewGuid(), Quantity = 1, Name = "Art", Description = "Desc", PriceValue = 10m }
            },
            Status = "Processing"
        };

        var mediatorMock = new Mock<IMediator>();
        mediatorMock.Setup(m => m.Send(It.IsAny<GetOrderSummaryQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(summary);
        var stripeSessionServiceMock = new Mock<IStripeSessionService>();
        stripeSessionServiceMock.Setup(s => s.CreateAsync(It.IsAny<SessionCreateOptions>()))
            .ReturnsAsync(new Session { Url = "https://checkout.stripe.com/pay/session-id" });

        var loggerMock = new Mock<ILogger<BasketController>>();
        var controller = CreateController(mediatorMock, loggerMock, stripeSessionServiceMock);
        controller.HttpContext.Request.Headers[HeaderNames.Cookie] = $"BasketId={orderId:D}";

        var result = await controller.Checkout();

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("https://checkout.stripe.com/pay/session-id", ((CheckoutResponse)okResult.Value!).Url);
        stripeSessionServiceMock.Verify(s => s.CreateAsync(It.IsAny<SessionCreateOptions>()), Times.Once);
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
