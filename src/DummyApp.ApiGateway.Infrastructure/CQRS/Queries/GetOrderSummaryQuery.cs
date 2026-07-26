using DummyApp.ApiGateway.Infrastructure.Models.Dtos;
using MediatR;

namespace DummyApp.ApiGateway.Infrastructure.CQRS.Queries;

public sealed record GetOrderSummaryQuery(Guid OrderId) : IRequest<OrderSummaryDto?>;
