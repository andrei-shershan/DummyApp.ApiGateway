namespace DummyApp.ApiGateway.Infrastructure.Models.Dtos;

public sealed record ArtworkFiltersDto
{
    public IEnumerable<TagGroupDto> TagGroups { get; init; } = Array.Empty<TagGroupDto>();
    public IEnumerable<ArtworkAuthorDto> Authors { get; init; } = Array.Empty<ArtworkAuthorDto>();
}
