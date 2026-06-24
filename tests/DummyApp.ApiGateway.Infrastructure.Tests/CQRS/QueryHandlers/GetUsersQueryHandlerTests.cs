using DummyApp.ApiGateway.Infrastructure.CQRS.QueryHandlers;
using DummyApp.ApiGateway.Infrastructure.HttpClients;
using DummyApp.ApiGateway.Infrastructure.Models.Dtos;
using Moq;
using Xunit;

namespace DummyApp.ApiGateway.Infrastructure.Tests.CQRS.QueryHandlers;

public sealed class GetUsersQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsUsers_WhenIdentityClientReturnsUsers()
    {
        var expectedUsers = new[]
        {
            new UserDto { Id = "1", Email = "admin@example.com", FirstName = "Admin", LastName = "User", Roles = new[] { "Admin" } }
        };

        var identityClientMock = new Mock<IIdentityServiceHttpClient>();
        identityClientMock.Setup(x => x.GetUsersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedUsers);

        var handler = new GetUsersQueryHandler(identityClientMock.Object);

        var result = await handler.Handle(new DummyApp.ApiGateway.Infrastructure.CQRS.Queries.GetUsersQuery(), CancellationToken.None);

        Assert.Equal(expectedUsers, result);
    }

    [Fact]
    public async Task Handle_ReturnsEmptyList_WhenIdentityClientReturnsNull()
    {
        var identityClientMock = new Mock<IIdentityServiceHttpClient>();
        identityClientMock.Setup(x => x.GetUsersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((IEnumerable<UserDto>?)null);

        var handler = new GetUsersQueryHandler(identityClientMock.Object);

        var result = await handler.Handle(new DummyApp.ApiGateway.Infrastructure.CQRS.Queries.GetUsersQuery(), CancellationToken.None);

        Assert.Empty(result);
    }
}
