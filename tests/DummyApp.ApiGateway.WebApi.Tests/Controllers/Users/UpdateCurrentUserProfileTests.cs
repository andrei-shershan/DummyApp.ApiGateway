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

public sealed class UpdateCurrentUserProfileTests
{
    [Fact]
    public async Task UpdateCurrentUserProfile_ReturnsOkResult()
    {
        var expectedProfile = new UserDto
        {
            Id = "user-1",
            Email = "user@example.com",
            FirstName = "NewFirst",
            LastName = "NewLast",
            Roles = new[] { "Creator" },
            IsActive = true
        };

        var mediatorMock = new Mock<IMediator>();
        mediatorMock.Setup(m => m.Send(It.IsAny<UpdateUserProfileCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedProfile);

        var controller = new UsersController(mediatorMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, expectedProfile.Id),
                        new Claim(ClaimTypes.Role, "Creator")
                    }, "TestAuth"))
                }
            }
        };

        var request = new UsersController.UpdateCurrentUserProfileRequest(expectedProfile.FirstName, expectedProfile.LastName);
        var result = await controller.UpdateCurrentUserProfile(request, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(expectedProfile, Assert.IsAssignableFrom<UserDto>(okResult.Value));
        mediatorMock.Verify(m => m.Send(It.Is<UpdateUserProfileCommand>(c =>
            c.UserId == expectedProfile.Id &&
            c.FirstName == expectedProfile.FirstName &&
            c.LastName == expectedProfile.LastName), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateCurrentUserProfile_ReturnsForbid_WhenUserIdMissing()
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

        var request = new UsersController.UpdateCurrentUserProfileRequest("First", "Last");
        var result = await controller.UpdateCurrentUserProfile(request, CancellationToken.None);

        Assert.IsType<ForbidResult>(result);
        mediatorMock.Verify(m => m.Send(It.IsAny<UpdateUserProfileCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateCurrentUserProfile_ReturnsBadRequest_WhenRequestIsInvalid()
    {
        var mediatorMock = new Mock<IMediator>();
        var profileId = "user-1";
        var controller = new UsersController(mediatorMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, profileId),
                        new Claim(ClaimTypes.Role, "Creator")
                    }, "TestAuth"))
                }
            }
        };

        var result = await controller.UpdateCurrentUserProfile(new UsersController.UpdateCurrentUserProfileRequest(string.Empty, string.Empty), CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
        mediatorMock.Verify(m => m.Send(It.IsAny<UpdateUserProfileCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
