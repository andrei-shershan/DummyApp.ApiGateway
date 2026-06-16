using DummyApp.ApiGateway.Infrastructure.CQRS.Commands;
using DummyApp.ApiGateway.Infrastructure.HttpClients;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DummyApp.ApiGateway.Infrastructure.CQRS.CommandHandlers;

public sealed class CreateArtworkCommandHandler : IRequestHandler<CreateArtworkCommand, CreateArtworkCommandResult>
{
    private readonly IStorageServiceHttpClient _storageServiceClient;
    private readonly IBlobServiceHttpClient _blobServiceClient;
    private readonly ILogger<CreateArtworkCommandHandler> _logger;

    public CreateArtworkCommandHandler(
        IStorageServiceHttpClient storageServiceClient,
        IBlobServiceHttpClient blobServiceClient,
        ILogger<CreateArtworkCommandHandler> logger)
    {
        _storageServiceClient = storageServiceClient;
        _blobServiceClient = blobServiceClient;
        _logger = logger;
    }

    public async Task<CreateArtworkCommandResult> Handle(CreateArtworkCommand request, CancellationToken cancellationToken)
    {
        var command = request;

        if (!string.IsNullOrEmpty(request.UploadedImage))
        {
            var fileName = $"{Guid.NewGuid()}.jpg";
            var blobUrl = await _blobServiceClient.UploadImageAsync(request.UploadedImage, fileName, cancellationToken);
            if (string.IsNullOrEmpty(blobUrl))
            {
                _logger.LogError("Failed to upload image to blob storage.");
                return null!;
            }
            
            command = request with { ImgUrl = blobUrl, UploadedImage = null };
        }

        var result = await _storageServiceClient.CreateArtworkAsync(command, cancellationToken);
        _logger.LogInformation("Created artwork {ArtworkId} via storage service.", result.Id);
        return result;
    }
}
