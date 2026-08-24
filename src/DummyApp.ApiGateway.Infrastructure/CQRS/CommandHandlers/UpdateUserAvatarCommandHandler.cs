using System;
using System.IO;
using DummyApp.ApiGateway.Infrastructure.CQRS.Commands;
using DummyApp.ApiGateway.Infrastructure.HttpClients;
using DummyApp.ApiGateway.Infrastructure.Models;
using DummyApp.ApiGateway.Infrastructure.Models.Dtos;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DummyApp.ApiGateway.Infrastructure.CQRS.CommandHandlers;

public sealed class UpdateUserAvatarCommandHandler : IRequestHandler<UpdateUserAvatarCommand, UserDto?>
{
    private readonly IBlobServiceHttpClient _blobServiceClient;
    private readonly IIdentityServiceHttpClient _identityServiceClient;
    private readonly ILogger<UpdateUserAvatarCommandHandler> _logger;

    public UpdateUserAvatarCommandHandler(
        IBlobServiceHttpClient blobServiceClient,
        IIdentityServiceHttpClient identityServiceClient,
        ILogger<UpdateUserAvatarCommandHandler> logger)
    {
        _blobServiceClient = blobServiceClient;
        _identityServiceClient = identityServiceClient;
        _logger = logger;
    }

    public async Task<UserDto?> Handle(UpdateUserAvatarCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.UserId))
        {
            _logger.LogError("Invalid user id supplied for avatar update.");
            return null;
        }

        if (string.IsNullOrWhiteSpace(request.FileName) || string.IsNullOrWhiteSpace(request.Base64Image))
        {
            _logger.LogError("Invalid avatar upload request. FileName or Base64Image is missing.");
            return null;
        }

        var extension = Path.GetExtension(request.FileName);
        if (string.IsNullOrWhiteSpace(extension))
        {
            _logger.LogError("Avatar file name {FileName} does not have a valid extension.", request.FileName);
            return null;
        }

        var fileName = $"{Guid.NewGuid()}{extension}";

        var uploadResult = await _blobServiceClient.UploadImageAsync(request.Base64Image, fileName, ImageType.Avatar, cancellationToken);
        if (uploadResult is null || string.IsNullOrWhiteSpace(uploadResult.Url) || string.IsNullOrWhiteSpace(uploadResult.ThumbnailUrl))
        {
            _logger.LogError("Failed to upload avatar image to BlobService.");
            return null;
        }

        return await _identityServiceClient.UpdateUserAvatarAsync(request.UserId, uploadResult.Url, uploadResult.ThumbnailUrl, cancellationToken);
    }
}
