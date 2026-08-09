using DummyApp.ApiGateway.Infrastructure.CQRS.Queries;
using DummyApp.ApiGateway.Infrastructure.HttpClients;
using DummyApp.ApiGateway.Infrastructure.Models.Dtos;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DummyApp.ApiGateway.Infrastructure.CQRS.QueryHandlers;

public sealed class GetOrderAddressQueryHandler : IRequestHandler<GetOrderAddressQuery, OrderAddressDto?>
{
    private readonly IStorageServiceHttpClient _storageServiceClient;
    private readonly ILogger<GetOrderAddressQueryHandler> _logger;

    public GetOrderAddressQueryHandler(IStorageServiceHttpClient storageServiceClient, ILogger<GetOrderAddressQueryHandler> logger)
    {
        _storageServiceClient = storageServiceClient;
        _logger = logger;
    }

    public async Task<OrderAddressDto?> Handle(GetOrderAddressQuery request, CancellationToken cancellationToken)
    {
        if (request.OrderId == Guid.Empty)
        {
            _logger.LogWarning("Invalid order id supplied to GetOrderAddressQueryHandler.");
            return null;
        }

        var address = await _storageServiceClient.GetOrderAddressAsync(request.OrderId, cancellationToken);
        if (address is null)
        {
            _logger.LogWarning("Storage service returned null when getting order address for order {OrderId}.", request.OrderId);
        }

        return address;
    }
}
