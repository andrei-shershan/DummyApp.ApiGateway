namespace DummyApp.ApiGateway.Infrastructure.Models.Dtos;

public sealed record ArtworkAuthorDto
{
    public string Id { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
}
