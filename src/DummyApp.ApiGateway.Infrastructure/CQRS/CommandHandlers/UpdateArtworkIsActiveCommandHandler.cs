using DummyApp.ApiGateway.Infrastructure.CQRS.Commands;
using DummyApp.ApiGateway.Infrastructure.HttpClients;
using DummyApp.ApiGateway.Infrastructure.Models.Dtos;
using DummyApp.ApiGateway.Infrastructure.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DummyApp.ApiGateway.Infrastructure.CQRS.CommandHandlers;

public sealed class UpdateArtworkIsActiveCommandHandler : IRequestHandler<UpdateArtworkIsActiveCommand, ArtworkDto?>
{
    private readonly IStorageServiceHttpClient _storageServiceClient;
    private readonly IArtworkQueryFilterService _artworkQueryFilterService;
    private readonly ILogger<UpdateArtworkIsActiveCommandHandler> _logger;

    public UpdateArtworkIsActiveCommandHandler(
        IStorageServiceHttpClient storageServiceClient,
        IArtworkQueryFilterService artworkQueryFilterService,
        ILogger<UpdateArtworkIsActiveCommandHandler> logger)
    {
        _storageServiceClient = storageServiceClient;
        _artworkQueryFilterService = artworkQueryFilterService;
        _logger = logger;
    }

    public async Task<ArtworkDto?> Handle(UpdateArtworkIsActiveCommand request, CancellationToken cancellationToken)
    {
        if (request.ArtworkId == Guid.Empty)
        {
            _logger.LogError("Invalid artwork id {ArtworkId} supplied for active state update.", request.ArtworkId);
            return null;
        }

        var artwork = await _storageServiceClient.GetArtworkByIdAsync(request.ArtworkId, false, cancellationToken);
        if (artwork is null)
        {
            _logger.LogWarning("Artwork with id {ArtworkId} not found for active state update.", request.ArtworkId);
            return null;
        }

        if (!_artworkQueryFilterService.AdminOrCreatorsArtwork(artwork))
        {
            _logger.LogWarning("User is not authorized to update active state for artwork {ArtworkId}.", request.ArtworkId);
            return null;
        }

        return await _storageServiceClient.UpdateArtworkIsActiveAsync(request.ArtworkId, request.IsActive, cancellationToken);
    }
}
