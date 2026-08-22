using System;
using System.Collections.Generic;

namespace DummyApp.ApiGateway.Infrastructure.Models.Dtos;

public sealed record CreateArtworkRequestDto(
    string Name,
    string FileName,
    string Description,
    DateTime CreationDate,
    DateTime UploadDate,
    string ImgUrl,
    string ThumbnailUrl,
    bool IsActive,
    string CreatorId,
    IEnumerable<Guid> ExistingTagIds,
    IEnumerable<CreateArtworkTagDto> NewTags);
