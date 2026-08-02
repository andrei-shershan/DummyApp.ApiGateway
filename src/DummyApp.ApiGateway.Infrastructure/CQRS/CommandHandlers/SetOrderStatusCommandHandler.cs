using System;
using System.Linq;
using System.Threading;
using DummyApp.ApiGateway.Infrastructure.CQRS.Commands;
using DummyApp.ApiGateway.Infrastructure.HttpClients;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DummyApp.ApiGateway.Infrastructure.CQRS.CommandHandlers;

public sealed class SetOrderStatusCommandHandler : IRequestHandler<SetOrderStatusCommand, bool>
{
    private readonly IStorageServiceHttpClient _storageServiceClient;
    private readonly ILogger<SetOrderStatusCommandHandler> _logger;

    public SetOrderStatusCommandHandler(IStorageServiceHttpClient storageServiceClient, ILogger<SetOrderStatusCommandHandler> logger)
    {
        _storageServiceClient = storageServiceClient;
        _logger = logger;
    }

    public async Task<bool> Handle(SetOrderStatusCommand request, CancellationToken cancellationToken)
    {
        if (request.OrderId == Guid.Empty)
        {
            _logger.LogWarning("Invalid order id supplied to SetOrderStatusCommandHandler.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(request.Status))
        {
            _logger.LogWarning("Invalid status supplied to SetOrderStatusCommandHandler for order {OrderId}.", request.OrderId);
            return false;
        }

        if (request.Status.Equals("Processing", StringComparison.OrdinalIgnoreCase))
        {
            var summary = await _storageServiceClient.GetOrderSummaryAsync(request.OrderId, cancellationToken);
            if (summary is null)
            {
                _logger.LogWarning("Order summary not found when validating order {OrderId} before status change.", request.OrderId);
                return false;
            }

            var incompleteItem = summary.Items.FirstOrDefault(item => item.PrintSizeId == null || item.PriceId == null);
            if (incompleteItem is not null)
            {
                _logger.LogWarning("Order {OrderId} contains items without print size or price before transitioning to Processing.", request.OrderId);
                return false;
            }
        }

        var result = await _storageServiceClient.SetOrderStatusAsync(request.OrderId, request.Status, cancellationToken);
        if (!result)
        {
            _logger.LogError("Storage service failed to update status for order {OrderId} to {Status}.", request.OrderId, request.Status);
        }

        return result;
    }
}
