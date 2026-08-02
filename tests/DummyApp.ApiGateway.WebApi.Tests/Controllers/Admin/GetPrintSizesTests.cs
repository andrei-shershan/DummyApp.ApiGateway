using DummyApp.ApiGateway.Infrastructure.CQRS.Queries;
using DummyApp.ApiGateway.Infrastructure.Models.Dtos;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace DummyApp.ApiGateway.WebApi.Tests.Controllers.Admin;

public sealed class GetPrintSizesTests : AdminControllerTestBase
{
    [Fact]
    public async Task GetPrintSizes_ReturnsOkResult()
    {
        var expectedSizes = new[]
        {
            new PrintSizeDto { Id = 1, Name = "A4", Prices = Array.Empty<PriceDto>() }
        };

        var mediatorMock = new Mock<IMediator>();
        mediatorMock.Setup(m => m.Send(It.IsAny<GetPrintSizesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedSizes);

        var controller = CreateController(mediatorMock);

        var result = await controller.GetPrintSizes();

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(expectedSizes, Assert.IsAssignableFrom<IEnumerable<PrintSizeDto>>(okResult.Value));
        mediatorMock.Verify(m => m.Send(It.IsAny<GetPrintSizesQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
