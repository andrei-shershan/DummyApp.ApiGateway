namespace DummyApp.ApiGateway.Infrastructure.Models.Dtos;

public sealed record PriceDto
{
    public int Id { get; init; }
    public int PrintSizeId { get; init; }
    public decimal Value { get; init; }
    public DateTime UpdatedAt { get; init; }
    public bool IsDeleted { get; init; }
}
