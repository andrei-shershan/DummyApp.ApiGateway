namespace DummyApp.ApiGateway.Infrastructure.Models.Dtos;

public sealed record ArtworkDto
{
    public Guid Id { get; init; }
    public string CreatorId { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public DateTime CreationDate { get; init; } = DateTime.UtcNow;
    public DateTime UploadDate { get; init; }
    public string ImgUrl { get; init; } = string.Empty;
    public string ThumbnailUrl { get; init; } = string.Empty;
    public bool IsActive { get; init; }
}


