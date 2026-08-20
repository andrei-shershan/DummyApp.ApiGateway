using DummyApp.ApiGateway.Infrastructure.CQRS.Commands;
using DummyApp.ApiGateway.Infrastructure.HttpClients;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DummyApp.ApiGateway.Infrastructure.CQRS.CommandHandlers;

public sealed class VerifyVerificationCodeCommandHandler : IRequestHandler<VerifyVerificationCodeCommand, bool>
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

    public async Task<bool> Handle(VerifyVerificationCodeCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Code))
        {
            _logger.LogWarning("VerifyVerificationCodeCommand received invalid input.");
            return false;
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var code = request.Code.Trim();

        if (!normalizedEmail.Contains('@') || code.Length != 6)
        {
            _logger.LogWarning("VerifyVerificationCodeCommand received invalid email or code for email {Email}.", request.Email);
            return false;
        }

        var verified = await _storageServiceHttpClient.VerifyVerificationCodeAsync(normalizedEmail, code, cancellationToken);
        if (!verified)
        {
            _logger.LogWarning("Verification failed for email {Email}.", normalizedEmail);
            return false;
        }

        return true;
    }
}
