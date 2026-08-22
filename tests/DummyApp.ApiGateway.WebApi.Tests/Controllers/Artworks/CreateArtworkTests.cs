using System.Security.Claims;
using DummyApp.ApiGateway.Infrastructure.CQRS.Commands;
using DummyApp.ApiGateway.Infrastructure.Models.Dtos;
using DummyApp.ApiGateway.WebApi.Controllers;
using DummyApp.ApiGateway.WebApi.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DummyApp.ApiGateway.WebApi.Tests.Controllers.Artworks;

public sealed class CreateArtworkTests : ArtworksControllerTestBase
{
    [Fact]
    public async Task CreateArtwork_InvalidModelState_LogsWarningAndReturnsBadRequest()
    {
        var mediatorMock = new Mock<IMediator>();
        var loggerMock = new Mock<ILogger<ArtworksController>>();
        var controller = CreateController(mediatorMock, loggerMock);
        controller.ModelState.AddModelError("Name", "Required");

        var result = await controller.CreateArtwork(CreateValidArtworkRequest());

        Assert.IsType<BadRequestObjectResult>(result);
        loggerMock.VerifyLog(LogLevel.Warning, "CreateArtwork failed due to invalid model state", Times.Once());
        mediatorMock.Verify(m => m.Send(It.IsAny<CreateArtworkCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateArtwork_MissingCreatorId_LogsErrorAndReturnsForbid()
    {
        var mediatorMock = new Mock<IMediator>();
        var loggerMock = new Mock<ILogger<ArtworksController>>();
        var controller = CreateController(mediatorMock, loggerMock, CreateUserWithoutId());

        var result = await controller.CreateArtwork(CreateValidArtworkRequest());

        Assert.IsType<ForbidResult>(result);
        loggerMock.VerifyLog(LogLevel.Error, "CreateArtwork failed: creatorId is missing. User claims logged above.", Times.Once());
        mediatorMock.Verify(m => m.Send(It.IsAny<CreateArtworkCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateArtwork_TooManyTags_ReturnsBadRequestAndLogsWarning()
    {
        var mediatorMock = new Mock<IMediator>();
        var loggerMock = new Mock<ILogger<ArtworksController>>();
        var controller = CreateController(mediatorMock, loggerMock, CreateUserWithId("creator-123"));

        var request = CreateValidArtworkRequest();
        request.ExistingTagIds = Enumerable.Range(0, 8).Select(_ => Guid.NewGuid()).ToArray();
        request.NewTags = Enumerable.Range(0, 3).Select(_ => new NewTagRequest { Name = "Tag", Type = "None" }).ToArray();

        var result = await controller.CreateArtwork(request);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("A maximum of 10 tags is allowed.", badRequestResult.Value);
        loggerMock.VerifyLog(LogLevel.Warning, "CreateArtwork failed: too many tags provided. Count=11", Times.Once());
        mediatorMock.Verify(m => m.Send(It.IsAny<CreateArtworkCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateArtwork_MediatorReturnsNull_LogsErrorAndReturnsBadRequest()
    {
        var mediatorMock = new Mock<IMediator>();
        mediatorMock.Setup(m => m.Send(It.IsAny<CreateArtworkCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ArtworkDto?)null);

        var loggerMock = new Mock<ILogger<ArtworksController>>();
        var controller = CreateController(mediatorMock, loggerMock, CreateUserWithId("creator-123"));

        var result = await controller.CreateArtwork(CreateValidArtworkRequest());

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("An error occurred while creating the artwork.", badRequestResult.Value);
        loggerMock.VerifyLog(LogLevel.Error, "CreateArtwork failed: result is null after sending command.", Times.Once());
    }

    [Fact]
    public async Task CreateArtwork_Succeeds_ReturnsCreatedResult()
    {
        var expectedArtwork = new ArtworkDto
        {
            Id = Guid.NewGuid(),
            CreatorId = "creator-123",
            Name = "Name",
            Description = "Description",
            CreationDate = DateTime.UtcNow,
            UploadDate = DateTime.UtcNow,
            ImgUrl = "https://example.com/image.jpg",
            ThumbnailUrl = "https://example.com/image-small.jpg",
            IsActive = true
        };

        var mediatorMock = new Mock<IMediator>();
        mediatorMock.Setup(m => m.Send(It.Is<CreateArtworkCommand>(c =>
                c.Name == expectedArtwork.Name &&
                c.FileName == "file.png" &&
                c.Description == expectedArtwork.Description &&
                c.CreatorId == expectedArtwork.CreatorId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedArtwork);

        var loggerMock = new Mock<ILogger<ArtworksController>>();
        var controller = CreateController(mediatorMock, loggerMock, CreateUserWithId(expectedArtwork.CreatorId));

        var result = await controller.CreateArtwork(CreateValidArtworkRequest());

        var createdResult = Assert.IsType<CreatedResult>(result);
        Assert.Equal($"api/artworks/{expectedArtwork.Id}", createdResult.Location);
        Assert.Equal(expectedArtwork, Assert.IsType<ArtworkDto>(createdResult.Value));
    }
}
