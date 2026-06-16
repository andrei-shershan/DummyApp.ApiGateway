namespace DummyApp.ApiGateway.Infrastructure.Models;

public sealed record ClientCredentialsTokenCacheOptions
{
    public string? Authority { get; init; }
    public string? ClientId { get; init; }
    public string? ClientSecret { get; init; }
}
