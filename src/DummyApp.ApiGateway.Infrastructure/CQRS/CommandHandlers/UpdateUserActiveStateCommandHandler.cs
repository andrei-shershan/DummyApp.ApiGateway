using DummyApp.ApiGateway.Infrastructure.CQRS.Commands;
using DummyApp.ApiGateway.Infrastructure.HttpClients;
using DummyApp.ApiGateway.Infrastructure.Models.Dtos;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DummyApp.ApiGateway.Infrastructure.CQRS.CommandHandlers;

public sealed class UpdateUserActiveStateCommandHandler : IRequestHandler<UpdateUserActiveStateCommand, UserDto?>
{
    private readonly IIdentityServiceHttpClient _identityServiceClient;
    private readonly ILogger<UpdateUserActiveStateCommandHandler> _logger;

    public UpdateUserActiveStateCommandHandler(
        IIdentityServiceHttpClient identityServiceClient,
        ILogger<UpdateUserActiveStateCommandHandler> logger)
    {
        _identityServiceClient = identityServiceClient;
        _logger = logger;
    }

    public async Task<UserDto?> Handle(UpdateUserActiveStateCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.UserId))
        {
            _logger.LogError("Invalid user id supplied for active state update.");
            return null;
        }

        return await _identityServiceClient.UpdateUserActiveStateAsync(request.UserId, request.IsActive, cancellationToken);
    }
}
