namespace DummyApp.ApiGateway.Infrastructure.Models.Dtos;

public sealed record OrderStatusDto
{
    public string Status { get; init; } = string.Empty;
}
