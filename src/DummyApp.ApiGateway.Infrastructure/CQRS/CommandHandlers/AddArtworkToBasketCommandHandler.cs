using DummyApp.ApiGateway.Infrastructure.CQRS.Commands;
using DummyApp.ApiGateway.Infrastructure.HttpClients;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DummyApp.ApiGateway.Infrastructure.CQRS.CommandHandlers;

public sealed class AddArtworkToBasketCommandHandler : IRequestHandler<AddArtworkToBasketCommand, bool>
{
    private readonly IStorageServiceHttpClient _storageServiceClient;
    private readonly ILogger<AddArtworkToBasketCommandHandler> _logger;

    public AddArtworkToBasketCommandHandler(IStorageServiceHttpClient storageServiceClient, ILogger<AddArtworkToBasketCommandHandler> logger)
    {
        _storageServiceClient = storageServiceClient;
        _logger = logger;
    }

    public async Task<bool> Handle(AddArtworkToBasketCommand request, CancellationToken cancellationToken)
    {
        if (request.ArtworkId == Guid.Empty)
        {
            _logger.LogWarning("AddArtworkToBasketCommand received an empty ArtworkId.");
            return false;
        }

        var added = await _storageServiceClient.AddOrderItemAsync(request.OrderId, request.ArtworkId, request.Quantity, request.PrintSizeId, request.PriceId, cancellationToken);
        if (!added)
        {
            _logger.LogError("Storage service failed to add artwork {ArtworkId} to order {OrderId}.", request.ArtworkId, request.OrderId);
        }

        return added;
    }
}
