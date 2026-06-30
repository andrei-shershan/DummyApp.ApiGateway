using DummyApp.ApiGateway.Infrastructure.CQRS.Commands;
using DummyApp.ApiGateway.Infrastructure.HttpClients;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DummyApp.ApiGateway.Infrastructure.CQRS.CommandHandlers;

public sealed class SendInviteCommandHandler : IRequestHandler<SendInviteCommand, bool>
{
    private readonly IEmailServiceHttpClient _emailServiceHttpClient;
    private readonly IIdentityServiceHttpClient _identityServiceHttpClient;
    private readonly ILogger<SendInviteCommandHandler> _logger;

    public SendInviteCommandHandler(
        IEmailServiceHttpClient emailServiceHttpClient,
        IIdentityServiceHttpClient identityServiceHttpClient,
        ILogger<SendInviteCommandHandler> logger)
    {
        _emailServiceHttpClient = emailServiceHttpClient;
        _identityServiceHttpClient = identityServiceHttpClient;
        _logger = logger;
    }

    public async Task<bool> Handle(SendInviteCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            _logger.LogError("Invite email is required.");
            return false;
        }

        var token = Guid.NewGuid().ToString("N");

        var emailResult = await _emailServiceHttpClient.SendInviteAsync(request.Email, token, cancellationToken);
        if (!emailResult)
        {
            _logger.LogError("Failed to send invite email to {Email}.", request.Email);
            return false;
        }

        var result = await _identityServiceHttpClient.SaveInviteTokenAsync(request.Email, token, cancellationToken);
        if (!result)
        {
            _logger.LogError("Failed to save invite token for {Email}.", request.Email);
            return false;
        }

        return true;
    }
}
