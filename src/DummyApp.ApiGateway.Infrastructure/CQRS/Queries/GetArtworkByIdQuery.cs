using DummyApp.ApiGateway.Infrastructure.Models.Dtos;
using MediatR;

namespace DummyApp.ApiGateway.Infrastructure.CQRS.Queries;

public sealed record GetArtworkByIdQuery(Guid Id, bool ActiveOnly = true) : IRequest<ArtworkDto?>;
