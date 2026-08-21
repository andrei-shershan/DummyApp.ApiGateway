using DummyApp.ApiGateway.Infrastructure.CQRS.Queries;
using DummyApp.ApiGateway.Infrastructure.HttpClients;
using DummyApp.ApiGateway.Infrastructure.Models.Dtos;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DummyApp.ApiGateway.Infrastructure.CQRS.QueryHandlers;

public sealed class GetCompletedOrdersQueryHandler : IRequestHandler<GetCompletedOrdersQuery, IEnumerable<OrderSummaryDto>?>
{
    private readonly IStorageServiceHttpClient _storageServiceClient;
    private readonly ILogger<GetCompletedOrdersQueryHandler> _logger;

    public GetCompletedOrdersQueryHandler(IStorageServiceHttpClient storageServiceClient, ILogger<GetCompletedOrdersQueryHandler> logger)
    {
        _storageServiceClient = storageServiceClient;
        _logger = logger;
    }

    public async Task<IEnumerable<OrderSummaryDto>?> Handle(GetCompletedOrdersQuery request, CancellationToken cancellationToken)
    {
        if (request.Token == Guid.Empty)
        {
            _logger.LogWarning("Invalid completed orders token supplied to GetCompletedOrdersQueryHandler.");
            return null;
        }

        var summaries = await _storageServiceClient.GetCompletedOrdersByTokenAsync(request.Token, cancellationToken);
        if (summaries is null)
        {
            _logger.LogWarning("Storage service returned null when getting completed orders for token {Token}.", request.Token);
        }

        return summaries;
    }
}
