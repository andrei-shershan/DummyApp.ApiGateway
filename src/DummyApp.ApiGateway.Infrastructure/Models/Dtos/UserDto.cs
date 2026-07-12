namespace DummyApp.ApiGateway.Infrastructure.Models.Dtos;

public sealed record UserDto
{
    public string Id { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public bool IsActive { get; init; } = true;
    public IEnumerable<string> Roles { get; init; } = Array.Empty<string>();
}
