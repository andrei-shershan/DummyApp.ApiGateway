using DummyApp.ApiGateway.Infrastructure.CQRS.Commands;
using DummyApp.ApiGateway.Infrastructure.HttpClients;
using DummyApp.ApiGateway.Infrastructure.Models.Dtos;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DummyApp.ApiGateway.Infrastructure.CQRS.CommandHandlers;

public sealed class UpdateArtworkIsActiveCommandHandler : IRequestHandler<UpdateArtworkIsActiveCommand, ArtworkDto?>
{
    private readonly IStorageServiceHttpClient _storageServiceClient;
    private readonly ILogger<UpdateArtworkIsActiveCommandHandler> _logger;

    public UpdateArtworkIsActiveCommandHandler(
        IStorageServiceHttpClient storageServiceClient,
        ILogger<UpdateArtworkIsActiveCommandHandler> logger)
    {
        _storageServiceClient = storageServiceClient;
        _logger = logger;
    }

    public async Task<ArtworkDto?> Handle(UpdateArtworkIsActiveCommand request, CancellationToken cancellationToken)
    {
        if (request.ArtworkId <= 0)
        {
            _logger.LogError("Invalid artwork id {ArtworkId} supplied for active state update.", request.ArtworkId);
            return null;
        }

        return await _storageServiceClient.UpdateArtworkIsActiveAsync(request.ArtworkId, request.IsActive, cancellationToken);
    }
}
