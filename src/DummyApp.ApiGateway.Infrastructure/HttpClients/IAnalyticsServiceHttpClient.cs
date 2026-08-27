namespace DummyApp.ApiGateway.Infrastructure.HttpClients;

public interface IAnalyticsServiceHttpClient
{
    Task PublishEventAsync(AnalyticsEventRequest request, CancellationToken cancellationToken);
}
