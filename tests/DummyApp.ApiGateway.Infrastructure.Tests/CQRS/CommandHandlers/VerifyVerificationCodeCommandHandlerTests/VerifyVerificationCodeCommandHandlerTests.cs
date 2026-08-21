using DummyApp.ApiGateway.Infrastructure.CQRS.CommandHandlers;
using DummyApp.ApiGateway.Infrastructure.CQRS.Commands;
using DummyApp.ApiGateway.Infrastructure.HttpClients;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DummyApp.ApiGateway.Infrastructure.Tests.CQRS.CommandHandlers.VerifyVerificationCodeCommandHandlerTests;

public sealed class HandleTests
{
    private readonly Mock<IStorageServiceHttpClient> _storageServiceClientMock = new();
    private readonly Mock<ILogger<VerifyVerificationCodeCommandHandler>> _loggerMock = new();

    private VerifyVerificationCodeCommandHandler CreateHandler()
        => new(_storageServiceClientMock.Object, _loggerMock.Object);

    [Theory]
    [InlineData(null, "123456")]
    [InlineData("admin@example.com", null)]
    [InlineData("  ", "123456")]
    [InlineData("admin@example.com", "   ")]
    public async Task Handle_ReturnsFailure_WhenEmailOrCodeIsMissing(string? email, string? code)
    {
        var handler = CreateHandler();

        var result = await handler.Handle(new VerifyVerificationCodeCommand(email ?? string.Empty, code ?? string.Empty), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("Invalid input.", result.ErrorMessage);
        _storageServiceClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_ReturnsFailure_WhenEmailIsInvalid()
    {
        var handler = CreateHandler();

        var result = await handler.Handle(new VerifyVerificationCodeCommand("adminexample.com", "123456"), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("Invalid email or code.", result.ErrorMessage);
        _storageServiceClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_ReturnsFailure_WhenCodeLengthIsInvalid()
    {
        var handler = CreateHandler();

        var result = await handler.Handle(new VerifyVerificationCodeCommand("admin@example.com", "12345"), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("Invalid email or code.", result.ErrorMessage);
        _storageServiceClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_ReturnsFailure_WhenVerificationFails()
    {
        _storageServiceClientMock
            .Setup(x => x.VerifyVerificationCodeAsync("admin@example.com", "123456", CancellationToken.None))
            .ReturnsAsync(false);

        var handler = CreateHandler();

        var result = await handler.Handle(new VerifyVerificationCodeCommand("admin@example.com", "123456"), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("Invalid or expired verification code.", result.ErrorMessage);
        _storageServiceClientMock.Verify(x => x.VerifyVerificationCodeAsync("admin@example.com", "123456", CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handle_ReturnsFailure_WhenTokenPersistenceFails()
    {
        _storageServiceClientMock
            .Setup(x => x.VerifyVerificationCodeAsync("admin@example.com", "123456", CancellationToken.None))
            .ReturnsAsync(true);
        _storageServiceClientMock
            .Setup(x => x.CreateCompletedOrdersTokenAsync("admin@example.com", It.IsAny<Guid>(), It.IsAny<DateTime>(), CancellationToken.None))
            .ReturnsAsync(false);

        var handler = CreateHandler();

        var result = await handler.Handle(new VerifyVerificationCodeCommand("admin@example.com", "123456"), CancellationToken.None);

        Assert.False(result.Success);
        Assert.True(result.IsServerError);
        Assert.Equal("Unable to persist completed orders token.", result.ErrorMessage);
        _storageServiceClientMock.Verify(x => x.VerifyVerificationCodeAsync("admin@example.com", "123456", CancellationToken.None), Times.Once);
        _storageServiceClientMock.Verify(x => x.CreateCompletedOrdersTokenAsync("admin@example.com", It.IsAny<Guid>(), It.IsAny<DateTime>(), CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handle_ReturnsSuccess_WhenVerificationAndPersistenceSucceed()
    {
        _storageServiceClientMock
            .Setup(x => x.VerifyVerificationCodeAsync("admin@example.com", "123456", CancellationToken.None))
            .ReturnsAsync(true);
        _storageServiceClientMock
            .Setup(x => x.CreateCompletedOrdersTokenAsync("admin@example.com", It.IsAny<Guid>(), It.IsAny<DateTime>(), CancellationToken.None))
            .ReturnsAsync(true);

        var handler = CreateHandler();

        var result = await handler.Handle(new VerifyVerificationCodeCommand("admin@example.com", "123456"), CancellationToken.None);

        Assert.True(result.Success);
        Assert.NotEqual(Guid.Empty, result.Token);
        Assert.True(result.ExpiresAt > DateTime.UtcNow);
        _storageServiceClientMock.Verify(x => x.VerifyVerificationCodeAsync("admin@example.com", "123456", CancellationToken.None), Times.Once);
        _storageServiceClientMock.Verify(x => x.CreateCompletedOrdersTokenAsync("admin@example.com", result.Token, result.ExpiresAt, CancellationToken.None), Times.Once);
    }
}
