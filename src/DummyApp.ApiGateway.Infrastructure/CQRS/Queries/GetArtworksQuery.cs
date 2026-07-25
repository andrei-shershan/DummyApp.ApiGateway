using DummyApp.ApiGateway.Infrastructure.Models.Dtos;
using MediatR;

namespace DummyApp.ApiGateway.Infrastructure.CQRS.Queries;

public sealed record GetArtworksQuery(string? CreatorId = null, bool IsActive = true) : IRequest<IEnumerable<ArtworkDto>>;

public sealed record ArtworkQueryFilter(string? CreatorId, bool IsActive);
