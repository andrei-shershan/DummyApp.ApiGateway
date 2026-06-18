namespace DummyApp.ApiGateway.Infrastructure.Models;

public sealed record BlobStorageOptions
{
    public string? StorageUrl { get; init; }
    public string? ContainerName { get; init; }
    public string? SecretKey { get; init; }
}
