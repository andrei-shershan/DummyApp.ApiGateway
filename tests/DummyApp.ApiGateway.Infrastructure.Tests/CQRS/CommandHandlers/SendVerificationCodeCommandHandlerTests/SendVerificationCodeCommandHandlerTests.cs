using System.Text.Json;
using DummyApp.ApiGateway.Infrastructure.CQRS.CommandHandlers;
using DummyApp.ApiGateway.Infrastructure.CQRS.Commands;
using DummyApp.ApiGateway.Infrastructure.HttpClients;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DummyApp.ApiGateway.Infrastructure.Tests.CQRS.CommandHandlers.SendVerificationCodeCommandHandlerTests;

public sealed class HandleTests
{
    private readonly Mock<IStorageServiceHttpClient> _storageServiceHttpClientMock = new();
    private readonly Mock<IEmailServiceHttpClient> _emailServiceHttpClientMock = new();
    private readonly Mock<ILogger<SendVerificationCodeCommandHandler>> _loggerMock = new();

    private SendVerificationCodeCommandHandler CreateHandler()
        => new(
            _storageServiceHttpClientMock.Object,
            _emailServiceHttpClientMock.Object,
            _loggerMock.Object);

    [Fact]
    public async Task Handle_ReturnsFalse_WhenEmailIsMissing()
    {
        var handler = CreateHandler();
        var result = await handler.Handle(new SendVerificationCodeCommand("  "), CancellationToken.None);

        Assert.False(result);
        _storageServiceHttpClientMock.VerifyNoOtherCalls();
        _emailServiceHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_ReturnsFalse_WhenStorageServiceFails()
    {
        _storageServiceHttpClientMock
            .Setup(x => x.CreateVerificationCodeAsync("admin@example.com", It.IsAny<string>(), It.IsAny<DateTime>(), CancellationToken.None))
            .ReturnsAsync(false);

        var handler = CreateHandler();
        var result = await handler.Handle(new SendVerificationCodeCommand("admin@example.com"), CancellationToken.None);

        Assert.False(result);
        _emailServiceHttpClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_ReturnsFalse_WhenEmailServiceFails()
    {
        _storageServiceHttpClientMock
            .Setup(x => x.CreateVerificationCodeAsync("admin@example.com", It.IsAny<string>(), It.IsAny<DateTime>(), CancellationToken.None))
            .ReturnsAsync(true);

        _emailServiceHttpClientMock
            .Setup(x => x.SendEmailAsync(It.IsAny<SendEmailRequest>(), CancellationToken.None))
            .ReturnsAsync(false);

        var handler = CreateHandler();
        var result = await handler.Handle(new SendVerificationCodeCommand("admin@example.com"), CancellationToken.None);

        Assert.False(result);
        _storageServiceHttpClientMock.Verify(x => x.CreateVerificationCodeAsync("admin@example.com", It.IsAny<string>(), It.IsAny<DateTime>(), CancellationToken.None), Times.Once);
        _emailServiceHttpClientMock.Verify(x => x.SendEmailAsync(It.Is<SendEmailRequest>(request =>
            request.Subject == "DummyApp verification code"
            && request.Recipients.Contains("admin@example.com")
            && request.Template == "VerificationCode"
            && request.Parameters.HasValue
            && request.Parameters.Value.GetProperty("code").GetString()!.Length == 6
            && request.Parameters.Value.GetProperty("email").GetString() == "admin@example.com"), CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handle_ReturnsTrue_WhenAllStepsSucceed()
    {
        _storageServiceHttpClientMock
            .Setup(x => x.CreateVerificationCodeAsync("admin@example.com", It.IsAny<string>(), It.IsAny<DateTime>(), CancellationToken.None))
            .ReturnsAsync(true);

        _emailServiceHttpClientMock
            .Setup(x => x.SendEmailAsync(It.IsAny<SendEmailRequest>(), CancellationToken.None))
            .ReturnsAsync(true);

        var handler = CreateHandler();
        var result = await handler.Handle(new SendVerificationCodeCommand("admin@example.com"), CancellationToken.None);

        Assert.True(result);
        _storageServiceHttpClientMock.Verify(x => x.CreateVerificationCodeAsync("admin@example.com", It.IsAny<string>(), It.IsAny<DateTime>(), CancellationToken.None), Times.Once);
        _emailServiceHttpClientMock.Verify(x => x.SendEmailAsync(It.Is<SendEmailRequest>(request =>
            request.Subject == "DummyApp verification code"
            && request.Recipients.Contains("admin@example.com")
            && request.Template == "VerificationCode"
            && request.Parameters.HasValue
            && request.Parameters.Value.GetProperty("code").GetString()!.Length == 6
            && request.Parameters.Value.GetProperty("email").GetString() == "admin@example.com"), CancellationToken.None), Times.Once);
    }
}
