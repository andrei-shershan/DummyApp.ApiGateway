using DummyApp.ApiGateway.Infrastructure.CQRS.Commands;
using DummyApp.ApiGateway.Infrastructure.HttpClients;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DummyApp.ApiGateway.Infrastructure.CQRS.CommandHandlers;

public sealed class PayOrderCommandHandler : IRequestHandler<PayOrderCommand, bool>
{
    private readonly IStorageServiceHttpClient _storageServiceClient;
    private readonly ILogger<PayOrderCommandHandler> _logger;

    public PayOrderCommandHandler(IStorageServiceHttpClient storageServiceClient, ILogger<PayOrderCommandHandler> logger)
    {
        _storageServiceClient = storageServiceClient;
        _logger = logger;
    }

    public async Task<bool> Handle(PayOrderCommand request, CancellationToken cancellationToken)
    {
        if (request.OrderId == Guid.Empty)
        {
            _logger.LogWarning("Invalid order id supplied to PayOrderCommandHandler.");
            return false;
        }

        var result = await _storageServiceClient.PayOrderAsync(request.OrderId, cancellationToken);
        if (!result)
        {
            _logger.LogError("Storage service failed to transition order {OrderId} to processing.", request.OrderId);
        }

        return result;
    }
}
