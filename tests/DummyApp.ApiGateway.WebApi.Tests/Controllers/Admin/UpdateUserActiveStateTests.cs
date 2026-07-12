using DummyApp.ApiGateway.Infrastructure.CQRS.Commands;
using DummyApp.ApiGateway.Infrastructure.Models.Dtos;
using DummyApp.ApiGateway.WebApi.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace DummyApp.ApiGateway.WebApi.Tests.Controllers.Admin;

public sealed class UpdateUserActiveStateTests : AdminControllerTestBase
{
    [Fact]
    public async Task UpdateUserActiveState_RequestIsNull_ReturnsBadRequest()
    {
        var mediatorMock = new Mock<IMediator>();
        var controller = CreateController(mediatorMock);

        var result = await controller.UpdateUserActiveState("user-id", null);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Request body is required.", badRequest.Value);
        mediatorMock.Verify(m => m.Send(It.IsAny<UpdateUserActiveStateCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateUserActiveState_WhenUserNotFound_ReturnsNotFound()
    {
        var mediatorMock = new Mock<IMediator>();
        mediatorMock.Setup(m => m.Send(It.IsAny<UpdateUserActiveStateCommand>(), It.IsAny<CancellationToken>() ))
            .ReturnsAsync((UserDto?)null);

        var controller = CreateController(mediatorMock);
        var request = new UpdateUserActiveStateRequest { IsActive = true };

        var result = await controller.UpdateUserActiveState("missing-user-id", request);

        Assert.IsType<NotFoundResult>(result);
        mediatorMock.Verify(m => m.Send(It.Is<UpdateUserActiveStateCommand>(c => c.UserId == "missing-user-id" && c.IsActive), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateUserActiveState_WhenUserUpdated_ReturnsOkResult()
    {
        var expectedUser = new UserDto { Id = "user-id", Email = "user@example.com", FirstName = "User", LastName = "Example", Roles = new[] { "User" } };
        var mediatorMock = new Mock<IMediator>();
        mediatorMock.Setup(m => m.Send(It.IsAny<UpdateUserActiveStateCommand>(), It.IsAny<CancellationToken>() ))
            .ReturnsAsync(expectedUser);

        var controller = CreateController(mediatorMock);
        var request = new UpdateUserActiveStateRequest { IsActive = false };

        var result = await controller.UpdateUserActiveState("user-id", request);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(expectedUser, Assert.IsAssignableFrom<UserDto>(okResult.Value));
        mediatorMock.Verify(m => m.Send(It.Is<UpdateUserActiveStateCommand>(c => c.UserId == "user-id" && c.IsActive == false), It.IsAny<CancellationToken>()), Times.Once);
    }
}
