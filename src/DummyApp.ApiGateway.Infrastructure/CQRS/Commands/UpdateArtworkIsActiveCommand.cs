using DummyApp.ApiGateway.Infrastructure.Models.Dtos;
using MediatR;

namespace DummyApp.ApiGateway.Infrastructure.CQRS.Commands;

public sealed record UpdateArtworkIsActiveCommand(Guid ArtworkId, bool IsActive) : IRequest<ArtworkDto?>;
