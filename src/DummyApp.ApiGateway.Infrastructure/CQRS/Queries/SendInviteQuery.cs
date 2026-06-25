using MediatR;

namespace DummyApp.ApiGateway.Infrastructure.CQRS.Queries;

public sealed record SendInviteQuery(string Email) : IRequest<bool>;
