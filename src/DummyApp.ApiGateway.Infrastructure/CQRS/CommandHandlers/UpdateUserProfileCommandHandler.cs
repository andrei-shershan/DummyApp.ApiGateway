using DummyApp.ApiGateway.Infrastructure.CQRS.Commands;
using DummyApp.ApiGateway.Infrastructure.HttpClients;
using DummyApp.ApiGateway.Infrastructure.Models.Dtos;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DummyApp.ApiGateway.Infrastructure.CQRS.CommandHandlers;

public sealed class UpdateUserProfileCommandHandler : IRequestHandler<UpdateUserProfileCommand, UserDto?>
{
    private readonly IIdentityServiceHttpClient _identityServiceClient;
    private readonly ILogger<UpdateUserProfileCommandHandler> _logger;

    public UpdateUserProfileCommandHandler(
        IIdentityServiceHttpClient identityServiceClient,
        ILogger<UpdateUserProfileCommandHandler> logger)
    {
        _identityServiceClient = identityServiceClient;
        _logger = logger;
    }

    public async Task<UserDto?> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.UserId))
        {
            _logger.LogError("Invalid user id supplied for profile update.");
            return null;
        }

        return await _identityServiceClient.UpdateUserProfileAsync(request.UserId, request.FirstName, request.LastName, cancellationToken);
    }
}
