namespace DummyApp.ApiGateway.Infrastructure.Models.Dtos;

public sealed record PrintSizeDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public IEnumerable<PriceDto> Prices { get; init; } = Array.Empty<PriceDto>();
}
