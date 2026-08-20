using DummyApp.ApiGateway.WebApi.Controllers;
using DummyApp.ApiGateway.WebApi.Models;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DummyApp.ApiGateway.WebApi.Tests.Controllers.CompletedOrders;

public sealed class VerificationControllerTests
{
    [Fact]
    public async Task SendVerificationCode_ReturnsBadRequest_WhenRequestIsInvalid()
    {
        var mediatorMock = new Mock<IMediator>();
        var loggerMock = new Mock<ILogger<VerificationController>>();
        var controller = CreateController(mediatorMock, loggerMock);

        var result = await controller.SendVerificationCode(null!);

        Assert.IsType<BadRequestObjectResult>(result);
        mediatorMock.Verify(m => m.Send(It.IsAny<IRequest<bool>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SendVerificationCode_ReturnsInternalServerError_WhenMediatorReturnsFalse()
    {
        var mediatorMock = new Mock<IMediator>();
        mediatorMock.Setup(m => m.Send(It.IsAny<IRequest<bool>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var loggerMock = new Mock<ILogger<VerificationController>>();
        var controller = CreateController(mediatorMock, loggerMock);

        var result = await controller.SendVerificationCode(new SendVerificationCodeRequest { Email = "admin@example.com" });

        var statusResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, statusResult.StatusCode);
    }

    [Fact]
    public async Task SendVerificationCode_ReturnsOk_WhenMediatorReturnsTrue()
    {
        var mediatorMock = new Mock<IMediator>();
        mediatorMock.Setup(m => m.Send(It.IsAny<IRequest<bool>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var loggerMock = new Mock<ILogger<VerificationController>>();
        var controller = CreateController(mediatorMock, loggerMock);

        var result = await controller.SendVerificationCode(new SendVerificationCodeRequest { Email = "admin@example.com" });

        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task VerifyVerificationCode_ReturnsBadRequest_WhenRequestIsInvalid()
    {
        var mediatorMock = new Mock<IMediator>();
        var loggerMock = new Mock<ILogger<VerificationController>>();
        var controller = CreateController(mediatorMock, loggerMock);

        var result = await controller.VerifyVerificationCode(null!);

        Assert.IsType<BadRequestObjectResult>(result);
        mediatorMock.Verify(m => m.Send(It.IsAny<IRequest<bool>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task VerifyVerificationCode_ReturnsBadRequest_WhenMediatorReturnsFalse()
    {
        var mediatorMock = new Mock<IMediator>();
        mediatorMock.Setup(m => m.Send(It.IsAny<IRequest<bool>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var loggerMock = new Mock<ILogger<VerificationController>>();
        var controller = CreateController(mediatorMock, loggerMock);

        var result = await controller.VerifyVerificationCode(new VerifyVerificationCodeRequest { Email = "admin@example.com", Code = "123456" });

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task VerifyVerificationCode_ReturnsOk_AndSetsCompletedOrdersCookie_WhenMediatorReturnsTrue()
    {
        var mediatorMock = new Mock<IMediator>();
        mediatorMock.Setup(m => m.Send(It.IsAny<IRequest<bool>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var loggerMock = new Mock<ILogger<VerificationController>>();
        var controller = CreateController(mediatorMock, loggerMock);

        var result = await controller.VerifyVerificationCode(new VerifyVerificationCodeRequest { Email = "admin@example.com", Code = "123456" });

        Assert.IsType<OkResult>(result);
        Assert.True(controller.Response.Headers.ContainsKey("Set-Cookie"));
        Assert.Contains("CompletedOrders=true", controller.Response.Headers["Set-Cookie"].ToString());
    }

    private static VerificationController CreateController(Mock<IMediator> mediatorMock, Mock<ILogger<VerificationController>> loggerMock)
    {
        var controller = new VerificationController(mediatorMock.Object, loggerMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };

        return controller;
    }
}
