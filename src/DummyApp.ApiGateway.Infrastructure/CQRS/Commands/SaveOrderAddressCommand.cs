using DummyApp.ApiGateway.Infrastructure.Models.Dtos;
using MediatR;

namespace DummyApp.ApiGateway.Infrastructure.CQRS.Commands;

public sealed record SaveOrderAddressCommand(Guid OrderId, OrderAddressDto Address) : IRequest<bool>;
