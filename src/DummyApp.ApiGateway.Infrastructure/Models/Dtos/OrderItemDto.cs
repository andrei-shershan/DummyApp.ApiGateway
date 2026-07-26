using System;

namespace DummyApp.ApiGateway.Infrastructure.Models.Dtos;

public sealed record OrderItemDto
{
    public Guid OrderId { get; init; }
    public Guid ArtworkId { get; init; }
    public int Quantity { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string ImgUrl { get; init; } = string.Empty;
    public string ThumbnailUrl { get; init; } = string.Empty;
}
