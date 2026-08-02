using DummyApp.ApiGateway.Infrastructure.CQRS.Commands;
using DummyApp.ApiGateway.Infrastructure.HttpClients;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DummyApp.ApiGateway.Infrastructure.CQRS.CommandHandlers;

public sealed class UpdateArtworkInBasketCommandHandler : IRequestHandler<UpdateArtworkInBasketCommand, bool>
{
    private readonly IStorageServiceHttpClient _storageServiceClient;
    private readonly ILogger<UpdateArtworkInBasketCommandHandler> _logger;

    public UpdateArtworkInBasketCommandHandler(IStorageServiceHttpClient storageServiceClient, ILogger<UpdateArtworkInBasketCommandHandler> logger)
    {
        _storageServiceClient = storageServiceClient;
        _logger = logger;
    }

    public async Task<bool> Handle(UpdateArtworkInBasketCommand request, CancellationToken cancellationToken)
    {
        if (request.ArtworkId == Guid.Empty)
        {
            _logger.LogWarning("UpdateArtworkInBasketCommand received an empty ArtworkId.");
            return false;
        }

        if (request.Quantity < 0)
        {
            _logger.LogWarning("UpdateArtworkInBasketCommand received a negative quantity for artwork {ArtworkId}.", request.ArtworkId);
            return false;
        }

        var updated = await _storageServiceClient.UpdateOrderItemAsync(request.OrderId, request.ArtworkId, request.Quantity, request.PrintSizeId, request.PriceId, cancellationToken);
        if (!updated)
        {
            _logger.LogError("Storage service failed to update artwork {ArtworkId} in order {OrderId}.", request.ArtworkId, request.OrderId);
        }

        return updated;
    }
}
