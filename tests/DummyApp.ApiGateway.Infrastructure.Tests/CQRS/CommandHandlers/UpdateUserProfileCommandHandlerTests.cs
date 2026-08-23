using DummyApp.ApiGateway.Infrastructure.CQRS.CommandHandlers;
using DummyApp.ApiGateway.Infrastructure.CQRS.Commands;
using DummyApp.ApiGateway.Infrastructure.HttpClients;
using DummyApp.ApiGateway.Infrastructure.Models.Dtos;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DummyApp.ApiGateway.Infrastructure.Tests.CQRS.CommandHandlers.UpdateUserProfileCommandHandlerTests;

public sealed class HandleTests
{
    private readonly Mock<ILogger<UpdateUserProfileCommandHandler>> _loggerMock = new();
    private readonly Mock<IIdentityServiceHttpClient> _identityServiceHttpClientMock = new();

    private UpdateUserProfileCommandHandler CreateHandler()
        => new UpdateUserProfileCommandHandler(
            _identityServiceHttpClientMock.Object,
            _loggerMock.Object);

    private static CancellationToken None => CancellationToken.None;

    [Fact]
    public async Task Handle_ReturnsNull_WhenUserIdIsInvalid()
    {
        var handler = CreateHandler();
        var command = new UpdateUserProfileCommand(string.Empty, "First", "Last");

        var result = await handler.Handle(command, None);

        Assert.Null(result);
        _loggerMock.VerifyLog(LogLevel.Error, "Invalid user id supplied for profile update.", Times.Once());
        _identityServiceHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_CallsIdentityService_WhenUserIdIsValid()
    {
        var expectedUser = new UserDto
        {
            Id = "user-id",
            Email = "user@example.com",
            FirstName = "First",
            LastName = "Last",
            Roles = new[] { "Creator" }
        };

        _identityServiceHttpClientMock
            .Setup(x => x.UpdateUserProfileAsync("user-id", "First", "Last", None))
            .ReturnsAsync(expectedUser);

        var handler = CreateHandler();
        var command = new UpdateUserProfileCommand("user-id", "First", "Last");

        var result = await handler.Handle(command, None);

        Assert.Equal(expectedUser, result);
        _identityServiceHttpClientMock.Verify(x => x.UpdateUserProfileAsync("user-id", "First", "Last", None), Times.Once());
    }
}
