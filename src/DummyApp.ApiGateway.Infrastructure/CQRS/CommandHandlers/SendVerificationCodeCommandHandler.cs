using System.Security.Cryptography;
using System.Text.Json;
using DummyApp.ApiGateway.Infrastructure.CQRS.Commands;
using DummyApp.ApiGateway.Infrastructure.HttpClients;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DummyApp.ApiGateway.Infrastructure.CQRS.CommandHandlers;

public sealed class SendVerificationCodeCommandHandler : IRequestHandler<SendVerificationCodeCommand, bool>
{
    private readonly IStorageServiceHttpClient _storageServiceHttpClient;
    private readonly IEmailServiceHttpClient _emailServiceHttpClient;
    private readonly ILogger<SendVerificationCodeCommandHandler> _logger;

    public SendVerificationCodeCommandHandler(
        IStorageServiceHttpClient storageServiceHttpClient,
        IEmailServiceHttpClient emailServiceHttpClient,
        ILogger<SendVerificationCodeCommandHandler> logger)
    {
        _storageServiceHttpClient = storageServiceHttpClient;
        _emailServiceHttpClient = emailServiceHttpClient;
        _logger = logger;
    }

    public async Task<bool> Handle(SendVerificationCodeCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            _logger.LogWarning("SendVerificationCodeCommand received an empty email.");
            return false;
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        if (!normalizedEmail.Contains('@'))
        {
            _logger.LogWarning("SendVerificationCodeCommand received an invalid email: {Email}.", request.Email);
            return false;
        }

        var code = RandomNumberGenerator.GetInt32(0, 1000000).ToString("D6");
        var expiresAt = DateTime.UtcNow.AddMinutes(10);

        var stored = await _storageServiceHttpClient.CreateVerificationCodeAsync(normalizedEmail, code, expiresAt, cancellationToken);
        if (!stored)
        {
            _logger.LogError("Failed to persist verification code for email {Email}.", normalizedEmail);
            return false;
        }

        var emailPayload = new { code, email = normalizedEmail, expiresAt };
        var emailRequest = new SendEmailRequest
        {
            Subject = "DummyApp verification code",
            Recipients = new[] { normalizedEmail },
            Template = "VerificationCode",
            Parameters = JsonDocument.Parse(JsonSerializer.Serialize(emailPayload)).RootElement
        };

        var emailSent = await _emailServiceHttpClient.SendEmailAsync(emailRequest, cancellationToken);
        if (!emailSent)
        {
            _logger.LogError("Failed to send verification email to {Email}.", normalizedEmail);
            return false;
        }

        return true;
    }
}
