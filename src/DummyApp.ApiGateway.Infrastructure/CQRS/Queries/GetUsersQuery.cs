using DummyApp.ApiGateway.Infrastructure.Models.Dtos;
using MediatR;

namespace DummyApp.ApiGateway.Infrastructure.CQRS.Queries;

public sealed record GetUsersQuery() : IRequest<IEnumerable<UserDto>>;
