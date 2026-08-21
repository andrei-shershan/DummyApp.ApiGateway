using DummyApp.ApiGateway.Infrastructure.Models.Dtos;
using MediatR;

namespace DummyApp.ApiGateway.Infrastructure.CQRS.Queries;

public sealed record GetCompletedOrdersQuery(Guid Token) : IRequest<IEnumerable<OrderSummaryDto>?>;
