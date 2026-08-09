using DummyApp.ApiGateway.Infrastructure.CQRS.Commands;
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
using Xunit;

namespace DummyApp.ApiGateway.WebApi.Tests.Controllers.Basket;

public sealed class SaveAddressTests
{
    [Fact]
    public async Task SaveAddress_ReturnsBadRequest_WhenRequestIsNull()
    {
        var mediatorMock = new Mock<IMediator>();
        var loggerMock = new Mock<ILogger<BasketController>>();
        var controller = CreateController(mediatorMock, loggerMock);

        var result = await controller.SaveAddress(null!);

        Assert.IsType<BadRequestObjectResult>(result);
        mediatorMock.Verify(m => m.Send(It.IsAny<SaveOrderAddressCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SaveAddress_ReturnsBadRequest_WhenCookieMissing()
    {
        var mediatorMock = new Mock<IMediator>();
        var loggerMock = new Mock<ILogger<BasketController>>();
        var controller = CreateController(mediatorMock, loggerMock);

        var result = await controller.SaveAddress(new SaveBasketAddressRequest());

        Assert.IsType<BadRequestObjectResult>(result);
        mediatorMock.Verify(m => m.Send(It.IsAny<SaveOrderAddressCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SaveAddress_ReturnsBadRequest_WhenHandlerReturnsFalse()
    {
        var orderId = Guid.NewGuid();
        var mediatorMock = new Mock<IMediator>();
        mediatorMock.Setup(m => m.Send(It.IsAny<SaveOrderAddressCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var loggerMock = new Mock<ILogger<BasketController>>();
        var controller = CreateController(mediatorMock, loggerMock);
        controller.HttpContext.Request.Headers[HeaderNames.Cookie] = $"BasketId={orderId:D}";

        var result = await controller.SaveAddress(new SaveBasketAddressRequest { FirstName = "John", LastName = "Doe", Email = "john.doe@example.com", Phone = "+48123123123", Country = "PL", City = "Warsaw", Street = "Main", HouseNumber = "10", PostalCode = "00-001" });

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task SaveAddress_ReturnsOk_WhenHandlerReturnsTrue()
    {
        var orderId = Guid.NewGuid();
        var mediatorMock = new Mock<IMediator>();
        mediatorMock.Setup(m => m.Send(It.IsAny<SaveOrderAddressCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var loggerMock = new Mock<ILogger<BasketController>>();
        var controller = CreateController(mediatorMock, loggerMock);
        controller.HttpContext.Request.Headers[HeaderNames.Cookie] = $"BasketId={orderId:D}";

        var result = await controller.SaveAddress(new SaveBasketAddressRequest { FirstName = "John", LastName = "Doe", Email = "john.doe@example.com", Phone = "+48123123123", Country = "PL", City = "Warsaw", Street = "Main", HouseNumber = "10", PostalCode = "00-001" });

        Assert.IsType<OkResult>(result);
        mediatorMock.Verify(m => m.Send(It.Is<SaveOrderAddressCommand>(c => c.OrderId == orderId && c.Address.FirstName == "John" && c.Address.Email == "john.doe@example.com"), It.IsAny<CancellationToken>()), Times.Once);
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
