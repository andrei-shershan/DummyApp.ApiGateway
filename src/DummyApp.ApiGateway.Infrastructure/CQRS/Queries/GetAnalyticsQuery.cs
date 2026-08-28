using DummyApp.ApiGateway.Infrastructure.Models.Dtos;
using MediatR;

namespace DummyApp.ApiGateway.Infrastructure.CQRS.Queries;

public sealed record GetAnalyticsQuery(int PeriodDays) : IRequest<IEnumerable<AnalyticsEventDto>>;
