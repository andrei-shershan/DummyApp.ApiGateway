using DummyApp.ApiGateway.Infrastructure.CQRS.Queries;
using DummyApp.ApiGateway.Infrastructure.Models.Dtos;
using DummyApp.ApiGateway.WebApi.Configuration;
using DummyApp.ApiGateway.WebApi.Controllers;
using DummyApp.ApiGateway.WebApi.Services;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Moq;
using Xunit;

namespace DummyApp.ApiGateway.WebApi.Tests.Controllers.Basket;

public sealed class GetAddressTests
{
    [Fact]
    public async Task GetAddress_ReturnsNotFound_WhenCookieMissing()
    {
        var mediatorMock = new Mock<IMediator>();
        var loggerMock = new Mock<ILogger<BasketController>>();
        var controller = CreateController(mediatorMock, loggerMock);

        var result = await controller.GetAddress();

        Assert.IsType<NotFoundResult>(result);
        mediatorMock.Verify(m => m.Send(It.IsAny<GetOrderAddressQuery>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetAddress_ReturnsNotFound_WhenHandlerReturnsNull()
    {
        var orderId = Guid.NewGuid();
        var mediatorMock = new Mock<IMediator>();
        mediatorMock.Setup(m => m.Send(It.IsAny<GetOrderAddressQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrderAddressDto?)null);

        var loggerMock = new Mock<ILogger<BasketController>>();
        var controller = CreateController(mediatorMock, loggerMock);
        controller.HttpContext.Request.Headers[HeaderNames.Cookie] = $"BasketId={orderId:D}";

        var result = await controller.GetAddress();

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetAddress_ReturnsOk_WhenHandlerReturnsAddress()
    {
        var orderId = Guid.NewGuid();
        var expected = new OrderAddressDto { FirstName = "John", LastName = "Doe", Email = "john.doe@example.com", Phone = "+48123123123", Country = "PL", City = "Warsaw", Street = "Main", HouseNumber = "10", PostalCode = "00-001" };
        var mediatorMock = new Mock<IMediator>();
        mediatorMock.Setup(m => m.Send(It.Is<GetOrderAddressQuery>(q => q.OrderId == orderId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var loggerMock = new Mock<ILogger<BasketController>>();
        var controller = CreateController(mediatorMock, loggerMock);
        controller.HttpContext.Request.Headers[HeaderNames.Cookie] = $"BasketId={orderId:D}";

        var result = await controller.GetAddress();

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(expected, okResult.Value);
    }

    private static BasketController CreateController(Mock<IMediator> mediatorMock, Mock<ILogger<BasketController>> loggerMock)
    {
        return new BasketController(mediatorMock.Object, loggerMock.Object, new Mock<IStripeSessionService>().Object, new ApiGatewaySettings())
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }
}
