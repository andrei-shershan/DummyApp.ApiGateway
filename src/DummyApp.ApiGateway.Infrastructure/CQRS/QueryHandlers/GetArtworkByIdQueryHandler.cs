using DummyApp.ApiGateway.Infrastructure.CQRS.Queries;
using DummyApp.ApiGateway.Infrastructure.HttpClients;
using DummyApp.ApiGateway.Infrastructure.Models.Dtos;
using DummyApp.ApiGateway.Infrastructure.Services;
using MediatR;

namespace DummyApp.ApiGateway.Infrastructure.CQRS.QueryHandlers;

public sealed class GetArtworkByIdQueryHandler : IRequestHandler<GetArtworkByIdQuery, ArtworkDto?>
{
    private readonly IStorageServiceHttpClient _storageServiceClient;
    private readonly IArtworkQueryFilterService _artworkQueryFilterService;

    public GetArtworkByIdQueryHandler(
        IStorageServiceHttpClient storageServiceClient,
        IArtworkQueryFilterService artworkQueryFilterService)
    {
        _storageServiceClient = storageServiceClient;
        _artworkQueryFilterService = artworkQueryFilterService;
    }

    public async Task<ArtworkDto?> Handle(GetArtworkByIdQuery request, CancellationToken cancellationToken)
    {
        var activeOnly = _artworkQueryFilterService.ShouldRequestActiveOnly(request.ActiveOnly);
        var artwork = await _storageServiceClient.GetArtworkByIdAsync(request.Id, activeOnly, cancellationToken);
        if (artwork is null)
        {
            return null;
        }

        if (!artwork.IsActive && !_artworkQueryFilterService.CanAccessArtworkById(artwork))
        {
            return null;
        }

        return artwork;
    }
}
