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

        var uploadResult = await _blobServiceClient.UploadImageAsync(request.UploadedImage, fileName, cancellationToken);
        if (uploadResult is null || string.IsNullOrEmpty(uploadResult.Url) || string.IsNullOrEmpty(uploadResult.ThumbnailUrl))
        {
            _logger.LogError("Failed to upload image to blob storage.");
            return null;
        }

        var createArtwork = new ArtworkDto
        {
            CreatorId = request.CreatorId,
            CreationDate = request.CreationDate,
            Description = request.Description,
            ImgUrl = uploadResult.Url,
            IsActive = request.IsActive,
            Name = request.Name,
            SeriesName = request.SeriesName,
            ThumbnailUrl = uploadResult.ThumbnailUrl,
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
