using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using DummyApp.ApiGateway.Infrastructure.Constants;
using DummyApp.ApiGateway.Infrastructure.Models.Dtos;
using Microsoft.AspNetCore.Http;

namespace DummyApp.ApiGateway.Infrastructure.Services;

public sealed class TagFilterService : ITagFilterService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TagFilterService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public IEnumerable<TagDto> FilterTags(IEnumerable<TagDto> tags)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user is null || user.Identity?.IsAuthenticated != true)
        {
            return Enumerable.Empty<TagDto>();
        }

        if (user.IsInRole(RoleNames.Admin))
        {
            return tags;
        }

        if (user.IsInRole(RoleNames.Creator))
        {
            return tags.Where(tag => tag.Type == "None" || tag.Type == "Series");
        }

        return Enumerable.Empty<TagDto>();
    }
}
