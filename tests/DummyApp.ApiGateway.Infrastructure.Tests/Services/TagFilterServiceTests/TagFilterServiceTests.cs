using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using DummyApp.ApiGateway.Infrastructure.Constants;
using DummyApp.ApiGateway.Infrastructure.Models.Dtos;
using DummyApp.ApiGateway.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace DummyApp.ApiGateway.Infrastructure.Tests.Services.TagFilterServiceTests;

public sealed class TagFilterServiceTests
{
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock = new();
    private readonly TagFilterService _service;

    public TagFilterServiceTests()
    {
        _service = new TagFilterService(_httpContextAccessorMock.Object);
    }

    [Fact]
    public void FilterTags_ReturnsEmpty_WhenUserIsAnonymous()
    {
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext?)null);

        var tags = new[]
        {
            new TagDto { Id = Guid.NewGuid(), Name = "A", Type = "None" }
        };

        var result = _service.FilterTags(tags);

        Assert.Empty(result);
    }

    [Fact]
    public void FilterTags_ReturnsAllTags_WhenUserIsAdmin()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, RoleNames.Admin) }, "TestAuth"));
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(new DefaultHttpContext { User = user });

        var tags = new[]
        {
            new TagDto { Id = Guid.NewGuid(), Name = "A", Type = "None" },
            new TagDto { Id = Guid.NewGuid(), Name = "B", Type = "Series" },
            new TagDto { Id = Guid.NewGuid(), Name = "C", Type = "Other" }
        };

        var result = _service.FilterTags(tags);

        Assert.Equal(tags, result);
    }

    [Fact]
    public void FilterTags_ReturnsNoneAndSeries_WhenUserIsCreator()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, RoleNames.Creator) }, "TestAuth"));
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(new DefaultHttpContext { User = user });

        var tags = new[]
        {
            new TagDto { Id = Guid.NewGuid(), Name = "A", Type = "None" },
            new TagDto { Id = Guid.NewGuid(), Name = "B", Type = "Series" },
            new TagDto { Id = Guid.NewGuid(), Name = "C", Type = "Other" }
        };

        var result = _service.FilterTags(tags);

        Assert.Equal(2, result.Count());
        Assert.Contains(result, tag => tag.Type == "None");
        Assert.Contains(result, tag => tag.Type == "Series");
        Assert.DoesNotContain(result, tag => tag.Type == "Other");
    }
}
