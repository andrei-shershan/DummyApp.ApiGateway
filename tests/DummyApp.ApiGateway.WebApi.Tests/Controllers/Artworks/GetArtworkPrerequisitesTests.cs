using System;
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

public sealed class GetArtworkPrerequisitesTests : ArtworksControllerTestBase
{
    [Fact]
    public async Task GetArtworkPrerequisites_ReturnsOkResult()
    {
        var expected = new[]
        {
            new TagGroupDto { TagType = "None", Tags = new[] { new TagDto { Id = Guid.NewGuid(), Name = "A", Type = "None" } } }
        };

        var mediatorMock = new Mock<IMediator>();
        mediatorMock.Setup(m => m.Send(It.IsAny<GetArtworkPrerequisiteQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var loggerMock = new Mock<ILogger<ArtworksController>>();
        var controller = CreateController(mediatorMock, loggerMock);

        var result = await controller.GetArtworkPrerequisites();

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(expected, Assert.IsAssignableFrom<IEnumerable<TagGroupDto>>(okResult.Value));
        mediatorMock.Verify(m => m.Send(It.IsAny<GetArtworkPrerequisiteQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
