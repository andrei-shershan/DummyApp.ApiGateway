namespace DummyApp.ApiGateway.Infrastructure.Models.Dtos;

public sealed record OrderSummaryDto
{
    public Guid OrderId { get; init; }
    public IEnumerable<OrderItemDto> Items { get; init; } = Array.Empty<OrderItemDto>();
    public string Status { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public OrderAddressDto? Address { get; init; }
}
