using System.Security.Claims;
using DummyApp.ApiGateway.Infrastructure.Constants;
using DummyApp.ApiGateway.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace DummyApp.ApiGateway.Infrastructure.Tests.Services.ArtworkQueryFilterServiceTests;

public class ShouldRequestActiveOnlyTests : ArtworkQueryFilterServiceTestBase
{
    [Theory]
    [InlineData(true, null, true)]
    [InlineData(true, RoleNames.Admin, true)]
    [InlineData(true, RoleNames.Creator, true)]
    [InlineData(true, "other-role", true)]
    public void ShouldRequestActiveOnly_RequestedTrue_AlwaysReturnsTrue(bool requestedActiveOnly, string? role, bool expected)
    {
        HttpContextAccessorMock.Setup(x => x.HttpContext).Returns(role is null ? null : new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, role) }, "TestAuth")) });

        var result = Service.ShouldRequestActiveOnly(requestedActiveOnly);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void ShouldRequestActiveOnly_RequestedFalse_AnonymousUserReturnsTrue()
    {
        HttpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext?)null);

        var result = Service.ShouldRequestActiveOnly(false);

        Assert.True(result);
    }

    [Theory]
    [InlineData(RoleNames.Admin, false)]
    [InlineData(RoleNames.Creator, false)]
    [InlineData("other-role", true)]
    public void ShouldRequestActiveOnly_RequestedFalse_ReturnsExpectedForAuthenticatedUser(string role, bool expected)
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, role) }, "TestAuth"));
        HttpContextAccessorMock.Setup(x => x.HttpContext).Returns(new DefaultHttpContext { User = user });

        var result = Service.ShouldRequestActiveOnly(false);

        Assert.Equal(expected, result);
    }
}
