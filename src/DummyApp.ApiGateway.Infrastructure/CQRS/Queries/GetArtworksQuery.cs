using DummyApp.ApiGateway.Infrastructure.Models.Dtos;
using MediatR;

namespace DummyApp.ApiGateway.Infrastructure.CQRS.Queries;

public sealed record GetArtworksQuery(string? CreatorId = null, bool IsActive = true) : IRequest<IEnumerable<ArtworkDto>>;
public sealed record GetArtworksPageQuery(string? CreatorId = null, bool IsActive = true, int PageNumber = 1, int PageSize = 10, IEnumerable<Guid>? TagIds = null) : IRequest<PaginatedResult<ArtworkDto>>;

public sealed record ArtworkQueryFilter(string? CreatorId, bool IsActive);
