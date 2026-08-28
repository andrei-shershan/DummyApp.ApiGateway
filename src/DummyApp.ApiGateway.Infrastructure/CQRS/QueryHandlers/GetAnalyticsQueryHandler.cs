using DummyApp.ApiGateway.Infrastructure.CQRS.Queries;
using DummyApp.ApiGateway.Infrastructure.HttpClients;
using DummyApp.ApiGateway.Infrastructure.Models.Dtos;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DummyApp.ApiGateway.Infrastructure.CQRS.QueryHandlers;

public sealed class GetAnalyticsQueryHandler : IRequestHandler<GetAnalyticsQuery, IEnumerable<AnalyticsEventDto>>
{
    private readonly IAnalyticsServiceHttpClient _analyticsServiceClient;
    private readonly ILogger<GetAnalyticsQueryHandler> _logger;

    public GetAnalyticsQueryHandler(IAnalyticsServiceHttpClient analyticsServiceClient, ILogger<GetAnalyticsQueryHandler> logger)
    {
        _analyticsServiceClient = analyticsServiceClient;
        _logger = logger;
    }

    public async Task<IEnumerable<AnalyticsEventDto>> Handle(GetAnalyticsQuery request, CancellationToken cancellationToken)
    {
        if (request.PeriodDays <= 0)
        {
            _logger.LogWarning("GetAnalyticsQueryHandler received invalid periodDays {PeriodDays}.", request.PeriodDays);
            return Array.Empty<AnalyticsEventDto>();
        }

        return await _analyticsServiceClient.GetAnalyticsAsync(request.PeriodDays, cancellationToken);
    }
}
