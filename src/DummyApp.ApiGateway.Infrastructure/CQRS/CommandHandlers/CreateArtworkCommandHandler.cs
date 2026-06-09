using System;
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
    private readonly IBlobServiceClient _blobServiceClient;
    private readonly ILogger<CreateArtworkCommandHandler> _logger;

    public CreateArtworkCommandHandler(
        IStorageServiceClient storageServiceClient,
        IBlobServiceClient blobServiceClient,
        ILogger<CreateArtworkCommandHandler> logger)
    {
        _storageServiceClient = storageServiceClient;
        _blobServiceClient = blobServiceClient;
        _logger = logger;
    }

    public async Task<CreateArtworkCommandResult> Handle(CreateArtworkCommand request, CancellationToken cancellationToken)
    {
        var command = request;

        _logger.LogInformation("Hello from CreateArtworkCommandHandler. Request: {Request}", request);
        _logger.LogInformation("Request contains image: {HasImage}", !string.IsNullOrEmpty(request.UploadedImage));

        if (!string.IsNullOrEmpty(request.UploadedImage))
        {
            var fileName = $"{Guid.NewGuid()}.jpg";
            var blobUrl = await _blobServiceClient.UploadImageAsync(request.UploadedImage, fileName, cancellationToken);
            _logger.LogInformation("Image uploaded to blob storage: {BlobUrl}", blobUrl);
            command = request with { ImgUrl = blobUrl, UploadedImage = null };
        }

        var result = await _storageServiceClient.CreateArtworkAsync(command, cancellationToken);
        _logger.LogInformation("Created artwork {ArtworkId} via storage service.", result.Id);
        return result;
    }
}
