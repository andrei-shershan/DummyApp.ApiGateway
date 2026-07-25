using System;
using System.Security.Claims;
using DummyApp.ApiGateway.Infrastructure.Constants;
using DummyApp.ApiGateway.Infrastructure.CQRS.Queries;
using DummyApp.ApiGateway.Infrastructure.Models.Dtos;
using Microsoft.AspNetCore.Http;

namespace DummyApp.ApiGateway.Infrastructure.Services;

public sealed class ArtworkQueryFilterService : IArtworkQueryFilterService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ArtworkQueryFilterService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }


    public bool ShouldRequestActiveOnly(bool requestedActiveOnly)
    {
        if (requestedActiveOnly)
        {
            Console.WriteLine("Requested active only is true, returning true.");
            return true;
        }

        var user = _httpContextAccessor.HttpContext?.User;
        if (user is null || user.Identity?.IsAuthenticated != true)
        {
            Console.WriteLine("User is anonymous, returning true for active only.");
            return true;
        }

        if (user.IsInRole(RoleNames.Admin) || user.IsInRole(RoleNames.Creator))
        {
            Console.WriteLine("User is Admin or Creator, returning true for active only.");
            return false;
        }

        Console.WriteLine("User is authenticated but not Admin or Creator, returning true for active only.");

        return true;
    }

    public bool CanAccessArtworkById(ArtworkDto artwork)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user is null || user.Identity?.IsAuthenticated != true)
        {
            return artwork.IsActive;
        }

        if (user.IsInRole(RoleNames.Admin))
        {
            return true;
        }

        if (user.IsInRole(RoleNames.Creator))
        {
            var currentUserId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrWhiteSpace(currentUserId) && string.Equals(currentUserId, artwork.CreatorId, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return artwork.IsActive;
        }

        return artwork.IsActive;
    }

    public bool AdminOrCreatorsArtwork(ArtworkDto artwork)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user is null || user.Identity?.IsAuthenticated != true)
        {
            return false;
        }

        if (user.IsInRole(RoleNames.Admin))
        {
            return true;
        }

        if (user.IsInRole(RoleNames.Creator))
        {
            var currentUserId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return !string.IsNullOrWhiteSpace(currentUserId)
                && string.Equals(currentUserId, artwork.CreatorId, StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }
}
