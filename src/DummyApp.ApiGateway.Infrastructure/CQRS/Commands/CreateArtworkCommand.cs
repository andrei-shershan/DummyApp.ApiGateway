using MediatR;
using System;

namespace DummyApp.ApiGateway.Infrastructure.CQRS.Commands;

public sealed record CreateArtworkCommand(
    string Name,
    string Description,
    DateTime CreationDate,
    string ImgUrl,
    string SmallImgUrl,
    bool IsActive,
    string? UploadedImage) : IRequest<CreateArtworkCommandResult>;

public sealed record CreateArtworkCommandResult(
    int Id,
    string CreatorId,
    string Name,
    string Description,
    DateTime CreationDate,
    DateTime UploadDate,
    string ImgUrl,
    string SmallImgUrl,
    bool IsActive);
