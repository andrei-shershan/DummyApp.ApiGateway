namespace DummyApp.ApiGateway.Infrastructure.Models;

public sealed record AnalyticsServiceOptions
{
    public string? SecretKey { get; init; }
}
