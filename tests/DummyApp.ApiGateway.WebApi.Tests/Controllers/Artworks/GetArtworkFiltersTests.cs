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

public sealed class GetArtworkFiltersTests : ArtworksControllerTestBase
{
    [Fact]
    public async Task GetArtworkFilters_ReturnsOkResult()
    {
        var expected = new[]
        {
            new TagGroupDto { TagType = "Category", Tags = new[] { new TagDto { Id = Guid.NewGuid(), Name = "Landscape", Type = "Category" } } }
        };

        var mediatorMock = new Mock<IMediator>();
        mediatorMock.Setup(m => m.Send(It.IsAny<GetArtworkFiltersQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var loggerMock = new Mock<ILogger<ArtworksController>>();
        var controller = CreateController(mediatorMock, loggerMock);

        var result = await controller.GetArtworkFilters();

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(expected, Assert.IsAssignableFrom<IEnumerable<TagGroupDto>>(okResult.Value));
        mediatorMock.Verify(m => m.Send(It.IsAny<GetArtworkFiltersQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
