using DummyApp.ApiGateway.Infrastructure.Models.Dtos;
using MediatR;

namespace DummyApp.ApiGateway.Infrastructure.CQRS.Commands;

public sealed record UpdateUserActiveStateCommand(string UserId, bool IsActive) : IRequest<UserDto?>;
