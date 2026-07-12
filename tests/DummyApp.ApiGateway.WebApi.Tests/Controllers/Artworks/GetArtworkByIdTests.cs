using DummyApp.ApiGateway.Infrastructure.CQRS.Queries;
using DummyApp.ApiGateway.Infrastructure.Models.Dtos;
using DummyApp.ApiGateway.WebApi.Controllers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DummyApp.ApiGateway.WebApi.Tests.Controllers.Artworks;

public sealed class GetArtworkByIdTests : ArtworksControllerTestBase
{
    [Fact]
    public async Task GetArtworkById_ReturnsOkResult_WhenArtworkExists()
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
        mediatorMock.Setup(m => m.Send(It.Is<GetArtworkByIdQuery>(q => q.Id == expectedArtwork.Id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedArtwork);

        var loggerMock = new Mock<ILogger<ArtworksController>>();
        var controller = CreateController(mediatorMock, loggerMock);

        var result = await controller.GetArtworkById(expectedArtwork.Id);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(expectedArtwork, Assert.IsType<ArtworkDto>(okResult.Value));
        mediatorMock.Verify(m => m.Send(It.Is<GetArtworkByIdQuery>(q => q.Id == expectedArtwork.Id), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetArtworkById_ReturnsNotFound_WhenArtworkDoesNotExist()
    {
        var mediatorMock = new Mock<IMediator>();
        mediatorMock.Setup(m => m.Send(It.IsAny<GetArtworkByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ArtworkDto?)null);

        var loggerMock = new Mock<ILogger<ArtworksController>>();
        var controller = CreateController(mediatorMock, loggerMock);

        var missingArtworkId = Guid.NewGuid();
        var result = await controller.GetArtworkById(missingArtworkId);

        Assert.IsType<NotFoundResult>(result);
        mediatorMock.Verify(m => m.Send(It.Is<GetArtworkByIdQuery>(q => q.Id == missingArtworkId), It.IsAny<CancellationToken>()), Times.Once);
    }
}
