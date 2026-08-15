namespace DummyApp.ApiGateway.Infrastructure.Models;

public sealed record FileServiceOptions
{
    public string? SecretKey { get; init; }
}
