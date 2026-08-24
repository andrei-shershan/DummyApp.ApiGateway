using DummyApp.ApiGateway.Infrastructure.Models.Dtos;
using MediatR;

namespace DummyApp.ApiGateway.Infrastructure.CQRS.Commands;

public sealed record UpdateUserAvatarCommand(string UserId, string FileName, string Base64Image) : IRequest<UserDto?>;
