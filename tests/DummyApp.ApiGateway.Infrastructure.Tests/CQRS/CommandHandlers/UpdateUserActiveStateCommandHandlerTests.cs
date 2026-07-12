using DummyApp.ApiGateway.Infrastructure.CQRS.CommandHandlers;
using DummyApp.ApiGateway.Infrastructure.CQRS.Commands;
using DummyApp.ApiGateway.Infrastructure.HttpClients;
using DummyApp.ApiGateway.Infrastructure.Models.Dtos;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DummyApp.ApiGateway.Infrastructure.Tests.CQRS.CommandHandlers.UpdateUserActiveStateCommandHandlerTests;

public sealed class HandleTests
{
    private readonly Mock<ILogger<UpdateUserActiveStateCommandHandler>> _loggerMock = new();
    private readonly Mock<IIdentityServiceHttpClient> _identityServiceHttpClientMock = new();

    private UpdateUserActiveStateCommandHandler CreateHandler()
        => new UpdateUserActiveStateCommandHandler(
            _identityServiceHttpClientMock.Object,
            _loggerMock.Object);

    private static CancellationToken None => CancellationToken.None;

    [Fact]
    public async Task Handle_ReturnsNull_WhenUserIdIsInvalid()
    {
        var handler = CreateHandler();
        var command = new UpdateUserActiveStateCommand(string.Empty, true);

        var result = await handler.Handle(command, None);

        Assert.Null(result);
        _loggerMock.VerifyLog(LogLevel.Error, "Invalid user id supplied for active state update.", Times.Once());
        _identityServiceHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_CallsIdentityService_WhenUserIdIsValid()
    {
        var expectedUser = new UserDto
        {
            Id = "user-id",
            Email = "user@example.com",
            FirstName = "User",
            LastName = "Example",
            Roles = new[] { "User" }
        };

        _identityServiceHttpClientMock
            .Setup(x => x.UpdateUserActiveStateAsync("user-id", false, None))
            .ReturnsAsync(expectedUser);

        var handler = CreateHandler();
        var command = new UpdateUserActiveStateCommand("user-id", false);

        var result = await handler.Handle(command, None);

        Assert.Equal(expectedUser, result);
        _identityServiceHttpClientMock.Verify(x => x.UpdateUserActiveStateAsync("user-id", false, None), Times.Once());
    }
}
