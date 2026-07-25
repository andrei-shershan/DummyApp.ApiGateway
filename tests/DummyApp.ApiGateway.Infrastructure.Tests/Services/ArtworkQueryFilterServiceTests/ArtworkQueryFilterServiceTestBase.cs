using DummyApp.ApiGateway.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Moq;

namespace DummyApp.ApiGateway.Infrastructure.Tests.Services.ArtworkQueryFilterServiceTests;

public abstract class ArtworkQueryFilterServiceTestBase
{
    protected readonly Mock<IHttpContextAccessor> HttpContextAccessorMock = new();
    protected readonly ArtworkQueryFilterService Service;

    protected ArtworkQueryFilterServiceTestBase()
    {
        Service = new ArtworkQueryFilterService(HttpContextAccessorMock.Object);
    }
}
