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

public sealed class GetArtworksPageTests : ArtworksControllerTestBase
{
    [Fact]
    public async Task GetArtworksPage_ReturnsOkResult()
    {
        var expected = new PaginatedResult<ArtworkDto>(
            new[] { new ArtworkDto { Id = Guid.NewGuid(), Name = "Artwork #1", CreatorId = "creator-1", Description = "Desc", CreationDate = new DateTime(2024, 1, 1), UploadDate = DateTime.UtcNow, ImgUrl = "img", ThumbnailUrl = "small", IsActive = true } },
            2,
            20,
            40);

        var mediatorMock = new Mock<IMediator>();
        mediatorMock.Setup(m => m.Send(It.Is<GetArtworksPageQuery>(q => q.CreatorId == null && q.IsActive == true && q.PageNumber == 2 && q.PageSize == 20), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var loggerMock = new Mock<ILogger<ArtworksController>>();
        var controller = CreateController(mediatorMock, loggerMock);

        var result = await controller.GetArtworksPage(null, true, 2, 20);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(expected, Assert.IsAssignableFrom<PaginatedResult<ArtworkDto>>(okResult.Value));
        mediatorMock.Verify(m => m.Send(It.IsAny<GetArtworksPageQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
