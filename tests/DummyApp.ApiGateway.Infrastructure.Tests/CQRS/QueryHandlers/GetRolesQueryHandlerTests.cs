using DummyApp.ApiGateway.Infrastructure.CQRS.QueryHandlers;
using DummyApp.ApiGateway.Infrastructure.HttpClients;
using DummyApp.ApiGateway.Infrastructure.Models.Dtos;
using Moq;
using Xunit;

namespace DummyApp.ApiGateway.Infrastructure.Tests.CQRS.QueryHandlers;

public sealed class GetRolesQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsRoles_WhenIdentityClientReturnsRoles()
    {
        var expectedRoles = new[]
        {
            new RoleDto { Id = "role-1", Name = "Admin" }
        };

        var identityClientMock = new Mock<IIdentityServiceHttpClient>();
        identityClientMock.Setup(x => x.GetRolesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedRoles);

        var handler = new GetRolesQueryHandler(identityClientMock.Object);

        var result = await handler.Handle(new DummyApp.ApiGateway.Infrastructure.CQRS.Queries.GetRolesQuery(), CancellationToken.None);

        Assert.Equal(expectedRoles, result);
    }

    [Fact]
    public async Task Handle_ReturnsEmptyList_WhenIdentityClientReturnsNull()
    {
        var identityClientMock = new Mock<IIdentityServiceHttpClient>();
        identityClientMock.Setup(x => x.GetRolesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((IEnumerable<RoleDto>?)null);

        var handler = new GetRolesQueryHandler(identityClientMock.Object);

        var result = await handler.Handle(new DummyApp.ApiGateway.Infrastructure.CQRS.Queries.GetRolesQuery(), CancellationToken.None);

        Assert.Empty(result);
    }
}
