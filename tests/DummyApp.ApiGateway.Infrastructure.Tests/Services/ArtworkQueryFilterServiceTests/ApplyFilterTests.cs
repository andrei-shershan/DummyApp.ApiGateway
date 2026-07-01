using System.Security.Claims;
using DummyApp.ApiGateway.Infrastructure.Constants;
using DummyApp.ApiGateway.Infrastructure.CQRS.Queries;
using DummyApp.ApiGateway.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace DummyApp.ApiGateway.Infrastructure.Tests.Services.ArtworkQueryFilterServiceTests;

public class ApplyFilterTests
{
    [Fact]
    public void ApplyFilter_AnonymousUser_ReturnsActiveFilterOnly()
    {
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext?)null);

        var service = new ArtworkQueryFilterService(httpContextAccessorMock.Object);
        var result = service.ApplyFilter(new GetArtworksQuery("creator-1", null));

        Assert.Equal("creator-1", result.CreatorId);
        Assert.True(result.IsActive);
    }

    [Fact]
    public void ApplyFilter_AnonymousUser_ReturnsActiveFilterEvenWhenRequestIsFalse()
    {
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext?)null);

        var service = new ArtworkQueryFilterService(httpContextAccessorMock.Object);
        var result = service.ApplyFilter(new GetArtworksQuery("creator-1", false));

        Assert.Equal("creator-1", result.CreatorId);
        Assert.True(result.IsActive);
    }

    [Fact]
    public void ApplyFilter_AdminUser_ReturnsOriginalFilter()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, RoleNames.Admin) }, "TestAuth"));
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        httpContextAccessorMock.Setup(x => x.HttpContext).Returns(new DefaultHttpContext { User = user });

        var service = new ArtworkQueryFilterService(httpContextAccessorMock.Object);
        var result = service.ApplyFilter(new GetArtworksQuery("creator-1", false));

        Assert.Equal("creator-1", result.CreatorId);
        Assert.False(result.IsActive);
    }

    [Fact]
    public void ApplyFilter_CreatorRequestsOwnArtworks_ReturnsOriginalFilter()
    {
        var creatorId = "creator-1";
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Role, RoleNames.Creator),
            new Claim(ClaimTypes.NameIdentifier, creatorId)
        }, "TestAuth"));
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        httpContextAccessorMock.Setup(x => x.HttpContext).Returns(new DefaultHttpContext { User = user });

        var service = new ArtworkQueryFilterService(httpContextAccessorMock.Object);
        var result = service.ApplyFilter(new GetArtworksQuery(creatorId, false));

        Assert.Equal(creatorId, result.CreatorId);
        Assert.False(result.IsActive);
    }

    [Fact]
    public void ApplyFilter_CreatorRequestsOtherCreatorArtworks_ReturnsActiveFilterOnly()
    {
        var creatorId = "creator-1";
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Role, RoleNames.Creator),
            new Claim(ClaimTypes.NameIdentifier, creatorId)
        }, "TestAuth"));
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        httpContextAccessorMock.Setup(x => x.HttpContext).Returns(new DefaultHttpContext { User = user });

        var service = new ArtworkQueryFilterService(httpContextAccessorMock.Object);
        var result = service.ApplyFilter(new GetArtworksQuery("other-creator", false));

        Assert.Equal("other-creator", result.CreatorId);
        Assert.True(result.IsActive);
    }

    [Fact]
    public void ApplyFilter_CreatorRequestsAllArtworksWithoutCreatorFilter_ReturnsOriginalFilter()
    {
        var creatorId = "creator-1";
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Role, RoleNames.Creator),
            new Claim(ClaimTypes.NameIdentifier, creatorId)
        }, "TestAuth"));
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        httpContextAccessorMock.Setup(x => x.HttpContext).Returns(new DefaultHttpContext { User = user });

        var service = new ArtworkQueryFilterService(httpContextAccessorMock.Object);
        var result = service.ApplyFilter(new GetArtworksQuery(null, null));

        Assert.Null(result.CreatorId);
        Assert.Null(result.IsActive);
    }
}
