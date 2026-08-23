using DummyApp.ApiGateway.Infrastructure.CQRS.Queries;
using DummyApp.ApiGateway.Infrastructure.HttpClients;
using DummyApp.ApiGateway.Infrastructure.Models.Dtos;
using MediatR;

namespace DummyApp.ApiGateway.Infrastructure.CQRS.QueryHandlers;

public sealed class GetArtworkFiltersQueryHandler : IRequestHandler<GetArtworkFiltersQuery, IEnumerable<TagGroupDto>>
{
    private readonly IStorageServiceHttpClient _storageServiceClient;

    public GetArtworkFiltersQueryHandler(IStorageServiceHttpClient storageServiceClient)
    {
        _storageServiceClient = storageServiceClient;
    }

    public async Task<IEnumerable<TagGroupDto>> Handle(GetArtworkFiltersQuery request, CancellationToken cancellationToken)
    {
        var tags = await _storageServiceClient.GetFilteredTagsAsync(cancellationToken) ?? Array.Empty<TagDto>();

        return tags
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
