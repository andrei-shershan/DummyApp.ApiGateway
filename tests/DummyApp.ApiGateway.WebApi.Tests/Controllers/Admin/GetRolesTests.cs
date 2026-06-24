using DummyApp.ApiGateway.Infrastructure.CQRS.Queries;
using DummyApp.ApiGateway.Infrastructure.Models.Dtos;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace DummyApp.ApiGateway.WebApi.Tests.Controllers.Admin;

public sealed class GetRolesTests : AdminControllerTestBase
{
    [Fact]
    public async Task GetRoles_ReturnsOkResult()
    {
        var expectedRoles = new[]
        {
            new RoleDto { Id = "role-1", Name = "Admin" }
        };

        var mediatorMock = new Mock<IMediator>();
        mediatorMock.Setup(m => m.Send(It.IsAny<GetRolesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedRoles);

        var controller = CreateController(mediatorMock);

        var result = await controller.GetRoles();

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(expectedRoles, Assert.IsAssignableFrom<IEnumerable<RoleDto>>(okResult.Value));
        mediatorMock.Verify(m => m.Send(It.IsAny<GetRolesQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
