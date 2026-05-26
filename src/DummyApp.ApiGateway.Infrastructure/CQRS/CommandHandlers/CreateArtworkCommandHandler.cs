using System.Threading;
using System.Threading.Tasks;
using DummyApp.ApiGateway.Infrastructure.CQRS.Commands;
using DummyApp.ApiGateway.Infrastructure.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DummyApp.ApiGateway.Infrastructure.CQRS.CommandHandlers;

public sealed class CreateArtworkCommandHandler : IRequestHandler<CreateArtworkCommand, CreateArtworkCommandResult>
{
    private readonly IStorageServiceClient _storageServiceClient;
    private readonly ILogger<CreateArtworkCommandHandler> _logger;

    public CreateArtworkCommandHandler(
        IStorageServiceClient storageServiceClient,
        ILogger<CreateArtworkCommandHandler> logger)
    {
        _storageServiceClient = storageServiceClient;
        _logger = logger;
    }

    public async Task<CreateArtworkCommandResult> Handle(CreateArtworkCommand request, CancellationToken cancellationToken)
    {
        var result = await _storageServiceClient.CreateArtworkAsync(request, cancellationToken);
        _logger.LogInformation("Created artwork {ArtworkId} via storage service.", result.Id);
        return result;
    }
}
