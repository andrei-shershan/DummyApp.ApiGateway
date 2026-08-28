using DummyApp.ApiGateway.Infrastructure.Models.Dtos;

namespace DummyApp.ApiGateway.Infrastructure.HttpClients;

public interface IAnalyticsServiceHttpClient
{
    Task PublishEventAsync(AnalyticsEventRequest request, CancellationToken cancellationToken);
    Task<IEnumerable<AnalyticsEventDto>> GetAnalyticsAsync(int periodDays, CancellationToken cancellationToken);
}
