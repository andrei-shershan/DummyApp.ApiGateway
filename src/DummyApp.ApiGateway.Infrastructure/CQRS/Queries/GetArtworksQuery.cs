using MediatR;
using System;
using System.Collections.Generic;

namespace DummyApp.ApiGateway.Infrastructure.CQRS.Queries;

public sealed record ArtworkDto(
    int Id,
    string CreatorId,
    string Name,
    string PublicName,
    string Description,
    DateTime CreationDate,
    DateTime UploadDate,
    string ImgUrl,
    string SmallImgUrl,
    bool IsActive);

public sealed record GetArtworksQuery() : IRequest<IEnumerable<ArtworkDto>>;
