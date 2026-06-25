using MediatR;

namespace DummyApp.ApiGateway.Infrastructure.CQRS.Commands;

public sealed record SendInviteCommand(string Email) : IRequest<bool>;
