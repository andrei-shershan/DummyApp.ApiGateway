using DummyApp.ApiGateway.Infrastructure.Models.Dtos;
using MediatR;

namespace DummyApp.ApiGateway.Infrastructure.CQRS.Commands;

public sealed record UpdateUserProfileCommand(string UserId, string FirstName, string LastName) : IRequest<UserDto?>;
