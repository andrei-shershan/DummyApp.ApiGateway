using System.Text.Json;
using DummyApp.ApiGateway.Infrastructure.CQRS.Commands;
using DummyApp.ApiGateway.Infrastructure.HttpClients;
using DummyApp.ApiGateway.Infrastructure.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DummyApp.ApiGateway.Infrastructure.CQRS.CommandHandlers;

public sealed class SendInviteCommandHandler : IRequestHandler<SendInviteCommand, bool>
{
    private readonly IEmailServiceHttpClient _emailServiceHttpClient;
    private readonly IIdentityServiceHttpClient _identityServiceHttpClient;
    private readonly string? _identityServiceBaseUrl;
    private readonly ILogger<SendInviteCommandHandler> _logger;

    public SendInviteCommandHandler(
        IEmailServiceHttpClient emailServiceHttpClient,
        IIdentityServiceHttpClient identityServiceHttpClient,
        IOptions<InviteOptions> inviteOptions,
        ILogger<SendInviteCommandHandler> logger)
    {
        _emailServiceHttpClient = emailServiceHttpClient;
        _identityServiceHttpClient = identityServiceHttpClient;
        _logger = logger;
        _identityServiceBaseUrl = inviteOptions?.Value?.IdentityServiceBaseUrl?.TrimEnd('/');
    }

    public async Task<bool> Handle(SendInviteCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            _logger.LogError("Invite email is required.");
            return false;
        }

        var token = Guid.NewGuid().ToString("N");
        var inviteUrl = !string.IsNullOrWhiteSpace(_identityServiceBaseUrl)
            ? $"{_identityServiceBaseUrl}/account/register/{Uri.EscapeDataString(token)}"
            : null;

        var emailRequest = new SendEmailRequest
        {
            Subject = "Invitation to DummyApp",
            Recipients = new[] { request.Email },
            Template = "Invite",
            Parameters = JsonDocument.Parse(JsonSerializer.Serialize(new { token, url = inviteUrl })).RootElement
        };

        var emailResult = await _emailServiceHttpClient.SendEmailAsync(emailRequest, cancellationToken);
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
