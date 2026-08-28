using DummyApp.ApiGateway.Infrastructure.Models.Dtos;

namespace DummyApp.ApiGateway.Infrastructure.HttpClients;

public sealed record AnalyticsEventRequest
{
    public Guid OrderId { get; init; }
    public string Status { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string SiteId { get; init; } = string.Empty;
    public AnalyticsOrderAddress? Address { get; init; }
    public IEnumerable<AnalyticsOrderItem> Items { get; init; } = Array.Empty<AnalyticsOrderItem>();
    public IEnumerable<string> Tags { get; init; } = Array.Empty<string>();
    public DateTimeOffset EventTimestamp { get; init; }
}

public sealed record AnalyticsOrderAddress
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Country { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string Street { get; init; } = string.Empty;
    public string HouseNumber { get; init; } = string.Empty;
    public string PostalCode { get; init; } = string.Empty;
}

public sealed record AnalyticsOrderItem
{
    public Guid OrderId { get; init; }
    public Guid ArtworkId { get; init; }
    public int Quantity { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string ImgUrl { get; init; } = string.Empty;
    public string ThumbnailUrl { get; init; } = string.Empty;
    public int? PrintSizeId { get; init; }
    public string PrintSizeName { get; init; } = string.Empty;
    public int? PriceId { get; init; }
    public decimal? PriceValue { get; init; }
    public IEnumerable<AnalyticsOrderTag> Tags { get; init; } = Array.Empty<AnalyticsOrderTag>();
}

public sealed record AnalyticsOrderTag
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
}
