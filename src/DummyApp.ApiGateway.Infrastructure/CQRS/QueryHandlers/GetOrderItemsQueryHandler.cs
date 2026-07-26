using DummyApp.ApiGateway.Infrastructure.CQRS.Queries;
using DummyApp.ApiGateway.Infrastructure.HttpClients;
using DummyApp.ApiGateway.Infrastructure.Models.Dtos;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DummyApp.ApiGateway.Infrastructure.CQRS.QueryHandlers;

public sealed class GetOrderItemsQueryHandler : IRequestHandler<GetOrderItemsQuery, IEnumerable<OrderItemDto>?>
{
    private readonly IStorageServiceHttpClient _storageServiceClient;
    private readonly ILogger<GetOrderItemsQueryHandler> _logger;

    public GetOrderItemsQueryHandler(IStorageServiceHttpClient storageServiceClient, ILogger<GetOrderItemsQueryHandler> logger)
    {
        _storageServiceClient = storageServiceClient;
        _logger = logger;
    }

    public async Task<IEnumerable<OrderItemDto>?> Handle(GetOrderItemsQuery request, CancellationToken cancellationToken)
    {
        if (request.OrderId == Guid.Empty)
        {
            _logger.LogWarning("Invalid order id supplied to GetOrderItemsQueryHandler.");
            return null;
        }

        var items = await _storageServiceClient.GetOrderItemsAsync(request.OrderId, cancellationToken);
        if (items is null)
        {
            _logger.LogWarning("Storage service returned null when getting items for order {OrderId}.", request.OrderId);
        }

        return items;
    }
}
