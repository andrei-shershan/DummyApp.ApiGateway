using MediatR;

namespace DummyApp.ApiGateway.Infrastructure.CQRS.Commands;

public sealed record SetOrderStatusCommand(Guid OrderId, string Status) : IRequest<bool>;
