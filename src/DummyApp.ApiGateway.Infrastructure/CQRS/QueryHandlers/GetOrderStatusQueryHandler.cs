using DummyApp.ApiGateway.Infrastructure.CQRS.Queries;
using DummyApp.ApiGateway.Infrastructure.HttpClients;
using DummyApp.ApiGateway.Infrastructure.Models.Dtos;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DummyApp.ApiGateway.Infrastructure.CQRS.QueryHandlers;

public sealed class GetOrderStatusQueryHandler : IRequestHandler<GetOrderStatusQuery, OrderStatusDto?>
{
    private readonly IStorageServiceHttpClient _storageServiceClient;
    private readonly ILogger<GetOrderStatusQueryHandler> _logger;

    public GetOrderStatusQueryHandler(IStorageServiceHttpClient storageServiceClient, ILogger<GetOrderStatusQueryHandler> logger)
    {
        _storageServiceClient = storageServiceClient;
        _logger = logger;
    }

    public async Task<OrderStatusDto?> Handle(GetOrderStatusQuery request, CancellationToken cancellationToken)
    {
        if (request.OrderId == Guid.Empty)
        {
            _logger.LogWarning("Invalid order id supplied to GetOrderStatusQueryHandler.");
            return null;
        }

        var status = await _storageServiceClient.GetOrderStatusAsync(request.OrderId, cancellationToken);
        if (status is null)
        {
            _logger.LogWarning("Storage service returned null when getting status for order {OrderId}.", request.OrderId);
        }

        return status;
    }
}
