using System.Linq;
using DummyApp.ApiGateway.Infrastructure.CQRS.Queries;
using DummyApp.ApiGateway.Infrastructure.HttpClients;
using DummyApp.ApiGateway.Infrastructure.Models.Dtos;
using DummyApp.ApiGateway.Infrastructure.Services;
using MediatR;

namespace DummyApp.ApiGateway.Infrastructure.CQRS.QueryHandlers;

public sealed class GetArtworksPageQueryHandler : IRequestHandler<GetArtworksPageQuery, PaginatedResult<ArtworkDto>>
{
    private readonly IStorageServiceHttpClient _storageServiceClient;
    private readonly IStorageUrlService _storageUrlService;
    private readonly IArtworkQueryFilterService _artworkQueryFilterService;

    public GetArtworksPageQueryHandler(
        IStorageServiceHttpClient storageServiceClient,
        IStorageUrlService storageUrlService,
        IArtworkQueryFilterService artworkQueryFilterService)
    {
        _storageServiceClient = storageServiceClient;
        _storageUrlService = storageUrlService;
        _artworkQueryFilterService = artworkQueryFilterService;
    }

    public async Task<PaginatedResult<ArtworkDto>> Handle(GetArtworksPageQuery request, CancellationToken cancellationToken)
    {
        var activeOnly = _artworkQueryFilterService.ShouldRequestActiveOnly(request.IsActive);

        if (activeOnly)
        {
            var pagedResult = await _storageServiceClient.GetArtworksPageAsync(request.CreatorId, activeOnly, request.PageNumber, request.PageSize, request.TagIds, cancellationToken);
            if (pagedResult is null)
            {
                return new PaginatedResult<ArtworkDto>(Array.Empty<ArtworkDto>(), request.PageNumber, request.PageSize, 0);
            }

            var filteredItems = pagedResult.Items
                .Where(_artworkQueryFilterService.CanAccessArtworkById)
                .Select(art => art with
                {
                    ImgUrl = _storageUrlService.GetBlobUrl(art.ImgUrl),
                    ThumbnailUrl = _storageUrlService.GetBlobUrl(art.ThumbnailUrl)
                })
                .ToArray();

            return new PaginatedResult<ArtworkDto>(filteredItems, pagedResult.PageNumber, pagedResult.PageSize, pagedResult.TotalCount);
        }

        var artworks = await _storageServiceClient.GetArtworksAsync(request.CreatorId, activeOnly, cancellationToken);
        var filtered = artworks?
            .Where(_artworkQueryFilterService.CanAccessArtworkById)
            .Select(art => art with
            {
                ImgUrl = _storageUrlService.GetBlobUrl(art.ImgUrl),
                ThumbnailUrl = _storageUrlService.GetBlobUrl(art.ThumbnailUrl)
            })
            .ToArray() ?? Array.Empty<ArtworkDto>();

        var totalCount = filtered.Length;
        var pageItems = filtered
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToArray();

        return new PaginatedResult<ArtworkDto>(pageItems, request.PageNumber, request.PageSize, totalCount);
    }
}
