using DummyApp.ApiGateway.Infrastructure.HttpClients;

namespace DummyApp.ApiGateway.Infrastructure.Models.Dtos;

public sealed record AnalyticsEventDto(
    string Id,
    Guid OrderId,
    string Status,
    string Email,
    string SiteId,
    AnalyticsOrderAddress? Address,
    IEnumerable<AnalyticsOrderItem> Items,
    IEnumerable<string> Tags,
    DateTimeOffset EventTimestamp);
