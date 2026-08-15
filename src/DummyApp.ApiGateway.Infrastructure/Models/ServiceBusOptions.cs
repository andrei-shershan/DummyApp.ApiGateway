namespace DummyApp.ApiGateway.Infrastructure.Models;

public sealed record ServiceBusOptions
{
    public string? ConnectionString { get; init; }
    public string? CompletedOrderEventsQueueName { get; init; }
}
