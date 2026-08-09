using DummyApp.ApiGateway.Infrastructure.CQRS.Commands;
using DummyApp.ApiGateway.Infrastructure.HttpClients;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DummyApp.ApiGateway.Infrastructure.CQRS.CommandHandlers;

public sealed class SaveOrderAddressCommandHandler : IRequestHandler<SaveOrderAddressCommand, bool>
{
    private readonly IStorageServiceHttpClient _storageServiceClient;
    private readonly ILogger<SaveOrderAddressCommandHandler> _logger;

    public SaveOrderAddressCommandHandler(IStorageServiceHttpClient storageServiceClient, ILogger<SaveOrderAddressCommandHandler> logger)
    {
        _storageServiceClient = storageServiceClient;
        _logger = logger;
    }

    public async Task<bool> Handle(SaveOrderAddressCommand request, CancellationToken cancellationToken)
    {
        if (request.OrderId == Guid.Empty)
        {
            _logger.LogWarning("Invalid order id supplied to SaveOrderAddressCommandHandler.");
            return false;
        }

        if (request.Address is null)
        {
            _logger.LogWarning("Invalid address supplied to SaveOrderAddressCommandHandler for order {OrderId}.", request.OrderId);
            return false;
        }

        var result = await _storageServiceClient.SaveOrderAddressAsync(request.OrderId, request.Address, cancellationToken);
        if (!result)
        {
            _logger.LogError("Storage service failed to save address for order {OrderId}.", request.OrderId);
        }

        return result;
    }
}
