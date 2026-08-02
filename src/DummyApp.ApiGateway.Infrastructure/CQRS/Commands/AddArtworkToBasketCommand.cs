using MediatR;

namespace DummyApp.ApiGateway.Infrastructure.CQRS.Commands;

public sealed record AddArtworkToBasketCommand(Guid OrderId, Guid ArtworkId, int Quantity = 1, int? PrintSizeId = null, int? PriceId = null) : IRequest<bool>;
