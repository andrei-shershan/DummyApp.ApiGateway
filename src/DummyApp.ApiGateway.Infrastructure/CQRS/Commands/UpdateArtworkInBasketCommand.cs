using MediatR;

namespace DummyApp.ApiGateway.Infrastructure.CQRS.Commands;

public sealed record UpdateArtworkInBasketCommand(Guid OrderId, Guid ArtworkId, int Quantity, int? PrintSizeId = null, int? PriceId = null) : IRequest<bool>;
