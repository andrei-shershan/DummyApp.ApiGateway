using DummyApp.ApiGateway.Infrastructure.CQRS.CommandHandlers;
using DummyApp.ApiGateway.Infrastructure.CQRS.Commands;
using DummyApp.ApiGateway.Infrastructure.HttpClients;
using DummyApp.ApiGateway.Infrastructure.Models.Dtos;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DummyApp.ApiGateway.Infrastructure.Tests.CQRS.CommandHandlers.CreateArtworkCommandHandlerTests;

public class HandleTests
{
    private readonly Mock<ILogger<CreateArtworkCommandHandler>> _loggerMock = new();
    private readonly Mock<IBlobServiceHttpClient> _blobServiceClientMock = new();
    private readonly Mock<IStorageServiceHttpClient> _storageServiceClientMock = new();

    private CreateArtworkCommandHandler CreateHandler()
        => new CreateArtworkCommandHandler(
            _storageServiceClientMock.Object,
            _blobServiceClientMock.Object,
            _loggerMock.Object);

    private static CancellationToken None => CancellationToken.None;

    [Fact]
    public async Task Handle_ReturnsNull_WhenFileNameIsMissing()
    {
        var handler = CreateHandler();
        var command = new CreateArtworkCommand(
            Name: "Test",
            FileName: "   ",
            Description: "desc",
            CreationDate: DateTime.UtcNow,
            IsActive: true,
            UploadedImage: "data",
            CreatorId: "creator");

        var result = await handler.Handle(command, None);

        Assert.Null(result);
        _loggerMock.VerifyLog(LogLevel.Error, "No file name provided for artwork creation.", Times.Once());
        _blobServiceClientMock.VerifyNoOtherCalls();
        _storageServiceClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_ReturnsNull_WhenFileNameHasNoExtension()
    {
        var handler = CreateHandler();
        var command = new CreateArtworkCommand(
            Name: "Test",
            FileName: "file",
            Description: "desc",
            CreationDate: DateTime.UtcNow,
            IsActive: true,
            UploadedImage: "data",
            CreatorId: "creator");

        var result = await handler.Handle(command, None);

        Assert.Null(result);
        _loggerMock.VerifyLog(LogLevel.Error, "File name file does not have a valid extension.", Times.Once());
        _blobServiceClientMock.VerifyNoOtherCalls();
        _storageServiceClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_ReturnsNull_WhenUploadImageFails()
    {
        _blobServiceClientMock
            .Setup(x => x.UploadImageAsync(It.IsAny<string>(), It.IsAny<string>(), None))
            .ReturnsAsync((ImageUploadResult?)null);

        var handler = CreateHandler();
        var command = new CreateArtworkCommand(
            Name: "Test",
            FileName: "file.png",
            Description: "desc",
            CreationDate: DateTime.UtcNow,
            IsActive: true,
            UploadedImage: "data",
            CreatorId: "creator");

        var result = await handler.Handle(command, None);

        Assert.Null(result);
        _loggerMock.VerifyLog(LogLevel.Error, "Failed to upload image to blob storage.", Times.Once());
        _storageServiceClientMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_ReturnsNull_WhenStorageServiceReturnsNull()
    {
        _blobServiceClientMock
            .Setup(x => x.UploadImageAsync(It.IsAny<string>(), It.IsAny<string>(), None))
            .ReturnsAsync(new ImageUploadResult("https://example.com/blob.png", "https://example.com/blob-small.png"));

        _storageServiceClientMock
            .Setup(x => x.CreateArtworkAsync(It.IsAny<ArtworkDto>(), None))
            .ReturnsAsync((ArtworkDto?)null);

        var handler = CreateHandler();
        var command = new CreateArtworkCommand(
            Name: "Test",
            FileName: "file.png",
            Description: "desc",
            CreationDate: DateTime.UtcNow,
            IsActive: true,
            UploadedImage: "data",
            CreatorId: "creator");

        var result = await handler.Handle(command, None);

        Assert.Null(result);
        _loggerMock.VerifyLog(LogLevel.Error, "Failed to create artwork in storage service.", Times.Once());
    }

    [Fact]
    public async Task Handle_ReturnsArtwork_WhenAllStepsSucceed()
    {
        var expected = new ArtworkDto { Id = 1, CreatorId = "creator", Name = "Test", Description = "desc", CreationDate = DateTime.UtcNow, UploadDate = DateTime.UtcNow, ImgUrl = "https://example.com/blob.png", ThumbnailUrl = "https://example.com/blob-small.png", IsActive = true };

        _blobServiceClientMock
            .Setup(x => x.UploadImageAsync(It.IsAny<string>(), It.IsAny<string>(), None))
            .ReturnsAsync(new ImageUploadResult("https://example.com/blob.png", "https://example.com/blob-small.png"));

        _storageServiceClientMock
            .Setup(x => x.CreateArtworkAsync(It.IsAny<ArtworkDto>(), None))
            .ReturnsAsync(expected);

        var handler = CreateHandler();
        var command = new CreateArtworkCommand(
            Name: "Test",
            FileName: "file.png",
            Description: "desc",
            CreationDate: DateTime.UtcNow,
            IsActive: true,
            UploadedImage: "data",
            CreatorId: "creator");

        var result = await handler.Handle(command, None);

        Assert.Equal(expected, result);
        _loggerMock.VerifyNoOtherCalls();
    }
}
