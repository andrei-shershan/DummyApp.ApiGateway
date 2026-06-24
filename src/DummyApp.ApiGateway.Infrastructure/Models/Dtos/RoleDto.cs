namespace DummyApp.ApiGateway.Infrastructure.Models.Dtos;

public sealed record RoleDto
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
}
