using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DummyApp.ApiGateway.Infrastructure.CQRS.Queries;
using DummyApp.ApiGateway.Infrastructure.Models.Dtos;
using DummyApp.ApiGateway.WebApi.Controllers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DummyApp.ApiGateway.WebApi.Tests.Controllers.Artworks;

public sealed class GetArtworksTests : ArtworksControllerTestBase
{
    [Fact]
    public async Task GetArtworks_ReturnsOkResult()
    {
        var expectedArtworks = new[]
        {
            new ArtworkDto { Id = Guid.NewGuid(), Name = "Artwork #1", CreatorId = "creator-1", Description = "Desc", CreationDate = new DateTime(2024, 1, 1), UploadDate = DateTime.UtcNow, ImgUrl = "img", ThumbnailUrl = "small", IsActive = true }
        };

        var mediatorMock = new Mock<IMediator>();
        mediatorMock.Setup(m => m.Send(It.IsAny<GetArtworksQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedArtworks);

        var loggerMock = new Mock<ILogger<ArtworksController>>();
        var controller = CreateController(mediatorMock, loggerMock);

        var result = await controller.GetArtworks(null);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(expectedArtworks, Assert.IsAssignableFrom<IEnumerable<ArtworkDto>>(okResult.Value));
        mediatorMock.Verify(m => m.Send(It.IsAny<GetArtworksQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetArtworks_ReturnsOkResult_WhenCreatorIdProvided()
    {
        var creatorId = "creator-1";
        var expectedArtworks = new[]
        {
            new ArtworkDto { Id = Guid.NewGuid(), Name = "Artwork #1", CreatorId = creatorId, Description = "Desc", CreationDate = new DateTime(2024, 1, 1), UploadDate = DateTime.UtcNow, ImgUrl = "img", ThumbnailUrl = "small", IsActive = true }
        };

        var mediatorMock = new Mock<IMediator>();
        mediatorMock.Setup(m => m.Send(It.Is<GetArtworksQuery>(q => q.CreatorId == creatorId && q.IsActive == true), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedArtworks);

        var loggerMock = new Mock<ILogger<ArtworksController>>();
        var controller = CreateController(mediatorMock, loggerMock);

        var result = await controller.GetArtworks(creatorId);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(expectedArtworks, Assert.IsAssignableFrom<IEnumerable<ArtworkDto>>(okResult.Value));
        mediatorMock.Verify(m => m.Send(It.Is<GetArtworksQuery>(q => q.CreatorId == creatorId && q.IsActive == true), It.IsAny<CancellationToken>()), Times.Once);
    }
}
