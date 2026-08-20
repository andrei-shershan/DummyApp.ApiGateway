using MediatR;

namespace DummyApp.ApiGateway.Infrastructure.CQRS.Commands;

public sealed record SendVerificationCodeCommand(string Email) : IRequest<bool>;
