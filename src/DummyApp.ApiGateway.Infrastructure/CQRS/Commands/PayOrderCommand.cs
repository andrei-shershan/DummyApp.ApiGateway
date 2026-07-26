using MediatR;

namespace DummyApp.ApiGateway.Infrastructure.CQRS.Commands;

public sealed record PayOrderCommand(Guid OrderId) : IRequest<bool>;
