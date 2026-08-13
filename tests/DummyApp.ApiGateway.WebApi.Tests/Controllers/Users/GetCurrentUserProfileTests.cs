using System.Security.Claims;
using DummyApp.ApiGateway.Infrastructure.CQRS.Queries;
using DummyApp.ApiGateway.Infrastructure.Models.Dtos;
using DummyApp.ApiGateway.WebApi.Controllers;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace DummyApp.ApiGateway.WebApi.Tests.Controllers.Users;

public sealed class GetCurrentUserProfileTests
{
    [Fact]
    public async Task GetCurrentUserProfile_ReturnsOkResult()
    {
        var expectedProfile = new UserDto
        {
            Id = "user-1",
            Email = "user@example.com",
            FirstName = "First",
            LastName = "Last",
            Roles = new[] { "Creator" },
            IsActive = true
        };

        var mediatorMock = new Mock<IMediator>();
        mediatorMock.Setup(m => m.Send(It.IsAny<GetUserProfileQuery>(), It.IsAny<CancellationToken>()))
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

        var result = await controller.GetCurrentUserProfile(CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(expectedProfile, Assert.IsAssignableFrom<UserDto>(okResult.Value));
        mediatorMock.Verify(m => m.Send(It.Is<GetUserProfileQuery>(q => q.UserId == expectedProfile.Id), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetCurrentUserProfile_ReturnsForbid_WhenUserIdMissing()
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

        var result = await controller.GetCurrentUserProfile(CancellationToken.None);

        Assert.IsType<ForbidResult>(result);
        mediatorMock.Verify(m => m.Send(It.IsAny<GetUserProfileQuery>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
