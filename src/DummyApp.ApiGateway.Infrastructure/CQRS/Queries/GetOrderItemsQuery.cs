using DummyApp.ApiGateway.Infrastructure.Models.Dtos;
using MediatR;

namespace DummyApp.ApiGateway.Infrastructure.CQRS.Queries;

public sealed record GetOrderItemsQuery(Guid OrderId) : IRequest<IEnumerable<OrderItemDto>?>;
