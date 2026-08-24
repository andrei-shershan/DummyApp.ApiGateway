using System.Security.Claims;
using DummyApp.ApiGateway.Infrastructure.CQRS.Commands;
using DummyApp.ApiGateway.Infrastructure.Models.Dtos;
using DummyApp.ApiGateway.WebApi.Controllers;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace DummyApp.ApiGateway.WebApi.Tests.Controllers.Users;

public sealed class UpdateCurrentUserAvatarTests
{
    [Fact]
    public async Task UpdateCurrentUserAvatar_ReturnsOkResult()
    {
        var expectedProfile = new UserDto
        {
            Id = "user-1",
            Email = "user@example.com",
            FirstName = "First",
            LastName = "Last",
            AvatarUrl = "https://cdn.example.com/avatar.png",
            AvatarSmallUrl = "https://cdn.example.com/avatar-small.png",
            Roles = new[] { "Creator" },
            IsActive = true
        };

        var mediatorMock = new Mock<IMediator>();
        mediatorMock.Setup(m => m.Send(It.IsAny<UpdateUserAvatarCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedProfile);

        var controller = new UsersController(mediatorMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, expectedProfile.Id)
                    }, "TestAuth"))
                }
            }
        };

        var request = new UsersController.UpdateCurrentUserAvatarRequest("avatar.png", "base64data");
        var result = await controller.UpdateCurrentUserAvatar(request, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(expectedProfile, Assert.IsAssignableFrom<UserDto>(okResult.Value));
        mediatorMock.Verify(m => m.Send(It.Is<UpdateUserAvatarCommand>(c =>
            c.UserId == expectedProfile.Id &&
            c.FileName == request.FileName &&
            c.Base64Image == request.Base64Image), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateCurrentUserAvatar_ReturnsForbid_WhenUserIdMissing()
    {
        var mediatorMock = new Mock<IMediator>();
        var controller = new UsersController(mediatorMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(Array.Empty<Claim>(), "TestAuth"))
                }
            }
        };

        var request = new UsersController.UpdateCurrentUserAvatarRequest("avatar.png", "base64data");
        var result = await controller.UpdateCurrentUserAvatar(request, CancellationToken.None);

        Assert.IsType<ForbidResult>(result);
        mediatorMock.Verify(m => m.Send(It.IsAny<UpdateUserAvatarCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateCurrentUserAvatar_ReturnsBadRequest_WhenRequestIsInvalid()
    {
        var mediatorMock = new Mock<IMediator>();
        var controller = new UsersController(mediatorMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, "user-1")
                    }, "TestAuth"))
                }
            }
        };

        var result = await controller.UpdateCurrentUserAvatar(new UsersController.UpdateCurrentUserAvatarRequest(string.Empty, string.Empty), CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
        mediatorMock.Verify(m => m.Send(It.IsAny<UpdateUserAvatarCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
