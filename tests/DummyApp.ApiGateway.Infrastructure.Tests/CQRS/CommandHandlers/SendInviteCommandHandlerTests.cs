using DummyApp.ApiGateway.Infrastructure.CQRS.CommandHandlers;
using DummyApp.ApiGateway.Infrastructure.CQRS.Commands;
using DummyApp.ApiGateway.Infrastructure.HttpClients;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DummyApp.ApiGateway.Infrastructure.Tests.CQRS.CommandHandlers.SendInviteCommandHandlerTests;

public sealed class HandleTests
{
    private readonly Mock<ILogger<SendInviteCommandHandler>> _loggerMock = new();
    private readonly Mock<IEmailServiceHttpClient> _emailServiceHttpClientMock = new();
    private readonly Mock<IIdentityServiceHttpClient> _identityServiceHttpClientMock = new();

    private SendInviteCommandHandler CreateHandler()
        => new SendInviteCommandHandler(
            _emailServiceHttpClientMock.Object,
            _identityServiceHttpClientMock.Object,
            _loggerMock.Object);

    private static CancellationToken None => CancellationToken.None;

    [Fact]
    public async Task Handle_ReturnsFalse_WhenEmailIsMissing()
    {
        var handler = CreateHandler();
        var result = await handler.Handle(new SendInviteCommand("  "), None);

        Assert.False(result);
        _emailServiceHttpClientMock.VerifyNoOtherCalls();
        _identityServiceHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_ReturnsFalse_WhenEmailServiceFails()
    {
        _emailServiceHttpClientMock
            .Setup(x => x.SendInviteAsync(It.IsAny<string>(), It.IsAny<string>(), None))
            .ReturnsAsync(false);

        var handler = CreateHandler();
        var result = await handler.Handle(new SendInviteCommand("admin@example.com"), None);

        Assert.False(result);
        _emailServiceHttpClientMock.Verify(x => x.SendInviteAsync("admin@example.com", It.IsAny<string>(), None), Times.Once);
        _identityServiceHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_ReturnsFalse_WhenIdentityServiceFails()
    {
        _emailServiceHttpClientMock
            .Setup(x => x.SendInviteAsync(It.IsAny<string>(), It.IsAny<string>(), None))
            .ReturnsAsync(true);
        _identityServiceHttpClientMock
            .Setup(x => x.SaveInviteTokenAsync(It.IsAny<string>(), It.IsAny<string>(), None))
            .ReturnsAsync(false);

        var handler = CreateHandler();
        var result = await handler.Handle(new SendInviteCommand("admin@example.com"), None);

        Assert.False(result);
        _emailServiceHttpClientMock.Verify(x => x.SendInviteAsync("admin@example.com", It.IsAny<string>(), None), Times.Once);
        _identityServiceHttpClientMock.Verify(x => x.SaveInviteTokenAsync("admin@example.com", It.IsAny<string>(), None), Times.Once);
    }

    [Fact]
    public async Task Handle_ReturnsTrue_WhenAllStepsSucceed()
    {
        _emailServiceHttpClientMock
            .Setup(x => x.SendInviteAsync(It.IsAny<string>(), It.IsAny<string>(), None))
            .ReturnsAsync(true);
        _identityServiceHttpClientMock
            .Setup(x => x.SaveInviteTokenAsync(It.IsAny<string>(), It.IsAny<string>(), None))
            .ReturnsAsync(true);

        var handler = CreateHandler();
        var result = await handler.Handle(new SendInviteCommand("admin@example.com"), None);

        Assert.True(result);
        _emailServiceHttpClientMock.Verify(x => x.SendInviteAsync("admin@example.com", It.IsAny<string>(), None), Times.Once);
        _identityServiceHttpClientMock.Verify(x => x.SaveInviteTokenAsync("admin@example.com", It.IsAny<string>(), None), Times.Once);
    }
}
