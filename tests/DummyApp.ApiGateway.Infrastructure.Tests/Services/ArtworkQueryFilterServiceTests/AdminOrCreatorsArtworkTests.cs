using System;
using System.Security.Claims;
using DummyApp.ApiGateway.Infrastructure.Constants;
using DummyApp.ApiGateway.Infrastructure.Models.Dtos;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace DummyApp.ApiGateway.Infrastructure.Tests.Services.ArtworkQueryFilterServiceTests;

public sealed class AdminOrCreatorsArtworkTests : ArtworkQueryFilterServiceTestBase
{
    [Fact]
    public void CanUpdateArtwork_ReturnsFalse_WhenUserIsAnonymous()
    {
        HttpContextAccessorMock
            .Setup(x => x.HttpContext)
            .Returns((HttpContext?)null);

        var artwork = new ArtworkDto { CreatorId = "creator", IsActive = true };

        var result = Service.AdminOrCreatorsArtwork(artwork);

        Assert.False(result);
    }

    [Fact]
    public void CanUpdateArtwork_ReturnsTrue_WhenUserIsAdmin()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, RoleNames.Admin) }, "TestAuth"));
        var context = new DefaultHttpContext { User = user };

        HttpContextAccessorMock
            .Setup(x => x.HttpContext)
            .Returns(context);

        var artwork = new ArtworkDto { CreatorId = "creator", IsActive = false };

        var result = Service.AdminOrCreatorsArtwork(artwork);

        Assert.True(result);
    }

    [Fact]
    public void CanUpdateArtwork_ReturnsTrue_WhenCreatorOwnsArtwork()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, RoleNames.Creator), new Claim(ClaimTypes.NameIdentifier, "creator") }, "TestAuth"));
        var context = new DefaultHttpContext { User = user };

        HttpContextAccessorMock
            .Setup(x => x.HttpContext)
            .Returns(context);

        var artwork = new ArtworkDto { CreatorId = "creator", IsActive = false };

        var result = Service.AdminOrCreatorsArtwork(artwork);

        Assert.True(result);
    }

    [Fact]
    public void CanUpdateArtwork_ReturnsFalse_WhenCreatorDoesNotOwnArtwork()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, RoleNames.Creator), new Claim(ClaimTypes.NameIdentifier, "different") }, "TestAuth"));
        var context = new DefaultHttpContext { User = user };

        HttpContextAccessorMock
            .Setup(x => x.HttpContext)
            .Returns(context);

        var artwork = new ArtworkDto { CreatorId = "creator", IsActive = true };

        var result = Service.AdminOrCreatorsArtwork(artwork);

        Assert.False(result);
    }

    [Fact]
    public void CanUpdateArtwork_ReturnsFalse_WhenUserIsAuthenticatedButNotAdminOrCreator()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "Customer") }, "TestAuth"));
        var context = new DefaultHttpContext { User = user };

        HttpContextAccessorMock
            .Setup(x => x.HttpContext)
            .Returns(context);

        var artwork = new ArtworkDto { CreatorId = "creator", IsActive = true };

        var result = Service.AdminOrCreatorsArtwork(artwork);

        Assert.False(result);
    }
}
