using DummyApp.ApiGateway.Infrastructure.CQRS.CommandHandlers;
using DummyApp.ApiGateway.Infrastructure.CQRS.Commands;
using DummyApp.ApiGateway.Infrastructure.HttpClients;
using DummyApp.ApiGateway.Infrastructure.Models;
using DummyApp.ApiGateway.Infrastructure.Models.Dtos;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DummyApp.ApiGateway.Infrastructure.Tests.CQRS.CommandHandlers.UpdateUserAvatarCommandHandlerTests;

public sealed class UpdateUserAvatarCommandHandlerTests
{
    private readonly Mock<IBlobServiceHttpClient> _blobServiceHttpClientMock = new();
    private readonly Mock<IIdentityServiceHttpClient> _identityServiceHttpClientMock = new();
    private readonly Mock<ILogger<UpdateUserAvatarCommandHandler>> _loggerMock = new();

    private UpdateUserAvatarCommandHandler CreateHandler()
        => new(_blobServiceHttpClientMock.Object, _identityServiceHttpClientMock.Object, _loggerMock.Object);

    [Fact]
    public async Task Handle_ReturnsNull_WhenUserIdIsMissing()
    {
        var handler = CreateHandler();

        var result = await handler.Handle(new UpdateUserAvatarCommand(string.Empty, "avatar.png", "base64data"), CancellationToken.None);

        Assert.Null(result);
        _blobServiceHttpClientMock.Verify(x => x.UploadImageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ImageType>(), It.IsAny<CancellationToken>()), Times.Never);
        _identityServiceHttpClientMock.Verify(x => x.UpdateUserAvatarAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ReturnsNull_WhenRequestIsMissingFileNameOrImageData()
    {
        var handler = CreateHandler();

        var resultWithNoFileName = await handler.Handle(new UpdateUserAvatarCommand("user-id", string.Empty, "base64data"), CancellationToken.None);
        var resultWithNoImage = await handler.Handle(new UpdateUserAvatarCommand("user-id", "avatar.png", string.Empty), CancellationToken.None);

        Assert.Null(resultWithNoFileName);
        Assert.Null(resultWithNoImage);
        _blobServiceHttpClientMock.Verify(x => x.UploadImageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ImageType>(), It.IsAny<CancellationToken>()), Times.Never);
        _identityServiceHttpClientMock.Verify(x => x.UpdateUserAvatarAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ReturnsNull_WhenBlobUploadFails()
    {
        _blobServiceHttpClientMock
            .Setup(x => x.UploadImageAsync("base64data", It.IsAny<string>(), ImageType.Avatar, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ImageUploadResult?)null);

        var handler = CreateHandler();

        var result = await handler.Handle(new UpdateUserAvatarCommand("user-id", "avatar.png", "base64data"), CancellationToken.None);

        Assert.Null(result);
        _blobServiceHttpClientMock.Verify(x => x.UploadImageAsync("base64data", It.IsAny<string>(), ImageType.Avatar, It.IsAny<CancellationToken>()), Times.Once);
        _identityServiceHttpClientMock.Verify(x => x.UpdateUserAvatarAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ReturnsUser_WhenUploadAndIdentityUpdateSucceed()
    {
        var uploadResult = new ImageUploadResult("https://example.com/avatar.png", "https://example.com/avatar-small.png");
        var expected = new UserDto
        {
            Id = "user-id",
            Email = "user@example.com",
            FirstName = "First",
            LastName = "Last",
            AvatarUrl = uploadResult.Url,
            AvatarSmallUrl = uploadResult.ThumbnailUrl,
            Roles = Array.Empty<string>(),
            IsActive = true
        };

        _blobServiceHttpClientMock
            .Setup(x => x.UploadImageAsync("base64data", It.IsAny<string>(), ImageType.Avatar, It.IsAny<CancellationToken>()))
            .ReturnsAsync(uploadResult);

        _identityServiceHttpClientMock
            .Setup(x => x.UpdateUserAvatarAsync("user-id", uploadResult.Url, uploadResult.ThumbnailUrl, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var handler = CreateHandler();

        var result = await handler.Handle(new UpdateUserAvatarCommand("user-id", "avatar.png", "base64data"), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(expected, result);
        _blobServiceHttpClientMock.Verify(x => x.UploadImageAsync("base64data", It.IsAny<string>(), ImageType.Avatar, It.IsAny<CancellationToken>()), Times.Once);
        _identityServiceHttpClientMock.Verify(x => x.UpdateUserAvatarAsync("user-id", uploadResult.Url, uploadResult.ThumbnailUrl, It.IsAny<CancellationToken>()), Times.Once);
    }
}
