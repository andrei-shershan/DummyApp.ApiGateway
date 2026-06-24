using DummyApp.ApiGateway.Infrastructure.CQRS.Queries;
using DummyApp.ApiGateway.Infrastructure.Models.Dtos;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace DummyApp.ApiGateway.WebApi.Tests.Controllers.Admin;

public sealed class GetUsersTests : AdminControllerTestBase
{
    [Fact]
    public async Task GetUsers_ReturnsOkResult()
    {
        var expectedUsers = new[]
        {
            new UserDto { Id = "1", Email = "admin@example.com", FirstName = "Admin", LastName = "User", Roles = new[] { "Admin" } }
        };

        var mediatorMock = new Mock<IMediator>();
        mediatorMock.Setup(m => m.Send(It.IsAny<GetUsersQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedUsers);

        var controller = CreateController(mediatorMock);

        var result = await controller.GetUsers();

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(expectedUsers, Assert.IsAssignableFrom<IEnumerable<UserDto>>(okResult.Value));
        mediatorMock.Verify(m => m.Send(It.IsAny<GetUsersQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
