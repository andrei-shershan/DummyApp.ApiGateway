namespace DummyApp.ApiGateway.Infrastructure.Models;

public sealed record BlobStorageOptions
{
    public string? SecretKey { get; init; }
}
