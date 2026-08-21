using DummyApp.ApiGateway.Infrastructure.CQRS.Commands;
using DummyApp.ApiGateway.Infrastructure.HttpClients;
using DummyApp.ApiGateway.Infrastructure.Models.Dtos;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DummyApp.ApiGateway.Infrastructure.CQRS.CommandHandlers;

public sealed class VerifyVerificationCodeCommandHandler : IRequestHandler<VerifyVerificationCodeCommand, VerifyVerificationCodeResult>
{
    private readonly IStorageServiceHttpClient _storageServiceHttpClient;
    private readonly ILogger<VerifyVerificationCodeCommandHandler> _logger;

    public VerifyVerificationCodeCommandHandler(
        IStorageServiceHttpClient storageServiceHttpClient,
        ILogger<VerifyVerificationCodeCommandHandler> logger)
    {
        _storageServiceHttpClient = storageServiceHttpClient;
        _logger = logger;
    }

    public async Task<VerifyVerificationCodeResult> Handle(VerifyVerificationCodeCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Code))
        {
            _logger.LogWarning("VerifyVerificationCodeCommand received invalid input.");
            return new VerifyVerificationCodeResult { Success = false, ErrorMessage = "Invalid input." };
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var code = request.Code.Trim();

        if (!normalizedEmail.Contains('@') || code.Length != 6)
        {
            _logger.LogWarning("VerifyVerificationCodeCommand received invalid email or code for email {Email}.", request.Email);
            return new VerifyVerificationCodeResult { Success = false, ErrorMessage = "Invalid email or code." };
        }

        var verified = await _storageServiceHttpClient.VerifyVerificationCodeAsync(normalizedEmail, code, cancellationToken);
        if (!verified)
        {
            _logger.LogWarning("Verification failed for email {Email}.", normalizedEmail);
            return new VerifyVerificationCodeResult { Success = false, ErrorMessage = "Invalid or expired verification code." };
        }

        var token = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddDays(1);
        var persisted = await _storageServiceHttpClient.CreateCompletedOrdersTokenAsync(normalizedEmail, token, expiresAt, cancellationToken);
        if (!persisted)
        {
            _logger.LogError("Failed to persist completed orders token for email {Email}.", normalizedEmail);
            return new VerifyVerificationCodeResult { Success = false, IsServerError = true, ErrorMessage = "Unable to persist completed orders token." };
        }

        return new VerifyVerificationCodeResult
        {
            Success = true,
            Token = token,
            ExpiresAt = expiresAt
        };
    }
}
