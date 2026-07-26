using DummyApp.ApiGateway.Infrastructure.CQRS.Queries;
using DummyApp.ApiGateway.Infrastructure.HttpClients;
using DummyApp.ApiGateway.Infrastructure.Models.Dtos;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DummyApp.ApiGateway.Infrastructure.CQRS.QueryHandlers;

public sealed class GetOrderSummaryQueryHandler : IRequestHandler<GetOrderSummaryQuery, OrderSummaryDto?>
{
    private readonly IStorageServiceHttpClient _storageServiceClient;
    private readonly ILogger<GetOrderSummaryQueryHandler> _logger;

    public GetOrderSummaryQueryHandler(IStorageServiceHttpClient storageServiceClient, ILogger<GetOrderSummaryQueryHandler> logger)
    {
        _storageServiceClient = storageServiceClient;
        _logger = logger;
    }

    public async Task<OrderSummaryDto?> Handle(GetOrderSummaryQuery request, CancellationToken cancellationToken)
    {
        if (request.OrderId == Guid.Empty)
        {
            _logger.LogWarning("Invalid order id supplied to GetOrderSummaryQueryHandler.");
            return null;
        }

        var summary = await _storageServiceClient.GetOrderSummaryAsync(request.OrderId, cancellationToken);
        if (summary is null)
        {
            _logger.LogWarning("Storage service returned null when getting order summary for order {OrderId}.", request.OrderId);
        }

        return summary;
    }
}
