using DummyApp.ApiGateway.Infrastructure.CQRS.Queries;
using DummyApp.ApiGateway.Infrastructure.Constants;
using DummyApp.ApiGateway.Infrastructure.HttpClients;
using DummyApp.ApiGateway.Infrastructure.Models.Dtos;
using DummyApp.ApiGateway.Infrastructure.Services;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace DummyApp.ApiGateway.Infrastructure.CQRS.QueryHandlers;

public sealed class GetArtworkPrerequisiteQueryHandler : IRequestHandler<GetArtworkPrerequisiteQuery, IEnumerable<TagGroupDto>>
{
    private readonly IStorageServiceHttpClient _storageServiceClient;
    private readonly ITagFilterService _tagFilterService;

    public GetArtworkPrerequisiteQueryHandler(
        IStorageServiceHttpClient storageServiceClient,
        ITagFilterService tagFilterService)
    {
        _storageServiceClient = storageServiceClient;
        _tagFilterService = tagFilterService;
    }

    public async Task<IEnumerable<TagGroupDto>> Handle(GetArtworkPrerequisiteQuery request, CancellationToken cancellationToken)
    {
        var allTags = await _storageServiceClient.GetTagsAsync(cancellationToken) ?? Array.Empty<TagDto>();
        var filteredTags = _tagFilterService.FilterTags(allTags);

        return filteredTags
            .GroupBy(tag => tag.Type)
            .Select(group => new TagGroupDto
            {
                TagType = group.Key,
                Tags = group.OrderBy(tag => tag.Name).ToArray()
            })
            .OrderBy(group => group.TagType)
            .ToArray();
    }
}
