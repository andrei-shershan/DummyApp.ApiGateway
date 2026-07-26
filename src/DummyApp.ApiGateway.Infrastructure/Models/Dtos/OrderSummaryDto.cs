namespace DummyApp.ApiGateway.Infrastructure.Models.Dtos;

public sealed record OrderSummaryDto
{
    public IEnumerable<OrderItemDto> Items { get; init; } = Array.Empty<OrderItemDto>();
    public string Status { get; init; } = string.Empty;
}
