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

public sealed class GetArtworksByCreatorIdTests : ArtworksControllerTestBase
{
    [Fact]
    public async Task GetArtworksByCreatorId_ReturnsOkResult()
    {
        var creatorId = "creator-1";
        var expectedArtworks = new[]
        {
            new ArtworkDto { Id = 1, Name = "Artwork #1", CreatorId = creatorId, Description = "Desc", CreationDate = new DateTime(2024, 1, 1), UploadDate = DateTime.UtcNow, ImgUrl = "img", ThumbnailUrl = "small", IsActive = true }
        };

        var mediatorMock = new Mock<IMediator>();
        mediatorMock.Setup(m => m.Send(It.Is<GetArtworksByCreatorIdQuery>(q => q.CreatorId == creatorId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedArtworks);

        var loggerMock = new Mock<ILogger<ArtworksController>>();
        var controller = CreateController(mediatorMock, loggerMock);

        var result = await controller.GetArtworksByCreatorId(creatorId);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(expectedArtworks, Assert.IsAssignableFrom<IEnumerable<ArtworkDto>>(okResult.Value));
        mediatorMock.Verify(m => m.Send(It.Is<GetArtworksByCreatorIdQuery>(q => q.CreatorId == creatorId), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetArtworksByCreatorId_ReturnsBadRequest_WhenCreatorIdIsMissing()
    {
        var mediatorMock = new Mock<IMediator>();
        var loggerMock = new Mock<ILogger<ArtworksController>>();
        var controller = CreateController(mediatorMock, loggerMock);

        var result = await controller.GetArtworksByCreatorId(string.Empty);

        Assert.IsType<BadRequestObjectResult>(result);
        mediatorMock.Verify(m => m.Send(It.IsAny<GetArtworksByCreatorIdQuery>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
