using System.Linq;
using DummyApp.ApiGateway.Infrastructure.CQRS.Queries;
using DummyApp.ApiGateway.Infrastructure.HttpClients;
using DummyApp.ApiGateway.Infrastructure.Models.Dtos;
using MediatR;

namespace DummyApp.ApiGateway.Infrastructure.CQRS.QueryHandlers;

public sealed class GetArtworkFiltersQueryHandler : IRequestHandler<GetArtworkFiltersQuery, ArtworkFiltersDto>
{
    private readonly IStorageServiceHttpClient _storageServiceClient;
    private readonly IIdentityServiceHttpClient _identityServiceClient;

    public GetArtworkFiltersQueryHandler(
        IStorageServiceHttpClient storageServiceClient,
        IIdentityServiceHttpClient identityServiceClient)
    {
        _storageServiceClient = storageServiceClient;
        _identityServiceClient = identityServiceClient;
    }

    public async Task<ArtworkFiltersDto> Handle(GetArtworkFiltersQuery request, CancellationToken cancellationToken)
    {
        var tags = await _storageServiceClient.GetFilteredTagsAsync(cancellationToken) ?? Array.Empty<TagDto>();
        var artworks = await _storageServiceClient.GetArtworksAsync(null, true, cancellationToken) ?? Array.Empty<ArtworkDto>();

        var activeCreatorIds = artworks
            .Select(artwork => artwork.CreatorId)
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var users = await _identityServiceClient.GetUsersAsync(cancellationToken) ?? Array.Empty<UserDto>();

        var authors = users
            .Where(user => activeCreatorIds.Contains(user.Id, StringComparer.OrdinalIgnoreCase))
            .Select(user => new ArtworkAuthorDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = string.IsNullOrWhiteSpace(user.FirstName) && string.IsNullOrWhiteSpace(user.LastName)
                    ? string.IsNullOrWhiteSpace(user.Email) ? user.Id : user.Email
                    : $"{user.FirstName} {user.LastName}".Trim()
            })
            .OrderBy(author => author.FullName)
            .ToArray();

        var groupedTags = tags
            .GroupBy(tag => tag.Type)
            .Select(group => new TagGroupDto
            {
                TagType = group.Key,
                Tags = group.OrderBy(tag => tag.Name).ToArray()
            })
            .OrderBy(group => group.TagType)
            .ToArray();

        return new ArtworkFiltersDto
        {
            TagGroups = groupedTags,
            Authors = authors
        };
    }
}
