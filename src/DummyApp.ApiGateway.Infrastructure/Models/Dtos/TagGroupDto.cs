namespace DummyApp.ApiGateway.Infrastructure.Models.Dtos;

public sealed record TagGroupDto
{
    public string TagType { get; init; } = string.Empty;
    public IEnumerable<TagDto> Tags { get; init; } = Array.Empty<TagDto>();
}
