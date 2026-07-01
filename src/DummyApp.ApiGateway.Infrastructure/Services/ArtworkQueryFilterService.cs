using System;
using System.Security.Claims;
using DummyApp.ApiGateway.Infrastructure.Constants;
using DummyApp.ApiGateway.Infrastructure.CQRS.Queries;
using Microsoft.AspNetCore.Http;

namespace DummyApp.ApiGateway.Infrastructure.Services;

public sealed class ArtworkQueryFilterService : IArtworkQueryFilterService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ArtworkQueryFilterService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public ArtworkQueryFilter ApplyFilter(GetArtworksQuery request)
    {
        var creatorId = string.IsNullOrWhiteSpace(request.CreatorId) ? null : request.CreatorId.Trim();
        var user = _httpContextAccessor.HttpContext?.User;
        if (user is null || user.Identity?.IsAuthenticated != true)
        {
            return new ArtworkQueryFilter(creatorId, true);
        }

        if (user.IsInRole(RoleNames.Admin))
        {
            return new ArtworkQueryFilter(creatorId, request.IsActive);
        }

        if (user.IsInRole(RoleNames.Creator))
        {
            var currentUserId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrWhiteSpace(currentUserId) && string.Equals(creatorId, currentUserId, StringComparison.OrdinalIgnoreCase))
            {
                return new ArtworkQueryFilter(creatorId, request.IsActive);
            }

            return new ArtworkQueryFilter(creatorId, true);
        }

        return new ArtworkQueryFilter(creatorId, true);
    }

    public bool GetArtworkByIdActiveOnly()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user is null || user.Identity?.IsAuthenticated != true)
        {
            return true;
        }

        if (user.IsInRole(RoleNames.Admin) || user.IsInRole(RoleNames.Creator))
        {
            return false;
        }

        return true;
    }
}
