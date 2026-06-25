namespace DummyApp.ApiGateway.Infrastructure.Models;

public sealed record EmailServiceOptions
{
    public string? SecretKey { get; init; }
}
