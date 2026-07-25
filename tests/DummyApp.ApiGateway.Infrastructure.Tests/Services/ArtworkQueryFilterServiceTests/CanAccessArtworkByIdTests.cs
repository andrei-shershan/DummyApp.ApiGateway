using System.Security.Claims;
using DummyApp.ApiGateway.Infrastructure.Constants;
using DummyApp.ApiGateway.Infrastructure.Models.Dtos;
using DummyApp.ApiGateway.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace DummyApp.ApiGateway.Infrastructure.Tests.Services.ArtworkQueryFilterServiceTests;

public class CanAccessArtworkByIdTests : ArtworkQueryFilterServiceTestBase
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void CanAccessArtworkById_AnonymousUser_ReturnsArtworkIsActive(bool isActive)
    {
        var artwork = new ArtworkDto { CreatorId = "creator-1", IsActive = isActive };

        HttpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext?)null);

        var result = Service.CanAccessArtworkById(artwork);

        Assert.Equal(isActive, result);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void CanAccessArtworkById_AdminUser_AlwaysReturnsTrue(bool isActive)
    {
        var artwork = new ArtworkDto { CreatorId = "creator-1", IsActive = isActive };
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, RoleNames.Admin) }, "TestAuth"));

        HttpContextAccessorMock.Setup(x => x.HttpContext).Returns(new DefaultHttpContext { User = user });

        var result = Service.CanAccessArtworkById(artwork);

        Assert.True(result);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void CanAccessArtworkById_AuthenticatedOtherRole_ReturnsArtworkIsActive(bool isActive)
    {
        var artwork = new ArtworkDto { CreatorId = "creator-1", IsActive = isActive };
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "other-role") }, "TestAuth"));

        HttpContextAccessorMock.Setup(x => x.HttpContext).Returns(new DefaultHttpContext { User = user });

        var result = Service.CanAccessArtworkById(artwork);

        Assert.Equal(isActive, result);
    }

    [Fact]
    public void CanAccessArtworkById_CreatorOwner_ReturnsTrueForInactiveArtwork()
    {
        var creatorId = "creator-1";
        var artwork = new ArtworkDto { CreatorId = creatorId, IsActive = false };
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Role, RoleNames.Creator),
            new Claim(ClaimTypes.NameIdentifier, creatorId)
        }, "TestAuth"));

        HttpContextAccessorMock.Setup(x => x.HttpContext).Returns(new DefaultHttpContext { User = user });

        var result = Service.CanAccessArtworkById(artwork);

        Assert.True(result);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void CanAccessArtworkById_CreatorNotOwner_ReturnsArtworkIsActive(bool isActive)
    {
        var artwork = new ArtworkDto { CreatorId = "other-creator", IsActive = isActive };
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Role, RoleNames.Creator),
            new Claim(ClaimTypes.NameIdentifier, "creator-1")
        }, "TestAuth"));

        HttpContextAccessorMock.Setup(x => x.HttpContext).Returns(new DefaultHttpContext { User = user });

        var result = Service.CanAccessArtworkById(artwork);

        Assert.Equal(isActive, result);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void CanAccessArtworkById_CreatorWithoutNameIdentifier_ReturnsArtworkIsActive(bool isActive)
    {
        var artwork = new ArtworkDto { CreatorId = "creator-1", IsActive = isActive };
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, RoleNames.Creator) }, "TestAuth"));

        HttpContextAccessorMock.Setup(x => x.HttpContext).Returns(new DefaultHttpContext { User = user });

        var result = Service.CanAccessArtworkById(artwork);

        Assert.Equal(isActive, result);
    }
}
