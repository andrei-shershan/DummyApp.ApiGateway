using DummyApp.ApiGateway.Infrastructure.Models.Dtos;
using MediatR;

namespace DummyApp.ApiGateway.Infrastructure.CQRS.Commands;

public sealed record CreateArtworkCommand(
    string Name,
    string FileName,
    string Description,
    DateTime CreationDate,
    bool IsActive,
    string UploadedImage,
    string CreatorId,
    IEnumerable<Guid> ExistingTagIds,
    IEnumerable<CreateArtworkTagDto> NewTags) : IRequest<ArtworkDto?>;
