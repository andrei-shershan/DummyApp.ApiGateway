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

public sealed class UpdateArtworkIsActiveTests : ArtworksControllerTestBase
{
    [Fact]
    public async Task UpdateArtworkActive_ReturnsBadRequest_WhenRequestIsNull()
    {
        var mediatorMock = new Mock<IMediator>();
        var loggerMock = new Mock<ILogger<ArtworksController>>();
        var controller = CreateController(mediatorMock, loggerMock, CreateUserWithId("creator-1"));

        var artworkId = Guid.NewGuid();
        var result = await controller.UpdateArtworkActive(artworkId, null!);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Request body is required.", badRequest.Value);
        mediatorMock.Verify(m => m.Send(It.IsAny<UpdateArtworkIsActiveCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateArtworkActive_ReturnsOk_WhenCommandSucceeds()
    {
        var expectedArtwork = new ArtworkDto
        {
            Id = Guid.NewGuid(),
            CreatorId = "creator-1",
            Name = "Artwork",
            Description = "Description",
            CreationDate = DateTime.UtcNow,
            UploadDate = DateTime.UtcNow,
            ImgUrl = "img-path",
            ThumbnailUrl = "thumb-path",
            IsActive = true
        };

        var mediatorMock = new Mock<IMediator>();
        var artworkId = Guid.NewGuid();
        mediatorMock.Setup(m => m.Send(It.Is<UpdateArtworkIsActiveCommand>(c => c.ArtworkId == artworkId && c.IsActive == true), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedArtwork);

        var loggerMock = new Mock<ILogger<ArtworksController>>();
        var controller = CreateController(mediatorMock, loggerMock, CreateUserWithId("creator-1"));

        var result = await controller.UpdateArtworkActive(artworkId, new UpdateArtworkIsActiveRequest { IsActive = true });

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(expectedArtwork, okResult.Value);
        mediatorMock.Verify(m => m.Send(It.Is<UpdateArtworkIsActiveCommand>(c => c.ArtworkId == artworkId && c.IsActive == true), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateArtworkActive_ReturnsBadRequest_WhenCommandReturnsNull()
    {
        var artworkId = Guid.NewGuid();
        var mediatorMock = new Mock<IMediator>();
        mediatorMock.Setup(m => m.Send(It.IsAny<UpdateArtworkIsActiveCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ArtworkDto?)null);

        var loggerMock = new Mock<ILogger<ArtworksController>>();
        var controller = CreateController(mediatorMock, loggerMock, CreateUserWithId("creator-1"));

        var result = await controller.UpdateArtworkActive(artworkId, new UpdateArtworkIsActiveRequest { IsActive = false });

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("An error occurred while updating artwork active state.", badRequest.Value);
    }
}
