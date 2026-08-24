using DummyApp.ApiGateway.Infrastructure.CQRS.Queries;
using DummyApp.ApiGateway.Infrastructure.HttpClients;
using DummyApp.ApiGateway.Infrastructure.Models.Dtos;
using DummyApp.ApiGateway.Infrastructure.Services;
using MediatR;

namespace DummyApp.ApiGateway.Infrastructure.CQRS.QueryHandlers;

public sealed class GetArtworksQueryHandler : IRequestHandler<GetArtworksQuery, IEnumerable<ArtworkDto>>
{
    private readonly IStorageServiceHttpClient _storageServiceClient;
    private readonly IArtworkQueryFilterService _artworkQueryFilterService;

    public GetArtworksQueryHandler(
        IStorageServiceHttpClient storageServiceClient,
        IArtworkQueryFilterService artworkQueryFilterService)
    {
        _storageServiceClient = storageServiceClient;
        _artworkQueryFilterService = artworkQueryFilterService;
    }

    public async Task<IEnumerable<ArtworkDto>> Handle(GetArtworksQuery request, CancellationToken cancellationToken)
    {
        var activeOnly = _artworkQueryFilterService.ShouldRequestActiveOnly(request.IsActive);
        var artworks = await _storageServiceClient.GetArtworksAsync(request.CreatorId, activeOnly, cancellationToken);

        return artworks?.Where(x => _artworkQueryFilterService.CanAccessArtworkById(x)).Select(art => art) ?? Array.Empty<ArtworkDto>();
    }
}
