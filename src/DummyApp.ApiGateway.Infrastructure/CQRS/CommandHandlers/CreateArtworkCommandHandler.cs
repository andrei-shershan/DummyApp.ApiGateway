using DummyApp.ApiGateway.Infrastructure.CQRS.Commands;
using DummyApp.ApiGateway.Infrastructure.HttpClients;
using DummyApp.ApiGateway.Infrastructure.Models.Dtos;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DummyApp.ApiGateway.Infrastructure.CQRS.CommandHandlers;

public sealed class CreateArtworkCommandHandler : IRequestHandler<CreateArtworkCommand, ArtworkDto?>
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

    public async Task<ArtworkDto?> Handle(CreateArtworkCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.FileName))
        {
            _logger.LogError("No file name provided for artwork creation.");
            return null;
        }

        var extension = Path.GetExtension(request.FileName);
        if (string.IsNullOrWhiteSpace(extension))
        {
            _logger.LogError("File name {FileName} does not have a valid extension.", request.FileName);
            return null;
        }

        var fileName = $"{Guid.NewGuid()}{extension}";

        var blobUrl = await _blobServiceClient.UploadImageAsync(request.UploadedImage, fileName, cancellationToken);
        if (string.IsNullOrEmpty(blobUrl))
        {
            _logger.LogError("Failed to upload image to blob storage.");
            return null;
        }

        var createArtwork = new ArtworkDto
        {
            CreatorId = request.CreatorId,
            CreationDate = request.CreationDate,
            Description = request.Description,
            ImgUrl = blobUrl,
            IsActive = request.IsActive,
            Name = request.Name,
            PublicName = request.Name, // Assuming public name is the same as name for simplicity.
            SmallImgUrl = blobUrl, // Assuming the same URL for simplicity; in a real scenario, you might generate a thumbnail.
            UploadDate = DateTime.UtcNow
        };


        var result = await _storageServiceClient.CreateArtworkAsync(createArtwork, cancellationToken);
        if (result is null)
        {
            _logger.LogError("Failed to create artwork in storage service.");
            return null;
        }

        return result;
    }
}
